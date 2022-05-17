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
        [Range(0, 1)]
        [SerializeField] float m_BeamHitPercentageToTriggerFocusedBeam;
        [SerializeField] float m_BeamCutoffFudge;

        [SerializeField] Rigidbody m_Rigidbody;
        [SerializeField] Transform m_InsidesParent;
        [SerializeField] Gravity m_Gravity;
        [SerializeField] LoadableSemiEagerGameObjectPool FocusedFlashlightBeamPool;

        XRBaseController currentController;
        FlashlightBeam currentFocusedFlashlightBeam;

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
            manageFocusedBeamLifetime(beamHitInfo);

            if (currentFocusedFlashlightBeam != null) orientFocusedBeam(beamHitInfo);
        }

        public void OnBeamExited (FlashlightBeam flashlightBeam)
        {
            flashlightBeam.ResetDimensions();
            tryKillBeam();
        }

        void tryKillBeam ()
        {
            if (currentFocusedFlashlightBeam != null)
            {
                FocusedFlashlightBeamPool.Release(currentFocusedFlashlightBeam);
                currentFocusedFlashlightBeam = null;
            }
        }

        void manageFocusedBeamLifetime (BeamHitInfo beamHitInfo)
        {
            if (beamHitInfo.PercentageHit >= m_BeamHitPercentageToTriggerFocusedBeam)
            {
                FlashlightBeam.Dimensions dimensions = beamHitInfo.SourceBeam.CurrentDimensions;
                dimensions.Length = Vector3.Distance(beamHitInfo.Centroid, beamHitInfo.SourceBeam.transform.position) + m_BeamCutoffFudge;
                beamHitInfo.SourceBeam.SetDimensions(dimensions);

                if (currentFocusedFlashlightBeam == null)
                {
                    currentFocusedFlashlightBeam = FocusedFlashlightBeamPool.Get<FlashlightBeam>(transform);
                }
            }
            else
            {
                beamHitInfo.SourceBeam.ResetDimensions();
                tryKillBeam();
            }
        }

        void orientFocusedBeam (BeamHitInfo beamHitInfo)
        {
            Vector3 reflectionPlaneNormal = (transform.position - beamHitInfo.SourceBeam.transform.position).normalized;
            Vector3 beamDirection = beamHitInfo.SourceBeam.transform.forward;

            currentFocusedFlashlightBeam.transform.forward = -Vector3.Reflect(beamDirection, reflectionPlaneNormal).normalized;
        }
    }
}
