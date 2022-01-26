using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    public class Tracker
    {
        readonly Func<Vector3> _velocityGetter;
        public Vector3 Velocity => _velocityGetter();

        readonly Func<Vector3> _accelerationGetter;
        public Vector3 AcceelrationThisFrame => _accelerationGetter();

        readonly Action _groundCallback;

        public Tracker (Func<Vector3> velocityGetter, Func<Vector3> accelerationGetter, Action groundCallback)
        {
            _velocityGetter = velocityGetter;
            _accelerationGetter = accelerationGetter;
            _groundCallback = groundCallback;
        }

        public void TrackGroundTouch ()
        {
            _groundCallback();
        }
    }

    private class InternalTracker
    {
        public Vector3 CurrentVelocity => gravityDirection * gravitySpeed;

        public Vector3 AccelerationThisFrame { get; private set; }

        readonly Gravity gravity;

        Vector3 gravityDirection;
        float gravitySpeed;
        Tweener gravityDirectionTweener, gravitySpeedTweener;

        public InternalTracker (Gravity gravity)
        {
            this.gravity = gravity;

            gravityDirection = gravity.Direction;
            gravitySpeed = 0;
        }

        public void Update ()
        {
            Vector3 previousVelocity = CurrentVelocity;

            switch (gravity.State)
            {
                case GravityState.Active:
                    gravitySpeed += gravity.m_AccelerationAmount * Time.deltaTime;
                    break;

                case GravityState.Flux:
                    // this is handled automatically by the tweener, don't need to do anything
                    break;

                default:
                    throw new InvalidOperationException($"unexpected GravityState {gravity.State}");
            }

            AccelerationThisFrame = CurrentVelocity - previousVelocity;
        }

        public void OnGravityStateChanged (GravityState newState)
        {
            switch (newState)
            {
                case GravityState.Active:
                    gravitySpeedTweener?.Kill(); // leave the gravity speed at whatever deceleration value we are currently at

                    float turnTime = gravity.m_MaxTurnTime *
                        gravity.m_TurnTimeByGravitySpeed.Evaluate(gravitySpeed) *
                        gravity.m_TurnTimeByGravityDirectionDistance.Evaluate(Vector3.Distance(gravityDirection, gravity.Direction));

                    gravityDirectionTweener?.Kill();
                    gravityDirectionTweener = DOTween
                        .To(() => gravityDirection, v => gravityDirection = v, gravity.Direction, turnTime)
                        .SetEase(gravity.m_TurnEase)
                        .OnKill(() => gravityDirectionTweener = null);
                    break;

                case GravityState.Flux:
                    float decelerationTime = Mathf.Max(gravity.m_MinStopTime, gravitySpeed / gravity.m_DecelerationAmount);
                    gravitySpeedTweener?.Kill();
                    gravitySpeedTweener = DOTween
                        .To(() => gravitySpeed, v => gravitySpeed = v, 0, decelerationTime)
                        .SetEase(gravity.m_StopEase)
                        .OnKill(() => gravitySpeedTweener = null);
                    break;

                default:
                    throw new InvalidOperationException($"unexpected GravityState {newState}");
            }
        }

        public void Ground ()
        {
            gravitySpeed = 0;
        }
    }

    [SerializeField] float m_AccelerationAmount, m_DecelerationAmount;
    [SerializeField] AnimationCurve m_TurnTimeByGravitySpeed, m_TurnTimeByGravityDirectionDistance;
    [SerializeField] float m_MaxTurnTime, m_MinStopTime;
    [SerializeField] Ease m_TurnEase, m_StopEase;

    public GravityState State { get; private set; }
    public Vector3 Direction { get; private set; }
    public Quaternion Rotation { get; private set; }

    readonly List<(InternalTracker, Tracker)> trackers = new List<(InternalTracker, Tracker)>();

    void OnEnable ()
    {
        State = GravityState.Active;
        setOrientation(transform);
    }

    void Update ()
    {
        trackers.ForEach(p => p.Item1.Update());
    }

    public Tracker GetNewTracker ()
    {
        InternalTracker internalTracker = new InternalTracker(this);
        Tracker externalTracker = new Tracker(() => internalTracker.CurrentVelocity, () => internalTracker.AccelerationThisFrame, internalTracker.Ground);

        trackers.Add((internalTracker, externalTracker));

        return externalTracker;
    }

    public void UnregisterTracker (Tracker tracker)
    {
        trackers.RemoveAll(p => p.Item2 == tracker);
    }

    public void StartFlux ()
    {
        if (State == GravityState.Flux) return;

        updateState(GravityState.Flux);
    }

    public void StartActive (Transform newDirectionSource)
    {
        if (State == GravityState.Active) return;

        setOrientation(newDirectionSource);
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
        trackers.ForEach(p => p.Item1.OnGravityStateChanged(newState));
    }
}
