using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HeatShield")]
public class HeatShield : ShopItem
{
    public string shieldName;
    public Sprite shieldSprite;
    public float heatMultiplier = 1;
    public int id = 0;
}
