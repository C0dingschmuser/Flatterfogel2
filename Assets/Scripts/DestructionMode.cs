using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DestructionMode
{
    public enum Type
    {
        Skyscraper,
        SmallSkyscraper,
        BlussiPlane,
        Flak,
        EnemyPlaneSmall1,
    }

    public Type type;
    public float minRightDist = 0;
    public float speed = 0;
    public Vector3 spawnPos;
    public int maxAmount = -1;
    public bool supportsSwap = true, instantFracture = true, doRotate = false;
    public GameObject obj = null;
}
