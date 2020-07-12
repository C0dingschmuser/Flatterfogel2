using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Test")]
public class Test : ShopItem
{
    [Header("Grafik")]
    public Sprite sprite = null;
    public Sprite[] animatedSprites = null;
}
