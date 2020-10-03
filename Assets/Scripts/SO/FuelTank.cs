using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FuelTank")]
public class FuelTank : ShopItem
{
    public string tankName;
    public Sprite sprite;
    public int capacity = 200;
    public int id = 0;
}
