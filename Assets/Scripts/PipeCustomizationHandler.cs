using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using TMPro;

public class PipeCustomizationHandler : MonoBehaviour
{
    public Color[] allColors = new Color[25];
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
        colorDisabled = null,
        skinPreview = null,
        wingPreview = null,
        typeParent = null,
        smallPreviewParent = null;

    [SerializeField]
    private GameObject priceText, priceImage;

    private Tween[] priceTextTween = new Tween[4];
    private Tween[] priceImageTween = new Tween[4];

    private CustomizationType type = CustomizationType.Pipe;
    private float time = 0.25f, bigScale = 1.43f, smallScale = 0.63f;
    private int middleID = 1;
    private bool switchRunning = false, changeApplied = false, interaction = false;
    private ObscuredInt selectedID = 0;

    public static PipeCustomizationHandler Instance;

    private void Awake()
    {
        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        smallPreviewParent.GetChild(0).GetComponent<Image>().color = Color.green;
        TypeClicked((int)type);
        swDetector.enabled = true;
        changeApplied = true;
    }

    public void SetType(CustomizationType newType, bool start = false)
    {
        this.type = newType;

        if (start)
        {
            switchRunning = false;
        }

        switch (type)
        {
            case CustomizationType.Pipe:
                selectedID = shop.GetSelectedPipe();
                break;
            case CustomizationType.PipeColor:
                selectedID = shop.GetPipeColorID();
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
        if (swDetector != null)
        {
            swDetector.enabled = false;
        }

        if (!changeApplied && interaction)
        {
            changeApplied = true;
            shop.ApplyCustom(type, selectedID);
        }

        interaction = false;

        if(disable)
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

    public Color GetPipeColor(int id)
    {
        if (id < 0) id = allColors.Length + id;
        if (id >= allColors.Length) id = id - allColors.Length;

        return allColors[id];
    }

    private int CheckCustomSelected(CustomizationType type, int id)
    {
        switch(type)
        {
            case CustomizationType.PipeColor:
                if (id < 0) id = allColors.Length + id;
                if (id >= allColors.Length) id = id - allColors.Length;
                break;
        }

        return id;
    }

    public void UpdateDisplay(CustomizationType type)
    {
        previewImages[middleID].localScale = new Vector3(bigScale, bigScale, bigScale);

        Sprite newLeft = null, newMiddle = null, newRight = null,
            leftEnd = null, middleEnd = null, rightEnd = null;

        Color leftColor = Color.white,
            middleColor = Color.white,
            rightColor = Color.white;

        bool interactable = true;

        CostData[] cost = null;
        bool purchased = false;

        switch (type)
        {
            case CustomizationType.Pipe:
                selectedID = shop.GetSelectedPipe();

                cost = shop.allPipes[selectedID].cost;
                purchased = shop.allPipes[selectedID].purchased;

                newLeft = shop.GetPipeSprite(selectedID - 1);
                leftEnd = shop.GetPipeSprite(selectedID - 1, true);

                newMiddle = shop.GetPipeSprite(selectedID);
                middleEnd = shop.GetPipeSprite(selectedID, true);

                newRight = shop.GetPipeSprite(selectedID + 1);
                rightEnd = shop.GetPipeSprite(selectedID + 1, true);

                //Alle haben dieselbe Farbe da Skinauswahl
                if(shop.HasColorSupport(selectedID - 1))
                {
                    leftColor = allColors[shop.GetPipeColorID()];
                }

                if(shop.HasColorSupport(selectedID))
                {
                    middleColor = allColors[shop.GetPipeColorID()];
                }

                if(shop.HasColorSupport(selectedID + 1))
                {
                    rightColor = allColors[shop.GetPipeColorID()];
                }

                break;
            case CustomizationType.PipeColor:
                selectedID = shop.GetPipeColorID();

                purchased = true;

                interactable = false;

                //Alle dasselbe bild da sich nur Farbe ändert
                newLeft = shop.GetPipeSprite(shop.GetSelectedPipe());
                leftEnd = shop.GetPipeSprite(shop.GetSelectedPipe(), true);

                leftColor = GetPipeColor(selectedID - 1);

                newMiddle = newLeft;
                middleEnd = leftEnd;

                middleColor = GetPipeColor(selectedID);

                newRight = newMiddle;
                rightEnd = middleEnd;

                rightColor = GetPipeColor(selectedID + 1);
                break;
        }

        for (int i = 0; i < 2; i++)
        {
            previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite = leftEnd;
            previewImages[GetNewID(middleID - 1)].GetComponent<Image>().color = leftColor;

            previewImages[GetNewID(middleID - 1)].GetChild(i).GetComponent<Image>().sprite =
                newLeft;
            previewImages[GetNewID(middleID - 1)].GetChild(i).GetComponent<Image>().color =
                leftColor;

            previewImages[middleID].GetComponent<Image>().sprite = middleEnd;
            previewImages[middleID].GetComponent<Image>().color = middleColor;

            previewImages[middleID].GetChild(i).GetComponent<Image>().sprite =
                newMiddle;
            previewImages[middleID].GetChild(i).GetComponent<Image>().color =
                middleColor;

            previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite = rightEnd;
            previewImages[GetNewID(middleID + 1)].GetComponent<Image>().color =
                rightColor;

            previewImages[GetNewID(middleID + 1)].GetChild(i).GetComponent<Image>().sprite =
                newRight;
            previewImages[GetNewID(middleID + 1)].GetChild(i).GetComponent<Image>().color =
                rightColor;
        }

        for(int i = 0; i < smallPreviewParent.childCount; i++)
        {
            smallPreviewParent.GetChild(i).GetChild(0).GetComponent<Button>().interactable =
                interactable;
        }

        if (purchased)
        {
            priceImage.SetActive(false);
            priceText.SetActive(false);

            buyButton.GetComponent<Image>().color = Color.white;
            buyButton.GetComponent<Button>().interactable = false;
            buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                ShopHandler.Instance.boughtString;
        }
        else
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

        type = newType;
        UpdateDisplay(newType);
        UpdateSmallPreviews();
    }

    public void UpdateSmallPreviews(int dir = 1)
    {
        if(type == CustomizationType.PipeColor)
        {
            return;
        }

        int max = shop.GetMax(type);

        for (int i = 0; i < smallPreviewParent.childCount; i++)
        {
            int newID = i;

            if (smallPreviewParent.childCount < max)
            {
                if (dir == 1)
                {//rechts (start ist 0)
                    newID = shop.CheckSelected(type, selectedID + i);
                }
                else
                { //links (start ist pmax - 1)
                    newID = shop.CheckSelected(type,
                        (selectedID - (smallPreviewParent.childCount - 1)) + i);
                }
            }

            if (newID < max)
            {
                Sprite newSprite = null;

                switch (type)
                {
                    case CustomizationType.Pipe:
                        newSprite = shop.GetPipeSprite(newID, true);
                        break;
                }

                smallPreviewParent.GetChild(i).GetComponent<IDHolder>().realID = newID;

                if (newID == selectedID)
                {
                    smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.green;
                }
                else
                {
                    smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.white;
                }

                smallPreviewParent.GetChild(i).GetChild(0).gameObject.SetActive(true);
                smallPreviewParent.GetChild(i).GetChild(0).GetComponent<Image>().sprite =
                    newSprite;
            }
            else
            {
                smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.white;
                smallPreviewParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }

        }
    }

    public void SmallPreviewClicked(int newID)
    {
        if (switchRunning || 
            type == CustomizationType.PipeColor) return;

        interaction = true;

        int realID =
            smallPreviewParent.GetChild(newID).GetComponent<IDHolder>().realID;

        for (int i = 0; i < smallPreviewParent.childCount; i++)
        {
            smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.white;
        }

        smallPreviewParent.GetChild(newID).GetComponent<Image>().color = Color.green;

        changeApplied = false;

        selectedID = realID;

        Sprite newLeft = null, newMiddle = null, newRight = null,
            leftEnd = null, middleEnd = null, rightEnd = null;

        Color leftColor = Color.white, middleColor = Color.white,
            rightColor = Color.white;

        switch (type)
        {
            case CustomizationType.Pipe:
                newLeft = shop.GetPipeSprite(selectedID - 1);
                leftEnd = shop.GetPipeSprite(selectedID - 1, true);

                if(shop.HasColorSupport(selectedID - 1))
                {
                    leftColor = allColors[shop.GetPipeColorID()];
                }

                newMiddle = shop.GetPipeSprite(selectedID);
                middleEnd = shop.GetPipeSprite(selectedID, true);

                if (shop.HasColorSupport(selectedID))
                {
                    middleColor = allColors[shop.GetPipeColorID()];
                }

                newRight = shop.GetPipeSprite(selectedID + 1);
                rightEnd = shop.GetPipeSprite(selectedID + 1, true);

                if (shop.HasColorSupport(selectedID + 1))
                {
                    rightColor = allColors[shop.GetPipeColorID()];
                }

                break;
        }

        for(int i = 0; i < 2; i++)
        {
            previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite = leftEnd;
            previewImages[GetNewID(middleID - 1)].GetComponent<Image>().color = leftColor;

            previewImages[GetNewID(middleID - 1)].GetChild(i).GetComponent<Image>().sprite =
                newLeft;
            previewImages[GetNewID(middleID - 1)].GetChild(i).GetComponent<Image>().color =
                leftColor;

            previewImages[middleID].GetComponent<Image>().sprite = middleEnd;
            previewImages[middleID].GetComponent<Image>().color = middleColor;

            previewImages[middleID].GetChild(i).GetComponent<Image>().sprite =
                newMiddle;
            previewImages[middleID].GetChild(i).GetComponent<Image>().color =
                middleColor;

            previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite = rightEnd;
            previewImages[GetNewID(middleID + 1)].GetComponent<Image>().color = rightColor;

            previewImages[GetNewID(middleID + 1)].GetChild(i).GetComponent<Image>().sprite =
                newRight;
            previewImages[GetNewID(middleID + 1)].GetChild(i).GetComponent<Image>().color =
                rightColor;
        }

        FetchPurchased(type, selectedID);
        CheckPipeColorSupport();
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
            ShopHandler.Instance.buyString;

        switch (code)
        {
            case 0: //nicht gekauft & kein geld
                buyButton.GetComponent<Image>().color = Color.red;

                break;
            case 1: //nicht gekauft & geld
                buyButton.GetComponent<Image>().color = Color.green;
                buyButton.GetComponent<Button>().interactable = true;
                break;
            case 2: //schon gekauft
                buyButton.GetComponent<Image>().color = Color.white;
                buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    ShopHandler.Instance.boughtString;
                break;
        }

        if (code < 2)
        {
            priceImage.SetActive(true);
            priceText.SetActive(true);

            SetPrice(shop.GetCost(type, id));
        }
        else
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

        Sprite newLeft = null, leftEnd = null, newRight = null, rightEnd = null;

        changeApplied = false;

        if (dir == 0)
        { //nach links -> previews nach rechts

            //linkes preview in mitte bewegen & upscalen
            previewImages[GetNewID(middleID - 1)].transform.DOMove(previewPositions[1], time);
            previewImages[GetNewID(middleID - 1)].transform.DOScale(bigScale, time);

            //mittleres preview nach rechts & runterscalen
            previewImages[middleID].transform.DOMove(previewPositions[2], time);
            previewImages[middleID].transform.DOScale(smallScale, time);

            //rechtes preview an linke pos & dummy an rechte pos & beide x + 125
            Vector3 leftStartPos = previewPositions[0];
            leftStartPos.x -= 125;
            previewImages[GetNewID(middleID + 1)].transform.position = leftStartPos;
            previewImages[GetNewID(middleID + 1)].DOMove(previewPositions[0], time);

            //dummy altes image zuweisen & nach rechts bewegen
            previewImages[3].transform.position = previewPositions[2];

            previewImages[3].GetComponent<Image>().sprite =
                previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite;
            previewImages[3].GetComponent<Image>().color =
                previewImages[GetNewID(middleID + 1)].GetComponent<Image>().color;

            previewImages[3].GetChild(0).GetComponent<Image>().sprite =
                previewImages[GetNewID(middleID + 1)].GetChild(0).GetComponent<Image>().sprite;
            previewImages[3].GetChild(0).GetComponent<Image>().color =
                previewImages[GetNewID(middleID + 1)].GetChild(0).GetComponent<Image>().color;

            previewImages[3].GetChild(1).GetComponent<Image>().sprite =
                previewImages[GetNewID(middleID + 1)].GetChild(1).GetComponent<Image>().sprite;
            previewImages[3].GetChild(1).GetComponent<Image>().color =
                previewImages[GetNewID(middleID + 1)].GetChild(1).GetComponent<Image>().color;

            previewImages[3].transform.DOMoveX(previewPositions[2].x + 125, time);

            previewImages[3].gameObject.SetActive(true);

            Color newColor = Color.white;

            switch (type)
            {
                case CustomizationType.Pipe:
                    newLeft = shop.GetPipeSprite(selectedID - 2);
                    leftEnd = shop.GetPipeSprite(selectedID - 2, true);

                    if (shop.HasColorSupport(selectedID - 2))
                    { //Farbe nur zuweisen wenn unterstützt
                        newColor = allColors[shop.GetPipeColorID()];
                    }
                    else
                    {
                        newColor = Color.white;
                    }
                    break;
                case CustomizationType.PipeColor:
                    newLeft = shop.GetPipeSprite(shop.GetSelectedPipe());
                    leftEnd = shop.GetPipeSprite(shop.GetSelectedPipe(), true);

                    newColor = allColors[CheckCustomSelected(type, selectedID - 2)];

                    break;
            }

            if(type != CustomizationType.PipeColor)
            {
                selectedID = shop.CheckSelected(type, selectedID - 1);
            } else
            {
                selectedID = CheckCustomSelected(type, selectedID - 1);
            }

            previewImages[GetNewID(middleID + 1)].GetComponent<Image>().sprite =
                leftEnd;
            previewImages[GetNewID(middleID + 1)].GetComponent<Image>().color =
                newColor;

            previewImages[GetNewID(middleID + 1)].GetChild(0).GetComponent<Image>().sprite =
                newLeft;
            previewImages[GetNewID(middleID + 1)].GetChild(0).GetComponent<Image>().color =
                newColor;

            previewImages[GetNewID(middleID + 1)].GetChild(1).GetComponent<Image>().sprite =
                newLeft;
            previewImages[GetNewID(middleID + 1)].GetChild(1).GetComponent<Image>().color =
                newColor;

            middleID = GetNewID(middleID - 1);

            FetchPurchased(type, selectedID);
        }
        else if (dir == 1)
        { //nach rechts -> previews nach links

            //rechtes preview in mitte bewegen & upscalen
            previewImages[GetNewID(middleID + 1)].transform.DOMove(previewPositions[1], time);
            previewImages[GetNewID(middleID + 1)].transform.DOScale(bigScale, time);

            //mittleres preview nach links & runterscalen
            previewImages[middleID].transform.DOMove(previewPositions[0], time);
            previewImages[middleID].transform.DOScale(smallScale, time);

            //linkes preview an rechte pos & dummy an linke pos & beide x - 125
            Vector3 rightStartPos = previewPositions[2];
            rightStartPos.x += 125;
            previewImages[GetNewID(middleID - 1)].transform.position = rightStartPos;
            previewImages[GetNewID(middleID - 1)].DOMove(previewPositions[2], time);

            //dummy altes image zuweisen & nach links bewegen
            previewImages[3].transform.position = previewPositions[0];

            previewImages[3].GetComponent<Image>().sprite =
                previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite;
            previewImages[3].GetComponent<Image>().color =
                previewImages[GetNewID(middleID - 1)].GetComponent<Image>().color;

            previewImages[3].GetChild(0).GetComponent<Image>().sprite =
                previewImages[GetNewID(middleID - 1)].GetChild(0).GetComponent<Image>().sprite;
            previewImages[3].GetChild(0).GetComponent<Image>().color =
                previewImages[GetNewID(middleID - 1)].GetChild(0).GetComponent<Image>().color;

            previewImages[3].GetChild(1).GetComponent<Image>().sprite =
                previewImages[GetNewID(middleID - 1)].GetChild(1).GetComponent<Image>().sprite;
            previewImages[3].GetChild(1).GetComponent<Image>().color =
                previewImages[GetNewID(middleID - 1)].GetChild(1).GetComponent<Image>().color;

            previewImages[3].transform.DOMoveX(previewPositions[0].x - 125, time);

            previewImages[3].gameObject.SetActive(true);

            Color newColor = Color.white;

            switch (type)
            {
                case CustomizationType.Pipe:
                    newRight = shop.GetPipeSprite(selectedID + 2);
                    rightEnd = shop.GetPipeSprite(selectedID + 2, true);

                    if (shop.HasColorSupport(selectedID + 2))
                    { //Farbe nur zuweisen wenn unterstützt
                        newColor = allColors[shop.GetPipeColorID()];
                    }
                    else
                    {
                        newColor = Color.white;
                    }
                    break;
                case CustomizationType.PipeColor:
                    newRight = shop.GetPipeSprite(shop.GetSelectedPipe());
                    rightEnd = shop.GetPipeSprite(shop.GetSelectedPipe(), true);

                    newColor = allColors[CheckCustomSelected(type, selectedID + 2)];
                    break;
            }

            if (type != CustomizationType.PipeColor)
            {
                selectedID = shop.CheckSelected(type, selectedID + 1);
            }
            else
            {
                selectedID = CheckCustomSelected(type, selectedID + 1);
            }

            previewImages[GetNewID(middleID - 1)].GetComponent<Image>().sprite =
                rightEnd;
            previewImages[GetNewID(middleID - 1)].GetComponent<Image>().color =
                newColor;

            previewImages[GetNewID(middleID - 1)].GetChild(0).GetComponent<Image>().sprite =
                newRight;
            previewImages[GetNewID(middleID - 1)].GetChild(0).GetComponent<Image>().color =
                newColor;

            previewImages[GetNewID(middleID - 1)].GetChild(1).GetComponent<Image>().sprite =
                newRight;
            previewImages[GetNewID(middleID - 1)].GetChild(1).GetComponent<Image>().color =
                newColor;

            middleID = GetNewID(middleID + 1);
            FetchPurchased(type, selectedID);
        }

        CheckPipeColorSupport();

        StartCoroutine(EndSwitch());
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

    private void CheckPipeColorSupport()
    {
        if (type == CustomizationType.Pipe)
        {
            if (shop.HasColorSupport(selectedID))
            {
                colorDisabled.gameObject.SetActive(false);
                colorDisabled.parent.GetChild(0).GetComponent<Button>().interactable = true;
            }
            else
            {
                colorDisabled.gameObject.SetActive(true);
                colorDisabled.parent.GetChild(0).GetComponent<Button>().interactable = false;
            }
        }
    }

    IEnumerator EndSwitch(int dir = 0)
    { //0 = links
        yield return new WaitForSeconds(time + 0.01f);

        if(type != CustomizationType.PipeColor)
        {
            bool found = false;
            for (int i = 0; i < smallPreviewParent.childCount; i++)
            {
                if (smallPreviewParent.GetChild(i).GetComponent<IDHolder>().realID == selectedID &&
                    smallPreviewParent.GetChild(i).GetChild(0).gameObject.activeSelf)
                {
                    found = true;
                    smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.green;
                }
                else
                {
                    smallPreviewParent.GetChild(i).GetComponent<Image>().color = Color.white;
                }

            }

            if (!found)
            {
                UpdateSmallPreviews(dir);
                if (dir == 0)
                {
                    smallPreviewParent.GetChild(smallPreviewParent.childCount - 1).
                        GetComponent<Image>().color = Color.green;
                }
                else
                {
                    smallPreviewParent.GetChild(0).GetComponent<Image>().color = Color.green;
                }
            }
        }

        switchRunning = false;
        previewImages[3].gameObject.SetActive(false);
    }

    public void BuyClicked()
    { //kann nur aufgerufen werden wenn noch nicht gekauft & genug geld
        priceImage.SetActive(false);
        priceText.SetActive(false);

        buyButton.GetComponent<Image>().color = Color.white;
        buyButton.GetComponent<Button>().interactable = false;
        buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            ShopHandler.Instance.boughtString;

        shop.BuyCustom(type, selectedID);
    }
}
