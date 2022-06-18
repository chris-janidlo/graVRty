using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Profiling;
using crass;
using NaughtyAttributes;

namespace GraVRty.Flashlights
{
    public class FlashlightBeam : MonoBehaviour
    {
        public enum RaycastHitState
        {
            Missed, HitNonTarget, HitTarget
        }

        public enum Direction
        {
            _Unset,
            PointToBase, BaseToPoint
        }

        public Cone Dimensions;

        [SerializeField] Direction m_Direction;
        [SerializeField] int m_Raycasts;

        [Range(0, 1)]
        [SerializeField] float m_LockOnStrength;

        [SerializeField] LayerMask m_RaycastLayers;
        [SerializeField] bool m_DrawDebugRays;
        [SerializeField] EnumMap<RaycastHitState, Color> m_DebugRayColors;

        [SerializeField] AFlashlightBeamVisual m_BeamVisual;

        List<Vector3> circlePoints;
        Dictionary<FlashlightBeamTarget, BeamHitInfo> hitDataThisFrame; // create this at class scope to limit GC
        HashSet<FlashlightBeamTarget> targetsHitLastFrame;
        HashSet<FlashlightBeamTarget> targetsToManage;

        Transform coneTransform, coneTipTransform, coneBaseTransform;

        FlashlightBeamTarget currentLockOn;
        Vector3 lockOnCentroid;

        void Start ()
        {
            hitDataThisFrame = new Dictionary<FlashlightBeamTarget, BeamHitInfo>();
            targetsHitLastFrame = new HashSet<FlashlightBeamTarget>();
            targetsToManage = new HashSet<FlashlightBeamTarget>();

            Dimensions.SetCheckpoint();

            calculateCartesianPoints();

            createConeTransforms();
            updateConeTransforms();
        }

        void FixedUpdate ()
        {
            updateConeTransforms();
            targetTracking();
            if (currentLockOn != null) followLockOn();
        }

        void Update ()
        {
            m_BeamVisual.SetDimensions(Dimensions);
        }

        void OnDisable ()
        {
            cleanupTargetHits();
        }

        public void ResetDimensions ()
        {
            Dimensions.ResetToCheckpoint();
            updateConeTransforms();
            m_BeamVisual.SetDimensions(Dimensions);
        }

        void createConeTransforms ()
        {
            static Transform node (ref Transform variable, string name, params Transform[] children)
            {
                GameObject go = new ("(internal) " + name);
                variable = go.transform;

                foreach (Transform child in children)
                {
                    child.parent = variable;
                }
                
                return variable;
            }

            node(ref coneTransform, nameof(coneTransform),
                node(ref coneBaseTransform, nameof(coneBaseTransform)),
                node(ref coneTipTransform, nameof(coneTipTransform))
            ).parent = transform;

            coneTransform.SetPositionAndRotation(transform.position, transform.rotation);   

            switch (m_Direction)
            {
                case Direction.PointToBase:
                    coneTipTransform.localPosition = Vector3.zero;
                    coneBaseTransform.localPosition = Vector3.forward;
                    break;

                case Direction.BaseToPoint:
                    coneBaseTransform.localPosition = Vector3.zero;
                    coneTipTransform.localPosition = Vector3.forward;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(m_Direction));
            }
        }

        void updateConeTransforms ()
        {
            coneTransform.localScale = new Vector3(Dimensions.Radius, Dimensions.Radius, Dimensions.Height);
        }

        void calculateCartesianPoints ()
        {
            circlePoints = new List<Vector3>(m_Raycasts);

            // uses algorithm from https://stackoverflow.com/a/28572551/5931898, which is a modified sunflower seed arrangement with additional bias toward the edge of the circle
            const float phiSquared = 2.61803398875f; // golden ratio, squared
            const float alpha = 2; // amount of edge bias. author recommends 2

            int boundaryPointCount = Mathf.RoundToInt(alpha * Mathf.Sqrt(m_Raycasts));

            for (int i = 1; i <= m_Raycasts; i++)
            {
                float polarRadius = i > m_Raycasts - boundaryPointCount
                    ? 1
                    : Mathf.Sqrt(i - .5f) / Mathf.Sqrt(m_Raycasts - (boundaryPointCount + 1) / 2f); // normal sunflower seed arrangement would just use Mathf.Sqrt(i - .5f), but we apply an additional scaling factor to account for the boundary
                float polarAngle = 2 * Mathf.PI * i / phiSquared;

                Vector2 cartesianPoint = new Vector2(polarRadius * Mathf.Cos(polarAngle), polarRadius * Mathf.Sin(polarAngle));
                circlePoints.Add(cartesianPoint);
            }
        }

