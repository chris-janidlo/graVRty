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

        tracker = m_Gravity.GetNewTracker();
    }

    void OnDisable ()
    {
        m_Gravity.UnregisterTracker(tracker);
    }

    void Update ()
    {
        m_Rigidbody.velocity += tracker.AcceelrationThisFrame;
    }

    private void OnCollisionStay (Collision collision)
    {
        tracker.TrackGroundTouch();
    }
}
