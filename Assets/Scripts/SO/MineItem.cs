using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MineItem")]
public class MineItem : ScriptableObject
{
    public int id = 0, amount = 0;
    public string itemName;
    public CostData[] cost;
    public Sprite sprite;
}