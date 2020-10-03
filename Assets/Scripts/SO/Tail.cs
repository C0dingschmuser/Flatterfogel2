using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tail")]
public class Tail : ShopItem
{
    [Header("Grafik")]
    public Sprite sprite;
    public int type = 1;
    public float gravity = 0;

}