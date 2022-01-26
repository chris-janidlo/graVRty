using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowGlobe : MonoBehaviour
{
    [SerializeField] Rigidbody m_Rigidbody;
    [SerializeField] Transform m_InsidesParent;
    [SerializeField] Gravity m_Gravity;

    bool controllingGravity;

    void Start ()
    {
        if (m_Gravity == null) m_Gravity = FindObjectOfType<Gravity>();
    }

    void FixedUpdate ()
    {
        if (!controllingGravity) m_InsidesParent.rotation = m_Gravity.Rotation;
    }

    public void OnActivated ()
    {
        controllingGravity = true;
        m_Gravity.StartFlux();
    }

    public void OnDeactivated ()
    {
        controllingGravity = false;
        m_Gravity.StartActive(m_InsidesParent);
    }
}
