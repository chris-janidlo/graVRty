using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Items.SnowGlobes
{
    [Serializable]
    public class SnowGlobeStats
    {
        public AnimationCurve TargetDragByBrake, DragChangeSpeedByBrake;

        [Range(0f, 1f)]
        public float MinBrakeToHardStopGravity;
        public float GravityHardStopSpeed;
    }
}
