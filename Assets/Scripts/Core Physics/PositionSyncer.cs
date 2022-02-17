using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace GraVRty.CorePhysics
{
    public class PositionSyncer : MonoBehaviour
    {
        public Vector3Variable PositionVariable;

        void Update ()
        {
            PositionVariable.Value = transform.position;
        }
    }
}
