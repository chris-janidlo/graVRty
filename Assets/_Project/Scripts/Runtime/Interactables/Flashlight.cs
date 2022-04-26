using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;
using GraVRty.Combat;
using GraVRty.Loading;

namespace GraVRty.Interactables
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] LoadableSemiEagerGameObjectPool m_FlashlightBeamPool;
        [SerializeField] Transform m_BeamParent;

        FlashlightBeam currentLight;

        public void OnActivated (ActivateEventArgs args)
        {
            currentLight = m_FlashlightBeamPool.Get<FlashlightBeam>(m_BeamParent);
        }

        public void OnDeactivated (DeactivateEventArgs args)
        {
            tryKillLight();
        }

        public void OnSelectExited (SelectExitEventArgs args)
        {
            tryKillLight();
        }

        void tryKillLight ()
        {
            if (currentLight == null) return;

            m_FlashlightBeamPool.Release(currentLight);
            currentLight = null;
        }
    }
}
