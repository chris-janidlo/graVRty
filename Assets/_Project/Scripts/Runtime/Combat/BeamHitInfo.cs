using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Combat
{
    public struct BeamHitInfo
    {
        public int RayHitCount;
        public Vector3 Centroid;

        public BeamHitInfo PlusRayHit (Vector3 position)
        {
            RayHitCount++;
            Centroid += (position - Centroid) / RayHitCount; // cumulative average
            return this;
        }
    }
}
