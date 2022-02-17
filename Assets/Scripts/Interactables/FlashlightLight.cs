using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightLight : MonoBehaviour
{
    public float Size { get; private set; }

    public void SetSize (float size)
    {
        Size = size;
        transform.localScale = new Vector3(size, size, 1f/size);
    }

    public void Kill ()
    {
        Destroy(gameObject);
    }
}
