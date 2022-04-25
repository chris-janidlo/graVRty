using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GraVRty.Combat
{
    public class FlashlightBeamTarget : MonoBehaviour
    {
        [SerializeField] UnityEvent m_HitByBeam;

        public UnityEvent HitByBeam => m_HitByBeam;

        public void TrackBeamHit ()
        {
            HitByBeam.Invoke();
            onBeamHit();
        }

        protected virtual void onBeamHit () { }
    }
}
