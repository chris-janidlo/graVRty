using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GraVRty.Items.Flashlights;
using GraVRty.Gravity;

namespace GraVRty.Items.SnowGlobes
{
    public class OneButtonSnowGlobe : MonoBehaviour
    {
        [SerializeField] float m_Radius;
        [Range(0f, 1f)]
        [SerializeField] float m_PressThreshold;
        [SerializeField] SnowGlobeStats m_Stats;

        [SerializeField] BeamFocuser m_Focuser;
        [SerializeField] Rigidbody m_Rigidbody;
        [SerializeField] Transform m_InsidesParent, m_Glass;
        [SerializeField] SphereCollider m_SphereCollider;

        [SerializeField] GravityManager m_GravityManager;

        XRBaseController currentController;

        void Start ()
        {
            m_SphereCollider.radius = m_Radius;
            m_Glass.transform.localScale = m_Radius * 2 * Vector3.one;
            m_Focuser.FocusedBeamRadius = m_Radius;
            // TODO: set m_InsidesParent scale
        }

        void Update ()
        {
            if (currentController != null) controlGravity();

            visualizeGravity();
        }

        public void OnSelectEntered (SelectEnterEventArgs args)
        {
            currentController = args.interactorObject.transform.GetComponent<XRBaseController>();
        }

        public void OnSelectExited (SelectExitEventArgs args)
        {
            m_GravityManager.SetState(m_GravityManager.State.With(amount: 1, drag: 0));

            currentController = null;
        }

        void controlGravity ()
        {
            float strength = currentController.activateInteractionState.value;
            GravityState gravityState = m_GravityManager.State;

            if (strength >= m_PressThreshold)
            {
                gravityState.Direction = currentController.transform.forward;

                float targetDrag = m_Stats.TargetDragByBrake.Evaluate(strength),
                dragChangeSpeed = m_Stats.DragChangeSpeedByBrake.Evaluate(strength);

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
