using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;
using GraVRty.Combat;

namespace GraVRty.Interactables
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] FlashlightBeam m_BeamPrefab;
        [SerializeField] FlashlightBeamCache m_FlashlightBeamCache;
        [SerializeField] Transform m_BeamParent;

        FlashlightBeam currentLight;

        public void OnActivated (ActivateEventArgs args)
        {
            currentLight = m_FlashlightBeamCache.InstantiateFromCache(m_BeamPrefab, m_BeamParent);
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

            m_FlashlightBeamCache.ReleaseToCache(m_BeamPrefab);
            currentLight = null;
        }
    }
}
