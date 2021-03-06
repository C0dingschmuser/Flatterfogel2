﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinerCustomizationHandler : MonoBehaviour
{
    public ShopHandler shop;

    [SerializeField]
    ShopMenuHandler shopMenu = null;

    public GameObject moduleUpgrade, moduleUpgradeButton,
        priceHighlight, mineModePriceText,
        mineModePriceImage, buyButton, itemParent, moduleImage,
        moduleBlend, smallPriceHighlight, typeParent;

    public Transform smallPriceTop, smallPriceBottom;

    public Color unlockedColor, lockedColor;
    public Vector3[] objPositions;

    public static MinerCustomizationHandler Instance;
    public static int buyOption = 0;

    private int selectedUpgrade = 0, selectedItem = 0;
    private float dissolveAmount = 0;
    private bool itemSelected = false;

    [SerializeField]
    private Material imageMat = null, fontMat = null;

    private Tween objTween = null, objScaleTween = null, highlightTween = null,
        smallPriceMoveTween = null, smallPriceScaleTween = null;
    private Tween[] priceTextTween = new Tween[4];
    private Tween[] priceImageTween = new Tween[4];

    private CostData[] currentPrices = null;

    private void Awake()
    {
        Instance = this;
    }

    public void StartMineCustomizationHandler()
    {
        gameObject.SetActive(true);

        for (int i = 0; i < 4; i++)
        {
            priceTextTween[i] = null;
            priceImageTween[i] = null;
        }

        ObjClicked(CustomizationType.MinerTool);

        fontMat.SetFloat("_DissolveAmount", 0);
        imageMat.SetFloat("_DissolveAmount", 0);

        dissolveAmount = 0;

        Tween anTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, 1, ShopMenuHandler.anTime);
        anTween.OnUpdate(() =>
        {
            fontMat.SetFloat("_DissolveAmount", dissolveAmount);
            imageMat.SetFloat("_DissolveAmount", dissolveAmount);
        });

        StartCoroutine(EndStart());
    }

    private IEnumerator EndStart()
    {
        yield return new WaitForSeconds(ShopMenuHandler.anTime + 0.01f);
    }

    public void CloseButtonClicked()
    {
        SoundManager.Instance.PlaySound(Sound.MenuSelectEnd);

        fontMat.SetFloat("_DissolveAmount", 1);
        imageMat.SetFloat("_DissolveAmount", 1);

        dissolveAmount = 1;

        Tween anTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, 0, ShopMenuHandler.anTime);
        anTween.OnUpdate(() =>
        {
            fontMat.SetFloat("_DissolveAmount", dissolveAmount);
            imageMat.SetFloat("_DissolveAmount", dissolveAmount);
        });

        StartCoroutine(EndClose());
    }

    private IEnumerator EndClose()
    {
        yield return new WaitForSeconds(ShopMenuHandler.anTime + 0.01f);

        gameObject.SetActive(false);

        shopMenu.OpenMenu();
    }

    public void UpdateUI()
    {
        CustomizationType type = (CustomizationType)selectedUpgrade;

        for (int i = 0; i < moduleUpgrade.transform.childCount; i++)
        {
            if (shop.HasPurchased(type, 4 - i) == 2)
            {
                moduleUpgrade.transform.GetChild(i).GetComponent<Image>().color =
                    unlockedColor;
            }
            else
            {
                moduleUpgrade.transform.GetChild(i).GetComponent<Image>().color =
                    lockedColor;
            }
        }

        int latest = shop.GetLatestMineObj(type);
        float newScale = shop.GetScale(type, latest);

        Sprite sp;

        switch(type)
        {
            default:
            case CustomizationType.MinerTool:
                sp = shop.allMiners[latest].full;
                break;
            case CustomizationType.MinerHeatShield:
                sp = shop.allHeatShields[latest].shieldSprite;
                break;
            case CustomizationType.MinerFuelTank:
                sp = shop.allFuelTanks[latest].sprite;
                break;
        }

        moduleImage.GetComponent<Image>().sprite = sp;
        moduleImage.transform.localScale = new Vector3(newScale, newScale, newScale);

        if(shop.HasPurchased(type, 4) == 2)
        { //kein upgrade mehr möglich
            moduleUpgradeButton.GetComponent<Button>().interactable = false;
        }

        for(int i = 0; i < shop.allMineItems.Count; i++)
        {
            itemParent.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                shop.allMineItems[i].sprite;

            itemParent.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text =
                shop.allMineItems[i].amount.ToString();
        }
    }

    public void BuyOptionClicked(int pos)
    {
        if (smallPriceMoveTween != null)
        {
            smallPriceMoveTween.Kill();
        }

        if (smallPriceScaleTween != null)
        {
            smallPriceScaleTween.Kill();
        }

        buyOption = pos;

        if (pos == 0)
        {
            smallPriceMoveTween = smallPriceHighlight.transform.DOMove(smallPriceTop.position, 0.2f);
            smallPriceScaleTween = smallPriceHighlight.transform.GetComponent<RectTransform>().
                DOSizeDelta(smallPriceTop.GetComponent<RectTransform>().sizeDelta, 0.2f);
        } else
        {
            smallPriceMoveTween = smallPriceHighlight.transform.DOMove(smallPriceBottom.position, 0.2f);
            smallPriceScaleTween = smallPriceHighlight.transform.GetComponent<RectTransform>().
                DOSizeDelta(smallPriceBottom.GetComponent<RectTransform>().sizeDelta, 0.2f);
        }

        IsAffordable(true);
    }

    public void ItemClicked(int type)
    {
        itemSelected = true;

        selectedItem = type;

        Vector3 newPos = objPositions[type + 4];
        Vector2 newSize = new Vector2(128, 128);

        //smallPriceHighlight.SetActive(true);
        //BuyOptionClicked(1);

        shop.MineItemClicked(type);
        SetMinePrice(shop.allMineItems[type].cost);
    }

    public void ObjClicked(int type)
    {
        ObjClicked((CustomizationType)type);
    }

    private void ObjClicked(CustomizationType type)
    {
        smallPriceHighlight.SetActive(false);
        itemSelected = false;

        selectedUpgrade = (int)type;

        for(int i = 0; i < typeParent.transform.childCount; i++)
        {
            typeParent.transform.GetChild(i).GetComponent<Image>().color = Color.white;
        }

        switch(type)
        {
            default:
            case CustomizationType.MinerTool:
                typeParent.transform.GetChild(0).GetComponent<Image>().color = new Color32(238, 77, 46, 255);
                break;
            case CustomizationType.MinerHeatShield:
                typeParent.transform.GetChild(2).GetComponent<Image>().color = new Color32(238, 77, 46, 255);
                break;
            case CustomizationType.MinerFuelTank:
                typeParent.transform.GetChild(1).GetComponent<Image>().color = new Color32(238, 77, 46, 255);
                break;
        }

        CostData[] cost;
        int purchased = 0;

        int nextObj = shop.GetNextMineObj(type);

        cost = shop.GetCost(type, nextObj);
        purchased = shop.HasPurchased(type, nextObj);

        if(purchased == 0 || purchased == 1)
        {
            mineModePriceImage.SetActive(true);
            mineModePriceText.SetActive(true);

            SetMinePrice(cost, false);
            //HighlightPrice();
        } else
        { //deaktiviert wenn bereits gekauft
            buyButton.SetActive(false);

            mineModePriceImage.SetActive(false);
            mineModePriceText.SetActive(false);
        }

        UpdateUI();
    }

    public void UpgradeClicked()
    {
        /*if(selectedUpgrade != (int)type)
        {
            ObjClicked(type);
            return;
        }*/

        CustomizationType type = (CustomizationType)selectedUpgrade;

        CostData[] cost;
        bool upgraded = false;

        int nextObj = shop.GetNextMineObj(type);
        cost = shop.GetCost(type, nextObj);

        if(shop.IsAffordable(cost) == 1) //durch haspurchased ersetzen
        {
            upgraded = true;
            shop.MiningPurchase(type, nextObj);
        }

        if(upgraded)
        {
            UpdateUI();

            moduleBlend.SetActive(false);
            moduleBlend.SetActive(true);
            moduleBlend.GetComponent<Dissolver>().StartDissolve(new Color32(0, 143, 255, 255));

            ObjClicked(type);
        } else
        {
            HighlightPrice();
        }
    }

    private void HighlightPrice()
    {
        if(highlightTween != null)
        {
            highlightTween.Kill();
        }

        Color c = Color.red;
        c.a = 1;
        priceHighlight.GetComponent<Image>().color = c;

        highlightTween = priceHighlight.GetComponent<Image>().DOFade(0, 0.3f);
    }

    public void BuyClicked()
    {
        shop.MiningPurchase(CustomizationType.MinerItem, selectedItem);

        IsAffordable(true);
        UpdateUI();
    }

    public void IsAffordable(bool showBuy = true)
    {
        CostData[] prices = currentPrices;

        bool canAfford = true;

        int start = 0, end = prices.Length;

        /*if (itemSelected)
        {
            /*if (buyOption == 0)
            { //coins / benis
                start = 0;
                end = 1;
            }
            else
            { //minerale
                start = 1;
                end = 4;
            }
            start = 0;
            end = 4;
        }*/

        for (int i = start; i < end; i++)
        {
            if (prices[i].amount > 0)
            {
                ulong collectedAmount =
                    (ulong)Inventory.Instance.GetMineralAmount((int)prices[i].mineralID);

                if (prices[i].mineralID == MineralType.Coin)
                {
                    collectedAmount = shop.GetBlus();
                }

                if (collectedAmount < (ulong)prices[i].amount)
                {
                    canAfford = false;
                }
            }
        }

        if (showBuy)
        {
            buyButton.SetActive(true);
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
        else
        {
            buyButton.SetActive(false);
        }
    }

    private void SetMinePrice(CostData[] prices, bool showBuy = true)
    {
        currentPrices = prices;

        float fadeInTime = 0.3f;

        for (int i = 0; i < 4; i++)
        {
            if (prices[i].amount > 0)
            {
                ulong collectedAmount =
                    (ulong)Inventory.Instance.GetMineralAmount((int)prices[i].mineralID);

                if(prices[i].mineralID == MineralType.Coin)
                {
                    collectedAmount = shop.GetBlus();
                }

                //collectedAmount = Mathf.Clamp(collectedAmount, 0, prices[i].amount);

                if (collectedAmount >= (ulong)prices[i].amount)
                {
                    collectedAmount = (ulong)prices[i].amount;
                }

                if(priceTextTween[i] != null)
                {
                    priceTextTween[i].Kill();
                }

                Color tC = Color.black;
                tC.a = 0;

                mineModePriceText.transform.GetChild(i).GetComponent<TextMeshProUGUI>().color =
                    tC;
                priceTextTween[i] =
                    mineModePriceText.transform.GetChild(i).GetComponent<TextMeshProUGUI>().
                    DOFade(1, fadeInTime);

                mineModePriceText.transform.GetChild(i).gameObject.SetActive(true);
                mineModePriceText.transform.GetChild(i).GetComponent<TextMeshProUGUI>().text =
                    collectedAmount + "/" + prices[i].amount.ToString();

                if(priceImageTween[i] != null)
                {
                    priceImageTween[i].Kill();
                }

                tC = Color.white;
                tC.a = 0;

                mineModePriceImage.transform.GetChild(i).GetComponent<Image>().color =
                    tC;
                priceImageTween[i] =
                    mineModePriceImage.transform.GetChild(i).GetComponent<Image>().
                    DOFade(1, fadeInTime);

                mineModePriceImage.transform.GetChild(i).gameObject.SetActive(true);

                if(Inventory.Instance.allMinerals[(int)prices[i].mineralID].shopSprite == null)
                {
                    mineModePriceImage.transform.GetChild(i).GetComponent<Image>().sprite =
                        Inventory.Instance.allMinerals[(int)prices[i].mineralID].sprite;
                } else
                {
                    mineModePriceImage.transform.GetChild(i).GetComponent<Image>().sprite =
                        Inventory.Instance.allMinerals[(int)prices[i].mineralID].shopSprite;
                }
            }
            else
            {
                mineModePriceText.transform.GetChild(i).gameObject.SetActive(false);
                mineModePriceImage.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        IsAffordable(showBuy);
    }
}
