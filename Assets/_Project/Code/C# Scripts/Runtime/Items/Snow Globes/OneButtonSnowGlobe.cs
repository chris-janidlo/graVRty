using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GraVRty.Gravity;
using GraVRty.GameFlow;
using GraVRty.Items.Flashlights;

namespace GraVRty.Items.SnowGlobes
{
    // FIXME: make this work when paused
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

        [SerializeField] Pauser m_Pauser;
        [SerializeField] GravityManager m_GravityManager;

        XRBaseController currentController;
        bool paused;

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
            m_Pauser.Unpause();

            currentController = args.interactorObject.transform.GetComponent<XRBaseController>();
        }

        // FIXME: if you fall fast enough and then drop the snow globe, you can't seem to pick it up anymore
        public void OnSelectExited (SelectExitEventArgs args)
        {
            m_Pauser.Pause();

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
