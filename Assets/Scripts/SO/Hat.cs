using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Hat")]
public class Hat : ShopItem
{
    [Header("Grafik")]
    public Sprite sprite = null;
    public Sprite[] animatedSprites = null;

    [Header("Allgemein")]
    public float yDist = 0;
}
