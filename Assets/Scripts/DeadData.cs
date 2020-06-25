using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadData : MonoBehaviour
{
    public Vector3 originalPos;

    public void ResetPos()
    {
        transform.position = originalPos;
        transform.rotation = Quaternion.identity;
    }
}
