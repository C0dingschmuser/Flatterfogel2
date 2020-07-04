using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wing")]
public class Wing : ScriptableObject
{
    public Sprite[] sprite;
    public bool purchased = false;
    public CostData[] cost;
    public int wingID, salePercent = 0;
    public float shopScale = 3.333f;
    public string identifier;
}