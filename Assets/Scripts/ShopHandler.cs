using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;
using DG.Tweening;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using System;
using Random = UnityEngine.Random;
using Object = System.Object;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;
using Firebase.Analytics;

public enum Rarity
{
    Casual, //Grau
    Uncommon, //Grün
    Rare, //Blau
    Epic, //Lila
    Mystic //Gold
}

public class ShopHandler : MonoBehaviour
{
    [SerializeField]
    private ShopMenuHandler shopMenuHandler = null;
    [SerializeField]
    private CustomizationHandler customizationHandler = null;
    [SerializeField]
    private PipeCustomizationHandler pipeCustomizationHandler = null;
    [SerializeField]
    private GraveCustomizationHandler graveCustomizationHandler = null;
    [SerializeField]
    private RemoteConfigHandler remoteConfigHandler = null;

    public Canvas windowCanvas;
    public GameObject shopObj, eventSystem;
    public Vector3 startPos, defaultPos;
    private bool closing = false, pipeColorActive = false, opening = false;

    public static float moveTime = 0.25f;

    public GameObject buyButton, blusText, bgHinweis, bgParent, bgPreview, buyInfoObj,
        mineModePriceText, mineModePriceImage, blusEffect;
    public TextMeshProUGUI typeInfoText, bgType, bgTypeInfo, bgPreis;

    public List<Skin> allSkins = new List<Skin>();
    public List<Skin> allFilterSkins = new List<Skin>();
    public Skin errorSkin;

    public List<Wing> allWings = new List<Wing>();
    public Wing errorWing;

    public List<Tail> allTails = new List<Tail>();

    public List<Hat> allHats = new List<Hat>();
    public Hat errorHat;

    public List<MinerTool> allMiners = new List<MinerTool>();
    public List<HeatShield> allHeatShields = new List<HeatShield>();
    public List<FuelTank> allFuelTanks = new List<FuelTank>();
    public List<MineItem> allMineItems = new List<MineItem>();

    public List<Pipe> allPipes = new List<Pipe>();
    public Pipe errorPipe;

    public List<Background> allBackgrounds = new List<Background>();

    public List<GraveTop> allGraveTops = new List<GraveTop>();
    public List<GraveSide> allGraveSides = new List<GraveSide>();
    public List<GraveBottom> allGraveBottoms = new List<GraveBottom>();

    public Transform slotParent, bgSlotParent, colorSlotParent, minerSlotParent, shieldSlotParent, mineItemParent,
        minerParent, minigItemButtonParent;
    public Material fadeMat;

    public Color32 pipeColor = new Color32(238, 77, 46, 255);
    public Color[] rarityColors;

    private int pipeColorID = 0;

    private Tween coinEffectPosition = null, coinEffectScale = null, coinColorTween = null;

    public FF_PlayerData playerData;
    public BackgroundHandler bgHandler;

    public LocalizedString buy, select, bought, sale;
    public string buyString, selectString, boughtString, saleString;

    public static ShopHandler Instance;

#if UNITY_EDITOR
    private ObscuredULong blus = 100;
#else
    private ObscuredULong blus = 0;
#endif

    private Vector3 originalBlusTextPos;
    private CoroutineHandle uiScaleHandle;

    private int currentPage = 0, currentType = 0, wingAnimationCount = 0, wingAnimationDir = 0;
    private int colorFadeID = 0, lastSlotID = -1, minerType = 0, selectedSkin = 0, selectedWing = 0, selectedTail = 0,
        selectedMiner = 0, selectedPipe = 0, selectedBackground = 0, selectedHeatShield = 0, selectedFuelTank = 0, selectedHat = 0,
        selectedGraveTop = 0, selectedGraveSide = 0, selectedGraveBottom = 0;
    private bool shopActive = false;

    private void Awake()
    {
        Instance = this;

        allFilterSkins = allSkins;
    }

    private void SetupID()
    {
        for(int i = 0; i < allGraveTops.Count; i++)
        {
            allGraveTops[i].itemID = i;
        }

        for(int i = 0; i < allGraveSides.Count; i++)
        {
            allGraveSides[i].itemID = i;
        }

        for(int i = 0; i < allGraveBottoms.Count; i++)
        {
            allGraveBottoms[i].itemID = i;
        }

        for (int i = 0; i < allSkins.Count; i++)
        {
            allSkins[i].itemID = i;
        }

        for (int i = 0; i < allWings.Count; i++)
        {
            allWings[i].itemID = i;
        }

        for (int i = 0; i < allHats.Count; i++)
        {
            allHats[i].itemID = i;
        }

        for(int i = 0; i < allPipes.Count; i++)
        {
            allPipes[i].itemID = i;
        }
    }

    private void SortShopItems()
    {
        List<ShopItem> shopItems;

        shopItems = allSkins.Cast<ShopItem>().ToList();

        List<ShopItem> allPurchased = new List<ShopItem>();
        List<ShopItem> result = new List<ShopItem>();

        //Alle gekauften herausfiltern

        for (int i = 0; i < shopItems.Count; i++)
        {
            if (shopItems[i].purchased)
            {
                allPurchased.Add(shopItems[i]);
            }
        }

        //Die gekauften sortieren

        int maxRarity = Enum.GetValues(typeof(Rarity)).Cast<int>().Max() + 1;

        for (int i = 0; i < maxRarity; i++)
        {
            for (int a = 0; a < allPurchased.Count; a++)
            {
                if (allPurchased[a].rarity == (Rarity)i)
                {
                    result.Add(allPurchased[a]);
                }
            }
        }

        //Jetzt nicht gekaufte nach rarity sortieren

        for (int i = 0; i < maxRarity; i++)
        {
            List<ShopItem> rarityItems = new List<ShopItem>();

            for (int a = 0; a < shopItems.Count; a++)
            {
                if(!shopItems[a].purchased && shopItems[a].rarity == (Rarity)i)
                {
                    rarityItems.Add(shopItems[a]);

                    //result.Add(shopItems[a]);
                }
            }

            //Nach Preis sortieren
            List<ShopItem> final = rarityItems.OrderBy(o => o.cost[0].amount).ToList();

            for (int a = 0; a < final.Count; a++)
            {
                result.Add(final[a]);
            }
        }

        //Sortierung fertig, result zuweisen

        allSkins = result.Cast<Skin>().ToList();
        allFilterSkins = allSkins;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupID();

        LoadPurchasedItems();
        TypeClicked(0, true);

        //pipeColor =
        //    colorSlotParent.transform.GetChild(pipeColorID).GetChild(0).GetComponent<Image>().color;

        fadeMat.color = Color.red;
        fadeMat.DOColor(Color.blue, 0.5f).SetEase(Ease.Linear);

        Timing.RunCoroutine(Util._EmulateUpdate(_MainUpdate, this));
        Timing.RunCoroutine(_HandleWingAnimation(0.25f));

    }

