using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GraVRty.Combat
{
    public class FlashlightBeamTarget : MonoBehaviour
    {
        [Tooltip("Called the first frame a beam collides with this")]
        public UnityEvent<BeamHitInfo> OnBeamEntered;
        [Tooltip("Called every frame a beam is colliding with this")]
        public UnityEvent<BeamHitInfo> OnBeamStay;
        [Tooltip("Called the first frame after a beam stops colliding with this")]
        public UnityEvent<FlashlightBeam> OnBeamExited;

        void Start ()
        {
            OnBeamEntered.AddListener(onBeamEntered);
            OnBeamStay.AddListener(onBeamStay);
            OnBeamExited.AddListener(onBeamExited);
        }

        protected virtual void onBeamEntered (BeamHitInfo beamHitInfo) { }
        protected virtual void onBeamStay (BeamHitInfo beamHitInfo) { }
        protected virtual void onBeamExited (FlashlightBeam flashlightBeam) { }
    }
}
