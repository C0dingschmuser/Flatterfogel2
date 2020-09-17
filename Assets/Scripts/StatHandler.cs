using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatHandler : MonoBehaviour
{
    public static int classicCount = 0, miningCount = 0, destructionCount = 0,
        deathCount = 0, coinEarnedCount = 0, coinBossEarnedCount = 0, coinSpentCount = 0;
    public static double classicAvg = 0, miningAvg = 0, destructionAvg = 0;

    public static StatHandler Instance;

    private List<long> classicResults = new List<long>();
    private List<long> miningResults = new List<long>();
    private List<long> destructionResults = new List<long>();

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;

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

        deathCount = ObscuredPrefs.GetInt("DeathCount", 0);
        coinEarnedCount = ObscuredPrefs.GetInt("CoinCount", 0);
        coinBossEarnedCount = ObscuredPrefs.GetInt("CoinBossCount", 0);
        coinSpentCount = ObscuredPrefs.GetInt("CoinSpentCount", 0);

        string lastClassic = ObscuredPrefs.GetString("ClassicLast", "");
        string lastMining = ObscuredPrefs.GetString("MiningLast", "");
        string lastDestruction = ObscuredPrefs.GetString("DestructionLast", "");

        for(int i = 0; i < 3; i++)
        {
            string data = lastClassic;
            List<long> list = classicResults;

            if(i == 1)
            {
                data = lastMining;
                list = miningResults;
            } else if(i == 2)
            {
                data = lastDestruction;
                list = destructionResults;
            }

            if(data.Contains("#"))
            {
                string[] split = data.Split('#');

                for(int a = 0; a < split.Length - 1; a++)
                {
                    long score = Int32.Parse(split[a]);

                    list.Add(score);
                }
            }

            if(list.Count > 0)
            { //es sind values in der liste -> zuweisen & avg berechnen

                double avg = CalcAvg(list);

                switch (i)
                {
                    case 0: //classic
                        classicResults = list;

                        classicAvg = avg;
                        break;
                    case 1:
                        miningResults = list;

                        miningAvg = avg;
                        break;
                    case 2:
                        destructionResults = list;

                        destructionAvg = avg;
                        break;
                }
            }
        }
    }

    private double CalcAvg(List<long> list)
    {
        long value = 0;

        for (int a = 0; a < list.Count; a++)
        { //alles zusammenrechnen
            value += list[a];
        }

        return (double)(value / (float)list.Count);
    }

    public void AddResult(int mode, long result)
    {
        switch(mode)
        {
            case 0:
                classicResults.Insert(0, result);
                classicAvg = CalcAvg(classicResults);

                if(classicResults.Count > 50)
                {
                    classicResults.RemoveRange(49, classicResults.Count - 49);
                }
                break;
            case 1:
                miningResults.Insert(0, result);
                miningAvg = CalcAvg(miningResults);

                if(miningResults.Count > 50)
                {
                    miningResults.RemoveRange(49, miningResults.Count - 49);
                }
                break;
            case 2:
                destructionResults.Insert(0, result);
                destructionAvg = CalcAvg(destructionResults);

                if(destructionResults.Count > 50)
                {
                    destructionResults.RemoveRange(49, destructionResults.Count - 49);
                }
                break;
        }

        SaveStats();
    }

    private void SaveStats()
    {
        ObscuredPrefs.SetInt("ClassicCount", classicCount);
        ObscuredPrefs.SetDouble("ClassicAvg", classicAvg);

        ObscuredPrefs.SetInt("MiningCount", miningCount);
        ObscuredPrefs.SetDouble("MiningAvg", miningAvg);

        ObscuredPrefs.SetInt("DestructionCount", destructionCount);
        ObscuredPrefs.SetDouble("DestructionAvg", destructionAvg);

        ObscuredPrefs.SetInt("DeathCount", deathCount);
        ObscuredPrefs.SetInt("CoinCount", coinEarnedCount);
        ObscuredPrefs.SetInt("CoinBossCount", coinBossEarnedCount);
        ObscuredPrefs.SetInt("CoinSpentCount", coinSpentCount);

        for (int i = 0; i < 3; i++)
        {
            string data = "";

            List<long> list = classicResults;

            if(i == 1)
            {
                list = miningResults;
            } else if(i == 2)
            {
                list = destructionResults;
            }

            for(int a = 0; a < list.Count; a++)
            {
                data += list[a].ToString() + "#";
            }

            switch (i)
            {
                case 0:
                    ObscuredPrefs.SetString("ClassicLast", data);
                    break;
                case 1:
                    ObscuredPrefs.SetString("MiningLast", data);
                    break;
                case 2:
                    ObscuredPrefs.SetString("DestructionLast", data);
                    break;
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveStats();
    }
}
