using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.RemoteConfig;
using System.Threading.Tasks;
using System;
using Firebase.Extensions;

public class RemoteConfigHandler : MonoBehaviour
{
    public static bool loadComplete = false;

    [SerializeField]
    private ShopHandler shopHandler;

    public void SetGetDefaults()
    {
        Dictionary<string, object> defaults = new Dictionary<string, object>();

        List<Skin> allSkins = ShopHandler.Instance.allSkins;
        for(int i = 0; i < allSkins.Count; i++)
        {
            string price = GetPriceString(allSkins[i].cost);

            string final = price + "~0";

            defaults.Add("shop_skin_" + allSkins[i].identifier, final);
        }

        List<Wing> allWings = ShopHandler.Instance.allWings;
        for (int i = 0; i < allWings.Count; i++)
        {
            string price = GetPriceString(allWings[i].cost);

            string final = price + "~0";

            defaults.Add("shop_wing_" + allWings[i].identifier, final);
        }

        List<Hat> allHats = ShopHandler.Instance.allHats;
        for (int i = 0; i < allHats.Count; i++)
        {
            string price = GetPriceString(allHats[i].cost);

            string final = price + "~0";

            defaults.Add("shop_hat_" + allHats[i].identifier, final);
        }

        List<Pipe> allPipes = ShopHandler.Instance.allPipes;
        for (int i = 0; i < allPipes.Count; i++)
        {
            string price = GetPriceString(allPipes[i].cost);

            string final = price + "~0";

            defaults.Add("shop_pipe_" + allPipes[i].identifier, final);
        }

        List<GraveTop> allGraveTops = ShopHandler.Instance.allGraveTops;
        for(int i = 0; i < allGraveTops.Count; i++)
        {
            string price = GetPriceString(allGraveTops[i].cost);

            string final = price + "~0";

            defaults.Add("shop_gravetop_" + allGraveTops[i].identifier, final);
        }

        List<GraveSide> allGraveSides = ShopHandler.Instance.allGraveSides;
        for (int i = 0; i < allGraveSides.Count; i++)
        {
            string price = GetPriceString(allGraveSides[i].cost);

            string final = price + "~0";

            defaults.Add("shop_graveside_" + allGraveSides[i].identifier, final);
        }

        List<GraveBottom> allGraveBottoms = ShopHandler.Instance.allGraveBottoms;
        for (int i = 0; i < allGraveBottoms.Count; i++)
        {
            string price = GetPriceString(allGraveBottoms[i].cost);

            string final = price + "~0";

            defaults.Add("shop_gravebottom_" + allGraveBottoms[i].identifier, final);
        }

        Achievement[] allAchievements = AchievementHandler.Instance.allAchievements;
        for(int i = 0; i < allAchievements.Length; i++)
        {
            if(!allAchievements[i].upgradable)
            { //nur nicht upgradable einlesen
                string rewards = GetPriceString(allAchievements[i].rewards);
                string final = rewards + "~" + allAchievements[i].maxStep.ToString();

                defaults.Add("achievement_" + allAchievements[i].identifier, final);
            }
        }

        FirebaseRemoteConfig.SetDefaults(defaults);
        StartCoroutine(FetchValues());
    }

    private string GetPriceString(CostData[] cost)
    {
        string price = "";

        for (int a = 0; a < cost.Length; a++)
        {
            if (cost[a].amount > 0)
            {
                int id = (int)cost[a].mineralID;
                price += id.ToString() + "#" + cost[a].amount.ToString() + "|";
            }
        }

        return price;
    }

