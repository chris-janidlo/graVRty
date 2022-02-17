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
            public float Radius, Length;

            public static SizeSpec Lerp (SizeSpec a, SizeSpec b, float t)
            {
                return new SizeSpec
                {
                    Radius = Mathf.Lerp(a.Radius, b.Radius, t),
                    Length = Mathf.Lerp(a.Length, b.Length, t)
                };
            }
        }

        [SerializeField] SizeSpec m_StartSize, m_EndSize;
        [SerializeField] AnimationCurve m_SizeTransitionCurve;

        IEnumerator Start ()
        {
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
            transform.localScale = new Vector3(size.Radius * 2, size.Radius * 2, size.Length);
        }
    }
}
