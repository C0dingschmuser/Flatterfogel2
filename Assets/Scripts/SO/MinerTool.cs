using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CostData
{
    public MineralType mineralID;
    public int amount;
}

[CreateAssetMenu(menuName = "MinerTool")]
public class MinerTool : ScriptableObject
{
    public int id;
    public CostData[] cost;
    public int mineTime = 1500;
    public float shopScale = 1;
    public bool purchased = false;
    public Sprite main, effect, full;
    public Color laserColor;
}
