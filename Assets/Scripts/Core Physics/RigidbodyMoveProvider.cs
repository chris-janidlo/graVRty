using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GraVRty.CorePhysics
{
    public class RigidbodyMoveProvider : LocomotionProvider
    {
        [SerializeField] Rigidbody m_TrackedRigidbody;
        [SerializeField] CapsuleCollider m_DrivenCapsuleCollider;

        [SerializeField] float m_MinHeight, m_MaxHeight;

        void Start ()
        {
            beginLocomotion += updateCapsuleCollider;
            endLocomotion += updateCapsuleCollider;
        }

        void FixedUpdate ()
        {
            if (CanBeginLocomotion() && BeginLocomotion())
            {
                transform.position = m_TrackedRigidbody.position;
                EndLocomotion();
            }
        }

        void updateCapsuleCollider (LocomotionSystem _)
        {
            var originHeight = system.xrOrigin.CameraInOriginSpaceHeight;
            var originPos = system.xrOrigin.CameraInOriginSpacePos;

            var height = Mathf.Clamp(originHeight, m_MinHeight, m_MaxHeight);
            var center = new Vector3(originPos.x, height / 2f, originPos.z);

            m_DrivenCapsuleCollider.height = height;
            m_DrivenCapsuleCollider.center = center;
        }
    }
}
