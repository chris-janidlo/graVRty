using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GraVRty/Gravity Manager", fileName = "newGravityManager.asset")]
public class Gravity : ScriptableObject
{
    public float AccelerationAmount, DecelerationAmount;

    public delegate void GravityChanged (GravityState newState);
    public event GravityChanged OnGravityChanged;

    public GravityState State { get; private set; }
    public Vector3 Direction { get; private set; }
    public Quaternion Rotation { get; private set; }

    public void Initialize (Transform initialDirectionSource = null)
    {
        State = GravityState.Active;

        if (initialDirectionSource != null)
        {
            setOrientation(initialDirectionSource);
        }
        else
        {
            Direction = Vector3.down;
            Rotation = Quaternion.identity;
        }
    }

    public void StartFlux ()
    {
        if (State == GravityState.Flux) return;

        OnGravityChanged(State = GravityState.Flux);
    }

    public void StartActive (Transform newDirectionSource)
    {
        if (State == GravityState.Active) return;

        setOrientation(newDirectionSource);
        OnGravityChanged(State = GravityState.Active);
    }

    void setOrientation (Transform source)
    {
        Direction = -source.up;
        Rotation = source.rotation;
    }
}

/// <summary>
/// Describes how gravity should be affecting gravitized objects
/// </summary>
public enum GravityState
{
    /// <summary>
    /// The default state. Gravitized objects are accelerating in the current direction of gravity
    /// </summary>
    Active,
    /// <summary>
    /// The player (or someone else?) is currently modifying gravity. All gravitizeed objects are slowing down relative to the previous direction of gravity, and no additional acceleration due to gravity is being applied
    /// </summary>
    Flux
}
