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

        public virtual void TrackBeamHit ()
        {
            HitByBeam.Invoke();
            Debug.Log(gameObject.name + " was hit");
        }
    }
}
