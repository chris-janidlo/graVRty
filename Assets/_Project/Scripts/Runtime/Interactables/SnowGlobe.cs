using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
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
                float shortenedBeamLength = Vector3.Distance(beamHitInfo.Centroid, unfocusedBeam.transform.position) + m_BeamCutoffFudge;
                unfocusedBeam.SetLength(shortenedBeamLength);

                if (currentFocusedBeam == null)
                {
                    currentFocusedBeam = FocusedFlashlightBeamPool.Get<FlashlightBeam>(transform);
                }

                Vector3 reflectionPlaneNormal = (transform.position - unfocusedBeam.transform.position).normalized;
                currentFocusedBeam.transform.forward = -Vector3.Reflect(unfocusedBeam.transform.forward, reflectionPlaneNormal).normalized;
                currentFocusedBeam.SetRadius(m_Radius);
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
    }
}
