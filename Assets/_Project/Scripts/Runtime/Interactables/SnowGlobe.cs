using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using NaughtyAttributes;
using GraVRty.CorePhysics;
using GraVRty.Combat;
using GraVRty.Loading;

namespace GraVRty.Interactables
{
    public class SnowGlobe : MonoBehaviour
    {
        [SerializeField] float m_Radius;

        [Range(0, 1)]
        [SerializeField] float m_BeamHitPercentageToTriggerFocusedBeam;
        [SerializeField] float m_BeamCutoffFudge;
        [MinMaxSlider(0, 100)]
        [SerializeField] Vector2 m_FocusedBeamLengthRange;
        [MinMaxSlider(0, 5)]
        [SerializeField] Vector2 m_FlashlightDistanceClampRange;

        [SerializeField] Rigidbody m_Rigidbody;
        [SerializeField] Transform m_InsidesParent, m_Glass;
        [SerializeField] SphereCollider m_SphereCollider;

        [SerializeField] Gravity m_Gravity;
        [SerializeField] LoadableSemiEagerGameObjectPool FocusedFlashlightBeamPool;

        XRBaseController currentController;
        FlashlightBeam currentFocusedBeam;

        void Start ()
        {
            m_SphereCollider.radius = m_Radius;
            m_Glass.transform.localScale = m_Radius * 2 * Vector3.one;
            // TODO: set m_InsidesParent scale
        }

        void Update ()
        {
            if (currentController != null)
            {
                m_Gravity.SetGravity(m_InsidesParent, currentController.activateInteractionState.value);
            }

            if (m_Gravity.State == GravityState.Active)
            {
                m_InsidesParent.rotation = m_Gravity.Rotation;
            }
        }

        public void OnSelectEntered (SelectEnterEventArgs args)
        {
            currentController = args.interactorObject.transform.GetComponent<XRBaseController>();
        }

        public void OnSelectExited (SelectExitEventArgs args)
        {
            m_Gravity.SetGravity(m_InsidesParent, 0);
            currentController = null;
        }

        public void OnBeamStay (BeamHitInfo beamHitInfo)
        {
            focusBeam(beamHitInfo);
        }

        public void OnBeamExited (FlashlightBeam flashlightBeam)
        {
            flashlightBeam.ResetDimensions();
            tryKillFocusedBeam();
        }

        void focusBeam (BeamHitInfo beamHitInfo)
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
                currentFocusedBeam.SetDimension(length: focusedBeamLength(flashlightPosition), radius: m_Radius);
            }
            else
            {
                unfocusedBeam.ResetDimensions();
                tryKillFocusedBeam();
            }
        }

        void tryKillFocusedBeam ()
        {
            if (currentFocusedBeam != null)
            {
                FocusedFlashlightBeamPool.Release(currentFocusedBeam);
                currentFocusedBeam = null;
            }
        }

        float focusedBeamLength (Vector3 flashlightPosition)
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
