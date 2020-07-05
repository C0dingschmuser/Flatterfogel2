using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wing")]
public class Wing : ScriptableObject
{
    [Header("Grafik")]
    public Sprite[] sprite;

    [Header("Preis")]
    public bool purchased = false;
    public int salePercent = 0;
    public float shopScale = 3.333f;
    public CostData[] cost;

    [Header("Allgemein")]
    public string identifier;
    public int wingID;

}