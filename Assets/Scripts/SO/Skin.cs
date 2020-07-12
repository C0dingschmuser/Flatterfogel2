﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skin")]
public class Skin : ShopItem
{
    [Header("Grafik")]
    public Sprite sprite;
    public Wing overrideWing = null;
    public Sprite[] animatedSprites = null;

    [Header("Preis")]
    public int[] boughtWings;
    public int[] boughtHats;

    [Header("Allgemein")]
    public float hatDiff = 0;
    public bool wingSupport = true;
    public bool hatSupport = true;
    public bool boxCollider = false;
    public Vector2 colliderSize;
    public Vector2 colliderOffset;
}
