using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GraVRty.Combat
{
    public class FlashlightBeamTarget : MonoBehaviour
    {
        [SerializeField] UnityEvent<BeamHitInfo> m_HitByBeam;

        public UnityEvent<BeamHitInfo> HitByBeam => m_HitByBeam;

        public void TrackBeamHit (BeamHitInfo beamHitInfo)
        {
            HitByBeam.Invoke(beamHitInfo);
            onBeamHit(beamHitInfo);
        }

        protected virtual void onBeamHit (BeamHitInfo beamHitInfo) { }
    }
}
