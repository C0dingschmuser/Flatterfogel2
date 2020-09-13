#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public static class OffsetCamera
{
    [MenuItem("Debug/Camera/ZoomIn")]
    public static void ZoomIn()
    {
        Camera c = Camera.main;

        c.transform.position = new Vector3(-470, 681, -400);
        c.orthographicSize = 480;
    }

    [MenuItem("Debug/Camera/ZoomOut")]
    public static void ZoomOut()
    {
        Camera c = Camera.main;

        c.transform.position = new Vector3(-381, 791, -400);
        c.orthographicSize = 640;
    }
}

#endif