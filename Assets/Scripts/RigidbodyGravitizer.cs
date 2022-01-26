using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyGravitizer : MonoBehaviour
{
    [SerializeField] Rigidbody m_Rigidbody;
    [SerializeField] BaseGravitizer m_Gravitizer;

    void Update ()
    {
        m_Rigidbody.velocity = m_Gravitizer.GravityVelocity;
    }

    private void OnCollisionEnter (Collision collision)
    {
        m_Gravitizer.Ground();
    }
}
