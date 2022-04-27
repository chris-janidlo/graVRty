using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using crass;

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
        public struct SizeSpec
        {
            public float Angle, Length;
        }

        [SerializeField] SizeSpec m_Size;
        [SerializeField] Direction m_RayDirection;
        [SerializeField] int m_Raycasts;

        [SerializeField] LayerMask m_RaycastLayers;
        [SerializeField] bool m_DrawDebugRays;
        [SerializeField] EnumMap<RaycastHitState, Color> m_DebugRayColors;

        [SerializeField] Light m_Light;
        [SerializeField] Transform m_ConeScaler, m_ConePointTransform, m_ConeBaseTransform;

        List<Vector3> circlePoints;
        Dictionary<FlashlightBeamTarget, BeamHitInfo> targetTracking;

        void Start ()
        {
            if (m_Light.type != LightType.Spot)
            {
                throw new Exception("Flashlight Light should be a spotlight");
            }

            targetTracking = new Dictionary<FlashlightBeamTarget, BeamHitInfo>();

            applySize();
            calculateCartesianPoints();
        }

        void FixedUpdate ()
        {
            targetTracking.Clear();

            foreach (Vector3 circlePoint in circlePoints)
            {
                Vector3 circlePointInWorld = m_ConeBaseTransform.TransformPoint(circlePoint);

                Vector3 startPosition, endPosition;
                switch (m_RayDirection)
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
                        throw new InvalidOperationException($"{nameof(m_RayDirection)} has unexpected {typeof(Direction).Name} value {m_RayDirection}");
                }

                Vector3 rayCastDifference = endPosition - startPosition;

                if (!Physics.Raycast(startPosition, rayCastDifference.normalized, out RaycastHit rayHitInfo, rayCastDifference.magnitude, m_RaycastLayers))
                {
                    drawRay(startPosition, rayCastDifference, RaycastHitState.Missed);
                    continue;
                }

                Vector3 hitDifference = rayHitInfo.point - startPosition;

                if (!rayHitInfo.collider.TryGetComponent(out FlashlightBeamTarget target))
                {
                    drawRay(startPosition, hitDifference, RaycastHitState.HitNonTarget);
                    continue;
                }

                drawRay(startPosition, hitDifference, RaycastHitState.HitTarget);

                if (!targetTracking.TryGetValue(target, out BeamHitInfo beamHitInfo))
                {
                    beamHitInfo = new BeamHitInfo();
                }

                targetTracking[target] = beamHitInfo.PlusRayHit(rayHitInfo.point);
            }

            foreach (var pair in targetTracking)
            {
                pair.Key.TrackBeamHit(pair.Value);
            }
        }

        void applySize ()
        {
            m_Light.range = m_Size.Length;
            m_Light.spotAngle = m_Size.Angle;

            // radius / length = tan(angle)
            float baseRadius = m_Size.Length * Mathf.Tan(m_Size.Angle * Mathf.Deg2Rad / 2);
            m_ConeScaler.localScale = new Vector3(baseRadius, baseRadius, m_Size.Length);
        }

        void calculateCartesianPoints ()
        {
            circlePoints = new List<Vector3>();

            // uses algorithm from https://stackoverflow.com/a/28572551/5931898, which is a modified sunflower seed arrangement with additional bias toward the edge of the circle
            const float phiSquared = 2.61803398875f; // golden ratio, squared
            const float alpha = 2; // amount of edge bias. author recommends 2

            int boundaryPointCount = Mathf.RoundToInt(alpha * Mathf.Sqrt(m_Raycasts));

            for (int i = 0; i < m_Raycasts; i++)
            {
                float polarRadius = i > m_Raycasts - boundaryPointCount
                    ? 1
                    : Mathf.Sqrt(i - .5f) / Mathf.Sqrt(m_Raycasts - (boundaryPointCount + 1) / 2f); // normal sunflower seed arrangement would just use Mathf.Sqrt(i - .5f), but we apply an additional scaling factor to account for the boundary
                float polarAngle = 2 * Mathf.PI * i / phiSquared;

                Vector2 cartesianPoint = new Vector2(polarRadius * Mathf.Cos(polarAngle), polarRadius * Mathf.Sin(polarAngle));
                circlePoints.Add(cartesianPoint);
            }
        }

        void drawRay (Vector3 start, Vector3 direction, RaycastHitState hitState)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (m_DrawDebugRays) Debug.DrawRay(start, direction, m_DebugRayColors[hitState]);
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
        }
    }
}
