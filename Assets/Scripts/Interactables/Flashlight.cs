using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;

public class Flashlight : MonoBehaviour
{
    [SerializeField] float m_LightStartSize, m_LightEndSize, m_LightStartTime, m_LightTransitionTime;
    [SerializeField] Ease m_LightTransitionEase;

    [SerializeField] FlashlightLight m_LightPrefab;
    [SerializeField] Transform m_LightParent;

    FlashlightLight currentLight;
    Tweener lightTweener;

    public void OnActivated (ActivateEventArgs args)
    {
        currentLight = Instantiate(m_LightPrefab, m_LightParent);
        currentLight.SetSize(m_LightStartSize);

        lightTweener = DOTween.To(() => currentLight.Size, currentLight.SetSize, m_LightEndSize, m_LightTransitionTime)
            .SetEase(m_LightTransitionEase)
            .SetDelay(m_LightStartTime)
            .OnKill(() => lightTweener = null);
    }

    public void OnDeactivated (DeactivateEventArgs args)
    {
        currentLight.Kill();
        currentLight = null;

        lightTweener?.Kill();
        lightTweener = null;
    }
}
