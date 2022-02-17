using UnityEngine;
using UnityAtoms.BaseAtoms;

public class PositionSyncer : MonoBehaviour
{
    public Vector3Variable PositionVariable;

    void Update ()
    {
        PositionVariable.Value = transform.position;
    }
}
