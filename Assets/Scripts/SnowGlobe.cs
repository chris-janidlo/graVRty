using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowGlobe : MonoBehaviour
{
    [SerializeField] Rigidbody m_Rigidbody;
    [SerializeField] Transform m_InsidesParent;
    [SerializeField] Gravity m_Gravity;

    bool controllingGravity;

    void FixedUpdate ()
    {
        if (!controllingGravity) m_InsidesParent.rotation = m_Gravity.Rotation;
        else m_Gravity.SetOrientation(m_InsidesParent);
    }

    public void OnActivated ()
    {
        controllingGravity = true;
   }

    public void OnDeactivated ()
    {
        controllingGravity = false;
    }
}