        void targetTracking ()
        {
            hitDataThisFrame.Clear();
            targetsToManage.Clear();

            foreach (Vector3 circlePoint in circlePoints)
            {
                castRay(circlePoint);
            }

            targetsToManage.UnionWith(targetsHitLastFrame);

            // manage currentLockOn separately so that we can stop locking on to it
            // before we consider locking on to other lockable targets
            if (currentLockOn != null)
            {
                targetsToManage.Remove(currentLockOn);
                manageTargetHitState(currentLockOn);
            }

            foreach (FlashlightBeamTarget target in targetsToManage)
            {
                manageTargetHitState(target);
            }
        }

        void castRay (Vector3 circlePoint)
        {
            Vector3 basePoint = coneBaseTransform.TransformPoint(circlePoint);
            Vector3 tipPoint = coneTipTransform.position;

            Vector3 startPosition, endPosition;
            switch (m_Direction)
            {
                case Direction.PointToBase:
                    startPosition = tipPoint;
                    endPosition = basePoint;
                    break;

                case Direction.BaseToPoint:
                    startPosition = basePoint;
                    endPosition = tipPoint;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(m_Direction));
            }

            Vector3 rayCastDifference = endPosition - startPosition;

            if (!Physics.Raycast(startPosition, rayCastDifference.normalized, out RaycastHit rayHitInfo, rayCastDifference.magnitude, m_RaycastLayers))
            {
                drawRay(startPosition, rayCastDifference, RaycastHitState.Missed);
                return;
            }

            Vector3 hitDifference = rayHitInfo.point - startPosition;

            if (!rayHitInfo.collider.TryGetComponent(out FlashlightBeamTarget target))
            {
                drawRay(startPosition, hitDifference, RaycastHitState.HitNonTarget);
                return;
            }

            drawRay(startPosition, hitDifference, RaycastHitState.HitTarget);

            if (!hitDataThisFrame.TryGetValue(target, out BeamHitInfo beamHitInfo))
            {
                beamHitInfo = new BeamHitInfo(this, m_Raycasts);
            }

            hitDataThisFrame[target] = beamHitInfo.PlusRayHit(rayHitInfo.point);
            targetsToManage.Add(target);
        }

        void drawRay(Vector3 start, Vector3 direction, RaycastHitState hitState)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (m_DrawDebugRays) Debug.DrawRay(start, direction, m_DebugRayColors[hitState]);
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
        }

        void manageTargetHitState (FlashlightBeamTarget target)
        {
            bool contact;

            if (hitDataThisFrame.TryGetValue(target, out BeamHitInfo hitInfo))
            {
                contact = true;

                if (!targetsHitLastFrame.Contains(target)) target.OnBeamEntered.Invoke(hitInfo);
                target.OnBeamStay.Invoke(hitInfo);
                targetsHitLastFrame.Add(target);
            }
            else if (targetsHitLastFrame.Contains(target))
            {
                contact = false;

                target.OnBeamExited.Invoke(this);
                targetsHitLastFrame.Remove(target);
            }
            else
            {
                throw new InvalidOperationException($"should only call {nameof(manageTargetHitState)} on a target that was hit on the current or immediately previous frame");
            }

            if (!target.Lockable
                || currentLockOn != null && target != currentLockOn) return;

            if (currentLockOn != null)
            {
                if (!contact || hitInfo.PercentageHit <= target.BeamHitPercentageToExitLock)
                {
                    stopLockOn();
                }
                else
                {
                    target.OnLockStay.Invoke(hitInfo);
                    lockOnCentroid = hitInfo.Centroid;
                }
            }
            else if (contact && hitInfo.PercentageHit >= target.BeamHitPercentageToEnterLock)
            {
                target.OnLockEntered.Invoke(hitInfo);
                target.OnLockStay.Invoke(hitInfo);
                currentLockOn = target;
                lockOnCentroid = hitInfo.Centroid;
            }
        }

        void followLockOn ()
        {
            Vector3
                zeroStrengthDirection = coneTransform.parent.forward,
                fullStrengthDirection = (lockOnCentroid - coneTransform.position).normalized,
                lerpedDirection = Vector3.Slerp(zeroStrengthDirection, fullStrengthDirection, m_LockOnStrength);

            coneTransform.forward = lerpedDirection;
        }

        void stopLockOn ()
        {
            coneTransform.forward = coneTransform.parent.forward;
            currentLockOn.OnLockExited.Invoke(this);
            currentLockOn = null;
        }

        void cleanupTargetHits ()
        {
            foreach (FlashlightBeamTarget target in targetsHitLastFrame)
            {
                target.OnBeamExited.Invoke(this);
            }

            targetsHitLastFrame.Clear();

            if (currentLockOn != null) stopLockOn();
        }
    } 
}
