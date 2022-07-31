using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GraVRty.Gravity;
using GraVRty.Items.Flashlights;

namespace GraVRty.Items.SnowGlobes
{
    public class SnowGlobe : XRBaseInteractable
    {
        [SerializeField] float m_Radius;

        [Range(0f, 1f)]
        [SerializeField] float m_GasPressThreshold, m_BrakesPressThreshold;

        [SerializeField] SnowGlobeStats m_Stats;

        // TODO: expose this as a configuration option stored in PlayerPrefs
        [SerializeField] ControlMode m_ControlMode;

        [SerializeField] BeamFocuser m_Focuser;
        [SerializeField] Transform m_InsidesParent, m_Glass;
        [SerializeField] SphereCollider m_SphereCollider;

        [SerializeField] GravityManager m_GravityManager;

        XRBaseControllerInteractor interactor;
        bool interactorPreviouslyAllowedHover;

        public enum ControlMode
        {
            ActivateIsGasSelectIsBrakes,
            ActivateIsBrakesSelectIsGas
        }

        struct ControllerState
        {
            public float Gas;
            public float Brakes;
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

                    if (interactor != null) controlGravity(pollController());
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
            float
                activate = interactor.xrController.activateInteractionState.value,
                select = interactor.xrController.selectInteractionState.value;

            float gas, brakes;
            switch (m_ControlMode)
            {
                case ControlMode.ActivateIsGasSelectIsBrakes:
                    gas = activate;
                    brakes = select;
                    break;

                case ControlMode.ActivateIsBrakesSelectIsGas:
                    gas = select;
                    brakes = activate;
                    break;

                default:
                    throw new InvalidOperationException(m_ControlMode.ToString());
            }

            return new ControllerState { Gas = gas, Brakes = brakes };
        }

        void controlGravity (ControllerState input)
        {
            GravityState gravityState = m_GravityManager.State;

            if (input.Gas >= m_GasPressThreshold)
            {
                gravityState.Direction = interactor.transform.forward;
            }

            if (input.Brakes >= m_BrakesPressThreshold)
            {
                float targetDrag = m_Stats.TargetDragByBrake.Evaluate(input.Brakes),
                dragChangeSpeed = m_Stats.DragChangeSpeedByBrake.Evaluate(input.Brakes);

                if (gravityState.Drag < targetDrag)
                    gravityState.Drag = Mathf.Min(targetDrag, gravityState.Drag + dragChangeSpeed * Time.deltaTime);
                if (gravityState.Drag > targetDrag)
                    gravityState.Drag = Mathf.Max(targetDrag, gravityState.Drag - dragChangeSpeed * Time.deltaTime);
            }
            else
            {
                gravityState.Drag = 0;
            }

            if (gravityState.Drag >= m_Stats.MinBrakeToHardStopGravity)
            {
                float delta = m_Stats.GravityHardStopSpeed * Time.deltaTime;
                gravityState.Amount = Mathf.Max(0, gravityState.Amount - delta);
            }
            else
            {
                gravityState.Amount = 1;
            }

            m_GravityManager.SetState(gravityState);
        }

        void visualizeGravity ()
        {
            m_InsidesParent.up = -m_GravityManager.State.Direction;
        }
    }
}
