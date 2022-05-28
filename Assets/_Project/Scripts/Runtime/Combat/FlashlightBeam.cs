using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Profiling;
using crass;
using NaughtyAttributes;

namespace GraVRty.Combat
{
    public class FlashlightBeam : MonoBehaviour
    {
        public enum RaycastHitState
        {
            Missed, HitNonTarget, HitTarget
        }

        public enum Direction
        {
            PointToBase, BaseToPoint
        }

        [Serializable]
        public struct Dimensions
        {
            [ShowIf("Direction", Direction.PointToBase)]
            [AllowNesting]
            public float Angle;
            [ShowIf("Direction", Direction.BaseToPoint)]
            [AllowNesting]
            public float Radius;
            public float Length;
            public Direction Direction;

            public static bool operator == (Dimensions l, Dimensions r)
            {
                // should this only use one of Angle or Radius depending on the value of Direction?
                return
                    l.Angle == r.Angle &&
                    l.Radius == r.Radius &&
                    l.Length == r.Length &&
                    l.Direction == r.Direction;
            }

            public static bool operator != (Dimensions l, Dimensions r) => !(l == r);

            public Dimensions (float angle, float radius, float length, Direction direction)
            {
                Angle = angle;
                Radius = radius;
                Length = length;
                Direction = direction;
            }

            public Dimensions With (float? angle = null, float? radius = null, float? length = null, Direction? direction = null)
            {
                return new Dimensions(angle ?? Angle, radius ?? Radius, length ?? Length, direction ?? Direction);
            }

            public override bool Equals(object obj)
            {
                return obj is Dimensions other && this == other;
            }

            public override int GetHashCode()
            {
                // should this only use one of Angle or Radius depending on the value of Direction?
                return (Angle, Radius, Length, Direction).GetHashCode();
            }
        }

        [SerializeField] Dimensions m_InitialDimensions;
        [SerializeField] int m_Raycasts;

        [SerializeField] LayerMask m_RaycastLayers;
        [SerializeField] bool m_DrawDebugRays;
        [SerializeField] EnumMap<RaycastHitState, Color> m_DebugRayColors;

        [SerializeField] Light m_Light;
        [SerializeField] Transform m_Offset, m_ConeScaler, m_ConePointTransform, m_ConeBaseTransform;

        public Dimensions CurrentDimensions { get; private set; }

        List<Vector3> circlePoints;
        Dictionary<FlashlightBeamTarget, BeamHitInfo> hitDataThisFrame; // create this at class scope to limit GC
        HashSet<FlashlightBeamTarget> targetsHitLastFrame;
        HashSet<FlashlightBeamTarget> targetsToManage;

        void Start ()
        {
            if (m_Light.type != LightType.Spot)
            {
                throw new Exception("Flashlight Light should be a spotlight");
            }

            hitDataThisFrame = new Dictionary<FlashlightBeamTarget, BeamHitInfo>();
            targetsHitLastFrame = new HashSet<FlashlightBeamTarget>();
            targetsToManage = new HashSet<FlashlightBeamTarget>();

            ResetDimensions();
            calculateCartesianPoints();
        }

        void FixedUpdate ()
        {
            targetTracking();
        }

        void OnDisable ()
        {
            cleanupTargetHits();
        }

        public void ResetDimensions ()
        {
            SetDimensions(m_InitialDimensions);
        }

        public void SetDimensions (Dimensions dimensions)
        {
            CurrentDimensions = dimensions;

            Vector3 position;
            Quaternion rotation;

            // radius / length = tan(angle)
            float length = CurrentDimensions.Length;
            float radius, angle;

            switch (CurrentDimensions.Direction)
            {
                case Direction.BaseToPoint:
                    position = Vector3.forward * length;
                    rotation = Quaternion.Euler(0, 180, 0);
                    
                    radius = CurrentDimensions.Radius;
                    angle = Mathf.Atan(radius / length) * Mathf.Rad2Deg * 2;
                    break;

                case Direction.PointToBase:
                    position = Vector3.zero;
                    rotation = Quaternion.identity;

                    angle = CurrentDimensions.Angle;
                    radius = length * Mathf.Tan(angle * Mathf.Deg2Rad / 2);
                    break;

                default:
                    throw new InvalidOperationException($"{nameof(CurrentDimensions.Direction)} has unexpected {typeof(Direction).Name} value {CurrentDimensions.Direction}");
            }

            m_Offset.localPosition = position;
            m_Offset.localRotation = rotation;

            m_Light.range = length;
            m_Light.spotAngle = angle;

            m_ConeScaler.localScale = new Vector3(radius, radius, CurrentDimensions.Length);
        }

        public void SetDimension (float? length = null, float? angle = null, float? radius = null, Direction? direction = null)
        {
            Dimensions newDimensions = CurrentDimensions.With(length: length, angle: angle, radius: radius, direction: direction);
            if (newDimensions == CurrentDimensions) return;
            SetDimensions(newDimensions);
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

            foreach (FlashlightBeamTarget target in targetsToManage)
            {
                manageTargetHitState(target);
            }
        }

        void castRay (Vector3 circlePoint)
        {
            Vector3 circlePointInWorld = m_ConeBaseTransform.TransformPoint(circlePoint);

            Vector3 startPosition, endPosition;
            switch (CurrentDimensions.Direction)
            {
                case Direction.PointToBase:
                    startPosition = m_ConePointTransform.position;
                    endPosition = circlePointInWorld;
                    break;

                case Direction.BaseToPoint:
                    startPosition = circlePointInWorld;
                    endPosition = m_ConePointTransform.position;
                    break;

                default:
                    throw new InvalidOperationException($"{nameof(CurrentDimensions.Direction)} has unexpected {typeof(Direction).Name} value {CurrentDimensions.Direction}");
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
            if (hitDataThisFrame.TryGetValue(target, out BeamHitInfo hitInfo))
            {
                if (!targetsHitLastFrame.Contains(target)) target.OnBeamEntered.Invoke(hitInfo);
                target.OnBeamStay.Invoke(hitInfo);
                targetsHitLastFrame.Add(target);
            }
            else if (targetsHitLastFrame.Contains(target))
            {
                target.OnBeamExited.Invoke(this);
                targetsHitLastFrame.Remove(target);
            }
            else
            {
                throw new InvalidOperationException($"should only call {nameof(manageTargetHitState)} on a target that was hit on the current or immediately previous frame");
            }
        }

        void cleanupTargetHits ()
        {
            foreach (FlashlightBeamTarget target in targetsHitLastFrame)
            {
                target.OnBeamExited.Invoke(this);
            }

            targetsHitLastFrame.Clear();
        }
    } 
}
