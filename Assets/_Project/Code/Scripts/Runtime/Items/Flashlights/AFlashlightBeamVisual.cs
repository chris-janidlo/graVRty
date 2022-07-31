using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Items.Flashlights
{
    public abstract class AFlashlightBeamVisual : MonoBehaviour
    {
        public abstract void SetDimensions (Cone cone);
    }
}
