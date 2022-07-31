using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Items.Flashlights
{
    public class DivergingFlashlightBeamVisual : AFlashlightBeamVisual
    {
        [Tooltip("Light is assumed to be a spotlight with a Volumetric Light Beam attached")]
        [SerializeField] Light m_Light;

        public override void SetDimensions (Cone cone)
        {
            m_Light.spotAngle = cone.Angle;
            m_Light.range = cone.Height;
        }
    }
}
