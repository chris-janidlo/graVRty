using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GraVRty.CorePhysics;
using GraVRty.Flashlights;

namespace GraVRty.Interactables
{
    public class SnowGlobe : MonoBehaviour
    {
        [SerializeField] float m_Radius;

        [SerializeField] BeamFocuser m_Focuser;
        [SerializeField] Rigidbody m_Rigidbody;
        [SerializeField] Transform m_InsidesParent, m_Glass;
        [SerializeField] SphereCollider m_SphereCollider;

        [SerializeField] Gravity m_Gravity;

        XRBaseController currentController;

        void Start ()
        {
            m_SphereCollider.radius = m_Radius;
            m_Glass.transform.localScale = m_Radius * 2 * Vector3.one;
            m_Focuser.FocusedBeamRadius = m_Radius;
            // TODO: set m_InsidesParent scale
        }

        void Update ()
        {
            if (currentController != null)
            {
                m_Gravity.SetOrientation(currentController.transform.forward);
                m_Gravity.Brake(currentController.activateInteractionState.value);
            }
            
            m_InsidesParent.up = -m_Gravity.Direction;
        }

        public void OnSelectEntered (SelectEnterEventArgs args)
        {
            currentController = args.interactorObject.transform.GetComponent<XRBaseController>();
        }

        public void OnSelectExited (SelectExitEventArgs args)
        {
            m_Gravity.SetOrientation(currentController.transform.forward);
            m_Gravity.ReleaseBrakes();

            currentController = null;
        }
    }
}
