using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraVRty.Loading;

namespace GraVRty.Combat
{
    [CreateAssetMenu(menuName = "GraVRty/Loadables/Flashlight Beam Pool", fileName = "newFlashlightBeamLoadablePool.asset")]
    public class FlashlightBeamCache : LoadableEagerCache<FlashlightBeam> { }
}
