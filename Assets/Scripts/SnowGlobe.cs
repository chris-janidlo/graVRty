using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityAtoms.BaseAtoms;

public class SnowGlobe : MonoBehaviour
{
    [SerializeField] Rigidbody m_Rigidbody;
    [SerializeField] Transform m_InsidesParent;
    [SerializeField] QuaternionVariable m_GravityDirection;

    bool controllingGravity;

    void Awake ()
    {
        m_GravityDirection.Value = Quaternion.identity;
    }

    void FixedUpdate ()
    {
        updateGravityDirection();

        m_InsidesParent.rotation = m_GravityDirection.Value;
    }

    public void OnActivated ()
    {
        controllingGravity = true;
   }

    public void OnDeactivated ()
    {
        controllingGravity = false;
    }

    void updateGravityDirection ()
    {
        if (!controllingGravity) return;

        m_GravityDirection.Value = m_Rigidbody.rotation;
    }
}
