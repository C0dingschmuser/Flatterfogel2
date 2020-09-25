#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public static class OffsetDebug
{
    public static Transform parent, compare;

    [MenuItem("Assets/Copy Sprite Shape")]
    public static void CopySpriteShape()
    {
        if (Selection.activeObject != null)
        {
            parent = Selection.activeGameObject.transform;
            //Debug.Log(Selection.activeGameObject.name + " is at " + posToString(Selection.activeGameObject));
        }
    }

    [MenuItem("Assets/Copy Sprite Shape", true)]
    private static bool NewMenuOptionValidation()
    {
        // This returns true when the selected object is a Variable (the menu item will be disabled otherwise).
        return Selection.activeObject is Texture2D;
    }
}

#endif