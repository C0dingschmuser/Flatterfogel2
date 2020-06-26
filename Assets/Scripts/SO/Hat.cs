using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Hat")]
public class Hat : ScriptableObject
{
    public Sprite sprite;
    public float yDist = 0;
    public int hatID;
    public bool purchased = false;
    public CostData[] cost;
}
