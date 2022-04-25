using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraVRty.Combat;

namespace GraVRty.Enemies
{
    public class FakeBadGuy : FlashlightBeamTarget
    {
        protected override void onBeamHit (BeamHitInfo beamHitInfo)
        {
            Debug.Log($"{name} was hit by {beamHitInfo.RayHitCount} rays centered around {beamHitInfo.Centroid}");
        }
    }
}
