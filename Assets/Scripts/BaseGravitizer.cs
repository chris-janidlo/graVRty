using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGravitizer : MonoBehaviour
{
    [SerializeField] private Gravity m_Gravity;

    public Vector3 GravityVelocity => m_Gravity.Direction * gravitySpeed;

    float gravitySpeed;

    protected virtual void Update ()
    {
        switch (m_Gravity.State)
        {
            case GravityState.Active:
                gravitySpeed += m_Gravity.AccelerationAmount * Time.deltaTime;
                break;

            case GravityState.Flux:
                gravitySpeed = Mathf.Max(gravitySpeed - m_Gravity.DecelerationAmount * Time.deltaTime, 0);
                break;
        }
    }

    protected virtual void OnEnable ()
    {
        m_Gravity.OnGravityChanged += onGravityChanged;
    }

    protected virtual void OnDisable ()
    {
        m_Gravity.OnGravityChanged -= onGravityChanged;
    }

    /// <summary>
    /// Call this to let the BaseGravitizer know that this gravitizer has touched the ground.
    /// </summary>
    public void Ground ()
    {
        gravitySpeed = 0;
    }

    void onGravityChanged (GravityState newState)
    {
        if (newState == GravityState.Active) gravitySpeed = 0;
    }
}
