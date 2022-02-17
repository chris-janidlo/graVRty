using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.CorePhysics
{
    public class RigidbodyGravitizer : MonoBehaviour
    {
        public float BaseDrag => baseDrag;

        [SerializeField] Rigidbody m_Rigidbody;
        [SerializeField] Gravity m_Gravity;

        float baseDrag;

        void OnEnable ()
        {
            m_Rigidbody.useGravity = true;
            baseDrag = m_Rigidbody.drag;
        }

        void OnDisable ()
        {
            m_Rigidbody.useGravity = false;
            m_Rigidbody.drag = baseDrag;
        }

        void FixedUpdate ()
        {
            m_Rigidbody.drag = m_Gravity.GetGravitizerDrag(this);
        }
    }
}
