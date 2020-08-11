using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "MainMode")]
public class MainMode : ScriptableObject
{
    public Sprite previewSprite;
    public string modeName, identifier;
    public LocalizedString modeNameLocalized;
    public bool unlocked = false;
    public int modeID, unlockLevel;
}