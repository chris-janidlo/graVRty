using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using GraVRty.GameFlow;

namespace GraVRty.Gravity
{
    [CreateAssetMenu(menuName = "GraVRty/Gravity Manager", fileName = "newGravityManager.asset")]
    public class GravityManager : Loadable
    {
        [SerializeField] float m_GravityAcceleration, m_BrakeDragMax;

        public GravityState State { get; private set; }

        public override IEnumerator LoadRoutine ()
        {
            SetState(new GravityState
            {
                Direction = Vector3.down,
                Amount = 1,
                Drag = 0,
            });

            yield break;
        }

        public void SetState (GravityState state)
        {
            State = state;
            Physics.gravity = m_GravityAcceleration * State.Amount * State.Direction;
        }

        public float GetGravitizerDrag (RigidbodyGravitizer gravitizer)
        {
            return Mathf.Lerp(gravitizer.BaseDrag, m_BrakeDragMax, State.Drag);
        }
    }
}
