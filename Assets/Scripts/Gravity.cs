using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GraVRty/Gravity Manager", fileName = "newGravityManager.asset")]
public class Gravity : ScriptableObject
{
    public float AccelerationAmount;

    public Vector3 Direction { get; private set; } = Vector3.down;

    public Quaternion Rotation { get; private set; } = Quaternion.identity;

    public Vector3 AccelerationThisFrame => AccelerationAmount * Time.deltaTime * Direction;

    public void SetOrientation (Transform source)
    {
        Direction = -source.up;
        Rotation = source.rotation;

        Physics.gravity = Direction * AccelerationAmount;
    }
}
