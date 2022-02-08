using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GraVRty/Gravity Manager", fileName = "newGravityManager.asset")]
public class Gravity : ScriptableObject
{
    [SerializeField] float m_GravityAcceleration, m_FluxDragMax;
    [SerializeField] AnimationCurve m_FluxDragLerpByPressStrength, m_FluxDragLerpUpdateTimeByPressState;

    [Range(0, 1)]
    [SerializeField] float m_FluxPressThreshold, m_FluxHardStopThreshold;

    public GravityState State { get; private set; }
    public Vector3 Direction { get; private set; }
    public Quaternion Rotation { get; private set; }

    float dragLerp, dragLerpVelocity;

    public void Initialize (Transform initialDirectionSource)
    {
        State = GravityState.Active;
        setOrientation(initialDirectionSource);
    }

    public void SetGravity (Transform directionSource, float strength)
    {
        if (strength >= m_FluxPressThreshold)
        {
            updateState(GravityState.Flux);

            float targetDragLerp = m_FluxDragLerpByPressStrength.Evaluate(strength),
                dragLerpUpdateTime = m_FluxDragLerpUpdateTimeByPressState.Evaluate(strength);

            dragLerp = Mathf.SmoothDamp(dragLerp, targetDragLerp, ref dragLerpVelocity, dragLerpUpdateTime);
            setOrientation(directionSource);

            if (strength >= m_FluxHardStopThreshold) Physics.gravity = Vector3.zero;
        }
        else
        {
            updateState(GravityState.Active);
            dragLerp = 0;
        }
    }

    public float GetGravitizerDrag (RigidbodyGravitizer gravitizer)
    {
        return Mathf.Lerp(gravitizer.BaseDrag, m_FluxDragMax, dragLerp);
    }

    void setOrientation (Transform source)
    {
        Direction = -source.up;
        Rotation = source.rotation;

        Physics.gravity = Direction * m_GravityAcceleration;
    }

    void updateState (GravityState newState)
    {
        if (State != newState) State = newState;
    }
}
