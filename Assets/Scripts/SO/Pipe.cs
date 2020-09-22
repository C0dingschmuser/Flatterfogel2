using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pipe")]
public class Pipe : ShopItem
{
    public Sprite sprite, endSprite;
    public Sprite[] animatedSprites = null;
    public Sprite[] animatedEndSprites = null;
    public Color defaultColor;
    public bool colorChangeSupported = false;
}