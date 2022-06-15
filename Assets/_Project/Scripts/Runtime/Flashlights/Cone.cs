using System;
using UnityEngine;
using NaughtyAttributes;

namespace GraVRty.Flashlights
{
    [Serializable]
    // a right circular cone, at no particular point or orientation in space
    public class Cone : ISerializationCallbackReceiver
    {
        const float
            MIN_ANGLE = float.Epsilon, MAX_ANGLE = 180 - float.Epsilon,
            MIN_HEIGHT = float.Epsilon,
            MIN_RADIUS = float.Epsilon;

        public enum DrivingDimensionCombo
        {
            AngleAndHeight,
            AngleAndRadius,
            HeightAndRadius,
        }

        [SerializeField] DrivingDimensionCombo _drivingDimensions;

        [Tooltip("Angle in degrees, equal to twice the angle measured from the outside wall of the cone to the axis (the line connecting the tip and the center of the base)")]
        [AllowNesting, EnableIf(nameof(angleEditable))]
        [Range(MIN_ANGLE, MAX_ANGLE)]
        [SerializeField] float _angle = 90;

        [Tooltip("The distance between the tip of the cone and the center of the base")]
        [AllowNesting, EnableIf(nameof(heightEditable))]
        [Min(MIN_HEIGHT)]
        [SerializeField] float _height = 1;

        [Tooltip("The radius of the base")]
        [AllowNesting, EnableIf(nameof(radiusEditable))]
        [Min(MIN_RADIUS)]
        [SerializeField] float _radius = 1;

        public DrivingDimensionCombo DrivingDimensions
        {
            get => _drivingDimensions;
            set { _drivingDimensions = value; calculateDrivenDimensions(); }
        }

        public float Angle
        {
            get => _angle;
            set => tryEditDimension(ref _angle, value, angleEditable, nameof(Angle), min: MIN_ANGLE, max: MAX_ANGLE);
        }

        public float Height
        {
            get => _height;
            set => tryEditDimension(ref _height, value, heightEditable, nameof(Height), min: MIN_HEIGHT);
        }

        public float Radius
        {
            get => _radius;
            set => tryEditDimension(ref _radius, value, radiusEditable, nameof(Radius), min: MIN_RADIUS);
        }

        bool angleEditable => _drivingDimensions != DrivingDimensionCombo.HeightAndRadius;
        bool heightEditable => _drivingDimensions != DrivingDimensionCombo.AngleAndRadius;
        bool radiusEditable => _drivingDimensions != DrivingDimensionCombo.AngleAndHeight;

        public void OnBeforeSerialize ()
        {
            calculateDrivenDimensions();
        }

        public void OnAfterDeserialize () { }

        void tryEditDimension (ref float dimension, float value, bool editable, string variableName, float min = float.NegativeInfinity, float max = float.PositiveInfinity)
        {
            if (!editable)
            {
                Debug.LogWarning($"Attempt to edit {variableName} will be ignored because the dimensions driving this {nameof(Cone)} are {DrivingDimensions}.");
                return;
            }

            if (value < min || value > max)
            {
                Debug.LogWarning($"Clamping {variableName} value {value} to range [{min}, {max}].");
                value = Mathf.Clamp(value, min, max);
            }

            dimension = value;
            calculateDrivenDimensions();
        }

        void calculateDrivenDimensions()
        {
            // radius / height = tan(angle)

            // note that Mathf.Tan and Mathf.Atan work in radians, and that because
            // Angle is defined as sweeping the whole cone, we need to divide/multiply
            // by 2 in order to work with just the right triangle
            
            switch (DrivingDimensions)
            {
                case DrivingDimensionCombo.HeightAndRadius:
                    _angle = Mathf.Atan(Radius / Height) * Mathf.Rad2Deg * 2;
                    break;

                case DrivingDimensionCombo.AngleAndRadius:
                    _height = Radius / Mathf.Tan(Angle * Mathf.Deg2Rad / 2);
                    break;

                case DrivingDimensionCombo.AngleAndHeight:
                    _radius = Mathf.Tan(Angle * Mathf.Deg2Rad / 2) * Height;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(DrivingDimensions));
            }
        }
    }
}
