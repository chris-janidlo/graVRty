using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BaseGravitizer : MonoBehaviour
{
    [SerializeField] private Gravity m_Gravity;
    [SerializeField] private AnimationCurve m_TurnTimeByGravitySpeed, m_TurnTimeByGravityDirectionDistance;
    [SerializeField] private float m_MaxTurnTime;
    [SerializeField] private Ease m_TurnEase, m_StopEase;

    public Vector3 GravityVelocity => gravityDirection * gravitySpeed;

    Vector3 gravityDirection;
    float gravitySpeed;
    Tweener gravityDirectionTweener, gravitySpeedTweener;

    protected virtual void Update ()
    {
        switch (m_Gravity.State)
        {
            case GravityState.Active:
                gravitySpeed += m_Gravity.AccelerationAmount * Time.deltaTime;
                break;

            case GravityState.Flux:
                float decelerationTime = gravitySpeed / m_Gravity.DecelerationAmount;
                gravitySpeedTweener?.Kill();
                gravitySpeedTweener = DOTween
                    .To(() => gravitySpeed, v => gravitySpeed = v, 0, decelerationTime)
                    .SetEase(m_StopEase);
                break;
        }
    }

    protected virtual void OnEnable ()
    {
        m_Gravity.OnGravityChanged += onGravityChanged;

        gravityDirection = m_Gravity.Direction;
        gravitySpeed = 0;
    }

    protected virtual void OnDisable ()
    {
        m_Gravity.OnGravityChanged -= onGravityChanged;
    }

    /// <summary>
    /// Call this to let the BaseGravitizer know that this gravitizer has touched the ground.
    /// </summary>
    public void Ground ()
    {
        gravitySpeed = 0;
    }

    void onGravityChanged (GravityState newState)
    {
        if (newState != GravityState.Active) return;

        gravitySpeedTweener?.Kill(); // leave the gravity speed at whatever deceleration value we are currently at

        float turnTime = m_MaxTurnTime *
            m_TurnTimeByGravitySpeed.Evaluate(gravitySpeed) *
            m_TurnTimeByGravityDirectionDistance.Evaluate(Vector3.Distance(gravityDirection, m_Gravity.Direction));

        gravityDirectionTweener?.Kill();
        gravityDirectionTweener = DOTween
            .To(() => gravityDirection, v => gravityDirection = v, m_Gravity.Direction, turnTime)
            .SetEase(m_TurnEase);
    }
}
