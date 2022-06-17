using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Flashlights
{
    public abstract class AFlashlightBeamVisual : MonoBehaviour
    {
        public abstract void SetDimensions (Cone cone);
    }
}
