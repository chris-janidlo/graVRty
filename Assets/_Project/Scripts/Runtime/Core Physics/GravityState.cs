using UnityEngine;

namespace GraVRty.CorePhysics
{
    public struct GravityState
    {
        public Vector3 Direction { get; set; }

        float _amount;
        public float Amount
        {
            get => _amount;
            set => _amount = Mathf.Clamp01(value);
        }

        float _drag;
        public float Drag
        {
            get => _drag;
            set => _drag = Mathf.Clamp01(value);
        }

        public GravityState With (Vector3? direction = null, float? amount = null, float? drag = null)
        {
            return new GravityState
            {
                Direction = direction ?? Direction,
                Amount = amount ?? Amount,
                Drag = drag ?? Drag
            };
        }

        public override string ToString()
        {
            return $"gravity: {Direction} * {Amount}, {Drag} drag";
        }
    }
}
