using UnityEngine;

[CreateAssetMenu(menuName = "MainMode")]
public class MainMode : ScriptableObject
{
    public Sprite previewSprite;
    public string modeName;

    [TextArea]
    public string modeDescription;
}