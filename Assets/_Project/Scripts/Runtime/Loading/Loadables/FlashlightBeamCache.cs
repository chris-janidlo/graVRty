using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraVRty.Interactables;

namespace GraVRty.Loading.Loadables
{
    [CreateAssetMenu(menuName = "GraVRty/Loadables/Flashlight Beam Pool", fileName = "newFlashlightBeamLoadablePool.asset")]
    public class FlashlightBeamCache : LoadableEagerCache<FlashlightBeam> { }
}
