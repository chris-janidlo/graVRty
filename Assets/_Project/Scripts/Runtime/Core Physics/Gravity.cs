using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using GraVRty.Loading;

namespace GraVRty.CorePhysics
{
    [CreateAssetMenu(menuName = "GraVRty/Gravity Manager", fileName = "newGravityManager.asset")]
    public class Gravity : Loadable
    {
        [SerializeField] float m_GravityAcceleration, m_BrakeDragMax;
        [SerializeField] AnimationCurve m_DragLerpByBrakeStrength, m_DragLerpSpeedByBrakeStrength;

        [Range(0, 1)]
        [SerializeField] float m_BrakeHardStopThreshold;
        [SerializeField] float m_BrakeHardStopSpeed;

        public Vector3 Direction { get; private set; }

        float dragLerp, currentGravityAcceleration;

        public override IEnumerator LoadRoutine ()
        {
            ReleaseBrakes();
            SetOrientation(Vector3.down);
            yield break;
        }

        public void SetOrientation (Vector3 direction)
        {
            Direction = direction;
            updateGravity();
        }

        public void Brake (float strength)
        {
            float targetDragLerp = m_DragLerpByBrakeStrength.Evaluate(strength),
                dragLerpSpeed = m_DragLerpSpeedByBrakeStrength.Evaluate(strength);

            if (dragLerp < targetDragLerp)
                dragLerp = Mathf.Min(targetDragLerp, dragLerp + dragLerpSpeed * Time.deltaTime);
            if (dragLerp > targetDragLerp)
                dragLerp = Mathf.Max(targetDragLerp, dragLerp - dragLerpSpeed * Time.deltaTime);

            if (strength > m_BrakeHardStopThreshold)
            {
                float delta = m_BrakeHardStopSpeed * Time.deltaTime;
                currentGravityAcceleration = Mathf.Max(0, currentGravityAcceleration - delta);
            }
            else
            {
                currentGravityAcceleration = m_GravityAcceleration;
            }

            updateGravity();
        }

        public void ReleaseBrakes ()
        {
            currentGravityAcceleration = m_GravityAcceleration;
            dragLerp = 0;

            updateGravity();
        }

        public float GetGravitizerDrag (RigidbodyGravitizer gravitizer)
        {
            return Mathf.Lerp(gravitizer.BaseDrag, m_BrakeDragMax, dragLerp);
        }

        void updateGravity ()
        {
            Physics.gravity = Direction * currentGravityAcceleration;
        }
    }
}
