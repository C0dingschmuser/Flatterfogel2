using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wing")]
public class Wing : ShopItem
{
    [Header("Grafik")]
    public Sprite[] sprite;

    [Header("Preis")]
    public float shopScale = 3.333f;

}