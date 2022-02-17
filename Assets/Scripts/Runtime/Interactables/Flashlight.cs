using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;

namespace GraVRty.Interactables
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] FlashlightLight m_LightPrefab;
        [SerializeField] Transform m_LightParent;

        FlashlightLight currentLight;

        public void OnActivated (ActivateEventArgs args)
        {
            currentLight = Instantiate(m_LightPrefab, m_LightParent);
        }

        public void OnDeactivated (DeactivateEventArgs args)
        {
            currentLight.Kill();
            currentLight = null;
        }
    }
}
