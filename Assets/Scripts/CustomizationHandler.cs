using System.Collections;
using System.Collections.Generic;
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
        skinPreview = null,
        wingPreview = null,
        typeParent = null,
        smallPreviewParent = null;

    [SerializeField]
    private GameObject priceText, priceImage;

    private CustomizationType type = CustomizationType.Skin;
    private float time = 0.25f;
    private int middleID = 1;
    private bool switchRunning = false, changeApplied = false, interaction = false;
    private ObscuredInt selectedID = 0;

    private Tween[] priceTextTween = new Tween[4];
    private Tween[] priceImageTween = new Tween[4];

    private void Awake()
    {
        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
    }

    // Start is called before the first frame update
    void Start()
    {
        smallPreviewParent.GetChild(0).GetComponent<Image>().color = Color.green;
        TypeClicked((int)type);
        swDetector.enabled = true;
        changeApplied = true;
    }

    public void SetType(CustomizationType newType)
    {
        this.type = newType;

        switch(type)
        {
            case CustomizationType.Skin:
                selectedID = shop.GetSelectedSkin();
                break;
            case CustomizationType.Wing:
                selectedID = shop.GetSelectedWing();
                break;
        }

        TypeClicked((int)newType);
    }

    public void CloseButtonClicked()
    {
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

        if (disable)
        {
            this.gameObject.SetActive(false);
        }
    }

    private int GetNewID(int id)
    {
        if (id < 0) id = 2;
        if (id > 2) id = 0;

        return id;
    }

    public void UpdateDisplay(CustomizationType type)
    {
        Sprite left = null, middle = null, right = null;
        int id = 0;

        previewImages[middleID].localScale = new Vector3(3.333f, 3.333f, 3.333f);

        CostData[] cost = null;
        bool purchased = false;

        switch(type)
        {
            case CustomizationType.Skin:
                id = shop.GetSelectedSkin();

                cost = shop.allSkins[id].cost;
                purchased = shop.allSkins[id].purchased;

                left = shop.GetSkinSprite(id - 1);
                middle = shop.GetSkinSprite(id);
                right = shop.GetSkinSprite(id + 1);

                skinPreview.GetComponent<Image>().sprite = middle;

                break;
            case CustomizationType.Wing:
                id = shop.GetSelectedWing();
                float scale = shop.GetWingScale(id);

                cost = shop.allWings[id].cost;
                purchased = shop.allWings[id].purchased;

                left = shop.GetWingSprite(id - 1);
                middle = shop.GetWingSprite(id);
                right = shop.GetWingSprite(id + 1);

                wingPreview.GetComponent<Image>().sprite = middle;

                wingPreview.localScale = new Vector3(scale, scale, scale);
                previewImages[middleID].localScale = new Vector3(scale, scale, scale);

                break;
            case CustomizationType.Pipe:
                id = shop.GetSelectedPipe();

                left = shop.GetPipeSprite(id - 1);
                middle = shop.GetPipeSprite(id);
                right = shop.GetPipeSprite(id + 1);


                break;
        }

        selectedID = id;

        previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite = left;
        previewImages[middleID].GetComponent<Image>().sprite = middle;
        previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite = right;

        if(purchased)
        {
            priceImage.SetActive(false);
            priceText.SetActive(false);

            buyButton.GetComponent<Image>().color = Color.white;
            buyButton.GetComponent<Button>().interactable = false;
            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "Gekauft";
        } else
        {
            priceImage.SetActive(true);
            priceText.SetActive(true);

            SetPrice(cost);
        }
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

        if(newType != CustomizationType.Skin)
        {
            skinPreview.GetComponent<Image>().sprite = 
                shop.GetSkinSprite(shop.GetSelectedSkin());
            skinPreview.gameObject.SetActive(true);
        } else
        {
            skinPreview.gameObject.SetActive(false);
        }

        if(newType != CustomizationType.Wing)
        {
            wingPreview.GetComponent<Image>().sprite =
                shop.GetWingSprite(shop.GetSelectedWing());

            float scale = shop.GetWingScale(shop.GetSelectedWing());
            wingPreview.localScale = new Vector3(scale, scale, scale);

            if(shop.HasWingSupport(shop.GetSelectedSkin()))
            {
                wingPreview.gameObject.SetActive(true);
            } else
            {
                wingPreview.gameObject.SetActive(false);
            }
        } else
        {
            wingPreview.gameObject.SetActive(false);
        }

        type = newType;
        UpdateDisplay(newType);
        UpdateSmallPreviews();
    }

    public void UpdateSmallPreviews(int dir = 1)
    {
        int max = shop.GetMax(type);

        for (int i = 0; i < smallPreviewParent.childCount; i++)
        {
            int newID = i;
            
            if(smallPreviewParent.childCount < max)
            {
                if(dir == 1)
                {//rechts (start ist 0)
                    newID = shop.CheckSelected(type, selectedID + i);
                } else
                { //links (start ist pmax - 1)
                    newID = shop.CheckSelected(type, 
                        (selectedID - (smallPreviewParent.childCount - 1)) + i);
                }
            }

            if(newID < max)
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
                }

                smallPreviewParent.GetChild(i).GetComponent<IDHolder>().realID = newID;

                smallPreviewParent.GetChild(i).GetChild(0).gameObject.SetActive(true);
                smallPreviewParent.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                    newSprite;
            } else
            {
                smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.white;
                smallPreviewParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
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

        smallPreviewParent.GetChild(newID).GetComponent<Image>().color = Color.green;

        changeApplied = false;

        selectedID = realID;

        Sprite newLeft = null, newMiddle = null, newRight = null;

        float bigScale = 3.333f;

        switch (type)
        {
            case CustomizationType.Skin:
                newLeft = shop.GetSkinSprite(selectedID - 1);
                newMiddle = shop.GetSkinSprite(selectedID);
                newRight = shop.GetSkinSprite(selectedID + 1);
                break;
            case CustomizationType.Wing:
                newLeft = shop.GetWingSprite(selectedID - 1);
                newMiddle = shop.GetWingSprite(selectedID);
                newRight = shop.GetWingSprite(selectedID + 1);

                bigScale = shop.GetWingScale(selectedID);
                break;
        }

        previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite = newLeft;
        previewImages[GetNewID(middleID - 1)].localScale = Vector3.one;

        previewImages[middleID].GetComponent<Image>().sprite = newMiddle;
        previewImages[middleID].localScale = new Vector3(bigScale, bigScale, bigScale);

        previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite = newRight;
        previewImages[GetNewID(middleID + 1)].localScale = Vector3.one;

        FetchPurchased(type, selectedID);
        CheckWingSupport();
    }

    public void SwipeDetector_OnSwipe(SwipeData data)
    {
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
            "Kaufen";

        switch (code)
        {
            case 0: //nicht gekauft & kein geld
                buyButton.GetComponent<Image>().color = Color.red;
                buyButton.GetComponent<Button>().interactable = false;
                break;
            case 1: //nicht gekauft & geld
                buyButton.GetComponent<Image>().color = Color.green;
                buyButton.GetComponent<Button>().interactable = true;
                break;
            case 2: //schon gekauft
                buyButton.GetComponent<Image>().color = Color.white;
                buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    "Gekauft";
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

        float bigScale = 3.333f;

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

        if(dir == 0)
        { //nach links -> previews nach rechts

            //linkes preview in mitte bewegen & upscalen
            previewImages[GetNewID(middleID - 1)].transform.DOMove(previewPositions[1], time);
            previewImages[GetNewID(middleID - 1)].transform.DOScale(bigScale, time);

            //mittleres preview nach rechts & runterscalen
            previewImages[middleID].transform.DOMove(previewPositions[2], time);
            previewImages[middleID].transform.DOScale(1, time);

            //rechtes preview an linke pos & dummy an rechte pos & beide x + 125
            Vector3 leftStartPos = previewPositions[0];
            leftStartPos.x -= 125;
            previewImages[GetNewID(middleID + 1)].transform.position = leftStartPos;
            previewImages[GetNewID(middleID + 1)].DOMove(previewPositions[0], time);

            //dummy altes image zuweisen & nach rechts bewegen
            previewImages[3].transform.position = previewPositions[2];
            previewImages[3].GetComponent<Image>().sprite = 
                previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite;
            previewImages[3].transform.DOMoveX(previewPositions[2].x + 125, time);

            previewImages[3].gameObject.SetActive(true);

            switch (type)
            {
                case CustomizationType.Skin:
                    newLeft = shop.GetSkinSprite(selectedID - 2);
                    break;
                case CustomizationType.Wing:
                    newLeft = shop.GetWingSprite(selectedID - 2);
                    break;
            }

            selectedID = shop.CheckSelected(type, selectedID - 1);

            previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite =
                newLeft;

            middleID = GetNewID(middleID - 1);

            FetchPurchased(type, selectedID);
        } else if(dir == 1)
        { //nach rechts -> previews nach links

          //rechtes preview in mitte bewegen & upscalen
            previewImages[GetNewID(middleID + 1)].transform.DOMove(previewPositions[1], time);
            previewImages[GetNewID(middleID + 1)].transform.DOScale(bigScale, time);

            //mittleres preview nach links & runterscalen
            previewImages[middleID].transform.DOMove(previewPositions[0], time);
            previewImages[middleID].transform.DOScale(1, time);

            //linkes preview an rechte pos & dummy an linke pos & beide x - 125
            Vector3 rightStartPos = previewPositions[2];
            rightStartPos.x += 125;
            previewImages[GetNewID(middleID - 1)].transform.position = rightStartPos;
            previewImages[GetNewID(middleID - 1)].DOMove(previewPositions[2], time);

            //dummy altes image zuweisen & nach links bewegen
            previewImages[3].transform.position = previewPositions[0];
            previewImages[3].GetComponent<Image>().sprite =
                previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite;
            previewImages[3].transform.DOMoveX(previewPositions[0].x - 125, time);

            previewImages[3].gameObject.SetActive(true);

            switch (type)
            {
                case CustomizationType.Skin:
                    newRight = shop.GetSkinSprite(selectedID + 2);
                    break;
                case CustomizationType.Wing:
                    newRight = shop.GetWingSprite(selectedID + 2);
                    break;
            }

            selectedID = shop.CheckSelected(type, selectedID + 1);

            previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite =
                newRight;

            middleID = GetNewID(middleID + 1);
            FetchPurchased(type, selectedID);
        }

        CheckWingSupport();

        StartCoroutine(EndSwitch());
    }

    private void CheckWingSupport()
    {
        if (type == CustomizationType.Skin)
        {
            if (shop.HasWingSupport(selectedID))
            {
                wingDisabled.gameObject.SetActive(false);
                wingDisabled.parent.GetChild(0).GetComponent<Button>().interactable = true;
                wingPreview.gameObject.SetActive(true);
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
                smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.green;
            } else
            {
                smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.white;
            }

        }

        if(!found)
        {
            UpdateSmallPreviews(dir);
            if(dir == 0)
            {
                smallPreviewParent.GetChild(smallPreviewParent.childCount - 1).
                    GetComponent<Image>().color = Color.green;
            } else
            {
                smallPreviewParent.GetChild(0).GetComponent<Image>().color = Color.green;
            }
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

                Color tC = Color.black;
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

    public void BuyClicked()
    { //kann nur aufgerufen werden wenn noch nicht gekauft & genug geld
        priceImage.SetActive(false);
        priceText.SetActive(false);

        buyButton.GetComponent<Image>().color = Color.white;
        buyButton.GetComponent<Button>().interactable = false;
        buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            "Gekauft";

        shop.BuyCustom(type, selectedID);
    }
}
