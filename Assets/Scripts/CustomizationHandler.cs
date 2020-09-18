using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using TMPro;

[System.Serializable]
public enum CustomizationType
{
    Skin = 0,
    Wing = 1,
    Pipe = 2,
    PipeColor = 3,
    Hat = 4,
    GraveTop = 5,
    GraveSide = 6,
    GraveBottom = 7,
}

public class CustomizationHandler : MonoBehaviour
{
    [SerializeField]
    private ShopHandler shop = null;
    [SerializeField]
    private ShopMenuHandler shopMenu = null;
    [SerializeField]
    private Vector3[] previewPositions = null;
    [SerializeField]
    private Transform[] previewImages = null;
    [SerializeField]
    private SwipeDetector swDetector = null;
    [SerializeField]
    private Transform buyButton = null,
        wingDisabled = null,
        hatDisabled = null,
        skinPreview = null,
        wingPreview = null,
        hatPreview = null,
        typeParent = null,
        smallPreviewParent = null;

    [SerializeField]
    private GameObject priceText = null, priceImage = null, buyInfo = null,
        saleObj = null;

    [SerializeField]
    private Material imageMat = null, fontMat = null;

    private CustomizationType type = CustomizationType.Skin;
    private float time = 0.25f, hatMiddleY = 810, dissolveAmount = 0; //time = 0.25 original
    private int middleID = 1;
    private bool changeApplied = false, interaction = false, customizationOpen = false;
    private ObscuredInt selectedID = 0;

    public bool switchRunning = false;

    private Tween[] priceTextTween = new Tween[4];
    private Tween[] priceImageTween = new Tween[4];

    [SerializeField]
    private Transform priceParent = null;

    private Vector3 pricePos;
    private int buyCode = 0;

    private Coroutine noMoneyRoutine = null, saleRoutine = null;

    private void Awake()
    {
        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;

        pricePos = priceParent.transform.position;
    }

    private void Start()
    {
        Timing.RunCoroutine(Util._EmulateUpdate(_MainUpdate, this));
    }

    public void StartCustomizationHandler()
    {
        FilterHandler.SetupFilter();
        SetType(CustomizationType.Skin, true);

        switchRunning = true;

        FF_PlayerData.Instance.inShop = true;

        fontMat.SetFloat("_DissolveAmount", 0);
        imageMat.SetFloat("_DissolveAmount", 0);

        dissolveAmount = 0;

        Tween anTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, 1, ShopMenuHandler.anTime);
        anTween.OnUpdate(() =>
        {
            fontMat.SetFloat("_DissolveAmount", dissolveAmount);
            imageMat.SetFloat("_DissolveAmount", dissolveAmount);
        });

        customizationOpen = true;

