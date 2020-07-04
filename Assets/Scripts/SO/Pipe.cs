using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pipe")]
public class Pipe : ScriptableObject
{
    public Sprite[] sprite, endSprite;
    public Color defaultColor;
    public bool purchased = false, colorChangeSupported = false;
    public CostData[] cost;
    public int pipeID, salePercent = 0;
    public string identifier;
}