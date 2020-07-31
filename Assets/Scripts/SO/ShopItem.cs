using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItem : ScriptableObject
{
    [Header("General")]
    public string identifier;
    public int itemID = 0;
    public Rarity rarity = Rarity.Casual;

    [Header("Price")]
    public bool purchased = false;
    public int salePercent = 0;
    public CostData[] cost;

    [Header("Animation")]
    public bool animated = false;
    public float animationSpeed = 0.25f;
    public float shopTime = 0;
    public int shopStep = 0;
}
