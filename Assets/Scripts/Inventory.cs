using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using TMPro;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

public class Inventory : MonoBehaviour
{
    public class MineralHolder
    {
        public Item mineral;
        public int amount = 0;
    }

    public List<Item> allMinerals = new List<Item>();
    private List<MineralHolder> mineralInventory = new List<MineralHolder>();

    public static Inventory Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadInventory();
    }

    private void LoadInventory()
    {
        string inventoryString = ObscuredPrefs.GetString("Player_MineralString", "");
        if(inventoryString.Contains("|"))
        {
            string[] iSplit = inventoryString.Split('|');
            for(int i = 0; i < iSplit.Length; i++)
            {
                if(iSplit[i].Contains("#"))
                {
                    string[] mData = iSplit[i].Split('#');

                    if(mData.Length >= 2)
                    {
                        int mineralID = Int32.Parse(mData[0]);
                        int mineralAmount = Int32.Parse(mData[1]);

#if UNITY_EDITOR
                        mineralAmount = 999;
#endif

                        MineralHolder newHolder = new MineralHolder
                        {
                            mineral = allMinerals[mineralID],
                            amount = mineralAmount
                        };

                        mineralInventory.Add(newHolder);
                    }
                }
            }
        }
    }

    public void SaveInventory()
    {
        string inventoryString = "";

        for(int i = 0; i < mineralInventory.Count; i++)
        {
            string id = ((int)mineralInventory[i].mineral.id).ToString();
            string amount = mineralInventory[i].amount.ToString();

            inventoryString += id + "#" + amount + "|";
        }

        ObscuredPrefs.SetString("Player_MineralString", inventoryString);
    }

    public int GetMineralAmount(int id)
    {
        int amount = 0;

        for(int i = 0; i < mineralInventory.Count; i++)
        {
            if(mineralInventory[i].mineral.id == (MineralType)id)
            {
                amount = mineralInventory[i].amount;
                break;
            }
        }

        return amount;
    }

    public void SetMineralAmount(MineralType id, int amount, int mode = 0)
    {
        //Modes
        //0 = Set Amount raw
        //1 = Add to existing
        //2 = Subtract from existing

        MineralHolder mH = null;

        for(int i = 0; i < mineralInventory.Count; i++)
        {
            if(mineralInventory[i].mineral.id == id)
            {
                mH = mineralInventory[i];
                break;
            }
        }

        if(mH == null)
        { //mineral nicht gefunden -> erstellen
            
            if(mode == 2)
            {
                //braucht nix zu machen da mineral bereits nicht mehr existiert
                return;
            }

            mH = new MineralHolder
            {
                mineral = allMinerals[(int)id],
                amount = amount
            };
            mineralInventory.Add(mH);
        } else
        { //mineral gefunden
            int currentAmount = mH.amount;

            if(mode == 0)
            { //raw
                currentAmount = amount;
            } else if(mode == 1)
            { //add
                currentAmount += amount;
            } else if(mode == 2)
            { //subtract
                currentAmount -= amount;
            }

            mH.amount = currentAmount;

            if (mH.amount <= 0)
            {
                mineralInventory.Remove(mH);
            }
        }
        SaveInventory();
    }
}
