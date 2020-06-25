using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HeatShield")]
public class HeatShield : ScriptableObject
{
    public string shieldName;
    public Sprite shieldSprite;
    public bool purchased = false;
    public float heatMultiplier = 1, shopScale = 1;
    public int id = 0;
    public CostData[] cost;
}
