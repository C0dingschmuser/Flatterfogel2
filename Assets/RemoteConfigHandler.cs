using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.RemoteConfig;
using System.Threading.Tasks;
using System;
using Firebase.Extensions;

public class RemoteConfigHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetGetDefaults();
    }

    public void SetGetDefaults()
    {
        Dictionary<string, object> defaults = new Dictionary<string, object>();

        List<Skin> allSkins = ShopHandler.Instance.allSkins;
        for(int i = 0; i < allSkins.Count; i++)
        {
            string price = GetPriceString(allSkins[i].cost);

            defaults.Add("shop_skin_" + allSkins[i].identifier, price);
        }

        List<Wing> allWings = ShopHandler.Instance.allWings;
        for (int i = 0; i < allWings.Count; i++)
        {
            string price = GetPriceString(allWings[i].cost);

            defaults.Add("shop_wing_" + allWings[i].identifier, price);
        }

        List<Hat> allHats = ShopHandler.Instance.allHats;
        for (int i = 0; i < allHats.Count; i++)
        {
            string price = GetPriceString(allHats[i].cost);

            defaults.Add("shop_hat_" + allHats[i].identifier, price);
        }

        List<Pipe> allPipes = ShopHandler.Instance.allPipes;
        for (int i = 0; i < allPipes.Count; i++)
        {
            string price = GetPriceString(allPipes[i].cost);

            defaults.Add("shop_pipe_" + allPipes[i].identifier, price);
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

            string[] costParts = price.Split('|');

            for(int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allSkins[i].cost[a] = new CostData((MineralType)id, amount);
            }
        }

        List<Wing> allWings = ShopHandler.Instance.allWings;
        for (int i = 0; i < allWings.Count; i++)
        {
            string price =
                FirebaseRemoteConfig.GetValue("shop_wing_" + allWings[i].identifier).StringValue;

            string[] costParts = price.Split('|');

            for (int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allWings[i].cost[a] = new CostData((MineralType)id, amount);
            }
        }

        List<Hat> allHats = ShopHandler.Instance.allHats;
        for (int i = 0; i < allHats.Count; i++)
        {
            string price =
                FirebaseRemoteConfig.GetValue("shop_hat_" + allHats[i].identifier).StringValue;

            string[] costParts = price.Split('|');

            for (int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allHats[i].cost[a] = new CostData((MineralType)id, amount);
            }
        }

        List<Pipe> allPipes = ShopHandler.Instance.allPipes;
        for (int i = 0; i < allPipes.Count; i++)
        {
            string price =
                FirebaseRemoteConfig.GetValue("shop_pipe_" + allPipes[i].identifier).StringValue;

            string[] costParts = price.Split('|');

            for (int a = 0; a < costParts.Length - 1; a++)
            {
                string[] costData = costParts[a].Split('#');

                int id = Int32.Parse(costData[0]);
                int amount = Int32.Parse(costData[1]);

                allPipes[i].cost[a] = new CostData((MineralType)id, amount);
            }
        }
    }
}
