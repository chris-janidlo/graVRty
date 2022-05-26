using UnityEngine;
using NaughtyAttributes;
using GraVRty.Loading;

namespace GraVRty.Combat
{
    public class BeamFocuser : FlashlightBeamTarget
    {
        public float FocusedBeamRadius;

        [Range(0, 1)]
        [SerializeField] float m_BeamHitPercentageToTriggerFocusedBeam;
        [SerializeField] float m_BeamCutoffFudge;
        [MinMaxSlider(0, 100)]
        [SerializeField] Vector2 m_FocusedBeamLengthRange;
        [MinMaxSlider(0, 5)]
        [SerializeField] Vector2 m_FlashlightDistanceClampRange;
        
        [SerializeField] LoadableSemiEagerGameObjectPool FocusedFlashlightBeamPool;

        FlashlightBeam currentFocusedBeam;

        protected override void onBeamStay(BeamHitInfo beamHitInfo)
        {
            focusBeam(beamHitInfo);
        }

        protected override void onBeamExited(FlashlightBeam flashlightBeam)
        {
            flashlightBeam.ResetDimensions();
            tryKillFocusedBeam();
        }

        void focusBeam(BeamHitInfo beamHitInfo)
        {
            FlashlightBeam unfocusedBeam = beamHitInfo.SourceBeam;

            if (beamHitInfo.PercentageHit >= m_BeamHitPercentageToTriggerFocusedBeam)
            {
                Vector3 flashlightPosition = unfocusedBeam.transform.position;

                float shortenedBeamLength = Vector3.Distance(beamHitInfo.Centroid, flashlightPosition) + m_BeamCutoffFudge;
                unfocusedBeam.SetDimension(length: shortenedBeamLength);

                if (currentFocusedBeam == null)
                {
                    currentFocusedBeam = FocusedFlashlightBeamPool.Get<FlashlightBeam>(transform);
                }

                Vector3 reflectionPlaneNormal = (transform.position - flashlightPosition).normalized;
                currentFocusedBeam.transform.forward = -Vector3.Reflect(unfocusedBeam.transform.forward, reflectionPlaneNormal).normalized;
                currentFocusedBeam.SetDimension(length: focusedBeamLength(flashlightPosition), radius: FocusedBeamRadius);
            }
            else
            {
                unfocusedBeam.ResetDimensions();
                tryKillFocusedBeam();
            }
        }

        void tryKillFocusedBeam()
        {
            if (currentFocusedBeam != null)
            {
                FocusedFlashlightBeamPool.Release(currentFocusedBeam);
                currentFocusedBeam = null;
            }
        }

        float focusedBeamLength(Vector3 flashlightPosition)
        {
            Vector3 snowGlobePosition = transform.position;

            float
                minDistance = m_FlashlightDistanceClampRange.x,
                maxDistance = m_FlashlightDistanceClampRange.y,
                distanceRange = maxDistance - minDistance,

                minLength = m_FocusedBeamLengthRange.x,
                maxLength = m_FocusedBeamLengthRange.y,
                lengthRange = maxLength - minLength;

            float distance = Mathf.Clamp
            (
                Vector3.Distance(snowGlobePosition, flashlightPosition),
                minDistance,
                maxDistance
            );

            return ((distance - minDistance) * lengthRange / distanceRange) + minLength;
        }
    }
}
