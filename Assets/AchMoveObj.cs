using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchMoveObj : MonoBehaviour
{
    public Vector3 target;
    public bool scaleUpDone = false, scaleDownDone = false;
    public float currentTime = 0, maxTime = 2, scaleUpTime = 0.1f, scaleDownTime = 0.5f;
    public long value;
}
