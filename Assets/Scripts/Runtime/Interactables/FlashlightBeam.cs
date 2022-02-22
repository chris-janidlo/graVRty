using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        [SerializeField] int m_RaycastsPerFrame;
        [SerializeField] LayerMask m_RaycastLayers;
        [SerializeField] bool m_DrawDebugRays;
        [SerializeField] EnumMap<RaycastHitState, Color> m_DebugRayColors;

        [SerializeField] Light m_Light;
        [SerializeField] Transform m_GeometryTransformer, m_Geometry;

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
            SizeSpec size = SizeSpec.Lerp(m_StartSize, m_EndSize, t);

            m_Light.range = size.Length;
            m_Light.spotAngle = size.Angle;

            // radius / length = tan(angle)
            float triggerRadius = size.Length * Mathf.Tan(size.Angle * Mathf.Deg2Rad / 2);
            m_GeometryTransformer.localScale = new Vector3(triggerRadius, triggerRadius, size.Length);
        }

        IEnumerable<FlashlightBeamTarget> getTargetsHitByBeam ()
        {
            HashSet<FlashlightBeamTarget> targetsAlreadyReturnedThisFrame = new HashSet<FlashlightBeamTarget>();

            foreach (Vector3 point in getRaycastTargetPoints())
            {
                Vector3 difference = point - transform.position;

                bool hit = Physics.Raycast(transform.position, difference.normalized, out RaycastHit hitInfo, difference.magnitude, m_RaycastLayers);
                if (!hit)
                {
                    drawRay(transform.position, difference, RaycastHitState.Missed);
                    continue;
                }

                FlashlightBeamTarget target = hitInfo.collider.GetComponent<FlashlightBeamTarget>();
                if (target == null)
                {
                    drawRay(transform.position, difference, RaycastHitState.HitNonTarget);
                    continue;
                }

                drawRay(transform.position, difference, RaycastHitState.HitTarget);

                if (!targetsAlreadyReturnedThisFrame.Contains(target))
                {
                    targetsAlreadyReturnedThisFrame.Add(target);
                    yield return target;
                }
            }
        }

        IEnumerable<Vector3> getRaycastTargetPoints ()
        {
            for (int i = 0; i < m_RaycastsPerFrame; i++)
            {
                yield return m_Geometry.TransformPoint(UnityEngine.Random.insideUnitCircle);
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
