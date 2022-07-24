using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GraVRty.CorePhysics;
using GraVRty.Flashlights;

namespace GraVRty.Interactables
{
    public class SnowGlobe : XRBaseInteractable
    {
        [SerializeField] float m_Radius;

        [Range(0f, 1f)]
        [SerializeField] float m_MinActivatePressThreshold, m_MinSelectPressThreshold;

        [SerializeField] BeamFocuser m_Focuser;
        [SerializeField] Transform m_InsidesParent, m_Glass;
        [SerializeField] SphereCollider m_SphereCollider;

        [SerializeField] Gravity m_Gravity;

        XRBaseControllerInteractor interactor;
        bool interactorPreviouslyAllowedHover;

        struct ControllerState
        {
            public float GasStrength;
            public float BrakeStrength;
        }

        void Start ()
        {
            m_SphereCollider.radius = m_Radius;
            m_Glass.transform.localScale = m_Radius * 2 * Vector3.one;
            m_Focuser.FocusedBeamRadius = m_Radius;
            // TODO: set m_InsidesParent scale
        }

        public override void ProcessInteractable (XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            switch (updatePhase)
            {
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                    tryFollowController();

                    controlGravity(pollController());
                    visualizeGravity();
                    break;

                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                    tryFollowController();
                    break;
            }
        }

        protected override void OnSelectEntered (SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            updateInteractor(args);
        }

        protected override void OnHoverEntered (HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            if (interactor == null) updateInteractor(args);
        }

        void tryFollowController ()
        {
            if (interactor == null) return;

            transform.position = interactor.GetAttachTransform(this).position;
        }

        void updateInteractor (BaseInteractionEventArgs args)
        {
            XRBaseControllerInteractor newInteractor = args.interactorObject as XRBaseControllerInteractor;
            if (newInteractor == interactor) return;

            if (interactor != null)
            {
                interactor.allowHover = interactorPreviouslyAllowedHover;
            }

            interactor = newInteractor;

            interactorPreviouslyAllowedHover = interactor.allowHover;
            interactor.allowHover = false;
        }

        ControllerState pollController ()
        {
            return interactor == null
                ? new ControllerState
                {
                    GasStrength = 0,
                    BrakeStrength = 0,
                }
                : new ControllerState
                {
                    GasStrength = interactor.xrController.activateInteractionState.value,
                    BrakeStrength = interactor.xrController.selectInteractionState.value
                };
        }

        void controlGravity (ControllerState state)
        {
            Debug.Log($"{state.GasStrength}, {state.BrakeStrength}");

            bool
                gasPressed = state.GasStrength >= m_MinActivatePressThreshold,
                brakePressed = state.BrakeStrength >= m_MinSelectPressThreshold;

            if (!gasPressed && !brakePressed)
            {
                m_Gravity.ReleaseBrakes();
                return;
            }

            if (gasPressed) m_Gravity.SetOrientation(interactor.transform.forward);

            float combinedBrakeStrength = Mathf.Min(1 - state.GasStrength + state.BrakeStrength, 1);
            m_Gravity.Brake(combinedBrakeStrength);
        }

        void visualizeGravity ()
        {
            m_InsidesParent.up = -m_Gravity.Direction;
        }
    }
}
