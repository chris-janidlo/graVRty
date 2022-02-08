using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnowGlobe : MonoBehaviour
{
    [SerializeField] Rigidbody m_Rigidbody;
    [SerializeField] Transform m_InsidesParent;
    [SerializeField] Gravity m_Gravity;

    XRBaseController currentController;

    void Update ()
    {
        if (currentController != null)
        {
            m_Gravity.SetGravity(m_InsidesParent, currentController.currentControllerState.activateInteractionState.value);
        }
        
        if (m_Gravity.State == GravityState.Active)
        {
            m_InsidesParent.rotation = m_Gravity.Rotation;
        }
    }

    public void OnSelectEntered (SelectEnterEventArgs args)
    {
        currentController = args.interactorObject.transform.GetComponent<XRBaseController>();
    }

    public void OnSelectExited (SelectExitEventArgs args)
    {
        currentController = null;
    }
}
