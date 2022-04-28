using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GraVRty.Combat
{
    // needs to be after FlashlightBeam in script excecution order so that multi frame hits are tracked properly
    // FIXME: assumes there will only ever be one beam touching a target at a time
    public class FlashlightBeamTarget : MonoBehaviour
    {
        [Tooltip("Called the first frame a beam collides with this")]
        public UnityEvent<BeamHitInfo> OnBeamEntered;
        [Tooltip("Called every frame a beam is colliding with this")]
        public UnityEvent<BeamHitInfo> OnBeamStay;
        [Tooltip("Called the first frame after a beam stops colliding with this")]
        public UnityEvent<FlashlightBeam> OnBeamExited;

        FlashlightBeam currentBeam;
        int frameHitCounter;

        void FixedUpdate ()
        {
            frameHitCounter--;

            if (frameHitCounter <= 0 && currentBeam != null)
            {
                OnBeamExited.Invoke(currentBeam);
                onBeamExited(currentBeam);
                currentBeam = null;
            }
        }

        public void TrackBeamHit (BeamHitInfo beamHitInfo)
        {
            if (frameHitCounter <= 0)
            {
                OnBeamEntered.Invoke(beamHitInfo);
                onBeamEntered(beamHitInfo);
            }

            OnBeamStay.Invoke(beamHitInfo);
            onBeamStay(beamHitInfo);

            frameHitCounter = 2; // 1 for the update after this call, and 1 for the actual frame counting aspect
            currentBeam = beamHitInfo.SourceBeam;
        }

        protected virtual void onBeamEntered (BeamHitInfo beamHitInfo) { }
        protected virtual void onBeamStay (BeamHitInfo beamHitInfo) { }
        protected virtual void onBeamExited (FlashlightBeam flashlightBeam) { }
    }
}
