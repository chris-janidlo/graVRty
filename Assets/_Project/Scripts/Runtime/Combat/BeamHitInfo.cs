using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Combat
{
    public struct BeamHitInfo
    {
        public int RayHitCount, TotalRaycasts;
        public Vector3 Centroid;

        public float PercentageHit => (float) RayHitCount / TotalRaycasts;

        public BeamHitInfo (int totalRaycasts)
        {
            TotalRaycasts = totalRaycasts;
            RayHitCount = 0;
            Centroid = Vector3.zero;
        }

        public BeamHitInfo PlusRayHit (Vector3 position)
        {
            RayHitCount++;
            Centroid += (position - Centroid) / RayHitCount; // cumulative average
            return this;
        }
    }
}
