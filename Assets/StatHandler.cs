using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatHandler : MonoBehaviour
{
    public static int classicCount = 0, miningCount = 0, destructionCount = 0;
    public static double classicAvg = 0, miningAvg = 0, destructionAvg = 0;

    // Start is called before the first frame update
    void Awake()
    {
        LoadStats();
    }

    private void LoadStats()
    {
        classicCount = ObscuredPrefs.GetInt("ClassicCount", 0);
        classicAvg = ObscuredPrefs.GetDouble("ClassicAvg", 0);

        miningCount = ObscuredPrefs.GetInt("MiningCount", 0);
        miningAvg = ObscuredPrefs.GetDouble("MiningAvg", 0);

        destructionCount = ObscuredPrefs.GetInt("DestructionCount", 0);
        destructionAvg = ObscuredPrefs.GetDouble("DestructionAvg", 0);
    }

    private void SaveStats()
    {
        ObscuredPrefs.SetInt("ClassicCount", classicCount);
        ObscuredPrefs.SetDouble("ClassicAvg", classicAvg);

        ObscuredPrefs.SetInt("MiningCount", miningCount);
        ObscuredPrefs.SetDouble("MiningAvg", miningAvg);

        ObscuredPrefs.SetInt("DestructionCount", destructionCount);
        ObscuredPrefs.SetDouble("DestructionAvg", destructionAvg);
    }

    private void OnApplicationQuit()
    {
        SaveStats();
    }
}