    private IEnumerator FetchValues()
    {
        yield return FirebaseRemoteConfig.FetchAsync(TimeSpan.Zero);

        FirebaseRemoteConfig.ActivateFetched();

        List<Skin> allSkins = ShopHandler.Instance.allSkins;
        for (int i = 0; i < allSkins.Count; i++)
        {
            string price = 
                FirebaseRemoteConfig.GetValue("shop_skin_" + allSkins[i].identifier).StringValue;

            string[] dataPart = price.Split('~'); //andere daten
            string[] costParts = dataPart[0].Split('|'); //preise

            for(int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allSkins[i].cost[a] = new CostData((MineralType)id, amount);
            }

            if(dataPart[1].Length > 0)
            {
                int salePercent = Int32.Parse(dataPart[1]);

                allSkins[i].salePercent = salePercent;
            }
        }

        List<Wing> allWings = ShopHandler.Instance.allWings;
        for (int i = 0; i < allWings.Count; i++)
        {
            string price =
                FirebaseRemoteConfig.GetValue("shop_wing_" + allWings[i].identifier).StringValue;

            string[] dataPart = price.Split('~');
            string[] costParts = price.Split('|');

            for (int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allWings[i].cost[a] = new CostData((MineralType)id, amount);
            }

            if (dataPart[1].Length > 0)
            {
                int salePercent = Int32.Parse(dataPart[1]);

                allWings[i].salePercent = salePercent;
            }
        }

        List<Hat> allHats = ShopHandler.Instance.allHats;
        for (int i = 0; i < allHats.Count; i++)
        {
            string price =
                FirebaseRemoteConfig.GetValue("shop_hat_" + allHats[i].identifier).StringValue;

            string[] dataPart = price.Split('~');
            string[] costParts = price.Split('|');

            for (int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allHats[i].cost[a] = new CostData((MineralType)id, amount);
            }

            if (dataPart[1].Length > 0)
            {
                int salePercent = Int32.Parse(dataPart[1]);

                allHats[i].salePercent = salePercent;
            }
        }

        List<Pipe> allPipes = ShopHandler.Instance.allPipes;
        for (int i = 0; i < allPipes.Count; i++)
        {
            string price =
                FirebaseRemoteConfig.GetValue("shop_pipe_" + allPipes[i].identifier).StringValue;

            string[] dataPart = price.Split('~');
            string[] costParts = price.Split('|');

            for (int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allPipes[i].cost[a] = new CostData((MineralType)id, amount);
            }

            if (dataPart[1].Length > 0)
            {
                int salePercent = Int32.Parse(dataPart[1]);

                allPipes[i].salePercent = salePercent;
            }
        }

        List<GraveTop> allGraveTops = ShopHandler.Instance.allGraveTops;
        for(int i = 0; i < allGraveTops.Count; i++)
        {
            string price =
                FirebaseRemoteConfig.GetValue("shop_gravetop_" + allGraveTops[i].identifier).StringValue;

            string[] dataPart = price.Split('~');
            string[] costParts = price.Split('|');

            for (int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allGraveTops[i].cost[a] = new CostData((MineralType)id, amount);
            }

            if (dataPart[1].Length > 0)
            {
                int salePercent = Int32.Parse(dataPart[1]);

                allGraveTops[i].salePercent = salePercent;
            }
        }

        List<GraveSide> allGraveSides = ShopHandler.Instance.allGraveSides;
        for (int i = 0; i < allGraveSides.Count; i++)
        {
            string price =
                FirebaseRemoteConfig.GetValue("shop_graveside_" + allGraveSides[i].identifier).StringValue;

            string[] dataPart = price.Split('~');
            string[] costParts = price.Split('|');

            for (int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allGraveSides[i].cost[a] = new CostData((MineralType)id, amount);
            }

            if (dataPart[1].Length > 0)
            {
                int salePercent = Int32.Parse(dataPart[1]);

                allGraveSides[i].salePercent = salePercent;
            }
        }

        List<GraveBottom> allGraveBottoms = ShopHandler.Instance.allGraveBottoms;
        for (int i = 0; i < allGraveBottoms.Count; i++)
        {
            string price =
                FirebaseRemoteConfig.GetValue("shop_gravebottom_" + allGraveBottoms[i].identifier).StringValue;

            string[] dataPart = price.Split('~');
            string[] costParts = price.Split('|');

            for (int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allGraveBottoms[i].cost[a] = new CostData((MineralType)id, amount);
            }

            if (dataPart[1].Length > 0)
            {
                int salePercent = Int32.Parse(dataPart[1]);

                allGraveBottoms[i].salePercent = salePercent;
            }
        }

        Achievement[] allAchievements = AchievementHandler.Instance.allAchievements;
        for(int i = 0; i < allAchievements.Length; i++)
        {
            if(!allAchievements[i].upgradable)
            {
                string data = 
                    FirebaseRemoteConfig.GetValue("achievement_" + allAchievements[i].identifier).StringValue;


                string[] split = data.Split('~');

                string rewardFull = split[0].Remove(split[0].Length - 1);

                string[] reward = rewardFull.Split('#');

                MineralType type = (MineralType)Int32.Parse(reward[0]);
                int amount = Int32.Parse(reward[1]);

                allAchievements[i].rewards[0].mineralID = type;
                allAchievements[i].rewards[0].amount = amount;

                int maxStep = Int32.Parse(split[1]);

                allAchievements[i].maxStep = maxStep;

                if(allAchievements[i].unlockMode != null)
                {
                    allAchievements[i].unlockMode.unlockLevel = maxStep;
                }
            }
        }

        loadComplete = true;
        shopHandler.CompleteLoad();
    }
}
