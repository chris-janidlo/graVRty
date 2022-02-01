using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyGravitizer : MonoBehaviour
{
    [SerializeField] Rigidbody m_Rigidbody;
    [SerializeField] Gravity m_Gravity;

    Gravity.Tracker tracker;

    void OnEnable ()
    {
        if (m_Gravity == null) m_Gravity = FindObjectOfType<Gravity>();

        tracker = m_Gravity.GetNewTracker(true);
    }

    void OnDisable ()
    {
        m_Gravity.UnregisterTracker(tracker);
    }

    void FixedUpdate ()
    {
        m_Rigidbody.velocity = tracker.Velocity;
    }

    private void OnCollisionStay (Collision collision)
    {
        tracker.TrackGroundTouch();
    }
}