    public void CompleteLoad()
    {
        //Debug.Log("start");
        //InvokeRepeating(nameof(NextColorStep), 0.51f, 0.251f);

    }

    public void StartLoadLocalization()
    {
        StartCoroutine(LoadLocalization());
    }

    private IEnumerator LoadLocalization()
    {
        AsyncOperationHandle handle;

        yield return handle = buy.GetLocalizedString();
        buyString = (string)handle.Result;

        yield return handle = select.GetLocalizedString();
        selectString = (string)handle.Result;

        yield return handle = bought.GetLocalizedString();
        boughtString = (string)handle.Result;

        yield return handle = sale.GetLocalizedString();
        saleString = (string)handle.Result;

        for(int i = 0; i < allHats.Count; i++)
        {
            if(allHats[i].special)
            {
                yield return handle = allHats[i].unlockLocale.GetLocalizedString();

                allHats[i].unlockString = (string)handle.Result;
            }
        }
    }

    public Color32 GetRarity(CustomizationType type, int id, byte alpha = 255)
    {
        Color32 c;

        switch(type)
        {
            default:
                c = rarityColors[(int)allSkins[id].rarity];
                break;
            case CustomizationType.Wing:
                c = rarityColors[(int)allWings[id].rarity];
                break;
            case CustomizationType.Hat:
                c = rarityColors[(int)allHats[id].rarity];
                break;
            case CustomizationType.Pipe:
                c = rarityColors[(int)allPipes[id].rarity];
                break;
        }

        c.a = alpha;

        return c;
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
            return allPipes[pipeID].endSprite;
        }

