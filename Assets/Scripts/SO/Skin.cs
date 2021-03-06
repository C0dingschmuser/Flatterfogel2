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
    public int[] boughtTails;
    public int lastHat = 0, lastTail = 0;

    [Header("Allgemein")]
    public float hatStart = 18.662f, wingStart = 0;

    /*private float shopHatStart, shopWingStart;

    public float GetShopHatStart()
    {
        //bei player abstand 37.7f

    }*/

    public bool wingSupport = true;
    public bool hatSupport = true;
    public bool tailSupport = true;
    public bool boxCollider = false;
    public Vector2 colliderSize;
    public Vector2 colliderOffset;
}
