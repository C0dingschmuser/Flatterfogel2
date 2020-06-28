using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using System;
using Random = UnityEngine.Random;
using Object = System.Object;

public class ShopHandler : MonoBehaviour
{
    [SerializeField]
    private ShopMenuHandler shopMenuHandler = null;
    [SerializeField]
    private CustomizationHandler customizationHandler = null;
    [SerializeField]
    private PipeCustomizationHandler pipeCustomizationHandler = null;

    public Canvas windowCanvas;
    public GameObject shopObj, eventSystem;
    public Vector3 startPos, defaultPos;
    private bool closing = false, pipeColorActive = false, opening = false;

    public static float moveTime = 0.25f;

    public GameObject buyButton, blusText, bgHinweis, bgParent, bgPreview, buyInfoObj, colorChangeObj,
        mineModePriceText, mineModePriceImage, blusEffect;
    public TextMeshProUGUI typeInfoText, bgType, bgTypeInfo, bgPreis;

    public List<Skin> allSkins = new List<Skin>();
    public List<Wing> allWings = new List<Wing>();
    public List<Hat> allHats = new List<Hat>();
    public List<MinerTool> allMiners = new List<MinerTool>();
    public List<HeatShield> allHeatShields = new List<HeatShield>();
    public List<MineItem> allMineItems = new List<MineItem>();
    public List<Pipe> allPipes = new List<Pipe>();
    public List<Background> allBackgrounds = new List<Background>();

    public Transform slotParent, bgSlotParent, colorSlotParent, minerSlotParent, shieldSlotParent, mineItemParent,
        minerParent, minigItemButtonParent;
    public Material fadeMat;

    public Color32 pipeColor = new Color32(238, 77, 46, 255);
    private int pipeColorID = 0;

    private Tween coinEffectPosition = null, coinEffectScale = null, coinColorTween = null;

    public FF_PlayerData playerData;
    public BackgroundHandler bgHandler;
    public static ShopHandler Instance;

#if UNITY_EDITOR
    private ObscuredULong blus = 100;
#else
    private ObscuredULong blus = 0;
#endif

    private int currentPage = 0, currentType = 0, wingAnimationCount = 0, wingAnimationDir = 0;
    private int colorFadeID = 0, lastSlotID = -1, minerType = 0, selectedSkin = 0, selectedWing = 0, selectedMiner = 0,
        selectedPipe = 0, selectedBackground = 0, selectedHeatShield = 0, selectedHat = 0;
    private bool shopActive = false;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //GenerateMinerSprites();
        LoadPurchasedItems();
        TypeClicked(0, true);

        pipeColorID = PlayerPrefs.GetInt("Pipe_Color", 0);
        pipeColor =
            colorSlotParent.transform.GetChild(pipeColorID).GetChild(0).GetComponent<Image>().color;

