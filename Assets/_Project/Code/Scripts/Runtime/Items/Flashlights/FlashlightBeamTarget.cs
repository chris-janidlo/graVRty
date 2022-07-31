using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace GraVRty.Items.Flashlights
{
    public class FlashlightBeamTarget : MonoBehaviour
    {
        const string LOCKABLE_FOLDOUT = "Lock-On Settings";

        [Tooltip("Called the first frame a beam collides with this")]
        public UnityEvent<BeamHitInfo> OnBeamEntered;
        [Tooltip("Called every frame a beam is colliding with this")]
        public UnityEvent<BeamHitInfo> OnBeamStay;
        [Tooltip("Called the first frame after a beam stops colliding with this")]
        public UnityEvent<FlashlightBeam> OnBeamExited;

        [SerializeField] bool m_Lockable;

        [ShowIf(nameof(Lockable)), Foldout(LOCKABLE_FOLDOUT)]
        [Tooltip("Called the first frame a beam locks on to this")]
        public UnityEvent<BeamHitInfo> OnLockEntered;
        [ShowIf(nameof(Lockable)), Foldout(LOCKABLE_FOLDOUT)]
        [Tooltip("Called every frame a beam is locked on to this")]
        public UnityEvent<BeamHitInfo> OnLockStay;
        [ShowIf(nameof(Lockable)), Foldout(LOCKABLE_FOLDOUT)]
        [Tooltip("Called the first frame after a beam stops locking on to this")]
        public UnityEvent<FlashlightBeam> OnLockExited; // TODO: should this also include a BeamHitInfo if one is available?

        [ShowIf(nameof(Lockable)), Foldout(LOCKABLE_FOLDOUT)]
        [Range(0, 1)]
        [SerializeField] float m_BeamHitPercentageToEnterLock, m_BeamHitPercentageToExitLock;
        
        public bool Lockable => m_Lockable;
        public float BeamHitPercentageToEnterLock => m_BeamHitPercentageToEnterLock;
        public float BeamHitPercentageToExitLock => m_BeamHitPercentageToExitLock;

        protected virtual void Start ()
        {
            OnBeamEntered.AddListener(onBeamEntered);
            OnBeamStay.AddListener(onBeamStay);
            OnBeamExited.AddListener(onBeamExited);

            OnLockEntered.AddListener(onLockEntered);
            OnLockStay.AddListener(onLockStay);
            OnLockExited.AddListener(onLockExited);
        }

        protected virtual void onBeamEntered (BeamHitInfo beamHitInfo) { }
        protected virtual void onBeamStay (BeamHitInfo beamHitInfo) { }
        protected virtual void onBeamExited (FlashlightBeam flashlightBeam) { }
        protected virtual void onLockEntered(BeamHitInfo beamHitInfo) { }
        protected virtual void onLockStay(BeamHitInfo beamHitInfo) { }
        protected virtual void onLockExited(FlashlightBeam flashlightBeam) { }
    }
}