        StartCoroutine(EndStart());
    }

    private IEnumerator EndStart()
    {
        yield return new WaitForSeconds(ShopMenuHandler.anTime + 0.01f);

        swDetector.enabled = true;
        changeApplied = true;
        switchRunning = false;
    }

    public void SetType(CustomizationType newType, bool start = false)
    {
        this.type = newType;

        if(start)
        {
            switchRunning = false;
        }

        shop.ApplyCustom(CustomizationType.Skin, shop.GetSelectedSkin(), false);

        if (noMoneyRoutine != null)
        {
            StopCoroutine(noMoneyRoutine);
        }

        priceParent.position = pricePos;

        switch(type)
        {
            case CustomizationType.Skin:
                selectedID = shop.GetSelectedSkin();
                break;
            case CustomizationType.Wing:
                selectedID = shop.GetSelectedWing();
                break;
            case CustomizationType.Hat:
                selectedID = shop.GetSelectedHat();
                break;
        }

        TypeClicked((int)newType);
    }

    public void CloseButtonClicked()
    {
        switchRunning = true;

        fontMat.SetFloat("_DissolveAmount", 1);
        imageMat.SetFloat("_DissolveAmount", 1);

        dissolveAmount = 1;

        SoundManager.Instance.PlaySound(Sound.MenuSelectEnd);

        Tween anTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, 0, ShopMenuHandler.anTime);
        anTween.OnUpdate(() =>
        {
            fontMat.SetFloat("_DissolveAmount", dissolveAmount);
            imageMat.SetFloat("_DissolveAmount", dissolveAmount);
        });

        if(saleRoutine != null)
        {
            StopCoroutine(saleRoutine);
        }

        saleObj.SetActive(false);

        FF_PlayerData.Instance.inShop = false;

        StartCoroutine(EndClose());
    }

    private IEnumerator EndClose()
    {
        yield return new WaitForSeconds(ShopMenuHandler.anTime + 0.01f);

        CloseCustomization(true);

        shopMenu.OpenMenu();
    }

    public void CloseCustomization(bool disable = false)
    {
        if(swDetector != null)
        {
            swDetector.enabled = false;
        }

        if(!changeApplied && interaction)
        {
            changeApplied = true;
            shop.ApplyCustom(type, selectedID);
        }

        interaction = false;

        customizationOpen = false;

        if (disable)
        {
            this.gameObject.SetActive(false);
            FF_PlayerData.Instance.inShop = false;
        }
    }

    private int GetNewID(int id)
    {
        if (id < 0) id = 2;
        if (id > 2) id = 0;

        return id;
    }

    public void UpdateSale(CustomizationType type, int overrideID = -1)
    {
        int saleAmount = 0;

        int id = selectedID;

        if(overrideID > -1)
        {
            id = overrideID;
        }

        bool purchased = false;

        if (shop.HasPurchased(type, id) == 2)
        {
            purchased = true;
        }

        switch (type)
        {
            case CustomizationType.Skin:
                saleAmount = shop.allSkins[id].salePercent;

                break;
            case CustomizationType.Wing:
                saleAmount = shop.allWings[id].salePercent;

                break;
            case CustomizationType.Hat:
                saleAmount = shop.allHats[id].salePercent;

                break;
        }

        if(saleAmount != 0 && !purchased)
        {
            if (saleRoutine != null)
            {
                StopCoroutine(saleRoutine);
            }

            saleObj.SetActive(true);

            saleRoutine = StartCoroutine(SaleRoutine(saleAmount));
        } else
        {
            if(saleRoutine != null)
            {
                StopCoroutine(saleRoutine);
            }

            saleObj.SetActive(false);
        }
    }

    private IEnumerator SaleRoutine(int saleAmount)
    {
        string final = shop.saleString + ": " + saleAmount.ToString() + "%";

        saleObj.GetComponent<TextMeshProUGUI>().text = final;
        saleObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = final;

        while (true)
        {
            Color temp = saleObj.GetComponent<TextMeshProUGUI>().color;

            saleObj.GetComponent<TextMeshProUGUI>().color =
                saleObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;

            saleObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color =
                temp;
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void UpdateDisplay(CustomizationType type)
    {
        Sprite left = null, middle = null, right = null;
        int id = 0;

        previewImages[middleID].localScale = new Vector3(2.5f, 2.5f, 2.5f);

        CostData[] cost = null;
        bool purchased = false;

        id = shop.GetSelected(type);
        cost = shop.GetCost(type, id);

        if(shop.HasPurchased(type, id) == 2)
        {
            purchased = true;
        }

        left = shop.GetSprite(type, id - 1);
        middle = shop.GetSprite(type, id);
        right = shop.GetSprite(type, id + 1);

        switch(type)
        {
            case CustomizationType.Skin:

                skinPreview.GetComponent<Image>().sprite = middle;
                UpdateWing(id);

                break;
            case CustomizationType.Wing:
                float scale = shop.GetWingScale(id);

                wingPreview.GetComponent<Image>().sprite = middle;

                wingPreview.localScale = new Vector3(scale, scale, scale);
                previewImages[middleID].localScale = new Vector3(scale, scale, scale);

                break;
            case CustomizationType.Hat:

                hatPreview.GetComponent<Image>().sprite = middle;

                break;
        }
        UpdateSale(type, id);

        selectedID = id;

        previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite = left;
        previewImages[GetNewID(middleID - 1)].GetComponent<IDHolder>().realID =
            shop.CheckSelected(type, selectedID - 1);

        previewImages[middleID].GetComponent<Image>().sprite = middle;
        previewImages[middleID].GetComponent<IDHolder>().realID = selectedID;

        previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite = right;
        previewImages[GetNewID(middleID + 1)].GetComponent<IDHolder>().realID =
            shop.CheckSelected(type, selectedID + 1);

        if (type == CustomizationType.Hat)
        {
            Vector3 pos = previewImages[middleID].transform.position;

            int realID = previewImages[middleID].GetComponent<IDHolder>().realID;

            Debug.Log(realID);

            pos.y = hatMiddleY +
                shop.allHats[realID].shopYDist;

            previewImages[middleID].transform.position = pos;
        }
        else
        {
            previewImages[middleID].transform.position = previewPositions[1];
        }

        if (purchased)
        {
            priceImage.SetActive(false);
            priceText.SetActive(false);

            buyButton.GetComponent<Image>().color = Color.white;
            buyButton.GetComponent<Button>().interactable = false;
            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                ShopHandler.Instance.boughtString;
        } else
        {
            priceImage.SetActive(true);
            priceText.SetActive(true);

            SetPrice(cost);
        }
    }

    public void FilterUpdate()
    {
        TypeClicked((int)type);
    }

    public void TypeClicked(int itype)
    {
        CustomizationType newType = (CustomizationType)itype;

        if(interaction)
        {
            shop.ApplyCustom(type, selectedID);
            changeApplied = true;
        }

        interaction = false;

        for(int i = 0; i < typeParent.childCount; i++)
        {
            typeParent.GetChild(i).GetComponent<Image>().color = Color.white;
        }

        if(newType != CustomizationType.Skin)
        {
            skinPreview.GetComponent<Image>().sprite = 
                shop.GetSkinSprite(shop.GetSelectedSkin());

            skinPreview.GetComponent<IDHolder>().realID =
                shop.GetSelectedSkin();

            skinPreview.gameObject.SetActive(true);
        } else
        {
            CheckHatSupport();
            CheckWingSupport();

            typeParent.GetChild(1).GetComponent<Image>().color = Color.black;

            skinPreview.gameObject.SetActive(false);
        }

        if(newType != CustomizationType.Wing)
        {
            UpdateWing();
        } else
        {
            typeParent.GetChild(0).GetComponent<Image>().color = Color.black;

            wingPreview.gameObject.SetActive(false);
        }

        if(newType != CustomizationType.Hat)
        {
            UpdateHat();
        } else
        {
            typeParent.GetChild(2).GetComponent<Image>().color = Color.black;

            hatPreview.gameObject.SetActive(false);
        }

        type = newType;
        UpdateDisplay(newType);
        UpdateSmallPreviews(1);

        UpdateSale(newType);
    }

    private void UpdateHat()
    {
        hatPreview.GetComponent<Image>().sprite =
            shop.GetHatSprite(shop.GetSelectedHat());
        hatPreview.gameObject.SetActive(true);

        hatPreview.GetComponent<IDHolder>().realID =
            shop.GetSelectedHat();

        if (shop.HasHatSupport(shop.GetSelectedSkin()))
        {
            hatPreview.gameObject.SetActive(true);
        }
        else
        {
            hatPreview.gameObject.SetActive(false);
        }
    }

    private void UpdateWing(int overrideSelected = -1)
    {
        int sel = shop.GetSelectedSkin();

        if(overrideSelected > -1)
        {
            sel = overrideSelected;
        }

        wingPreview.GetComponent<Image>().sprite =
            shop.GetWingSprite(shop.GetSelectedWing());

        float scale = shop.GetWingScale(shop.GetSelectedWing());

        if (shop.allSkins[sel].overrideWing != null)
        {
            Wing oWing = shop.allSkins[sel].overrideWing;

            wingPreview.GetComponent<Image>().sprite = oWing.sprite[0];

            scale = oWing.shopScale;
        }

        wingPreview.localScale = new Vector3(scale, scale, scale);

        if (shop.HasWingSupport(sel))
        {
            wingPreview.gameObject.SetActive(true);
        }
        else
        {
            wingPreview.gameObject.SetActive(false);
        }
    }

    public void UpdateSmallPreviews(int dir = 1)
    {
        int max = shop.GetMax(type);

        int currentPage = selectedID / 10;
        //int posInSmallPreviews = selectedID - (currentPage * 10);

        for (int i = 0; i < smallPreviewParent.childCount; i++)
        {
            int newID = i;
            
            if(smallPreviewParent.childCount < max)
            { //wenn neue seite
                #region alt
                /*if(dir == 1) alter code für continuos
                {//rechts (start ist 0)
                    newID = selectedID + i;//shop.CheckSelected(type, selectedID + i);

                    newID = (currentPage * 10) + i;
                } else
                { //links (start ist pmax - 1)
                    newID =
                        (selectedID - (smallPreviewParent.childCount - 1)) + i;

                    newID = (currentPage * 10) + i;
                }*/
                #endregion

                newID = (currentPage * 10) + i;
            }

            if(newID < max && newID >= 0)
            {
                Sprite newSprite = null;

                switch (type)
                {
                    case CustomizationType.Skin:
                        newSprite = shop.GetSkinSprite(newID);
                        break;
                    case CustomizationType.Wing:
                        newSprite = shop.GetWingSprite(newID);
                        break;
                    case CustomizationType.Hat:
                        newSprite = shop.GetHatSprite(newID);
                        break;
                }

                smallPreviewParent.GetChild(i).GetComponent<IDHolder>().realID = newID;

                if(newID == selectedID)
                {
                    smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.black;
                } else
                {
                    smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.white;
                }

                //Farbe von Obj Sprite
                Color32 c = smallPreviewParent.GetChild(i).GetChild(1).GetComponent<Image>().color;
                if (shop.HasPurchased(type, newID) == 2)
                {
                    c.a = 255;
                } else
                {
                    c.a = 168;
                }

                smallPreviewParent.GetChild(i).GetChild(1).GetComponent<Image>().color = c;

                smallPreviewParent.GetChild(i).GetChild(0).gameObject.SetActive(true);
                smallPreviewParent.GetChild(i).GetChild(1).gameObject.SetActive(true);

                smallPreviewParent.GetChild(i).GetChild(0).GetComponent<Image>().color =
                    shop.GetRarity(type, newID, 170);

                smallPreviewParent.GetChild(i).GetChild(1).GetComponent<Image>().sprite =
                    newSprite;
            } else
            {
                smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.white;
                smallPreviewParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
                smallPreviewParent.GetChild(i).GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    public void SmallPreviewClicked(int newID)
    {
        if (switchRunning) return;

        interaction = true;

        int realID = 
            smallPreviewParent.GetChild(newID).GetComponent<IDHolder>().realID;

        for(int i = 0; i < smallPreviewParent.childCount; i++)
        {
            smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.white;
        }

        smallPreviewParent.GetChild(newID).GetComponent<Image>().color = Color.black;

        changeApplied = false;

        selectedID = realID;

        Sprite newLeft, newMiddle, newRight;

        float bigScale = 2.5f;

        newLeft = shop.GetSprite(type, selectedID - 1);
        newMiddle = shop.GetSprite(type, selectedID);
        newRight = shop.GetSprite(type, selectedID + 1);

        if(type == CustomizationType.Wing)
        {
            bigScale = shop.GetWingScale(selectedID);
        }

        previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite = newLeft;
        previewImages[GetNewID(middleID - 1)].localScale = Vector3.one;
        previewImages[GetNewID(middleID - 1)].GetComponent<IDHolder>().realID =
            shop.CheckSelected(type, selectedID - 1);

        previewImages[middleID].GetComponent<Image>().sprite = newMiddle;
        previewImages[middleID].localScale = new Vector3(bigScale, bigScale, bigScale);
        previewImages[middleID].GetComponent<IDHolder>().realID =
            selectedID;

        previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite = newRight;
        previewImages[GetNewID(middleID + 1)].localScale = Vector3.one;
        previewImages[GetNewID(middleID + 1)].GetComponent<IDHolder>().realID =
            shop.CheckSelected(type, selectedID + 1);

        if (type == CustomizationType.Hat)
        {
            Vector3 pos = previewImages[middleID].transform.position;
            pos.y = hatMiddleY +
                shop.allHats[previewImages[middleID].GetComponent<IDHolder>().realID].shopYDist;
            previewImages[middleID].transform.position = pos;
        }
        else
        {
            previewImages[middleID].transform.position = previewPositions[1];
        }

        FetchPurchased(type, selectedID);

        if (type == CustomizationType.Skin)
        { //wing zurücksetzen & hat laden bei skinänderung
            shop.ResetSelected(selectedID);
        }
        else if (type == CustomizationType.Hat)
        {
            if (shop.HasPurchased(type, selectedID) == 2)
            { //wenn gekauft hut zuweisen
                shop.ApplyCustom(type, selectedID);
            }
        }

        UpdateSale(type);

        CheckWingSupport();
        CheckHatSupport();
    }

    public void SwipeDetector_OnSwipe(SwipeData data)
    {
        if(!customizationOpen)
        {
            return;
        }

        switch (data.Direction)
        {
            case SwipeDirection.Left:
                ArrowClicked(1);
                break;
            case SwipeDirection.Right:
                ArrowClicked(0);
                break;
        }
    }

    public void FetchPurchased(CustomizationType type, int id)
    {
        int code = shop.HasPurchased(type, id);

        buyButton.GetComponent<Button>().interactable = false;
        buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            ShopHandler.Instance.buyString;

        buyCode = code;

        switch (code)
        {
            case 0: //nicht gekauft & kein geld
                buyButton.GetComponent<Image>().color = Color.red;
                buyButton.GetComponent<Button>().interactable = true;
                break;
            case 1: //nicht gekauft & geld
                buyButton.GetComponent<Image>().color = Color.green;
                buyButton.GetComponent<Button>().interactable = true;
                break;
            case 2: //schon gekauft
                buyButton.GetComponent<Image>().color = Color.white;
                buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    ShopHandler.Instance.boughtString;
                buyButton.GetComponent<Button>().interactable = false;
                break;
        }

        if(code < 2)
        {
            priceImage.SetActive(true);
            priceText.SetActive(true);

            SetPrice(shop.GetCost(type, id));
        } else
        {
            priceImage.SetActive(false);
            priceText.SetActive(false);
        }
    }
    
    public void ArrowClicked(int dir)
    {
        if (switchRunning) return;
        switchRunning = true;

        interaction = true;

        Sprite newLeft = null, newRight = null;

        float bigScale = 2.5f;

        if(type == CustomizationType.Wing)
        {
            int newMiddleID = shop.CheckSelected(type, selectedID - 1);

            if(dir == 1)
            {
                newMiddleID = shop.CheckSelected(type, selectedID + 1);
            }

            bigScale = shop.GetWingScale(newMiddleID);
        }

        changeApplied = false;

        Vector3 middlePos = previewPositions[1];

        if(type == CustomizationType.Hat)
        {
            if(dir == 0)
            {
                middlePos.y = hatMiddleY +
                    shop.allHats[shop.CheckSelected(type, selectedID - 1)].shopYDist;
            } else
            {
                middlePos.y = hatMiddleY +
                    shop.allHats[shop.CheckSelected(type, selectedID + 1)].shopYDist;
            }
        }

        if(dir == 0)
        { //nach links -> previews nach rechts

            //linkes preview in mitte bewegen & upscalen
            previewImages[GetNewID(middleID - 1)].transform.DOMove(middlePos, time);
            previewImages[GetNewID(middleID - 1)].transform.DOScale(bigScale, time);
            previewImages[GetNewID(middleID - 1)].GetComponent<IDHolder>().realID =
                shop.CheckSelected(type, selectedID - 1);

            //mittleres preview nach rechts & runterscalen
            previewImages[middleID].transform.DOMove(previewPositions[2], time);
            previewImages[middleID].transform.DOScale(1, time);
            previewImages[middleID].GetComponent<IDHolder>().realID =
                shop.CheckSelected(type, selectedID);

            //rechtes preview an linke pos & dummy an rechte pos & beide x + 125
            Vector3 leftStartPos = previewPositions[0];
            leftStartPos.x -= 125;
            previewImages[GetNewID(middleID + 1)].transform.position = leftStartPos;
            previewImages[GetNewID(middleID + 1)].DOMove(previewPositions[0], time);
            previewImages[GetNewID(middleID + 1)].GetComponent<IDHolder>().realID =
                shop.CheckSelected(type, selectedID - 2);

            //dummy altes image zuweisen & nach rechts bewegen
            previewImages[3].transform.position = previewPositions[2];
            previewImages[3].GetComponent<Image>().sprite = 
                previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite;
            previewImages[3].transform.DOMoveX(previewPositions[2].x + 125, time);

            previewImages[3].gameObject.SetActive(true);

            newLeft = shop.GetSprite(type, selectedID - 2);

            selectedID = shop.CheckSelected(type, selectedID - 1);

            previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite =
                newLeft;

            middleID = GetNewID(middleID - 1);

            FetchPurchased(type, selectedID);
        } else if(dir == 1)
        { //nach rechts -> previews nach links

          //rechtes preview in mitte bewegen & upscalen
            previewImages[GetNewID(middleID + 1)].transform.DOMove(middlePos, time);
            previewImages[GetNewID(middleID + 1)].transform.DOScale(bigScale, time);
            previewImages[GetNewID(middleID + 1)].GetComponent<IDHolder>().realID =
            shop.CheckSelected(type, selectedID + 1);

            //mittleres preview nach links & runterscalen
            previewImages[middleID].transform.DOMove(previewPositions[0], time);
            previewImages[middleID].transform.DOScale(1, time);
            previewImages[middleID].GetComponent<IDHolder>().realID =
                shop.CheckSelected(type, selectedID);

            //linkes preview an rechte pos & dummy an linke pos & beide x - 125
            Vector3 rightStartPos = previewPositions[2];
            rightStartPos.x += 125;
            previewImages[GetNewID(middleID - 1)].transform.position = rightStartPos;
            previewImages[GetNewID(middleID - 1)].DOMove(previewPositions[2], time);
            previewImages[GetNewID(middleID - 1)].GetComponent<IDHolder>().realID =
                shop.CheckSelected(type, selectedID + 2);

            //dummy altes image zuweisen & nach links bewegen
            previewImages[3].transform.position = previewPositions[0];
            previewImages[3].GetComponent<Image>().sprite =
                previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite;
            previewImages[3].transform.DOMoveX(previewPositions[0].x - 125, time);

            previewImages[3].gameObject.SetActive(true);

            newRight = shop.GetSprite(type, selectedID + 2);

            selectedID = shop.CheckSelected(type, selectedID + 1);

            previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite =
                newRight;

            middleID = GetNewID(middleID + 1);
            FetchPurchased(type, selectedID);
        }

        if(type == CustomizationType.Skin)
        { //wing zurücksetzen & hat laden bei skinänderung
            shop.ResetSelected(selectedID);
        } else if(type == CustomizationType.Hat)
        {
            if(shop.HasPurchased(type, selectedID) == 2)
            { //wenn gekauft hut zuweisen
                shop.ApplyCustom(type, selectedID);
            }
        }

        CheckWingSupport();
        CheckHatSupport();
        UpdateSale(type);

        StartCoroutine(EndSwitch(dir));
    }

    private void CheckHatSupport()
    {
        if(type == CustomizationType.Skin)
        {
            if(shop.HasHatSupport(selectedID))
            {
                hatDisabled.gameObject.SetActive(false);
                hatDisabled.parent.GetChild(0).GetComponent<Button>().interactable = true;
                hatPreview.gameObject.SetActive(true);

                UpdateHat();
            } else
            {
                hatDisabled.gameObject.SetActive(true);
                hatDisabled.parent.GetChild(0).GetComponent<Button>().interactable = false;
                hatPreview.gameObject.SetActive(false);
            }
        }
    }

    private void CheckWingSupport()
    {
        if (type == CustomizationType.Skin)
        {
            if (shop.HasWingSupport(selectedID))
            {
                if(shop.allSkins[selectedID].overrideWing == null)
                { //wing support nur wenn kein wing override festgelegt
                    wingDisabled.gameObject.SetActive(false);
                    wingDisabled.parent.GetChild(0).GetComponent<Button>().interactable = true;
                    wingPreview.gameObject.SetActive(true);
                } else
                {
                    wingDisabled.gameObject.SetActive(true);
                    wingDisabled.parent.GetChild(0).GetComponent<Button>().interactable = false;
                    //wingPreview.gameObject.SetActive(false);
                }

                UpdateWing(selectedID);
            }
            else
            {
                wingDisabled.gameObject.SetActive(true);
                wingDisabled.parent.GetChild(0).GetComponent<Button>().interactable = false;
                wingPreview.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator EndSwitch(int dir = 0)
    { //0 = links
        yield return new WaitForSeconds(time + 0.01f);

        bool found = false;
        for(int i = 0; i < smallPreviewParent.childCount; i++)
        {
            if(smallPreviewParent.GetChild(i).GetComponent<IDHolder>().realID == selectedID &&
                smallPreviewParent.GetChild(i).GetChild(0).gameObject.activeSelf)
            {
                found = true;
                smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.black;
            } else
            {
                smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.white;
            }

        }

        if(!found)
        {
            UpdateSmallPreviews(dir);
        }

        switchRunning = false;
        previewImages[3].gameObject.SetActive(false);
    }

    private void SetPrice(CostData[] prices, bool showBuy = true)
    {
        //currentPrices = prices;

        float fadeInTime = 0.3f;

        for (int i = 0; i < 4; i++)
        {
            if (prices[i].amount > 0)
            {
                ulong collectedAmount =
                    (ulong)Inventory.Instance.GetMineralAmount((int)prices[i].mineralID);

                if (prices[i].mineralID == MineralType.Coin)
                {
                    collectedAmount = shop.GetBlus();
                }

                //collectedAmount = Mathf.Clamp(collectedAmount, 0, prices[i].amount);

                if (collectedAmount >= (ulong)prices[i].amount)
                {
                    collectedAmount = (ulong)prices[i].amount;
                }

                if (priceTextTween[i] != null)
                {
                    priceTextTween[i].Kill();
                }

                Color tC = Color.green;
                tC.a = 0;

                priceText.transform.GetChild(i).GetComponent<TextMeshProUGUI>().color =
                    tC;
                priceTextTween[i] =
                    priceText.transform.GetChild(i).GetComponent<TextMeshProUGUI>().
                    DOFade(1, fadeInTime);

                priceText.transform.GetChild(i).gameObject.SetActive(true);
                priceText.transform.GetChild(i).GetComponent<TextMeshProUGUI>().text =
                    collectedAmount + "/" + prices[i].amount.ToString();

                if (priceImageTween[i] != null)
                {
                    priceImageTween[i].Kill();
                }

                tC = Color.white;
                tC.a = 0;

                priceImage.transform.GetChild(i).GetComponent<Image>().color =
                    tC;
                priceImageTween[i] =
                    priceImage.transform.GetChild(i).GetComponent<Image>().
                    DOFade(1, fadeInTime);

                priceImage.transform.GetChild(i).gameObject.SetActive(true);

                if (Inventory.Instance.allMinerals[(int)prices[i].mineralID].shopSprite == null)
                {
                    priceImage.transform.GetChild(i).GetComponent<Image>().sprite =
                        Inventory.Instance.allMinerals[(int)prices[i].mineralID].sprite;
                }
                else
                {
                    priceImage.transform.GetChild(i).GetComponent<Image>().sprite =
                        Inventory.Instance.allMinerals[(int)prices[i].mineralID].shopSprite;
                }
            }
            else
            {
                priceText.transform.GetChild(i).gameObject.SetActive(false);
                priceImage.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator NotEnoughMoney()
    {
        bool ok = false;

        float max = 10;
        float time = 0.025f;

        for(int i = 0; i < 10; i++)
        {
            Vector3 temp = pricePos;

            if(ok)
            {
                temp.x -= max;
                ok = false;
            } else
            {
                temp.x += max;
                ok = true;
            }

            priceParent.position = temp;

            yield return new WaitForSeconds(time);
        }

        priceParent.position = pricePos;
    }

    public void BuyClicked()
    { 
        if(buyCode == 0)
        {
            if(noMoneyRoutine != null)
            {
                StopCoroutine(noMoneyRoutine);
                priceParent.position = pricePos;
            }

            SoundManager.Instance.PlaySound(Sound.MenuError);

            noMoneyRoutine = StartCoroutine(NotEnoughMoney());
            return;
        }

        priceImage.SetActive(false);
        priceText.SetActive(false);

        buyButton.GetComponent<Image>().color = Color.white;
        buyButton.GetComponent<Button>().interactable = false;
        buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            ShopHandler.Instance.boughtString;

        shop.BuyCustom(type, selectedID);

        buyInfo.SetActive(true);
    }

    private void _MainUpdate()
    {
        Skin pSkin;

        for(int i = 0; i < shop.allSkins.Count; i++)
        {
            pSkin = shop.allSkins[i];

            if (pSkin.animated)
            {
                pSkin.shopTime += Time.deltaTime;

                if (pSkin.shopTime >= pSkin.animationSpeed)
                { //sprite update
                    pSkin.shopTime = 0;

                    pSkin.shopStep++;
                    if (pSkin.shopStep >= pSkin.animatedSprites.Length)
                    {
                        pSkin.shopStep = 0;
                    }

                    if(i == shop.GetSelectedSkin())
                    {
                        FF_PlayerData.Instance.OverrideSprite(pSkin.animatedSprites[pSkin.shopStep]);
                    }

                    if (skinPreview.gameObject.activeSelf)
                    {
                        if(i == skinPreview.GetComponent<IDHolder>().realID)
                        {
                            skinPreview.GetComponent<Image>().sprite = 
                                pSkin.animatedSprites[pSkin.shopStep];
                        }
                    }

                    if(type == CustomizationType.Skin)
                    {
                        for (int a = 0; a < smallPreviewParent.childCount; a++)
                        {
                            if (i == smallPreviewParent.GetChild(a).GetComponent<IDHolder>().realID)
                            {
                                smallPreviewParent.GetChild(a).GetChild(1).GetComponent<Image>().sprite =
                                    pSkin.animatedSprites[pSkin.shopStep];
                            }
                        }

                        for(int a = 0; a < 3; a++)
                        {
                            if(i == previewImages[a].GetComponent<IDHolder>().realID)
                            {
                                previewImages[a].GetComponent<Image>().sprite =
                                    pSkin.animatedSprites[pSkin.shopStep];
                            }
                        }
                    }
                }
            }
        }

        Hat cHat;

        for(int i = 0; i < shop.allHats.Count; i++)
        {
            cHat = shop.allHats[i];

            if (cHat.animated)
            {
                cHat.shopTime += Time.deltaTime;

                if (cHat.shopTime >= cHat.animationSpeed)
                { //sprite update
                    cHat.shopTime = 0;

                    cHat.shopStep++;
                    if (cHat.shopStep >= cHat.animatedSprites.Length)
                    {
                        cHat.shopStep = 0;
                    }

                    if(i == shop.GetSelectedHat())
                    {
                        FF_PlayerData.Instance.OverrideHatSprite(cHat.animatedSprites[cHat.shopStep]);
                    }

                    if(hatPreview.gameObject.activeSelf)
                    {
                        if (i == hatPreview.GetComponent<IDHolder>().realID)
                        {
                            hatPreview.GetComponent<Image>().sprite =
                                cHat.animatedSprites[cHat.shopStep];
                        }
                    }

                    if (type == CustomizationType.Hat)
                    {
                        for (int a = 0; a < smallPreviewParent.childCount; a++)
                        {
                            if (i == smallPreviewParent.GetChild(a).GetComponent<IDHolder>().realID)
                            {
                                smallPreviewParent.GetChild(a).GetChild(1).GetComponent<Image>().sprite =
                                    cHat.animatedSprites[cHat.shopStep];
                            }
                        }

                        for (int a = 0; a < 3; a++)
                        {
                            if (i == previewImages[a].GetComponent<IDHolder>().realID)
                            {
                                previewImages[a].GetComponent<Image>().sprite =
                                    cHat.animatedSprites[cHat.shopStep];
                            }
                        }
                    }
                }
            }
        }
    }
}
