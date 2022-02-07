using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GraVRty/Gravity Manager", fileName = "newGravityManager.asset")]
public class Gravity : ScriptableObject
{
    [SerializeField] float m_GravityAcceleration;
    [SerializeField] float m_FluxDragMax, m_FluxDragTimeToMax;
    [SerializeField] Ease m_FluxDragEase;

    public GravityState State { get; private set; }
    public Vector3 Direction { get; private set; }
    public Quaternion Rotation { get; private set; }
    public float? Drag { get; private set; }

    Tweener dragTweener;

    public void Initialize (Transform initialDirectionSource)
    {
        State = GravityState.Flux;
        StartActive(initialDirectionSource);
    }

    public void StartFlux ()
    {
        if (State == GravityState.Flux) return;

        Physics.gravity = Vector3.zero;

        dragTweener = DOTween.To(v => Drag = v, 0, m_FluxDragMax, m_FluxDragTimeToMax)
            .SetEase(m_FluxDragEase)
            .OnKill(() => dragTweener = null);

        updateState(GravityState.Flux);
    }

    public void StartActive (Transform newDirectionSource)
    {
        if (State == GravityState.Active) return;

        setOrientation(newDirectionSource);
        Physics.gravity = Direction * m_GravityAcceleration;

        dragTweener?.Kill();
        Drag = null;

        updateState(GravityState.Active);
    }

    void setOrientation (Transform source)
    {
        Direction = -source.up;
        Rotation = source.rotation;
    }

    void updateState (GravityState newState)
    {
        State = newState;
    }
}
