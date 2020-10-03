using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CostData
{
    public CostData(MineralType type, int amount)
    {
        mineralID = type;
        this.amount = amount;
    }

    public MineralType mineralID;
    public int amount;
}

[CreateAssetMenu(menuName = "MinerTool")]
public class MinerTool : ShopItem
{
    public int id;
    public int mineTime = 1500;
    public Sprite main, effect, full;
    public Color laserColor;
}
