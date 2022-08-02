using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Items.Flashlights
{
    public class FocusedFlashlightBeamVisual : AFlashlightBeamVisual
    {
        [SerializeField] ConvergingLight m_Light;

        Vector3 previousForward;

        // this is in LateUpdate so that this script can react to other scripts changing transform.forward during Update
        void LateUpdate ()
        {
            transform.forward = Vector3.Slerp(transform.forward, previousForward, 0.5f);
            previousForward = transform.forward;
        }

        public override void SetDimensions (Cone cone)
        {
            m_Light.Dimensions = cone;
        }
    }
}
