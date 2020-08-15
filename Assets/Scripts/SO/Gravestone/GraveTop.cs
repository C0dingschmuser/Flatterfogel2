using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gravestone/Top")]
public class GraveTop : ShopItem
{
    [Header("Grafik")]
    public Sprite sprite = null;

    [Header("Allgemein")]
    public Vector3 offset = Vector3.zero;
    public Vector3 menuOffset = new Vector3(-5.4f, 19.7f, 0);

    [Header("Wing")]
    public Vector3[] wingOffset;
    public Vector3[] wingMenuOffset;
}
