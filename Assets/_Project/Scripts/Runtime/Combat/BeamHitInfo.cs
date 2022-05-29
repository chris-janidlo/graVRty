using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Combat
{
    public struct BeamHitInfo
    {
        public int RayHitCount { get; private set; }
        public int TotalRaycasts { get; private set; }

        public FlashlightBeam SourceBeam { get; private set; }

        public float PercentageHit => (float) RayHitCount / TotalRaycasts;
        public Vector3 Centroid => cumulativeHitPosition / RayHitCount;

        Vector3 cumulativeHitPosition;

        public BeamHitInfo (FlashlightBeam sourceBeam, int raycasts)
        {
            SourceBeam = sourceBeam;
            TotalRaycasts = raycasts;
            RayHitCount = 0;

            cumulativeHitPosition = Vector3.zero;
        }

        public BeamHitInfo PlusRayHit (Vector3 position)
        {
            RayHitCount++;
            cumulativeHitPosition += position;
            return this;
        }
    }
}
