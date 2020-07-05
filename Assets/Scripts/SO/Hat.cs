using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Hat")]
public class Hat : ScriptableObject
{
    [Header("Grafik")]
    public Sprite sprite = null;
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

    [Header("Allgemein")]
    public float yDist = 0;
    public int hatID;
    public string identifier;
}
