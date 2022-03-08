using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using crass;

namespace GraVRty.Interactables
{
    public class FlashlightBeam : MonoBehaviour
    {
        public enum RaycastHitState
        {
            Missed, HitNonTarget, HitTarget
        }

        [Serializable]
        public struct SizeSpec
        {
            public float Angle, Length;
        }

        [SerializeField] SizeSpec m_Size;
        [SerializeField] int m_Raycasts;

        [SerializeField] LayerMask m_RaycastLayers;
        [SerializeField] bool m_DrawDebugRays;
        [SerializeField] EnumMap<RaycastHitState, Color> m_DebugRayColors;

        [SerializeField] Light m_Light;
        [SerializeField] Transform m_GeometryTransformer, m_Geometry;

        List<Vector3> circlePoints;

        void Start ()
        {
            if (m_Light.type != LightType.Spot)
            {
                throw new Exception("Flashlight Light should be a spotlight");
            }

            applySize();
            calculateCartesianPoints();
        }

        void FixedUpdate ()
        {
            foreach (FlashlightBeamTarget target in getTargetsHitByBeam())
            {
                target.TrackBeamHit();
            }
        }

        void applySize ()
        {
            m_Light.range = m_Size.Length;
            m_Light.spotAngle = m_Size.Angle;

            // radius / length = tan(angle)
            float baseRadius = m_Size.Length * Mathf.Tan(m_Size.Angle * Mathf.Deg2Rad / 2);
            m_GeometryTransformer.localScale = new Vector3(baseRadius, baseRadius, m_Size.Length);
        }

        readonly HashSet<FlashlightBeamTarget> _targetsAlreadyReturnedThisFrame = new HashSet<FlashlightBeamTarget>();
        IEnumerable<FlashlightBeamTarget> getTargetsHitByBeam ()
        {
            _targetsAlreadyReturnedThisFrame.Clear();

            foreach (Vector3 circlePoint in circlePoints)
            {
                Vector3 worldPoint = m_Geometry.TransformPoint(circlePoint);
                Vector3 difference = worldPoint - transform.position;

                if (!Physics.Raycast(transform.position, difference.normalized, out RaycastHit hitInfo, difference.magnitude, m_RaycastLayers))
                {
                    drawRay(transform.position, difference, RaycastHitState.Missed);
                    continue;
                }

                if (!hitInfo.collider.TryGetComponent(out FlashlightBeamTarget target))
                {
                    drawRay(transform.position, difference, RaycastHitState.HitNonTarget);
                    continue;
                }

                drawRay(transform.position, difference, RaycastHitState.HitTarget);

                if (!_targetsAlreadyReturnedThisFrame.Contains(target))
                {
                    _targetsAlreadyReturnedThisFrame.Add(target);
                    yield return target;
                }
            }
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
