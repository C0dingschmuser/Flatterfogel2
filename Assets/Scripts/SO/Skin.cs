using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skin")]
public class Skin : ScriptableObject
{
    public Sprite sprite;
    public int animated;
    public bool purchased = false, wingSupport = true, hatSupport = true;
    public CostData[] cost;
    public int skinID;
}
