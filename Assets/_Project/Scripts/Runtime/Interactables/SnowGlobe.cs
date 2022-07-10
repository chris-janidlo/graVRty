using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GraVRty.CorePhysics;
using GraVRty.Flashlights;

namespace GraVRty.Interactables
{
    public class SnowGlobe : XRBaseInteractable
    {
        [SerializeField] float m_Radius;

        [SerializeField] BeamFocuser m_Focuser;
        [SerializeField] Transform m_InsidesParent, m_Glass;
        [SerializeField] SphereCollider m_SphereCollider;

        [SerializeField] Gravity m_Gravity;

        XRBaseController controller;

        float gasStrength, brakeStrength;

        void Start ()
        {
            m_SphereCollider.radius = m_Radius;
            m_Glass.transform.localScale = m_Radius * 2 * Vector3.one;
            m_Focuser.FocusedBeamRadius = m_Radius;
            // TODO: set m_InsidesParent scale
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            switch (updatePhase)
            {
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                    followController();

                    if (controller != null)
                    {
                        pollController();
                        controlGravity();
                    }

                    visualizeGravity();
                    break;

                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                    followController();
                    break;
            }
        }

        protected override void OnSelectEntered (SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            updateControllerFromArgs(args);
        }

        protected override void OnHoverEntered (HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            if (controller == null) updateControllerFromArgs(args);
        }

        void followController ()
        {
            transform.position = interactorsHovering[0].GetAttachTransform(this).position;
        }

        void updateControllerFromArgs (BaseInteractionEventArgs args)
        {
            controller = args.interactorObject.transform.GetComponent<XRBaseController>();
        }

        void pollController ()
        {
            gasStrength = controller.activateInteractionState.value;
            brakeStrength = controller.selectInteractionState.value;
        }

        void controlGravity ()
        {
            if (gasStrength == 0 && brakeStrength == 0)
            {
                m_Gravity.ReleaseBrakes();
                return;
            }

            if (gasStrength > 0) m_Gravity.SetOrientation(controller.transform.forward);

            float combinedBrakeStrength = Mathf.Min(1 - gasStrength + brakeStrength, 1);
            m_Gravity.Brake(combinedBrakeStrength);
        }

        void visualizeGravity ()
        {
            m_InsidesParent.up = -m_Gravity.Direction;
        }
    }
}
