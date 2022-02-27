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

            // radius / length = tan(angle)
            public float BaseRadius => Length * Mathf.Tan(Angle * Mathf.Deg2Rad / 2);

            public static SizeSpec Lerp (SizeSpec a, SizeSpec b, float t)
            {
                return new SizeSpec
                {
                    Angle = Mathf.Lerp(a.Angle, b.Angle, t),
                    Length = Mathf.Lerp(a.Length, b.Length, t)
                };
            }
        }

        [SerializeField] SizeSpec m_StartSize, m_EndSize;
        [SerializeField] AnimationCurve m_SizeTransitionCurve;

        [SerializeField] int m_RaycastsPerUnitAreaPerFrame;
        [SerializeField] LayerMask m_RaycastLayers;
        [SerializeField] bool m_DrawDebugRays;
        [SerializeField] EnumMap<RaycastHitState, Color> m_DebugRayColors;

        [SerializeField] Light m_Light;
        [SerializeField] Transform m_GeometryTransformer, m_Geometry;

        SizeSpec currentSize;

        IEnumerator Start ()
        {
            if (m_Light.type != LightType.Spot)
            {
                throw new Exception("Flashlight Light should be a spotlight");
            }

            float currentTime = 0;
            float totalTime = m_SizeTransitionCurve.keys.Last().time;

            while (currentTime < totalTime)
            {
                setLerpedSize(m_SizeTransitionCurve.Evaluate(currentTime));
                currentTime += Time.deltaTime;
                yield return null;
            }

            setLerpedSize(m_SizeTransitionCurve.keys.Last().value);
        }

        void FixedUpdate ()
        {
            foreach (FlashlightBeamTarget target in getTargetsHitByBeam())
            {
                target.TrackBeamHit();
            }
        }

        public void Kill ()
        {
            Destroy(gameObject);
        }

        void setLerpedSize (float t)
        {
            currentSize = SizeSpec.Lerp(m_StartSize, m_EndSize, t);

            m_Light.range = currentSize.Length;
            m_Light.spotAngle = currentSize.Angle;

            m_GeometryTransformer.localScale = new Vector3(currentSize.BaseRadius, currentSize.BaseRadius, currentSize.Length);
        }

        readonly HashSet<FlashlightBeamTarget> _targetsAlreadyReturnedThisFrame = new HashSet<FlashlightBeamTarget>();
        IEnumerable<FlashlightBeamTarget> getTargetsHitByBeam ()
        {
            _targetsAlreadyReturnedThisFrame.Clear();

            foreach (Vector3 point in getRaycastTargetPoints())
            {
                Vector3 difference = point - transform.position;

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

        IEnumerable<Vector3> getRaycastTargetPoints ()
        {
            // uses algorithm from https://stackoverflow.com/a/28572551/5931898, which is a modified sunflower seed arrangement with additional bias toward the edge of the circle
            const float phiSquared = 2.61803398875f; // golden ratio, squared
            const float alpha = 2; // amount of edge bias. author recommends 2

            int numRaycasts = Mathf.RoundToInt(Mathf.PI * currentSize.BaseRadius * currentSize.BaseRadius * m_RaycastsPerUnitAreaPerFrame);
            int boundaryPointCount = Mathf.RoundToInt(alpha * Mathf.Sqrt(numRaycasts));

            for (int i = 0; i < numRaycasts; i++)
            {
                float polarRadius = i > numRaycasts - boundaryPointCount
                    ? 1
                    : Mathf.Sqrt(i - .5f) / Mathf.Sqrt(numRaycasts - (boundaryPointCount + 1) / 2f); // normal sunflower seed arrangement would just use Mathf.Sqrt(i - .5f), but we apply an additional scaling factor to account for the boundary
                float polarAngle = 2 * Mathf.PI * i / phiSquared;

                Vector2 cartesianPoint = new Vector2(polarRadius * Mathf.Cos(polarAngle), polarRadius * Mathf.Sin(polarAngle));
                yield return m_Geometry.TransformPoint(cartesianPoint);
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
