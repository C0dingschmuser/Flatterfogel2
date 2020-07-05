using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skin")]
public class Skin : ScriptableObject
{
    [Header("Grafik")]
    public Sprite sprite;
    public Wing overrideWing = null;
    public Sprite[] animatedSprites = null;

    [Header("Animation")]
    public bool animated = false;
    public float animationSpeed = 0.25f;
    public float shopTime = 0;
    public int shopStep = 0;

    [Header("Preis")]
    public bool purchased = false;
    public int salePercent = 0;
    public CostData[] cost;
    public int[] boughtWings;
    public int[] boughtHats;

    [Header("Allgemein")]
    public bool wingSupport = true;
    public bool hatSupport = true;
    public bool boxCollider = false;
    public Vector2 colliderSize;
    public Vector2 colliderOffset;
    public int skinID;
    public string identifier;
}
