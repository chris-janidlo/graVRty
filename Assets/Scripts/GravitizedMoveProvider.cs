using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GravitizedMoveProvider : LocomotionProvider
{
    [SerializeField] Gravity m_Gravity;
    [SerializeField] CharacterController m_CharacterController;

    Gravity.Tracker tracker;

    void OnEnable ()
    {
        if (m_Gravity == null) m_Gravity = FindObjectOfType<Gravity>();

        tracker = m_Gravity.GetNewTracker(false);
    }

    void OnDisable ()
    {
        m_Gravity.UnregisterTracker(tracker);
    }

    void Update ()
    {
        if (m_CharacterController.isGrounded) tracker.TrackGroundTouch();

        if (CanBeginLocomotion() && BeginLocomotion())
        {
            m_CharacterController.Move(tracker.Velocity * Time.deltaTime);
            EndLocomotion();
        }
    }
}
