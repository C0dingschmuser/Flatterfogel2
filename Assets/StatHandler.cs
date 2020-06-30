using CodeStage.AntiCheat.Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatHandler : MonoBehaviour
{
    public static int classicCount = 0;

    // Start is called before the first frame update
    void Awake()
    {
        LoadStats();
    }

    private void LoadStats()
    {
        classicCount = ObscuredPrefs.GetInt("ClassicCount", 0);
    }

    private void SaveStats()
    {
        ObscuredPrefs.SetInt("ClassicCount", classicCount);
    }

    private void OnApplicationQuit()
    {
        SaveStats();
    }
}
