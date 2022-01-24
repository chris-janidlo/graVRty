using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityInitializer : MonoBehaviour
{
    [SerializeField] Gravity m_Gravity;

    void Awake ()
    {
        m_Gravity.Initialize(transform);
    }
}
