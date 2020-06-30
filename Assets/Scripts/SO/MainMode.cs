using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "MainMode")]
public class MainMode : ScriptableObject
{
    public Sprite previewSprite;
    public string modeName;
    public LocalizedString modeNameLocalized;
    public int modeID;
}