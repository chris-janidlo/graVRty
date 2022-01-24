using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GravitizedMoveProvider : LocomotionProvider
{
    [SerializeField] BaseGravitizer m_Gravitizer;
    [SerializeField] CharacterController m_CharacterController;

    void Update ()
    {
        Vector3 gravity = m_Gravitizer != null && m_Gravitizer.enabled
            ? m_Gravitizer.GravityVelocity
            : Vector3.zero;

        if (m_CharacterController.isGrounded) m_Gravitizer.Ground();

        if (CanBeginLocomotion() && BeginLocomotion())
        {
            m_CharacterController.Move(gravity * Time.deltaTime);
            EndLocomotion();
        }
    }
}
