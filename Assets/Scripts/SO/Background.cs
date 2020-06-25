using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Background")]
public class Background : ScriptableObject
{
    public Sprite[] layer0, layer1, layer2, layer3;
    public Sprite[] layer0L, layer1L, layer2L, layer3L;

    public Sprite cover, nonScrollingSprite;
    public Color[] supportedColors;
    public Color topExtentColor;
    public Sprite topExtentSprite;
    public Vector2[] scale;
    //public Vector2 scale1 = new Vector2(635, 711.1f);
    public bool purchased = false, supportsColor = true, scrolling = true, supportsDayNight = true;
    public int lightLayerSpeedLvl = 1;
    public int cost;
    public int bgID;
}