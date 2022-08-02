using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraVRty.Items.Flashlights;

namespace GraVRty.Enemies
{
    public class FakeBadGuy : FlashlightBeamTarget
    {
        protected override void onBeamStay (BeamHitInfo beamHitInfo)
        {
            Debug.Log($"{name} was hit by {beamHitInfo.RayHitCount} rays ({beamHitInfo.PercentageHit:P} of all rays cast) centered around {beamHitInfo.Centroid}");
        }
    }
}
