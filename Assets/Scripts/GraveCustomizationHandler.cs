using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using TMPro;

public class GraveCustomizationHandler : MonoBehaviour
{
    [SerializeField]
    private ShopHandler shop = null;
    [SerializeField]
    private ShopMenuHandler shopMenu = null;
    [SerializeField]
    private Transform topParent = null, sideParent = null, bottomParent = null,
        previewTop = null, previewSideLeft = null, previewSideRight = null, previewBottom = null,
        previewParent = null, priceParent = null;
    [SerializeField]
    private GameObject saleObj = null, priceImage = null, priceText = null, buyButton = null, buyInfo = null;
    [SerializeField]
    private Material fontMat = null, imageMat = null;

    private CustomizationType type;
    private int selectedID = 0;

    private Tween[] priceTextTween = new Tween[4];
    private Tween[] priceImageTween = new Tween[4];

    private Vector3 pricePos;
    private Coroutine saleRoutine, noMoneyRoutine;
    private bool switchRunning = false, changeApplied = false, interaction = false;
    private int buyCode = 0;
    private float dissolveAmount = 0;

    private void Awake()
    {
        pricePos = priceParent.transform.position;
    }

    public void StartGraveCustomizationHandler()
    {
        switchRunning = true;

        UpdateUI();

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

        StartCoroutine(EndStart());
    }

    private IEnumerator EndStart()
    {
        yield return new WaitForSeconds(ShopMenuHandler.anTime + 0.01f);

        switchRunning = false;
    }

    public void CloseButtonClicked()
    {
        switchRunning = true;

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

        if (saleRoutine != null)
        {
            StopCoroutine(saleRoutine);
        }

        //saleObj.SetActive(false);

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
        //Änderungen werden direkt applied da nicht sichtbar -> hier kein apply

        interaction = false;

        if (disable)
        {
            this.gameObject.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < topParent.childCount - 1; i++) //-1 weil letztes text
        { //alles zurücksetzen
            topParent.GetChild(i).GetChild(0).GetComponent<Button>().enabled = false;
            topParent.GetChild(i).GetChild(1).gameObject.SetActive(false);

            sideParent.GetChild(i).GetChild(0).GetComponent<Button>().enabled = false;
            sideParent.GetChild(i).GetChild(1).gameObject.SetActive(false);

            bottomParent.GetChild(i).GetChild(0).GetComponent<Button>().enabled = false;
            bottomParent.GetChild(i).GetChild(1).gameObject.SetActive(false);
        }

        for(int a = 0; a < 3; a++)
        {
            Transform mainParent = topParent;
            List<ShopItem> mainList = shop.allGraveTops.Cast<ShopItem>().ToList();

            if (a == 1)
            {
                mainParent = sideParent;
                mainList = shop.allGraveSides.Cast<ShopItem>().ToList();
            } else if(a == 2)
            {
                mainParent = bottomParent;
                mainList = shop.allGraveBottoms.Cast<ShopItem>().ToList();
            }

            for (int i = 0; i < mainList.Count; i++) //-1 weil letztes text
            {
                Sprite sprite;
                ShopItem temp = mainList[i];

                switch(a)
                {
                    default:
                    case 0:
                        sprite = ((GraveTop)temp).sprite;
                        break;
                    case 1:
                        sprite = ((GraveSide)temp).sprite;
                        break;
                    case 2:
                        sprite = ((GraveBottom)temp).sprite;
                        break;
                }

                mainParent.GetChild(i).GetChild(0).GetComponent<Button>().enabled = true;
                mainParent.GetChild(i).GetChild(1).gameObject.SetActive(true);
                mainParent.GetChild(i).GetChild(1).GetComponent<Image>().sprite = sprite;

                Color32 c = mainParent.GetChild(i).GetChild(1).GetComponent<Image>().color;

                if (temp.purchased)
                {
                    c.a = 255;
                }
                else
                {
                    c.a = 168;
                }

                mainParent.GetChild(i).GetChild(1).GetComponent<Image>().color = c;
            }
        }

        SetSelected(CustomizationType.GraveBottom, shop.GetSelected(CustomizationType.GraveBottom));
        SetSelected(CustomizationType.GraveSide, shop.GetSelected(CustomizationType.GraveSide));
        SetSelected(CustomizationType.GraveTop, shop.GetSelected(CustomizationType.GraveTop));

        FetchPurchased(CustomizationType.GraveTop, selectedID);

        UpdatePreview();
    }

