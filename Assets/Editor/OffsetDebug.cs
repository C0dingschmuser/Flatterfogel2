#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public static class OffsetDebug
{
    public static Transform parent, compare;

    [MenuItem("Debug/Offset/SetParent")]
    public static void SetParent()
    {
        if (Selection.activeGameObject != null)
        {
            parent = Selection.activeGameObject.transform;
            //Debug.Log(Selection.activeGameObject.name + " is at " + posToString(Selection.activeGameObject));
        }
    }

    [MenuItem("Debug/Offset/SetChild")]
    public static void SetChild()
    {
        if (Selection.activeGameObject != null)
        {
            if(parent == null)
            {
                Debug.LogError("Parent nicht gesetzt!");
                return;
            }

            compare = Selection.activeGameObject.transform;

            float xDiff = compare.position.x - parent.position.x;
            float yDiff = compare.position.y - parent.position.y;
            float zDiff = compare.position.z - parent.position.z;

            Debug.Log("Offset ist " + posToString(new Vector3(xDiff, yDiff, zDiff)));
        }
    }

    private static string posToString(Vector3 posV)
    {
        string pos = "(" + posV.x.ToString("F3") + ", " +
            posV.y.ToString("F3") + ", " +
            posV.z.ToString("F3") + ")";

        return pos;
    }
}

#endif