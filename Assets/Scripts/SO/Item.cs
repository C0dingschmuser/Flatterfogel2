using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public bool meltable;
    public Item meltingResult;
    public MineralType id;
    public Sprite sprite, shopSprite = null;
    public int weight;
    public int sellPrice;
    public int fuelAmount;
}
