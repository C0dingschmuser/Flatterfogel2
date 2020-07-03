using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skin")]
public class Skin : ScriptableObject
{
    public Sprite sprite;
    public int animated;
    public bool purchased = false, wingSupport = true, hatSupport = true, boxCollider = false;
    public Wing overrideWing = null;
    public Vector2 colliderSize, colliderOffset;
    public CostData[] cost;
    public int skinID;
    public string identifier;
}