    private void UpdatePreview(int customTop = -1, int customSide = -1, int customBottom = -1)
    {
        GraveTop top = shop.allGraveTops[shop.GetSelected(CustomizationType.GraveTop)];
        GraveSide side = shop.allGraveSides[shop.GetSelected(CustomizationType.GraveSide)];
        GraveBottom bottom = shop.allGraveBottoms[shop.GetSelected(CustomizationType.GraveBottom)];

        if(customTop != -1)
        {
            top = shop.allGraveTops[customTop];
        }

        if(customSide != -1)
        {
            side = shop.allGraveSides[customSide];
        }

        if(customBottom != -1)
        {
            bottom = shop.allGraveBottoms[customBottom];
        }

        if(top == null)
        {
            Debug.Log("null");
        }

        previewTop.GetComponent<Image>().sprite = top.sprite;
        previewSideLeft.GetComponent<Image>().sprite = side.sprite;
        previewSideRight.GetComponent<Image>().sprite = side.sprite;
        previewBottom.GetComponent<Image>().sprite = bottom.sprite;

        ApplyOffset(previewParent, previewBottom, bottom.menuOffset);
        ApplyOffset(previewBottom, previewTop, top.menuOffset);
        ApplyOffset(previewTop, previewSideLeft, top.wingMenuOffset[0]);
        ApplyOffset(previewTop, previewSideRight, top.wingMenuOffset[1]);
    }

    private void ApplyOffset(Transform baseObj, Transform target, Vector3 offset)
    {
        Vector3 pos = baseObj.transform.position;
        pos.x += offset.x * 2.8f;
        pos.y += offset.y * 2.8f;
        pos.z += offset.z * 2.8f;

        target.transform.position = pos;
    }

    private void SetSelected(CustomizationType type, int id, bool onlyColor = false)
    {
        Transform parent = topParent;

        if(type == CustomizationType.GraveSide)
        {
            parent = sideParent;
        } else if(type == CustomizationType.GraveBottom)
        {
            parent = bottomParent;
        }

        if(!onlyColor)
        {
            if(this.type != type)
            {
                SetSelected(this.type, shop.GetSelected(this.type), true);
            }

            this.type = type;
            selectedID = id;
        }

        for(int i = 0; i < parent.childCount - 1; i++)
        {
            if(i == id)
            {
                parent.GetChild(i).GetComponent<Image>().color = Color.black;
            } else
            {
                parent.GetChild(i).GetComponent<Image>().color = Color.white;
            }
        }

        if(!onlyColor)
        {
            switch (type)
            {
                case CustomizationType.GraveTop:
                    UpdatePreview(selectedID);
                    break;
                case CustomizationType.GraveSide:
                    UpdatePreview(-1, selectedID);
                    break;
                case CustomizationType.GraveBottom:
                    UpdatePreview(-1, -1, selectedID);
                    break;
            }
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

        if (code < 2)
        {
            priceImage.SetActive(true);
            priceText.SetActive(true);

            SetPrice(shop.GetCost(type, id));
        }
        else
        {
            shop.ApplyCustom(type, id);

            priceImage.SetActive(false);
            priceText.SetActive(false);
        }
    }

    public void BuyClicked()
    {
        if (buyCode == 0)
        {
            if (noMoneyRoutine != null)
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

        for (int i = 0; i < 10; i++)
        {
            Vector3 temp = pricePos;

            if (ok)
            {
                temp.x -= max;
                ok = false;
            }
            else
            {
                temp.x += max;
                ok = true;
            }

            priceParent.position = temp;

            yield return new WaitForSeconds(time);
        }

        priceParent.position = pricePos;
    }

    public void TopClicked(int id)
    {
        SetSelected(CustomizationType.GraveTop, id);
        FetchPurchased(CustomizationType.GraveTop, id);
    }

    public void SideClicked(int id)
    {
        SetSelected(CustomizationType.GraveSide, id);
        FetchPurchased(CustomizationType.GraveSide, id);
    }

    public void BottomClicked(int id)
    {
        SetSelected(CustomizationType.GraveBottom, id);
        FetchPurchased(CustomizationType.GraveBottom, id);
    }
}
