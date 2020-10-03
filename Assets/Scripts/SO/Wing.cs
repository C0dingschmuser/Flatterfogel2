using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wing")]
public class Wing : ShopItem
{
    [Header("Grafik")]
    public Sprite[] sprite;
    public int middleID = 1;
    public float xDist = 0;

}