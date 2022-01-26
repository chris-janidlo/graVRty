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
