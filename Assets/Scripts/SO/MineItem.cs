using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MineItem")]
public class MineItem : ShopItem
{
    public int id = 0, amount = 0;
    public string itemName;
    public Sprite sprite;
}