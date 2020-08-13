using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gravestone/Bottom")]
public class GraveBottom : ShopItem
{
    [Header("Grafik")]
    public Sprite sprite = null;

    [Header("Allgemein")]
    public Vector3 offset = Vector3.zero;
}
