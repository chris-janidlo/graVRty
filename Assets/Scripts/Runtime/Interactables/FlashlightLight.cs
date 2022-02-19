using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GraVRty.Interactables
{
    public class FlashlightLight : MonoBehaviour
    {
        [Serializable]
        public struct SizeSpec
        {
            public float Angle, Length;

            public static SizeSpec Lerp (SizeSpec a, SizeSpec b, float t)
            {
                return new SizeSpec
                {
                    Angle = Mathf.Lerp(a.Angle, b.Angle, t),
                    Length = Mathf.Lerp(a.Length, b.Length, t)
                };
            }
        }

        [SerializeField] SizeSpec m_StartSize, m_EndSize;
        [SerializeField] AnimationCurve m_SizeTransitionCurve;

        [SerializeField] Light m_Light;

        IEnumerator Start ()
        {
            if (m_Light.type != LightType.Spot)
            {
                throw new Exception("Flashlight Light should be a spotlight");
            }

            float currentTime = 0;
            float totalTime = m_SizeTransitionCurve.keys.Last().time;

            while (currentTime < totalTime)
            {
                setLerpedSize(m_SizeTransitionCurve.Evaluate(currentTime));
                currentTime += Time.deltaTime;
                yield return null;
            }

            setLerpedSize(m_SizeTransitionCurve.keys.Last().value);
        }

        public void Kill ()
        {
            Destroy(gameObject);
        }

        void setLerpedSize (float t)
        {
            SizeSpec size = SizeSpec.Lerp(m_StartSize, m_EndSize, t);

            m_Light.range = size.Length;
            m_Light.spotAngle = size.Angle;
        }
    }
}