        return allPipes[pipeID].sprite;
    }

    public ShopItem GetItem(CustomizationType type, int id)
    {
        List<ShopItem> allItems = allSkins.Cast<ShopItem>().ToList();

        switch (type)
        {
            case CustomizationType.Wing:
                allItems = allWings.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Hat:
                allItems = allHats.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Pipe:
                allItems = allPipes.Cast<ShopItem>().ToList();
                break;
        }

        return allItems[id];
    }

    public ShopItem GetItemByString(CustomizationType type, string identifier)
    {
        List<ShopItem> allItems = allSkins.Cast<ShopItem>().ToList();

        switch(type)
        {
            case CustomizationType.Wing:
                allItems = allWings.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Hat:
                allItems = allHats.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Pipe:
                allItems = allPipes.Cast<ShopItem>().ToList();
                break;
        }

        for(int i = 0; i < allItems.Count; i++)
        {
            if(allItems[i].identifier.Equals(identifier))
            {
                return allItems[i];
            }
        }

        //Wenn wir hier ankommen wurde Obj nicht gefunden
        //  --> errorObj

        switch(type)
        {
            default: //skin
                return (ShopItem)errorSkin;
            case CustomizationType.Wing:
                return (ShopItem)errorWing;
            case CustomizationType.Hat:
                return (ShopItem)errorHat;
            case CustomizationType.Tail:
                return (ShopItem)allTails[0];
            case CustomizationType.Pipe:
                return (ShopItem)errorPipe;
        }
    }

    public int GetSelected(CustomizationType type)
    {
        int id;

        switch (type)
        {
            default:
            case CustomizationType.Skin:
                id = selectedSkin;
                break;
            case CustomizationType.Wing:
                id = selectedWing;
                break;
            case CustomizationType.Hat:
                id = selectedHat;
                break;
            case CustomizationType.Tail:
                id = selectedTail;
                break;
            case CustomizationType.Pipe:
                id = selectedPipe;
                break;
            case CustomizationType.PipeColor:
                id = pipeColorID;
                break;
            case CustomizationType.GraveTop:
                id = selectedGraveTop;
                break;
            case CustomizationType.GraveSide:
                id = selectedGraveSide;
                break;
            case CustomizationType.GraveBottom:
                id = selectedGraveBottom;
                break;
        }

        return id;
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
            default:
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
            case CustomizationType.Tail:
                if (id < 0) id = allTails.Count + id;
                if (id >= allTails.Count) id = id - allTails.Count;
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
            case CustomizationType.Tail:
                max = allTails.Count;
                break;
        }

        return max;
    }

    public int IsAffordable(CostData[] cost)
    {
        int ok = 1;

        for(int i = 0; i < cost.Length; i++)
        {
            if(cost[i].amount > 0)
            {
                ulong collectedAmount =
                    (ulong)Inventory.Instance.GetMineralAmount((int)cost[i].mineralID);

                if (cost[i].mineralID == MineralType.Coin)
                {
                    collectedAmount = GetBlus();
                } else if(cost[i].mineralID == MineralType.Special)
                {
                    ok = -1;
                    break;
                }

                if (collectedAmount < (ulong)cost[i].amount && ok != -1)
                {
                    ok = 0;
                    break;
                }
            }
        }

        return ok;
    }

    public Sprite GetSprite(CustomizationType type, int id)
    {
        id = CheckSelected(type, id);

        Sprite s = null;

        switch(type)
        {
            case CustomizationType.Skin:
                s = allSkins[id].sprite;
                break;
            case CustomizationType.Wing:
                s = allWings[id].sprite[0];
                break;
            case CustomizationType.Hat:
                s = allHats[id].sprite;
                break;
            case CustomizationType.Tail:
                s = allTails[id].sprite;
                break;
            case CustomizationType.Pipe:
                s = allPipes[id].endSprite;
                break;
            case CustomizationType.GraveTop:
                s = allGraveTops[id].sprite;
                break;
            case CustomizationType.GraveSide:
                s = allGraveSides[id].sprite;
                break;
            case CustomizationType.GraveBottom:
                s = allGraveBottoms[id].sprite;
                break;
        }

        return s;
    }

    public CostData[] GetCost(CustomizationType type, int id)
    {
        CostData[] cost = null;

        switch (type)
        {
            default:
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
            case CustomizationType.Tail:
                cost = allTails[id].cost;
                break;
            case CustomizationType.GraveTop:
                cost = allGraveTops[id].cost;
                break;
            case CustomizationType.GraveSide:
                cost = allGraveSides[id].cost;
                break;
            case CustomizationType.GraveBottom:
                cost = allGraveBottoms[id].cost;
                break;
            case CustomizationType.MinerTool:
                cost = allMiners[id].cost;
                break;
            case CustomizationType.MinerHeatShield:
                cost = allHeatShields[id].cost;
                break;
            case CustomizationType.MinerFuelTank:
                cost = allFuelTanks[id].cost;
                break;
            case CustomizationType.MinerItem:
                cost = allMineItems[id].cost;
                break;
        }

        return cost;
    }

    public int HasPurchased(CustomizationType type, int id)
    {
        /*  RETURN TYPES
         * -1 - Special Item - Nicht kaufbar
         *  0 - Nicht gekauft & nicht genug Geld
         *  1 - Nicht gekauft & genug Geld
         *  2 - Gekauft
         */

        int code = 0;

        if(type == CustomizationType.PipeColor)
        {
            return 2;
        }

        List<ShopItem> shopItems = null;

        switch(type)
        {
            case CustomizationType.Skin:
                shopItems = allSkins.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Wing:
                shopItems = allWings.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Pipe:
                shopItems = allPipes.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Hat:
                shopItems = allHats.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Tail:
                shopItems = allTails.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.GraveTop:
                shopItems = allGraveTops.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.GraveSide:
                shopItems = allGraveSides.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.GraveBottom:
                shopItems = allGraveBottoms.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.MinerTool:
                shopItems = allMiners.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.MinerHeatShield:
                shopItems = allHeatShields.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.MinerFuelTank:
                shopItems = allFuelTanks.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.MinerItem:
                shopItems = allMineItems.Cast<ShopItem>().ToList();
                break;

        }

        if(shopItems[id].purchased)
        {
            code = 2;
        } else
        {
            code = IsAffordable(shopItems[id].cost);
        }

        return code;
    }

    public float GetScale(CustomizationType type, int id)
    {
        float scale;

        switch(type)
        {
            default:
            case CustomizationType.MinerTool:
                scale = allMiners[id].shopScale;
                break;
            case CustomizationType.MinerHeatShield:
                scale = allHeatShields[id].shopScale;
                break;
            case CustomizationType.MinerFuelTank:
                scale = allFuelTanks[id].shopScale;
                break;
        }

        return scale;
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
            case CustomizationType.Tail:
                selectedTail = id;
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
                FlatterFogelHandler.Instance.AddRoundCoin();
                break;
        }

        ObscuredPrefs.SetULong("Blus", blus);

        blusText.GetComponent<TextMeshProUGUI>().text = blus.ToString();
        blusText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = blus.ToString();
    }

    public void CoinAddEffect()
    {
        UpdateBlus(1, 1, true);

        blusEffect.SetActive(false);
        blusEffect.SetActive(true);

        if (coinEffectPosition != null)
        {
            if (coinEffectPosition.active)
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

        if (coinColorTween != null)
        {
            if (coinColorTween.active)
            {
                coinColorTween.Kill();
            }
        }

        Vector3 bottomPos = originalBlusTextPos;
        bottomPos.y -= 36.7f;
        bottomPos.x += 41;

        blusText.transform.parent.position = bottomPos;//new Vector3(-669.3f, 1359.3f, 100);
        blusText.transform.parent.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        blusText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

        coinEffectPosition = //zurück zu original Pos
            blusText.transform.parent.DOMove(originalBlusTextPos, 0.5f); //new Vector3(-710.3f, 1396, 100)

        coinEffectScale =
            blusText.transform.parent.DOScale(1, 0.5f);

        coinColorTween =
            blusText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOColor(new Color32(233, 175, 0, 255), 0.5f);
    }

    public void UIScaleFinished()
    {
        Timing.KillCoroutines(uiScaleHandle);
        uiScaleHandle = Timing.RunCoroutine(_WaitUIScale());
    }

    private IEnumerator<float> _WaitUIScale()
    {
        yield return Timing.WaitForOneFrame;

        originalBlusTextPos = blusText.transform.parent.position;

        //Debug.Log(originalBlusTextPos);
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
        for(int i = 1; i < allGraveTops.Count; i++)
        {
            allGraveTops[i].purchased = false;
        }

        for(int i = 1; i < allGraveSides.Count; i++)
        {
            allGraveSides[i].purchased = false;
        }

        for(int i = 1; i < allGraveBottoms.Count; i++)
        {
            allGraveBottoms[i].purchased = false;
        }

        for (int i = 0; i < allSkins.Count; i++)
        {
            if(i > 0)
            {
                allSkins[i].purchased = false;
            }

            allSkins[i].salePercent = 0;

            allSkins[i].boughtWings = new int[1];
            allSkins[i].boughtWings[0] = 0;

            allSkins[i].boughtHats = new int[1];
            allSkins[i].boughtHats[0] = 0;
        }

        for (int i = 1; i < allWings.Count; i++)
        {
            allWings[i].salePercent = 0;

            allWings[i].purchased = false;
        }

        for(int i = 1; i < allTails.Count; i++)
        {
            allTails[i].salePercent = 0;

            allTails[i].purchased = false;
        }

        for (int i = 1; i < allPipes.Count; i++)
        {
            allPipes[i].salePercent = 0;

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

        for(int i = 1; i < allFuelTanks.Count; i++)
        {
            allFuelTanks[i].purchased = false;
        }

        for(int i = 0; i < allMineItems.Count; i++)
        {
            allMineItems[i].amount = 0;
        }

        for(int i = 1; i < allHats.Count; i++)
        {
            allHats[i].salePercent = 0;

            allHats[i].purchased = false;
        }
    }

    public bool HasSkinPurchasedWing(int skinID, int wingID)
    {
        bool ok = false;

        if(allSkins[skinID].boughtWings.Contains(wingID))
        {
            ok = true;
        }

        return ok;
    }

    private void LoadPurchasedItems()
    {
#if UNITY_EDITOR
        UpdateBlus(200, 0);
#else
        UpdateBlus(ObscuredPrefs.GetULong("Blus", 0));
#endif

        string selectedSkinString = ObscuredPrefs.GetString("SelectedSkinString", "original");
        string selectedWingString = ObscuredPrefs.GetString("SelectedWingString", "default");
        string selectedHatString = ObscuredPrefs.GetString("SelectedHatString", "default");
        string selectedPipeString = ObscuredPrefs.GetString("SelectedPipeString", "default");
        string selectedTailString = ObscuredPrefs.GetString("SelectedTailString", "default");

        string selectedGraveTopString = ObscuredPrefs.GetString("SelectedGraveTopString", "default");
        string selectedGraveSideString = ObscuredPrefs.GetString("SelectedGraveSideString", "default");
        string selectedGraveBottomString = ObscuredPrefs.GetString("SelectedGraveBottomString", "default");

        ResetAll();

        string data = ObscuredPrefs.GetString("ShopPurchasedGraveTops", "");

        if(data.Length > 0)
        {
            if(data.Contains(","))
            {
                string[] split = data.Split(',');
                for(int i = 0; i < split.Length; i++)
                {
                    if(split[i].Length > 0)
                    {
                        string identifier = split[i];

                        for(int a = 0; a < allGraveTops.Count; a++)
                        {
                            if(allGraveTops[a].identifier.Equals(identifier))
                            {
                                allGraveTops[a].purchased = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        data = ObscuredPrefs.GetString("ShopPurchasedGraveSides", "");

        if (data.Length > 0)
        {
            if (data.Contains(","))
            {
                string[] split = data.Split(',');
                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i].Length > 0)
                    {
                        string identifier = split[i];

                        for (int a = 0; a < allGraveSides.Count; a++)
                        {
                            if (allGraveSides[a].identifier.Equals(identifier))
                            {
                                allGraveSides[a].purchased = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        data = ObscuredPrefs.GetString("ShopPurchasedGraveBottoms", "");

        if (data.Length > 0)
        {
            if (data.Contains(","))
            {
                string[] split = data.Split(',');
                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i].Length > 0)
                    {
                        string identifier = split[i];

                        for (int a = 0; a < allGraveBottoms.Count; a++)
                        {
                            if (allGraveBottoms[a].identifier.Equals(identifier))
                            {
                                allGraveBottoms[a].purchased = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        data = ObscuredPrefs.GetString("ShopPurchasedSkins", "");

        if (data.Length > 0)
        {
            string types = data;
            if (types.Contains(","))
            { //gekaufte skins vorhanden -> einlesen
                string[] skinData = types.Split(',');
                for (int i = 0; i < skinData.Length; i++)
                {
                    if (skinData[i].Length > 0)
                    {
                        string[] type = skinData[i].Split('#');
                        //0 = skin identifier
                        //1 = gekaufte wings
                        //2 = gekaufte hats
                        //3 = letzter Hut
                        //4 = gekaufte tails
                        //5 = letzter tail

                        if (type.Length > 0)
                        {
                            string identifier = type[0];

                            string[] pWings = type[1].Split('~');
                            int[] purchasedWings = new int[pWings.Length - 1];

                            for (int a = 0; a < pWings.Length - 1; a++) //-1 weil letzte stelle leer
                            {
                                string wingIdentifier = pWings[a];

                                for (int b = 0; b < allWings.Count; b++)
                                {
                                    if (allWings[b].identifier.Equals(wingIdentifier))
                                    { //position in array gefunden -> zuweisen
                                        purchasedWings[a] = b;
                                        break;
                                    }
                                }
                            }

                            string[] pHats = type[2].Split('~');
                            int[] purchasedHats = new int[pHats.Length - 1];

                            for (int a = 0; a < pHats.Length - 1; a++) //-1 weil letzte stelle leer
                            {
                                string hatIdentifier = pHats[a];

                                for (int b = 0; b < allHats.Count; b++)
                                {
                                    if (allHats[b].identifier.Equals(hatIdentifier))
                                    {
                                        purchasedHats[a] = b;
                                        break;
                                    }
                                }
                            }

                            int lastHatID = 0;

                            if(type.Length > 3)
                            { //letzter ausgewählter hut gesetzt
                                string lastHatIdentifier = type[3].ToString();

                                for(int a = 0; a < allHats.Count; a++)
                                {
                                    if(allHats[a].identifier.Equals(lastHatIdentifier))
                                    {
                                        if(purchasedHats.Contains(a))
                                        { //nur zuweisen wenn auch gekauft
                                            lastHatID = a;
                                        }

                                        break;
                                    }
                                }
                            }

                            int[] purchasedTails = new int[1];
                            int lastTailID = 0;

                            if (type.Length > 4)
                            { //hat gekaufte tails
                                string[] pTails = type[4].Split('~');
                                purchasedTails = new int[pTails.Length - 1];

                                for (int a = 0; a < pTails.Length - 1; a++) //-1 weil letzte stelle leer
                                {
                                    string tailIdentifier = pTails[a];

                                    for (int b = 0; b < allTails.Count; b++)
                                    {
                                        if (allTails[b].identifier.Equals(tailIdentifier))
                                        {
                                            purchasedTails[a] = b;
                                            break;
                                        }
                                    }
                                }

                                if (type.Length > 5)
                                { //hat tail ausgewählt
                                    string lastTailIdentifier = type[5].ToString();

                                    for (int a = 0; a < allTails.Count; a++)
                                    {
                                        if (allTails[a].identifier.Equals(lastTailIdentifier))
                                        {
                                            if (purchasedTails.Contains(a))
                                            { //nur zuweisen wenn auch gekauft
                                                lastTailID = a;
                                            }

                                            break;
                                        }
                                    }
                                }
                            }

                            for (int a = 0; a < allSkins.Count; a++)
                            {
                                if (allSkins[a].identifier.Equals(identifier))
                                { //gekaufter skin gefunden
                                    allSkins[a].purchased = true;
                                    allSkins[a].boughtWings = purchasedWings;
                                    allSkins[a].boughtHats = purchasedHats;
                                    allSkins[a].lastHat = lastHatID;
                                    allSkins[a].boughtTails = purchasedTails;
                                    allSkins[a].lastTail = lastTailID;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        data = ObscuredPrefs.GetString("ShopPurchasedPipes", "");

        if (data.Length > 0)
        { //Pipes
            string types = data;
            if (types.Contains(","))
            {
                string[] pipeData = types.Split(',');
                for (int i = 0; i < pipeData.Length; i++)
                {
                    if (pipeData[i].Length > 0)
                    {
                        int id = Int32.Parse(pipeData[i]);
                        allPipes[id].purchased = true;
                    }
                }

            }
        }

        data = ObscuredPrefs.GetString("ShopPurchasedMinerTools", "");

        if(data.Length > 0)
        {
            string[] minerToolData = data.Split(',');

            for(int i = 0; i < minerToolData.Length; i++)
            {
                if(minerToolData[i].Length > 0)
                {
                    int id = Int32.Parse(minerToolData[i]);
                    allMiners[id].purchased = true;
                }
            }
        }

        data = ObscuredPrefs.GetString("ShopPurchasedMinerShields", "");

        if(data.Length > 0)
        {
            string[] minerShieldData = data.Split(',');

            for(int i = 0; i < minerShieldData.Length; i++)
            {
                if(minerShieldData[i].Length > 0)
                {
                    int id = Int32.Parse(minerShieldData[i]);
                    allHeatShields[id].purchased = true;
                }
            }
        }

        data = ObscuredPrefs.GetString("ShopPurchasedMinerTanks", "");

        if(data.Length > 0)
        {
            string[] minerTankData = data.Split(',');

            for(int i = 0; i < minerTankData.Length; i++)
            {
                if (minerTankData[i].Length > 0)
                {
                    int id = Int32.Parse(minerTankData[i]);
                    allFuelTanks[id].purchased = true;
                }
            }
        }

        data = ObscuredPrefs.GetString("ShopPurchasedMineItems", "");

        if(data.Length > 0)
        {
            string[] mineItemData = data.Split(',');

            for(int i = 0; i < mineItemData.Length; i++)
            {
                if(mineItemData[i].Length > 0)
                {
                    int amount = Int32.Parse(mineItemData[i]);
                    allMineItems[i].amount = amount;
                }
            }
        }

        /*if(types[4].Contains(","))
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
        }*/

        /*if(types.Length > 8)
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
        }*/

        SortShopItems();

        selectedGraveTop = 0;

        for(int i = 0; i < allGraveTops.Count; i++)
        {
            if(allGraveTops[i].identifier.Equals(selectedGraveTopString))
            {
                selectedGraveTop = i;
                break;
            }
        }

        selectedGraveSide = 0;

        for (int i = 0; i < allGraveSides.Count; i++)
        {
            if (allGraveSides[i].identifier.Equals(selectedGraveSideString))
            {
                selectedGraveSide = i;
                break;
            }
        }

        selectedGraveBottom = 0;

        for (int i = 0; i < allGraveBottoms.Count; i++)
        {
            if (allGraveBottoms[i].identifier.Equals(selectedGraveBottomString))
            {
                selectedGraveBottom = i;
                break;
            }
        }

        selectedSkin = 0;

        for(int i = 0; i < allSkins.Count; i++)
        {
            if(allSkins[i].identifier.Equals(selectedSkinString))
            {
                selectedSkin = i;
                break;
            }
        }

        selectedWing = 0;

        for(int i = 0; i < allWings.Count; i++)
        {
            if(allWings[i].identifier.Equals(selectedWingString))
            {
                selectedWing = i;
                break;
            }
        }

        selectedHat = 0;

        for(int i = 0; i < allHats.Count; i++)
        {
            if(allHats[i].identifier.Equals(selectedHatString))
            {
                selectedHat = i;
                break;
            }
        }

        selectedTail = 0;

        for (int i = 0; i < allTails.Count; i++)
        {
            if (allTails[i].identifier.Equals(selectedTailString))
            {
                selectedTail = i;
                break;
            }
        }

        selectedPipe = 0;

        for(int i = 0; i < allPipes.Count; i++)
        {
            if(allPipes[i].identifier.Equals(selectedPipeString))
            {
                selectedPipe = i;
                break;
            }
        }

        selectedMiner = ObscuredPrefs.GetInt("SelectedMiner", 0);

        pipeColorID = ObscuredPrefs.GetInt("SelectedPipeColorID", 1);

        pipeColor = pipeCustomizationHandler.GetPipeColor(pipeColorID); //Pipe Color laden
        FF_PlayerData.Instance.LoadPipe();

        selectedBackground = 0; //ObscuredPrefs.GetInt("SelectedBackground", 0); OVERRIDE
        selectedHeatShield = ObscuredPrefs.GetInt("SelectedHeatShield", 0);
        selectedFuelTank = ObscuredPrefs.GetInt("SelectedFuelTank", 0);

        MineHandler.Instance.currentHeatShield = allHeatShields[selectedHeatShield];

        PlayerMiner.currentMiner = allMiners[selectedMiner];
        FF_PlayerData.Instance.SetMaxFuel(allFuelTanks[selectedFuelTank].capacity);

        playerData.LoadPlayerSkin(allSkins[selectedSkin], allWings[selectedWing]);
        playerData.LoadHat(allHats[selectedHat]);
    }

    public GraveTop GetGraveTop(string identifier)
    {
        for(int i = 0; i < allGraveTops.Count; i++)
        {
            if(allGraveTops[i].identifier.Equals(identifier))
            {
                return allGraveTops[i];
            }
        }

        Debug.LogWarning("Wrong GraveTop ident: " + identifier);
        return allGraveTops[0];
    }

    public GraveSide GetGraveSide(string identifier)
    {
        for (int i = 0; i < allGraveSides.Count; i++)
        {
            if (allGraveSides[i].identifier.Equals(identifier))
            {
                return allGraveSides[i];
            }
        }

        Debug.LogWarning("Wrong GraveSide ident: " + identifier);
        return allGraveSides[0];
    }

    public GraveBottom GetGraveBottom(string identifier)
    {
        for (int i = 0; i < allGraveBottoms.Count; i++)
        {
            if (allGraveBottoms[i].identifier.Equals(identifier))
            {
                return allGraveBottoms[i];
            }
        }

        Debug.LogWarning("Wrong GraveBottom ident: " + identifier);
        return allGraveBottoms[0];
    }

    public void SavePurchasedItems()
    {
        string data = "";

        for(int i = 0; i < allGraveTops.Count; i++)
        {
            if(allGraveTops[i].purchased)
            {
                data += allGraveTops[i].identifier + ",";
            }
        }

        ObscuredPrefs.SetString("ShopPurchasedGraveTops", data);
        data = "";

        for (int i = 0; i < allGraveSides.Count; i++)
        {
            if (allGraveSides[i].purchased)
            {
                data += allGraveSides[i].identifier + ",";
            }
        }

        ObscuredPrefs.SetString("ShopPurchasedGraveSides", data);
        data = "";

        for (int i = 0; i < allGraveBottoms.Count; i++)
        {
            if (allGraveBottoms[i].purchased)
            {
                data += allGraveBottoms[i].identifier + ",";
            }
        }

        ObscuredPrefs.SetString("ShopPurchasedGraveBottoms", data);
        data = "";

        for (int i = 0; i < allSkins.Count; i++)
        {
            if(allSkins[i].purchased)
            {
                int[] purchasedWings = allSkins[i].boughtWings;
                int[] purchasedHats = allSkins[i].boughtHats;
                int[] purchasedTails = allSkins[i].boughtTails;

                string wingString = "", hatString = "", tailString = "",
                    lastHatIdentifier = "default", lastTailIdentifier = "default";

                for(int a = 0; a < purchasedWings.Length; a++)
                {
                    wingString += allWings[purchasedWings[a]].identifier + "~";
                }

                for(int a = 0; a < purchasedHats.Length; a++)
                {
                    hatString += allHats[purchasedHats[a]].identifier + "~";
                }

                lastHatIdentifier = allHats[allSkins[i].lastHat].identifier;

                for(int a = 0; a < purchasedTails.Length; a++)
                {
                    tailString += allTails[purchasedTails[a]].identifier + "~";
                }

                lastTailIdentifier = allTails[allSkins[i].lastTail].identifier;

                data += allSkins[i].identifier + "#" + wingString + "#" + hatString + "#" + lastHatIdentifier +
                    "#" + tailString + "#" + lastTailIdentifier + ",";
            }
        }

        ObscuredPrefs.SetString("ShopPurchasedSkins", data);
        data = "";

        for (int i = 0; i < allPipes.Count; i++)
        {
            if (allPipes[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }

        ObscuredPrefs.SetString("ShopPurchasedPipes", data);
        data = "";

        for (int i = 0; i < allMiners.Count; i++)
        {
            if(allMiners[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }

        ObscuredPrefs.SetString("ShopPurchasedMinerTools", data);
        data = "";

        for(int i = 0; i < allHeatShields.Count; i++)
        {
            if(allHeatShields[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }

        ObscuredPrefs.SetString("ShopPurchasedMinerShields", data);
        data = "";

        for (int i = 0; i < allFuelTanks.Count; i++)
        {
            if (allFuelTanks[i].purchased)
            {
                data += i.ToString() + ",";
            }
        }

        ObscuredPrefs.SetString("ShopPurchasedMinerTanks", data);
        data = "";

        for (int i = 0; i < allMineItems.Count; i++)
        {
            data += allMineItems[i].amount + ",";
        }

        ObscuredPrefs.SetString("ShopPurchasedMineItems", data);
        data = "";

        ObscuredPrefs.SetULong("Blus", blus);
        ObscuredPrefs.SetString("SelectedSkinString", allSkins[selectedSkin].identifier);
        ObscuredPrefs.SetString("SelectedWingString", allWings[selectedWing].identifier);
        ObscuredPrefs.SetString("SelectedPipeString", allPipes[selectedPipe].identifier);
        ObscuredPrefs.SetInt("SelectedBackground", selectedBackground);
        ObscuredPrefs.SetInt("SelectedMiner", selectedMiner);
        ObscuredPrefs.SetInt("SelectedHeatShield", selectedHeatShield);
        ObscuredPrefs.SetString("SelectedHatString", allHats[selectedHat].identifier);
        ObscuredPrefs.SetInt("SelectedPipeColorID", pipeColorID);

        ObscuredPrefs.SetString("SelectedGraveTopString", allGraveTops[selectedGraveTop].identifier);
        ObscuredPrefs.SetString("SelectedGraveSideString", allGraveSides[selectedGraveSide].identifier);
        ObscuredPrefs.SetString("SelectedGraveBottomString", allGraveBottoms[selectedGraveBottom].identifier);
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            //SavePurchasedItems();
        }
    }

    private void OnApplicationQuit()
    {
        //SavePurchasedItems();
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

    public int GetLatestMineObj(CustomizationType type)
    {
        List<ShopItem> list;

        switch(type)
        {
            default:
            case CustomizationType.MinerTool:
                list = allMiners.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.MinerHeatShield:
                list = allHeatShields.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.MinerFuelTank:
                list = allFuelTanks.Cast<ShopItem>().ToList();
                break;
        }

        int id = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].purchased)
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

        bgParent.SetActive(false);
        minerParent.gameObject.SetActive(false);
        slotParent.gameObject.SetActive(false);
        customizationHandler.gameObject.SetActive(false);
        pipeCustomizationHandler.gameObject.SetActive(false);
        graveCustomizationHandler.gameObject.SetActive(false);
        buyButton.gameObject.SetActive(false);

        shopMenuHandler.OpenMenu(true);
        shopObj.transform.position = startPos;
        //shopObj.transform.localScale = new Vector3(0, 0, 0);

        shopObj.transform.DOMove(defaultPos, moveTime).SetEase(Ease.OutBack);
        //shopObj.transform.DOScale(new Vector3(1, 1, 1), moveTime);

        Invoke(nameof(ReactivateEventSystem), moveTime + 0.01f);
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

            FirebaseHandler.SetCurrentScreen("MainMenu", "UnityPlayerActivity");

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
        if(closing)
        {
            return;
        }

        if (pipeColorActive)
        {
            CloseColorSelection();
        }

        SoundManager.Instance.PlaySound(Sound.MenuError);

        eventSystem.SetActive(false);

        shopActive = false;

        closing = true;

        shopObj.transform.DOMove(startPos, moveTime).SetEase(Ease.InBack);
        //shopObj.transform.DOMove(startPos, moveTime);
        //shopObj.transform.DOScale(new Vector3(0, 0, 0), moveTime);

        customizationHandler.CloseCustomization(true);
        pipeCustomizationHandler.CloseCustomization(true);
        graveCustomizationHandler.CloseCustomization(true);

        Invoke(nameof(ReactivateEventSystem), moveTime + 0.01f);
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

        //colorChangeObj.SetActive(false);

        minerParent.gameObject.SetActive(false);

        switch (id)
        {
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
            bgPreis.text = boughtString;
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

        PlayerPrefs.SetInt("Pipe_Color", pipeColorID);

        buyButton.GetComponent<Button>().interactable = false;
        buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Kaufen";
    }

    public int GetNextMineObj(CustomizationType type)
    {
        int latest = GetLatestMineObj(type);
        int max;

        switch(type)
        {
            default:
            case CustomizationType.MinerTool:
                max = allMiners.Count - 1;
                break;
            case CustomizationType.MinerHeatShield:
                max = allHeatShields.Count - 1;
                break;
            case CustomizationType.MinerFuelTank:
                max = allFuelTanks.Count - 1;
                break;
        }

        return Mathf.Clamp(latest + 1, 0, max);
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
                    for(int c = 0; c < costData[i].amount; c++)
                    {
                        FirebaseHandler.LogEvent("CoinSpent");
                        StatHandler.coinSpentCount++;
                    }

                    UpdateBlus((ulong)costData[i].amount, -1);
                } else
                {
                    Inventory.Instance.SetMineralAmount((MineralType)costData[i].mineralID,
                        costData[i].amount, 2);
                }
            }
        }
    }

    public void MiningPurchase(CustomizationType type, int id)
    {
        List<ShopItem> list;

        switch(type)
        {
            default:
            case CustomizationType.MinerTool:
                allMiners[id].purchased = true;
                list = allMiners.Cast<ShopItem>().ToList();

                selectedMiner = id;
                PlayerMiner.currentMiner = allMiners[id];
                break;
            case CustomizationType.MinerHeatShield:
                allHeatShields[id].purchased = true;
                list = allHeatShields.Cast<ShopItem>().ToList();

                selectedHeatShield = id;
                break;
            case CustomizationType.MinerFuelTank:
                allFuelTanks[id].purchased = true;
                list = allFuelTanks.Cast<ShopItem>().ToList();

                selectedFuelTank = id;

                FF_PlayerData.Instance.SetMaxFuel(allFuelTanks[selectedFuelTank].capacity);
                break;
            case CustomizationType.MinerItem:
                allMineItems[id].amount++;
                list = allMineItems.Cast<ShopItem>().ToList();
                break;
        }

        PurchaseMinerItem(list[id].cost);

        SavePurchasedItems();
    }

    public void BuyCustom(CustomizationType type, int id)
    {
        Sprite newSprite;

        CostData[] price;

        List<ShopItem> shopItems = null;

        switch (type)
        {
            case CustomizationType.Skin:
                shopItems = allSkins.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Wing:
                shopItems = allWings.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Pipe:
                shopItems = allPipes.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Hat:
                shopItems = allHats.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.Tail:
                shopItems = allTails.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.GraveTop:
                shopItems = allGraveTops.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.GraveSide:
                shopItems = allGraveSides.Cast<ShopItem>().ToList();
                break;
            case CustomizationType.GraveBottom:
                shopItems = allGraveBottoms.Cast<ShopItem>().ToList();
                break;
        }

        price = shopItems[id].cost;
        newSprite = GetSprite(type, id);

        switch (type)
        {
            default:
            case CustomizationType.Skin:

                if(type != CustomizationType.Skin)
                {
                    Debug.LogError("Default BuyType Called: " + id + " " + type);
                } else
                {
                    allSkins[id].purchased = true;
                }

                break;
            case CustomizationType.Wing:
                allWings[id].purchased = true;

                int[] boughtWings = allSkins[selectedSkin].boughtWings;
                int[] newBoughtWings = new int[boughtWings.Length + 1];

                for(int i = 0; i < boughtWings.Length; i++)
                {
                    newBoughtWings[i] = boughtWings[i];
                }

                newBoughtWings[newBoughtWings.Length - 1] = id;

                allSkins[selectedSkin].boughtWings = newBoughtWings;
                break;
            case CustomizationType.Pipe:
                allPipes[id].purchased = true;
                break;
            case CustomizationType.Hat:
                allHats[id].purchased = true;

                int[] boughtHats = allSkins[selectedSkin].boughtHats;
                int[] newBoughtHats = new int[boughtHats.Length + 1];

                for (int i = 0; i < boughtHats.Length; i++)
                {
                    newBoughtHats[i] = boughtHats[i];
                }

                newBoughtHats[newBoughtHats.Length - 1] = id;

                allSkins[selectedSkin].boughtHats = newBoughtHats;
                break;
            case CustomizationType.Tail:
                allTails[id].purchased = true;

                int[] boughtTails = allSkins[selectedSkin].boughtTails;
                int[] newBoughtTails = new int[boughtTails.Length + 1];

                for (int i = 0; i < boughtTails.Length; i++)
                {
                    newBoughtTails[i] = boughtTails[i];
                }

                newBoughtTails[newBoughtTails.Length - 1] = id;

                allSkins[selectedSkin].boughtTails = newBoughtTails;
                break;
            case CustomizationType.GraveTop:
                allGraveTops[id].purchased = true;
                break;
            case CustomizationType.GraveSide:
                allGraveSides[id].purchased = true;
                break;
            case CustomizationType.GraveBottom:
                allGraveBottoms[id].purchased = true;
                break;
        }

        buyInfoObj.GetComponent<BuyInfoHandler>().SetBuyData(newSprite);

        PurchaseMinerItem(price);

        ApplyCustom(type, id);
        SavePurchasedItems();
    }

    public void ResetSelected(int newSkin)
    {
        selectedWing = 0;

        if(!allSkins[newSkin].purchased)
        {
            selectedHat = 0;
        } else
        {
            selectedHat = allSkins[newSkin].lastHat;
        }
        //
    }

    private void CheckCrown()
    {
        if(ScoreHandler.bronzeOK)
        {
            GetItemByString(CustomizationType.Hat, "bronze").purchased = true;
        } else
        {
            GetItemByString(CustomizationType.Hat, "bronze").purchased = false;
        }

        if(ScoreHandler.silverOK)
        {
            GetItemByString(CustomizationType.Hat, "silver").purchased = true;
        } else
        {
            GetItemByString(CustomizationType.Hat, "silver").purchased = false;
        }

        if (ScoreHandler.goldOK)
        {
            GetItemByString(CustomizationType.Hat, "gold").purchased = true;
        } else
        {
            GetItemByString(CustomizationType.Hat, "gold").purchased = false;
        }
    }

    public void ApplyCustom(CustomizationType type, int id, bool reload = true)
    {
        Debug.Log("Try Apply: " + type + " " + id + " " + reload);

        if(HasPurchased(type, id) == 2)
        { //selected nur ändern wenn gekauft

            switch (type)
            {
                case CustomizationType.Skin:
                    selectedSkin = id;

                    for (int i = 1; i < this.allWings.Count; i++)
                    { //i = 1 weil default ist ja immer gekauft
                        this.allWings[i].purchased = false;
                    }

                    for(int i = 1; i < this.allHats.Count; i++)
                    {
                        this.allHats[i].purchased = false;
                    }

                    int[] allWings = allSkins[id].boughtWings;
                    int[] allHats = allSkins[id].boughtHats;

                    for(int i = 0; i < allWings.Length; i++)
                    {
                        this.allWings[allWings[i]].purchased = true;
                    }

                    for(int i = 0; i < allHats.Length; i++)
                    {
                        this.allHats[allHats[i]].purchased = true;
                    }

                    CheckCrown();

                    if(!this.allWings[selectedWing].purchased)
                    {
                        selectedWing = 0;
                    }

                    if(!this.allHats[selectedHat].purchased)
                    {
                        selectedHat = 0;
                    }
                    break;
                case CustomizationType.Wing:
                    selectedWing = id;
                    break;
                case CustomizationType.Pipe:
                    selectedPipe = id;
                    break;
                case CustomizationType.Hat:
                    selectedHat = id;
                    this.allSkins[selectedSkin].lastHat = id;
                    break;
                case CustomizationType.PipeColor:
                    pipeColorID = id;
                    break;
                case CustomizationType.GraveTop:
                    selectedGraveTop = id;
                    break;
                case CustomizationType.GraveSide:
                    selectedGraveSide = id;
                    break;
                case CustomizationType.GraveBottom:
                    selectedGraveBottom = id;
                    break;
            }
        } else
        {
            Debug.Log("Apply Failed: " + type + " " + id + " " + reload);
        }

        pipeColor = PipeCustomizationHandler.Instance.GetPipeColor(pipeColorID);

        SavePurchasedItems();

        if (playerData != null && reload)
        {
            switch(type)
            {
                case CustomizationType.Skin:
                case CustomizationType.Wing:
                    playerData.LoadPlayerSkin(allSkins[selectedSkin], allWings[selectedWing]);
                    playerData.LoadHat(allHats[selectedHat]);
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

    private IEnumerator<float> _HandleWingAnimation(float waitTime)
    {
        //if (currentType != 1) return;

        while(!RemoteConfigHandler.loadComplete)
        { //warte dass remote config load abgeschlossen
            yield return Timing.WaitForSeconds(0.25f);
        }

        yield return Timing.WaitForSeconds(waitTime);

        while(true)
        {
            int maxLen = playerData.currentWing.sprite.Length - 1;

            if (wingAnimationDir == 0)
            {
                wingAnimationCount++;
                if (wingAnimationCount > maxLen)
                {
                    wingAnimationCount = 1;
                    wingAnimationDir = 1;
                }
            }
            else
            {
                wingAnimationCount--;
                if (wingAnimationCount < 0)
                {
                    wingAnimationCount = 1;
                    wingAnimationDir = 0;
                }
            }

            if (shopActive && currentType == 1)
            {
                for (int i = 0; i < allWings.Count; i++)
                {
                    if (allWings[i].sprite.Length > 1)
                    {
                        slotParent.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                            allWings[i].sprite[wingAnimationCount];
                    }
                }
            }
            else
            {
                playerData.SetWingAnimation(wingAnimationCount);
            }

            yield return Timing.WaitForSeconds(0.25f);
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
    void _MainUpdate()
    {
        if(shopActive)
        {
            if(Input.GetKeyDown(KeyCode.Escape) && !closing)
            {
                CloseShop();
            }
        }

        //playerData.SetColor(fadeMat.color);
    }
}
