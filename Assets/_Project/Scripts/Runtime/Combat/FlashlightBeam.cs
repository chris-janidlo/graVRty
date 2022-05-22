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

            public Dimensions (float angle, float radius, float length, Direction direction)
            {
                Angle = angle;
                Radius = radius;
                Length = length;
                Direction = direction;
            }

            public Dimensions WithLength (float length)
            {
                return new Dimensions(Angle, Radius, length, Direction);
            }

            public Dimensions WithRadius (float radius)
            {
                return new Dimensions(Angle, radius, Length, Direction);
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
        Dictionary<FlashlightBeamTarget, BeamHitInfo> targetTracking;

        void Start ()
        {
            if (m_Light.type != LightType.Spot)
            {
                throw new Exception("Flashlight Light should be a spotlight");
            }

            targetTracking = new Dictionary<FlashlightBeamTarget, BeamHitInfo>();

            ResetDimensions();
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
                    beamHitInfo = new BeamHitInfo(this, m_Raycasts);
                }

                targetTracking[target] = beamHitInfo.PlusRayHit(rayHitInfo.point);
            }

            foreach (var pair in targetTracking)
            {
                pair.Key.TrackBeamHit(pair.Value);
            }
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
                    position = Vector3.forward * CurrentDimensions.Length;
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

        public void SetLength (float length)
        {
            SetDimensions(CurrentDimensions.WithLength(length));
        }

        public void SetRadius (float radius)
        {
            SetDimensions(CurrentDimensions.WithRadius(radius));
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

        void drawRay (Vector3 start, Vector3 direction, RaycastHitState hitState)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (m_DrawDebugRays) Debug.DrawRay(start, direction, m_DebugRayColors[hitState]);
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
        }
    }
}
