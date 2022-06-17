using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Flashlights
{
    public class ConvergingFlashlightBeamVisual : AFlashlightBeamVisual
    {
        [SerializeField] ConvergingLight m_Light;

        public override void SetDimensions (Cone cone)
        {
            m_Light.Dimensions = cone;
        }
    }
}