        fadeMat.color = Color.red;
        fadeMat.DOColor(Color.blue, 0.5f).SetEase(Ease.Linear);
        InvokeRepeating("NextColorStep", 0.51f, 0.251f);
        InvokeRepeating("HandleWingAnimation", 0.25f, 0.25f);
    }

    public Sprite GetSkinSprite(int skinID)
    {
        skinID = CheckSelected(CustomizationType.Skin, skinID);

        return allSkins[skinID].sprite;
    }

    public Sprite GetWingSprite(int wingID)
    {
        wingID = CheckSelected(CustomizationType.Wing, wingID);

        return allWings[wingID].sprite[0];
    }

    public Sprite GetHatSprite(int hatID)
    {
        hatID = CheckSelected(CustomizationType.Hat, hatID);

        return allHats[hatID].sprite;
    }

    public Sprite GetMinerSprite(int minerID)
    {
        return allMiners[minerID].main;
    }

    public Sprite GetPipeSprite(int pipeID, bool end = false)
    {
        pipeID = CheckSelected(CustomizationType.Pipe, pipeID);

        if(end)
        {
            return allPipes[pipeID].endSprite[0];
        }

        return allPipes[pipeID].sprite[0];
    }

    public int GetSelectedSkin()
    {
        return selectedSkin;
    }

    public int GetSelectedWing()
    {
        return selectedWing;
    }

    public int GetSelectedHat()
    {
        return selectedHat;
    }

    public int GetSelectedMiner()
    {
        return selectedMiner;
    }

    public int GetSelectedPipe()
    {
        return selectedPipe;
    }

    public int GetPipeColorID()
    {
        return pipeColorID;
    }

    public void BackClicked(GameObject caller)
    {
        caller.SetActive(false);

        buyButton.SetActive(false);

        shopMenuHandler.OpenMenu();
    }

    public int CheckSelected(CustomizationType type, int id)
    {
        switch(type)
        {
            case CustomizationType.Skin:
                if (id < 0) id = allSkins.Count + id;
                if (id >= allSkins.Count) id = id - allSkins.Count;
                break;
            case CustomizationType.Wing:
                if (id < 0) id = allWings.Count + id;
                if (id >= allWings.Count) id = id - allWings.Count;
                break;
            case CustomizationType.Pipe:
                if (id < 0) id = allPipes.Count + id;
                if (id >= allPipes.Count) id = id - allPipes.Count;
                break;
            case CustomizationType.Hat:
                if (id < 0) id = allHats.Count + id;
                if (id >= allHats.Count) id = id - allHats.Count;
                break;
        }

        return id;
    }

    public int GetMax(CustomizationType type)
    {
        int max = 0;
        switch(type)
        {
            case CustomizationType.Skin:
                max = allSkins.Count;
                break;
            case CustomizationType.Wing:
                max = allWings.Count;
                break;
            case CustomizationType.Pipe:
                max = allPipes.Count;
                break;
            case CustomizationType.Hat:
                max = allHats.Count;
                break;
        }

        return max;
    }

    public bool IsAffordable(CostData[] cost)
    {
        bool ok = true;

        for(int i = 0; i < cost.Length; i++)
        {
            if(cost[i].amount > 0)
            {
                ulong collectedAmount =
                    (ulong)Inventory.Instance.GetMineralAmount((int)cost[i].mineralID);

                if (cost[i].mineralID == MineralType.Coin)
                {
                    collectedAmount = GetBlus();
                }

                if (collectedAmount < (ulong)cost[i].amount)
                {
                    ok = false;
                    break;
                }
            }
        }

        return ok;
    }

    public CostData[] GetCost(CustomizationType type, int id)
    {
        CostData[] cost = null;

        switch (type)
        {
            case CustomizationType.Skin:
                cost = allSkins[id].cost;
                return cost;
            case CustomizationType.Wing:
                cost = allWings[id].cost;
                break;
            case CustomizationType.Pipe:
                cost = allPipes[id].cost;
                break;
            case CustomizationType.Hat:
                cost = allHats[id].cost;
                break;
        }

        return cost;
    }

    public int HasPurchased(CustomizationType type, int id)
    {
        /*  RETURN TYPES
         *  0 - Nicht gekauft & nicht genug Geld
         *  1 - Nicht gekauft & genug Geld
         *  2 - Gekauft
         */

        int code = 0;

        switch(type)
        {
            case CustomizationType.Skin:
                if(allSkins[id].purchased)
                {
                    code = 2;
                } else
                {
                    if(IsAffordable(allSkins[id].cost))
                    {
                        code = 1;
                    }
                }
                break;
            case CustomizationType.Wing:
                if(allWings[id].purchased)
                {
                    code = 2;
                } else
                {
                    if (IsAffordable(allWings[id].cost))
                    {
                        code = 1;
                    }
                }
                break;
            case CustomizationType.Pipe:
                if(allPipes[id].purchased)
                {
                    code = 2;
                } else
                {
                    if(IsAffordable(allPipes[id].cost))
                    {
                        code = 1;
                    }
                }
                break;
            case CustomizationType.PipeColor:
                code = 2;
                break;
            case CustomizationType.Hat:
                if (allHats[id].purchased)
                {
                    code = 2;
                }
                else
                {
                    if (IsAffordable(allHats[id].cost))
                    {
                        code = 1;
                    }
                }
                break;
        }

        return code;
    }

    public void SetSelected(CustomizationType type, int id)
    {
        switch(type)
        {
            case CustomizationType.Skin:
                selectedSkin = id;
                break;
            case CustomizationType.Wing:
                selectedWing = id;
                break;
            case CustomizationType.Pipe:
                selectedPipe = id;
                break;
            case CustomizationType.PipeColor:
                pipeColorID = id;
                break;
            case CustomizationType.Hat:
                selectedHat = id;
                break;
        }
    }

    public bool HasColorSupport(int id)
    {
        id = CheckSelected(CustomizationType.Pipe, id);

        return allPipes[id].colorChangeSupported;
    }

    public bool HasWingSupport(int id)
    {
        return allSkins[id].wingSupport;
    }

    public bool HasHatSupport(int id)
    {
        return allSkins[id].hatSupport;
    }

    public float GetWingScale(int id)
    {
        return allWings[id].shopScale;
    }

    private void GenerateMinerSprites()
    { //Erstellt minersprite mit laser aktiviert
        //(müssen überblendet werden da zwei sprites)
        for(int i = 0; i < allMiners.Count; i++)
        {
            Sprite main = allMiners[i].main;
            Sprite effect = allMiners[i].effect;
            Color color = allMiners[i].laserColor;

            ImageHelpers.overrideColor = color;

            Texture2D fullTexture =
                ImageHelpers.AlphaBlend(main.texture, 
                    ImageHelpers.CroppedTextureFromSprite(effect),
                    true);

            fullTexture.filterMode = FilterMode.Point;

            Sprite fullSprite = Sprite.Create(fullTexture,
                new Rect(0, 0, 64, 64),
                new Vector2(0.5f, 0.5f));
            allMiners[i].full = fullSprite;
        }
    }

    public void UpdateBlus(ulong newBlus, int add = 1, bool effect = false)
    {
        switch(add)
        {
            default:
                blus = newBlus;
                break;
            case -1: //minus
                if(newBlus > blus)
                {
                    blus = 0;
                } else
                {
                    blus -= newBlus;
                }
                break;
            case 1: //add
                blus += newBlus;
                break;
        }

        blusText.GetComponent<TextMeshProUGUI>().text = blus.ToString();

        if(effect)
        {
            blusEffect.SetActive(false);
            blusEffect.SetActive(true);

            if(coinEffectPosition != null)
            {
                if(coinEffectPosition.active)
                {
                    coinEffectPosition.Kill();
                }
            }

            if (coinEffectScale != null)
            {
                if (coinEffectScale.active)
                {
                    coinEffectScale.Kill();
                }
            }

            if(coinColorTween != null)
            {
                if(coinColorTween.active)
                {
                    coinColorTween.Kill();
                }
            }

            blusText.transform.parent.position = new Vector3(-669.3f, 1359.3f, 100);
            blusText.transform.parent.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            blusText.GetComponent<TextMeshProUGUI>().color = Color.white;

            coinEffectPosition = 
                blusText.transform.parent.DOMove(new Vector3(-710.3f, 1396, 100), 0.5f);

            coinEffectScale =
                blusText.transform.parent.DOScale(1, 0.5f);

            coinColorTween =
                blusText.GetComponent<TextMeshProUGUI>().DOColor(new Color32(192, 115, 0, 255), 0.5f);
        }
    }

    public void BuyInfoOkayClicked()
    {
        buyInfoObj.SetActive(false);
    }

    public ulong GetBlus()
    {
        return blus;
    }

    public Skin GetRandomSkin()
    {
        return allSkins[Random.Range(0, allSkins.Count)];
    }

    private void ResetAll()
    {
        for (int i = 1; i < allSkins.Count; i++)
        {
            allSkins[i].purchased = false;
        }

        for (int i = 1; i < allWings.Count; i++)
        {
            allWings[i].purchased = false;
        }

        for (int i = 1; i < allPipes.Count; i++)
        {
            allPipes[i].purchased = false;
        }

        for (int i = 2; i < allBackgrounds.Count; i++)
        {
            allBackgrounds[i].purchased = false;
        }

        for (int i = 1; i < allMiners.Count; i++)
        {
            allMiners[i].purchased = false;
        }

        for (int i = 1; i < allHeatShields.Count; i++)
        {
            allHeatShields[i].purchased = false;
        }

        for(int i = 0; i < allMineItems.Count; i++)
        {
            allMineItems[i].amount = 0;
        }

        for(int i = 1; i < allHats.Count; i++)
        {
            allHats[i].purchased = false;
        }
    }

    private void LoadPurchasedItems()
    {
        UpdateBlus(ObscuredPrefs.GetULong("Blus", 0), 0);

        selectedSkin = ObscuredPrefs.GetInt("SelectedSkin", 0);
        selectedWing = ObscuredPrefs.GetInt("SelectedWing", 0);
        selectedHat = ObscuredPrefs.GetInt("SelectedHat", 0);
        selectedMiner = ObscuredPrefs.GetInt("SelectedMiner", 0);

        selectedPipe = ObscuredPrefs.GetInt("SelectedPipe", 0);
        FF_PlayerData.Instance.LoadPipe();

        selectedBackground = 0; //ObscuredPrefs.GetInt("SelectedBackground", 0); OVERRIDE
        selectedHeatShield = ObscuredPrefs.GetInt("SelectedHeatShield", 0);

        MineHandler.Instance.currentHeatShield = allHeatShields[selectedHeatShield];

        ResetAll();

        string data = ObscuredPrefs.GetString("ShopPurchasedItems", "");
        if(data.Length > 0 && data.Contains("|"))
        {
            string[] types = data.Split('|');
            if(types[0].Contains(","))
            { //gekaufte skins vorhanden -> einlesen
                string[] skinData = types[0].Split(',');
                for(int i = 0; i < skinData.Length; i++)
                {
                    if(skinData[i].Length > 0)
                    {
                        int id = Int32.Parse(skinData[i]);
                        allSkins[id].purchased = true;
                    }
                }
            }
            if(types[1].Contains(","))
            {
                string[] wingData = types[1].Split(',');
                for(int i = 0; i < wingData.Length; i++)
                {
                    if(wingData[i].Length > 0)
                    {
                        int id = Int32.Parse(wingData[i]);
                        allWings[id].purchased = true;
                    }
                }
            }
            if(types[2].Contains(","))
            {
                string[] minerData = types[2].Split(',');
                for(int i = 0; i < minerData.Length; i++)
                {
                    if(minerData[i].Length > 0)
                    {
                        int id = Int32.Parse(minerData[i]);
                        allMiners[id].purchased = true;
                    }
                }
            }
            if(types[3].Contains(","))
            {
                string[] pipeData = types[3].Split(',');
                for (int i = 0; i < pipeData.Length; i++)
                {
                    if (pipeData[i].Length > 0)
                    {
                        int id = Int32.Parse(pipeData[i]);
                        allPipes[id].purchased = true;
                    }
                }
            }
            if(types[4].Contains(","))
            {
                string[] backgroundData = types[4].Split(',');
                for(int i = 0; i < backgroundData.Length; i++)
                {
                    if(backgroundData[i].Length > 0)
                    {
                        int id = Int32.Parse(backgroundData[i]);
                        allBackgrounds[id].purchased = true;
                    }
                }
            }
            if(types[5].Contains(","))
            {
                /*string[] minerData = types[5].Split(',');
                for(int i = 0; i < minerData.Length; i++)
                {
                    if(minerData[i].Length > 0)
                    {
                        int id = Int32.Parse(minerData[i]);
                        allMiners[id].purchased = true;
                    }
                }*/
            }
            if(types.Length > 6)
            {
                if (types[6].Contains(","))
                {
                    string[] heatShieldData = types[6].Split(',');
                    for (int i = 0; i < heatShieldData.Length; i++)
                    {
                        if (heatShieldData[i].Length > 0)
                        {
                            int id = Int32.Parse(heatShieldData[i]);
                            allHeatShields[id].purchased = true;
                        }
                    }
                }
            }
            if(types.Length > 7)
            {
                if(types[7].Contains(","))
                {
                    string[] mineItemData = types[7].Split(',');
                    for(int i = 0; i < mineItemData.Length; i++)
                    {
                        if(mineItemData[i].Length > 0)
                        {
                            int amount = Int32.Parse(mineItemData[i]);
                            allMineItems[i].amount = amount;
                        }
                    }
                }
            }
            if(types.Length > 8)
            {
                if (types[8].Contains(","))
                {
                    string[] hatData = types[8].Split(',');
                    for (int i = 0; i < hatData.Length; i++)
                    {
                        if (hatData[i].Length > 0)
                        {
                            int id = Int32.Parse(hatData[i]);
                            allHats[id].purchased = true;
                        }
                    }
                }
            }
        }

        PlayerMiner.currentMiner = allMiners[selectedMiner];

        playerData.LoadPlayerSkin(allSkins[selectedSkin], allWings[selectedWing]);
        playerData.LoadHat(allHats[selectedHat]);
    }

    public void SavePurchasedItems()
    {
        string data = "";
        for(int i = 0; i < allSkins.Count; i++)
        {
            if(allSkins[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }
        data += "|";

        for(int i = 0; i < allWings.Count; i++)
        {
            if(allWings[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }
        data += "|";

        for(int i = 0; i < allMiners.Count; i++)
        {
            if(allMiners[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }
        data += "|";

        for (int i = 0; i < allPipes.Count; i++)
        {
            if (allPipes[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }
        data += "|";

        for (int i = 0; i < allBackgrounds.Count; i++)
        {
            if (allBackgrounds[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }
        data += "|";

        for(int i = 0; i < allMiners.Count; i++)
        {
            if(allMiners[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }
        data += "|";

        for(int i = 0; i < allHeatShields.Count; i++)
        {
            if(allHeatShields[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }
        data += "|";

        for(int i = 0; i < allMineItems.Count; i++)
        {
            data += allMineItems[i].amount + ",";
        }
        data += "|";

        for (int i = 0; i < allHats.Count; i++)
        {
            if (allHats[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }
        data += "|";

        ObscuredPrefs.SetString("ShopPurchasedItems", data);
        ObscuredPrefs.SetULong("Blus", blus);
        ObscuredPrefs.SetInt("SelectedSkin", selectedSkin);
        ObscuredPrefs.SetInt("SelectedWing", selectedWing);
        ObscuredPrefs.SetInt("SelectedPipe", selectedPipe);
        ObscuredPrefs.SetInt("SelectedBackground", selectedBackground);
        ObscuredPrefs.SetInt("SelectedMiner", selectedMiner);
        ObscuredPrefs.SetInt("SelectedHeatShield", selectedHeatShield);
        ObscuredPrefs.SetInt("SelectedHat", selectedHat);
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            SavePurchasedItems();
        }
    }

    private void OnApplicationQuit()
    {
        SavePurchasedItems();
    }

    private void NextColorStep()
    {
        colorFadeID++;
        if(colorFadeID == 3)
        {
            colorFadeID = 0;
        }

        switch(colorFadeID)
        {
            case 0:
                fadeMat.DOColor(Color.blue, 0.5f).SetEase(Ease.Linear);
                break;
            case 1:
                fadeMat.DOColor(Color.green, 0.5f).SetEase(Ease.Linear);
                break;
            case 2:
                fadeMat.DOColor(Color.red, 0.5f).SetEase(Ease.Linear);
                break;
        }
    }

    public bool MinerPurchased(int id)
    {
        return allMiners[id].purchased;
    }

    public bool HeatShieldPurchased(int id)
    {
        return allHeatShields[id].purchased;
    }

    public bool HatPurchased(int id)
    {
        return allHats[id].purchased;
    }

    public int GetLatestMiner()
    {
        int id = 0;
        for(int i = 0; i < allMiners.Count; i++)
        {
            if(allMiners[i].purchased)
            {
                id = i;
            }
        }

        return id;
    }

    public int GetLatestHeatShield()
    {
        int id = 0;
        for (int i = 0; i < allHeatShields.Count; i++)
        {
            if (allHeatShields[i].purchased)
            {
                id = i;
            }
        }

        return id;
    }

    public void OpenShop()
    {
        if (shopActive || opening) return;

        opening = true;

        eventSystem.SetActive(false);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        shopActive = true;

        windowCanvas.sortingOrder = 11;

        ModeManager.Instance.BackClicked();
        StartCoroutine(MenuData.Instance.DoMoveAway());

        colorChangeObj.SetActive(false);
        bgParent.SetActive(false);
        minerParent.gameObject.SetActive(false);
        slotParent.gameObject.SetActive(false);
        customizationHandler.gameObject.SetActive(false);
        pipeCustomizationHandler.gameObject.SetActive(false);
        buyButton.gameObject.SetActive(false);

        shopMenuHandler.OpenMenu();
        shopObj.transform.position = startPos;
        //shopObj.transform.localScale = new Vector3(0, 0, 0);

        shopObj.transform.DOMove(defaultPos, moveTime).SetEase(Ease.OutBack);
        //shopObj.transform.DOScale(new Vector3(1, 1, 1), moveTime);

        Invoke("ReactivateEventSystem", moveTime + 0.01f);
    }

    private void ReactivateEventSystem()
    {
        eventSystem.SetActive(true);

        if(closing)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            closing = false;

            windowCanvas.sortingOrder = 10;
            StartCoroutine(MenuData.Instance.DoMoveIn());
            //MenuData.Instance.DoScaleUp();
        } else if(opening)
        {
            opening = false;
        }
    }

    public void CloseShop()
    {
        if (pipeColorActive)
        {
            CloseColorSelection();
        }

        eventSystem.SetActive(false);

        shopActive = false;

        closing = true;

        shopObj.transform.DOMove(startPos, moveTime).SetEase(Ease.InBack);
        //shopObj.transform.DOMove(startPos, moveTime);
        //shopObj.transform.DOScale(new Vector3(0, 0, 0), moveTime);

        customizationHandler.CloseCustomization(true);
        pipeCustomizationHandler.CloseCustomization(true);

        Invoke("ReactivateEventSystem", moveTime + 0.01f);
    }

    public void ColorChangeClicked()
    {
        colorChangeObj.transform.GetChild(1).gameObject.SetActive(true);

        pipeColorActive = true;

        HandleSlotColor(pipeColorID);

        buyButton.GetComponent<Image>().color = Color.white;
        buyButton.GetComponent<Button>().interactable = false;
        buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Wählen";
    }

    private void HandleSlotColor(int id)
    {
        for(int i = 0; i < colorSlotParent.childCount; i++)
        {
            if(i == id)
            {
                colorSlotParent.GetChild(i).GetComponent<Image>().color =
                    new Color32(238, 77, 46, 255);
            } else
            {
                colorSlotParent.GetChild(i).GetComponent<Image>().color =
                    Color.white;
            }
        }
    }

    public void ColorChangeSlotClicked(int id)
    {
        pipeColorID = id;

        pipeColor = 
            colorSlotParent.transform.GetChild(id).GetChild(0).GetComponent<Image>().color;

        HandleSlotColor(id);
        buyButton.GetComponent<Button>().interactable = true;
    }

    public void TypeClicked(int id)
    {
        TypeClicked(id, false);
    }

    public void TypeClicked(int id, bool start = false)
    {
        lastSlotID = -1;
        currentType = id;
        currentPage = 0;

        if(id != 4 && !start)
        {
            bgParent.SetActive(false);
            slotParent.gameObject.SetActive(true);
        }

        //if(id != 2)
        //{
            buyButton.SetActive(false);
        //} else
        //{
        //    buyButton.SetActive(true);
        //}

        colorChangeObj.SetActive(false);

        minerParent.gameObject.SetActive(false);

        switch (id)
        {
            case 0:
                typeInfoText.text = "SKINS";

                for(int i = 0; i < slotParent.childCount; i++)
                {
                    if(allSkins.Count > i)
                    {
                        slotParent.GetChild(i).GetChild(0).gameObject.SetActive(true);
                        if(!allSkins[i].purchased)
                        { //wenn nicht gekauft preis anzeigen
                            slotParent.GetChild(i).GetChild(1).gameObject.SetActive(true);
                            slotParent.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = allSkins[i].cost.ToString();
                        } else
                        {
                            slotParent.GetChild(i).GetChild(1).gameObject.SetActive(false);
                        }
                        slotParent.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                            allSkins[i].sprite;

                        slotParent.GetChild(i).GetChild(0).GetComponent<RectTransform>().sizeDelta =
                            new Vector2(75, 75);

                        slotParent.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
                    } else
                    {
                        slotParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
                        slotParent.GetChild(i).GetChild(1).gameObject.SetActive(false); //bild deaktivieren
                    }
                    slotParent.GetChild(i).GetComponent<Image>().color = Color.white;
                    slotParent.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
                }

                slotParent.GetChild(selectedSkin).GetComponent<Image>().color =
                    new Color32(255, 253, 0, 255);

                break;
            case 1:
                typeInfoText.text = "FLÜGEL";

                for (int i = 0; i < slotParent.childCount; i++)
                {
                    if(allWings.Count > i)
                    {
                        slotParent.GetChild(i).GetChild(0).gameObject.SetActive(true);
                        if (!allWings[i].purchased)
                        { //wenn nicht gekauft preis anzeigen
                            slotParent.GetChild(i).GetChild(1).gameObject.SetActive(true);
                            slotParent.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = allWings[i].cost.ToString();
                        }
                        else
                        {
                            slotParent.GetChild(i).GetChild(1).gameObject.SetActive(false);
                        }

                        slotParent.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                            allWings[i].sprite[0];

                        //slotParent.GetChild(i).GetChild(0).GetComponent<RectTransform>().sizeDelta =
                        //    new Vector2(allWings[i].shopSize, allWings[i].shopSize);

                    } else
                    {
                        slotParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
                        slotParent.GetChild(i).GetChild(1).gameObject.SetActive(false); //bild deaktivieren
                    }
                    slotParent.GetChild(i).GetComponent<Image>().color = Color.white;
                    slotParent.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
                }

                slotParent.GetChild(selectedWing).GetComponent<Image>().color =
                    new Color32(255, 253, 0, 255);

                break;
            case 2: //miners
                typeInfoText.text = "";

                //MinerSlotClicked(selectedMiner);
                //MinerShieldClicked(selectedHeatShield);

                slotParent.gameObject.SetActive(false);
                minerParent.gameObject.SetActive(true);

                /*for(int i = 0; i < mineItemParent.childCount; i++)
                {
                    if(mineItemParent.GetChild(i).gameObject.activeSelf)
                    {
                        mineItemParent.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                            allMineItems[i].sprite;

                        mineItemParent.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text =
                            allMineItems[i].amount.ToString();
                    }
                }*/

                // miner

                /*for (int i = 0; i < slotParent.childCount; i++)
                {
                    if (allMiners.Count > i)
                    {
                        slotParent.GetChild(i).GetChild(0).gameObject.SetActive(true);
                        if (!allMiners[i].purchased)
                        { //wenn nicht gekauft preis anzeigen
                            slotParent.GetChild(i).GetChild(1).gameObject.SetActive(true);
                            slotParent.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = allMiners[i].cost.ToString();
                        }
                        else
                        {
                            slotParent.GetChild(i).GetChild(1).gameObject.SetActive(false);
                        }
                        slotParent.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                            allMiners[i].full;

                        slotParent.GetChild(i).GetChild(0).GetComponent<RectTransform>().sizeDelta =
                            new Vector2(75, 75);

                    }
                    else
                    {
                        slotParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
                        slotParent.GetChild(i).GetChild(1).gameObject.SetActive(false); //bild deaktivieren
                    }
                    slotParent.GetChild(i).GetComponent<Image>().color = Color.white;
                    slotParent.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
                }

                slotParent.GetChild(selectedMiner).GetComponent<Image>().color =
                    new Color32(255, 253, 0, 255);*/

                break;
            case 3:
                typeInfoText.text = "RÖHREN";

                for (int i = 0; i < slotParent.childCount; i++)
                {
                    if (allPipes.Count > i)
                    {
                        slotParent.GetChild(i).GetChild(0).gameObject.SetActive(true);
                        if (!allPipes[i].purchased)
                        { //wenn nicht gekauft preis anzeigen
                            slotParent.GetChild(i).GetChild(1).gameObject.SetActive(true);
                            slotParent.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = allPipes[i].cost.ToString();
                        }
                        else
                        {
                            slotParent.GetChild(i).GetChild(1).gameObject.SetActive(false);
                        }
                        slotParent.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                            allPipes[i].sprite[0];

                        slotParent.GetChild(i).GetChild(0).GetComponent<RectTransform>().sizeDelta =
                            new Vector2(75, 75);

                        slotParent.GetChild(i).GetChild(0).GetComponent<Image>().color = allPipes[i].defaultColor;
                    }
                    else
                    {
                        slotParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
                        slotParent.GetChild(i).GetChild(1).gameObject.SetActive(false); //bild deaktivieren
                    }
                    slotParent.GetChild(i).GetComponent<Image>().color = Color.white;

                }

                break;
            case 4:
                typeInfoText.text = "HINTERGRÜNDE";

                slotParent.gameObject.SetActive(false);

                bgParent.SetActive(true);
                bgType.gameObject.SetActive(false);
                bgTypeInfo.gameObject.SetActive(false);
                bgPreis.gameObject.SetActive(false);
                bgHinweis.SetActive(false);
                bgPreview.transform.GetChild(0).gameObject.SetActive(false);

                for (int i = 0; i < bgSlotParent.childCount; i++)
                {
                    if(allBackgrounds.Count > i)
                    {
                        bgSlotParent.GetChild(i).GetChild(0).gameObject.SetActive(true);

                        bgSlotParent.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                            allBackgrounds[i].cover;
                    } else
                    {
                        bgSlotParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
                    }
                }

                break;
        }
    }

    /*public void SlotClicked(int id)
    {
        int realID = id + (slotParent.childCount * currentPage);

        if(lastSlotID > -1)
        {
            slotParent.GetChild(lastSlotID).GetComponent<Image>().color = Color.white;
        }

        lastSlotID = id;

        slotParent.GetChild(id).GetComponent<Image>().color = 
            new Color32(255, 253, 0, 255);

        bool purchased = false;
        switch(currentType)
        {
            case 0: //skins

                if(allSkins[realID].purchased)
                {
                    purchased = true;
                }

                if(selectedSkin != realID)
                {
                    slotParent.GetChild(selectedSkin).GetComponent<Image>().color = Color.white;
                }

                break;
            case 1: //wings

                if(allWings[realID].purchased)
                {
                    purchased = true;
                }

                if(selectedWing != realID)
                {
                    slotParent.GetChild(selectedWing).GetComponent<Image>().color = Color.white;
                }

                break;
            /*case 2: //miner

                if (allMiners[realID].purchased)
                {
                    purchased = true;
                }

                if (selectedMiner != realID)
                {
                    slotParent.GetChild(selectedMiner).GetComponent<Image>().color = Color.white;
                }

                break;
            case 3: //pipes

                if (allPipes[realID].purchased)
                {
                    purchased = true;
                }

                if(allPipes[realID].colorChangeSupported)
                {
                    colorChangeObj.SetActive(true);
                } else
                {
                    colorChangeObj.SetActive(false);
                }

                if (selectedPipe != realID)
                {
                    slotParent.GetChild(selectedPipe).GetComponent<Image>().color = Color.white;
                }

                break;
        }

        if(purchased)
        {
            buyButton.GetComponent<Image>().color = Color.white;
            buyButton.GetComponent<Button>().interactable = true;
            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Wählen";
        } else
        {
            bool price = false; //true wenn zu teuer

            switch(currentType)
            {
                case 0: //skin
                    if ((ulong)allSkins[realID].cost > blus)
                    {
                        price = true;
                    }
                    break;
                case 1: //wing
                    if ((ulong)allWings[realID].cost > blus)
                    {
                        price = true;
                    }
                    break;
                /*case 2: //miner
                    if ((ulong)allMiners[realID].cost > blus)
                    {
                        price = true;
                    }
                    break;
                case 3: //pipe
                    if ((ulong)allPipes[realID].cost > blus)
                    {
                        price = true;
                    }
                    break;
            }

            if(price)
            { //zu teuer
                buyButton.GetComponent<Button>().interactable = false;
                buyButton.GetComponent<Image>().color = Color.red;
            } else
            {
                buyButton.GetComponent<Button>().interactable = true;
                buyButton.GetComponent<Image>().color = Color.green;
            }
            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Kaufen";
        }
    }*/

    public void BackgroundSlotClicked(int id)
    {
        int realID = id + (bgSlotParent.childCount * currentPage);

        lastSlotID = id;

        bool purchased = allBackgrounds[realID].purchased;

        bgPreis.gameObject.SetActive(true);
        if (!purchased)
        { //wenn nicht gekauft preis anzeigen
            bgPreis.text = allBackgrounds[realID].cost.ToString();
        }
        else
        {
            bgPreis.text = "Gekauft";
        }

        string typeText = "Parralax";
        string typeInfoText = "-> Hintergrund bewegt sich";

        if (!allBackgrounds[realID].scrolling)
        {
            typeText = "Normal";
            typeInfoText = "-> Hintergrund bewegt sich nicht";
        }

        bgType.gameObject.SetActive(true);
        bgType.text = typeText;

        bgTypeInfo.gameObject.SetActive(true);
        bgTypeInfo.text = typeInfoText;

        if (allBackgrounds[realID].supportsColor)
        {
            bgHinweis.SetActive(true);
        }
        else
        {
            bgHinweis.SetActive(false);
        }

        bgPreview.transform.GetChild(0).gameObject.SetActive(true);
        bgPreview.transform.GetChild(0).GetComponent<Image>().sprite =
            allBackgrounds[realID].cover;

        if(purchased)
        {
            buyButton.GetComponent<Image>().color = Color.white;
            buyButton.GetComponent<Button>().interactable = true;
            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Wählen";
        } else
        {
            bool price = false;

            if((ulong)allBackgrounds[realID].cost > blus)
            { //zu teuer
                price = true;
            }

            if (price)
            {
                buyButton.GetComponent<Button>().interactable = false;
                buyButton.GetComponent<Image>().color = Color.red;
            }
            else
            {
                buyButton.GetComponent<Button>().interactable = true;
                buyButton.GetComponent<Image>().color = Color.green;
            }
            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Kaufen";
        }
    }

    private void CloseColorSelection()
    {
        pipeColorActive = false;

        colorChangeObj.transform.GetChild(1).gameObject.SetActive(false);

        PlayerPrefs.SetInt("Pipe_Color", pipeColorID);

        buyButton.GetComponent<Button>().interactable = false;
        buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Kaufen";
    }

    public void MinerSlotClicked(int id)
    {
        lastSlotID = id;
        minerType = 0;

        if(allMiners[id].purchased)
        {
            buyButton.GetComponent<Image>().color = Color.white;
            buyButton.GetComponent<Button>().interactable = true;

            DeactivatePurchaseInfo();

            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Wählen";
        } else
        {
            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Kaufen";

            SetMinePrices(allMiners[id].cost);
        }

        for(int i = 0; i < minerSlotParent.childCount; i++)
        {
            if(i == id)
            {
                minerSlotParent.GetChild(i).GetComponent<Image>().color =
                    new Color32(255, 253, 0, 255);
            } else
            {
                minerSlotParent.GetChild(i).GetComponent<Image>().color =
                    Color.white;
            }
        }
    }

    private void DeactivatePurchaseInfo()
    {
        for(int i = 0; i < 4; i++)
        {
            mineModePriceText.transform.GetChild(i).gameObject.SetActive(false);
            mineModePriceImage.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public int GetNextMiner()
    {
        int latestMiner = GetLatestMiner();

        latestMiner = Mathf.Clamp(latestMiner + 1, 0, allMiners.Count - 1);

        return latestMiner;
    }

    public int GetNextHeatShield()
    {
        int latestShield = GetLatestHeatShield();

        latestShield = Mathf.Clamp(latestShield + 1, 0, allHeatShields.Count - 1);

        return latestShield;
    }

    public bool CanAfford(CostData[] prices)
    {
        bool canAfford = true;

        for (int i = 0; i < 4; i++)
        {
            if (prices[i].amount > 0)
            {
                int collectedAmount =
                    Inventory.Instance.GetMineralAmount((int)prices[i].mineralID);

                collectedAmount = Mathf.Clamp(collectedAmount, 0, prices[i].amount);

                if (collectedAmount < prices[i].amount)
                {
                    canAfford = false;
                }
            }
        }

        return canAfford;
    }

    private void SetMinePrices(CostData[] prices)
    {
        bool canAfford = true;

        for(int i = 0; i < 4; i++)
        {
            if(prices[i].amount > 0)
            {
                int collectedAmount = 
                    Inventory.Instance.GetMineralAmount((int)prices[i].mineralID);

                collectedAmount = Mathf.Clamp(collectedAmount, 0, prices[i].amount);

                if(collectedAmount < prices[i].amount)
                {
                    canAfford = false;
                }

                mineModePriceText.transform.GetChild(i).gameObject.SetActive(true);
                mineModePriceText.transform.GetChild(i).GetComponent<TextMeshProUGUI>().text =
                    collectedAmount + "/" + prices[i].amount.ToString();

                mineModePriceImage.transform.GetChild(i).gameObject.SetActive(true);
                mineModePriceImage.transform.GetChild(i).GetComponent<Image>().sprite =
                    Inventory.Instance.allMinerals[(int)prices[i].mineralID].sprite;
            } else
            {
                mineModePriceText.transform.GetChild(i).gameObject.SetActive(false);
                mineModePriceImage.transform.GetChild(i).gameObject.SetActive(false);
            }
        }


        if (canAfford)
        {
            buyButton.GetComponent<Button>().interactable = true;
            buyButton.GetComponent<Image>().color = Color.green;
        }
        else
        {
            buyButton.GetComponent<Button>().interactable = false;
            buyButton.GetComponent<Image>().color = Color.red;
        }
    }

    public void MinerShieldClicked(int id)
    {
        lastSlotID = id;
        minerType = 1;

        if (allHeatShields[id].purchased)
        {
            buyButton.GetComponent<Image>().color = Color.white;
            buyButton.GetComponent<Button>().interactable = true;

            DeactivatePurchaseInfo();

            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Wählen";
        }
        else
        {
            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Kaufen";

            SetMinePrices(allHeatShields[id].cost);
        }

        for (int i = 0; i < shieldSlotParent.childCount; i++)
        {
            if (i == id)
            {
                shieldSlotParent.GetChild(i).GetComponent<Image>().color =
                    new Color32(255, 253, 0, 255);
            }
            else
            {
                shieldSlotParent.GetChild(i).GetComponent<Image>().color =
                    Color.white;
            }
        }
    }

    public void MineItemClicked(int id)
    {
        lastSlotID = id;
        minerType = 2;

        //buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Kaufen";

        //SetMinePrices(allMineItems[id].cost);
    }

    private void PurchaseMinerItem(CostData[] costData, bool mineItem = false)
    { //updatet mineralien
        int start = 0, end = 4;

        if(mineItem)
        {
            if (MinerCustomizationHandler.buyOption == 0)
            {
                start = 0;
                end = 1;
            } else
            {
                start = 1;
                end = 4;
            }
        }

        for (int i = start; i < end; i++)
        {
            if(costData[i].amount > 0)
            {
                if(costData[i].mineralID == MineralType.Coin)
                { //coins
                    UpdateBlus((ulong)costData[i].amount, -1);
                } else
                {
                    Inventory.Instance.SetMineralAmount((MineralType)costData[i].mineralID,
                        costData[i].amount, 2);
                }
            }
        }
    }

    public void ManualMinerPurchase(int id)
    {
        allMiners[id].purchased = true;

        PurchaseMinerItem(allMiners[id].cost);
        SavePurchasedItems();

        selectedMiner = id;
        PlayerMiner.currentMiner = allMiners[id];
    }

    public void ManualHeatShieldPurchase(int id)
    {
        allHeatShields[id].purchased = true;
        PurchaseMinerItem(allHeatShields[id].cost);
        SavePurchasedItems();

        selectedHeatShield = id;
    }

    public void ManualMineItemPurchase(int id)
    {
        allMineItems[id].amount++;

        CostData[] costData = allMineItems[id].cost;

        PurchaseMinerItem(costData);
        SavePurchasedItems();
    }

    /*public void PurchaseClicked()
    { //kein cost-check nötig da nur aufgerufen wenn genug blus vorhanden
        if(pipeColorActive)
        {
            CloseColorSelection();
            return;
        }

        bool purchased = false; //ob bereits gekauft

        if (currentType == 2)
        { //mining
            switch(minerType)
            {
                case 0: //buy miner
                    if (allMiners[lastSlotID].purchased)
                    {
                        purchased = true;
                    }

                    if (purchased)
                    {
                        selectedMiner = lastSlotID;
                    }
                    else
                    {
                        allMiners[lastSlotID].purchased = true;

                        PurchaseMinerItem(allMiners[lastSlotID].cost);
                        SavePurchasedItems();

                        selectedMiner = lastSlotID;
                    }

                    PlayerMiner.currentMiner = allMiners[selectedMiner];
                    break;
                case 1: //buy shield
                    if (allHeatShields[lastSlotID].purchased)
                    {
                        purchased = true;
                    }

                    if (purchased)
                    {
                        selectedHeatShield = lastSlotID;
                    }
                    else
                    {
                        allHeatShields[lastSlotID].purchased = true;
                        PurchaseMinerItem(allHeatShields[lastSlotID].cost);
                        SavePurchasedItems();

                        selectedHeatShield = lastSlotID;
                    }

                    MineHandler.Instance.currentHeatShield = allHeatShields[lastSlotID];
                    break;
                case 2: //buy mineItem
                    allMineItems[lastSlotID].amount++;

                    /*for (int i = 0; i < mineItemParent.childCount; i++)
                    { //ui amount update
                        mineItemParent.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                            allMineItems[i].sprite;

                        mineItemParent.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text =
                            allMineItems[i].amount.ToString();
                    }

                    CostData[] costData = allMineItems[lastSlotID].cost;

                    PurchaseMinerItem(costData);
                    SavePurchasedItems();

                    //Um erneut zu checken ob genug Geld/Mineralien
                    //da sind
                    //MineItemClicked(lastSlotID);

                    break;
            }

            if(minerType != 2)
            {
                buyButton.GetComponent<Image>().color = Color.white;
                buyButton.GetComponent<Button>().interactable = false;
            }

            return;
        }

        int realID = lastSlotID + (slotParent.childCount * currentPage);

        if(currentType == 4)
        {
            realID = lastSlotID + (bgSlotParent.childCount * currentPage);
        }
        int cost = 0;

        Sprite newObjSprite = null;

        switch(currentType)
        {
            case 0: //skins

                if(allSkins[realID].purchased)
                {
                    purchased = true;
                }

                newObjSprite = allSkins[realID].sprite;

                allSkins[realID].purchased = true;
                cost = allSkins[realID].cost;

                break;
            case 1: //wings

                if (allWings[realID].purchased)
                {
                    purchased = true;
                }

                newObjSprite = allWings[realID].sprite[0];

                allWings[realID].purchased = true;
                cost = allWings[realID].cost;

                break;
            case 2: //miner

                if (allMiners[realID].purchased)
                {
                    purchased = true;
                }

                newObjSprite = allMiners[realID].full;

                allMiners[realID].purchased = true;
                cost = allMiners[realID].cost;

                break;
            case 3: //pipes

                if(allPipes[realID].purchased)
                {
                    purchased = true;
                }

                newObjSprite = allPipes[realID].sprite[0];

                if(!allPipes[realID].colorChangeSupported)
                {
                    pipeColor = allPipes[realID].defaultColor;
                }

                allPipes[realID].purchased = true;
                cost = allPipes[realID].cost;

                break;
            case 4: //backgrounds

                if(allBackgrounds[realID].purchased)
                {
                    purchased = true;
                }

                newObjSprite = allBackgrounds[realID].cover;

                allBackgrounds[realID].purchased = true;
                cost = allBackgrounds[realID].cost;

                break;
        }

        if(currentType != 2)
        {
            buyButton.GetComponent<Image>().color = Color.white;
            buyButton.GetComponent<Button>().interactable = false;
            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Wählen";
        }

        if(!purchased)
        {
            if(cost > 0)
            {
                blus -= (ulong)cost;
            }

            buyInfoObj.transform.GetChild(2).GetComponent<Image>().sprite = newObjSprite;
            buyInfoObj.SetActive(true);
        }

        if(currentType == 0 || currentType == 1)
        {
            if(currentType == 0)
            {
                selectedSkin = realID;
            } else if(currentType == 1)
            {
                selectedWing = realID;
            }

            playerData.LoadPlayerSkin(allSkins[selectedSkin], allWings[selectedWing]);
        }

        if(currentType == 2)
        {
            // selectedMiner = realID;
            // PlayerMiner.currentMiner = allMiners[selectedMiner];
            MinerCustomizationHandler.Instance.UpdateUI();
            MinerCustomizationHandler.Instance.IsAffordable(true);
        }

        if(currentType == 3)
        {
            selectedPipe = realID;
        }

        if(currentType == 4)
        {
            selectedBackground = realID;
            bgHandler.SetNewBackground(allBackgrounds[selectedBackground]);
        }

        SavePurchasedItems();
    }*/

    public void BuyCustom(CustomizationType type, int id)
    {
        CostData[] price;
        switch(type)
        {
            default:
            case CustomizationType.Skin:
                price = allSkins[id].cost;
                allSkins[id].purchased = true;
                break;
            case CustomizationType.Wing:
                price = allWings[id].cost;
                allWings[id].purchased = true;
                break;
            case CustomizationType.Pipe:
                price = allPipes[id].cost;
                allPipes[id].purchased = true;
                break;
            case CustomizationType.Hat:
                price = allHats[id].cost;
                allHats[id].purchased = true;
                break;
        }

        PurchaseMinerItem(price);

        ApplyCustom(type, id);
        SavePurchasedItems();
    }

    public void ApplyCustom(CustomizationType type, int id, bool reload = true)
    {
        Debug.Log("Apply: " + type + " " + id + " " + reload);

        if(HasPurchased(type, id) == 2)
        { //selected nur ändern wenn gekauft
            switch (type)
            {
                case CustomizationType.Skin:
                    selectedSkin = id;
                    break;
                case CustomizationType.Wing:
                    selectedWing = id;
                    break;
                case CustomizationType.Pipe:
                    selectedPipe = id;
                    break;
                case CustomizationType.Hat:
                    selectedHat = id;
                    break;
                case CustomizationType.PipeColor:
                    pipeColorID = id;
                    break;
            }
        }

        SavePurchasedItems();

        if (playerData != null && reload)
        {
            switch(type)
            {
                case CustomizationType.Skin:
                case CustomizationType.Wing:
                    playerData.LoadPlayerSkin(allSkins[selectedSkin], allWings[selectedWing]);
                    break;
                case CustomizationType.Hat:
                    playerData.LoadHat(allHats[selectedHat]);
                    break;
                case CustomizationType.Pipe:
                    FF_PlayerData.Instance.LoadPipe();
                    //ObjectPooler.Instance.RecreatePool("DestroyedPipePart");
                    break;
            }
        }
    }

    public bool GetMineItem(int id)
    { //check ob mine item noch da ist
        bool ok = false;

        if(allMineItems[id].amount > 0)
        {
            allMineItems[id].amount--;
            SavePurchasedItems();
            UpdateMineItemUI();
            ok = true;
        }

        return ok;
    }

    public void UpdateMineItemUI()
    {
        for(int i = 0; i < minigItemButtonParent.childCount; i++)
        {
            minigItemButtonParent.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                allMineItems[i].amount.ToString(); //bg
            minigItemButtonParent.GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                allMineItems[i].amount.ToString(); //normal
        }
    }

    private void HandleWingAnimation()
    {
        //if (currentType != 1) return;

        if(wingAnimationDir == 0)
        {
            wingAnimationCount++;
            if (wingAnimationCount > 2)
            {
                wingAnimationCount = 1;
                wingAnimationDir = 1;
            }
        } else
        {
            wingAnimationCount--;
            if (wingAnimationCount < 0)
            {
                wingAnimationCount = 1;
                wingAnimationDir = 0;
            }
        }

        if(shopActive && currentType == 1)
        {
            for (int i = 0; i < allWings.Count; i++)
            {
                if (allWings[i].sprite.Length > 1)
                {
                    slotParent.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                        allWings[i].sprite[wingAnimationCount];
                }
            }
        } else
        {
            playerData.SetWingAnimation(wingAnimationCount);
        }
    }

    public Pipe GetCurrentPipe()
    {
        if (!allPipes[selectedPipe].colorChangeSupported)
        {
            pipeColor = allPipes[selectedPipe].defaultColor;
        }

        return allPipes[selectedPipe];
    }

    public Background GetCurrentBackground()
    {
        return allBackgrounds[selectedBackground];
    }

    // Update is called once per frame
    void Update()
    {
        if(shopActive)
        {
            switch (currentType)
            {
                case 0:
                    for (int i = 0; i < allSkins.Count; i++)
                    {
                        if (allSkins[i].animated == 1)
                        { //farben fade
                            slotParent.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = fadeMat.color;
                        }
                    }
                    break;
            }

            if(Input.GetKeyDown(KeyCode.Escape) && !closing)
            {
                CloseShop();
            }
        }

        playerData.SetColor(fadeMat.color);
    }
}
