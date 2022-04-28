using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
        public struct Dimensions
        {
            public float Angle, Length;
            public Direction Direction;
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
        Dictionary<FlashlightBeamTarget, BeamHitInfo> targetTracking;

        void Start ()
        {
            if (m_Light.type != LightType.Spot)
            {
                throw new Exception("Flashlight Light should be a spotlight");
            }

            targetTracking = new Dictionary<FlashlightBeamTarget, BeamHitInfo>();

            SetDimensions(m_InitialDimensions);
            calculateCartesianPoints();
        }

        void FixedUpdate ()
        {
            targetTracking.Clear();

            foreach (Vector3 circlePoint in circlePoints)
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
                    beamHitInfo = new BeamHitInfo(m_Raycasts);
                }

                targetTracking[target] = beamHitInfo.PlusRayHit(rayHitInfo.point);
            }

            foreach (var pair in targetTracking)
            {
                pair.Key.TrackBeamHit(pair.Value);
            }
        }

        public void SetDimensions (Dimensions dimensions)
        {
            CurrentDimensions = dimensions;

            switch (CurrentDimensions.Direction)
            {
                case Direction.BaseToPoint:
                    m_Offset.localRotation = Quaternion.Euler(0, 180, 0);
                    m_Offset.localPosition = Vector3.forward * CurrentDimensions.Length;
                    break;

                case Direction.PointToBase:
                    m_Offset.localRotation = Quaternion.identity;
                    m_Offset.localPosition = Vector3.zero;
                    break;

                default:
                    throw new InvalidOperationException($"{nameof(CurrentDimensions.Direction)} has unexpected {typeof(Direction).Name} value {CurrentDimensions.Direction}");
            }

            m_Light.range = CurrentDimensions.Length;
            m_Light.spotAngle = CurrentDimensions.Angle;

            // radius / length = tan(angle)
            float baseRadius = CurrentDimensions.Length * Mathf.Tan(CurrentDimensions.Angle * Mathf.Deg2Rad / 2);
            m_ConeScaler.localScale = new Vector3(baseRadius, baseRadius, CurrentDimensions.Length);
        }

        void calculateCartesianPoints ()
        {
            circlePoints = new List<Vector3>(m_Raycasts);

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
