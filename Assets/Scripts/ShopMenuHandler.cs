using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Firebase.Analytics;

public class ShopMenuHandler : MonoBehaviour
{
    [SerializeField]
    private ShopHandler shop = null;
    [SerializeField]
    private CustomizationHandler customizationHandler = null;
    [SerializeField]
    private PipeCustomizationHandler pipeCustomizationHandler = null;
    [SerializeField]
    private MinerCustomizationHandler minerCustomizationHandler = null;
    [SerializeField]
    private GraveCustomizationHandler graveCustomizationHandler = null;
    [SerializeField]
    private Image skinImage = null, wingImage = null;
    [SerializeField]
    private Image miningSkinImage = null, minerImage = null;
    [SerializeField]
    private Image pipeImage = null;
    [SerializeField]
    private Image topGrave, sideLeftGrave, sideRightGrave, bottomGrave;

    [SerializeField]
    private Material fontMat = null, imageMat = null;

    public static float anTime = 0.25f;

    private float dissolveAmount = 1;
    private bool anRunning = false;

    public void OpenMenu(bool start = false)
    {
        gameObject.SetActive(true);

        FirebaseHandler.SetCurrentScreen("ShopMenu", "UnityPlayerActivity");

        if (start)
        {
            fontMat.SetFloat("_DissolveAmount", 1);
            imageMat.SetFloat("_DissolveAmount", 1);
            dissolveAmount = 1;
        } else
        { //animaion
            fontMat.SetFloat("_DissolveAmount", 0);
            imageMat.SetFloat("_DissolveAmount", 0);

            dissolveAmount = 0;

            Tween anTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, 1, anTime);
            anTween.OnUpdate(() =>
            {
                fontMat.SetFloat("_DissolveAmount", dissolveAmount);
                imageMat.SetFloat("_DissolveAmount", dissolveAmount);
            });
        }

        int selectedSkin = shop.GetSelectedSkin();

        skinImage.sprite = shop.GetSkinSprite(selectedSkin);
        if(shop.HasWingSupport(selectedSkin))
        {
            if(shop.allSkins[selectedSkin].overrideWing == null)
            {
                wingImage.sprite = shop.GetWingSprite(shop.GetSelectedWing());
            } else
            {
                wingImage.sprite = shop.allSkins[selectedSkin].overrideWing.sprite[0];
            }

            wingImage.gameObject.SetActive(true);
        } else
        {
            wingImage.gameObject.SetActive(false);
        }

        miningSkinImage.sprite = skinImage.sprite;

        minerImage.sprite = shop.GetMinerSprite(shop.GetSelectedMiner());

        int selectedPipe = shop.GetSelectedPipe();

        pipeImage.sprite = shop.GetPipeSprite(selectedPipe, true);
        Color pipeColor = Color.white;

        if(shop.HasColorSupport(selectedPipe))
        {
            pipeColor = shop.pipeColor;
        }

        pipeImage.color = pipeColor;


        Transform graveMenuParent = topGrave.transform.parent.parent;

        GraveTop currentTop = shop.allGraveTops[shop.GetSelected(CustomizationType.GraveTop)];
        GraveSide currentSide = shop.allGraveSides[shop.GetSelected(CustomizationType.GraveSide)];
        GraveBottom currentBottom = shop.allGraveBottoms[shop.GetSelected(CustomizationType.GraveBottom)];

        topGrave.sprite = currentTop.sprite;
        sideLeftGrave.sprite = currentSide.sprite;
        sideRightGrave.sprite = currentSide.sprite;
        bottomGrave.sprite = currentBottom.sprite;

        ApplyOffset(graveMenuParent, bottomGrave.transform, currentBottom.menuOffset);
        ApplyOffset(bottomGrave.transform, topGrave.transform, currentTop.menuOffset);
        ApplyOffset(topGrave.transform, sideLeftGrave.transform, currentTop.wingMenuOffset[0]);
        ApplyOffset(topGrave.transform, sideRightGrave.transform, currentTop.wingMenuOffset[1]);
    }

    private void ApplyOffset(Transform baseObj, Transform target, Vector3 offset)
    {
        Vector3 pos = baseObj.transform.position;
        pos.x += offset.x;
        pos.y += offset.y;
        pos.z += offset.z;

        target.transform.position = pos;
    }

    public void TypeClicked(int id)
    {
        if(anRunning)
        {
            return;
        }

        SoundManager.Instance.PlaySound(Sound.MenuSelect);

        Tween anTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, 0, anTime);
        anTween.OnUpdate(() =>
        {
            fontMat.SetFloat("_DissolveAmount", dissolveAmount);
            imageMat.SetFloat("_DissolveAmount", dissolveAmount);
        });

        StartCoroutine(EndTypeClicked(id));
    }

    private IEnumerator EndTypeClicked(int id)
    {
        yield return new WaitForSeconds(anTime + 0.01f);

        switch (id)
        {
            case 0: //skins
                //customizationHandler.SetType(CustomizationType.Skin, true);
                customizationHandler.gameObject.SetActive(true);
                customizationHandler.StartCustomizationHandler();
                break;
            case 1: //mining
                //shop.TypeClicked(2);
                minerCustomizationHandler.StartMineCustomizationHandler();
                break;
            case 2: //pipes
                pipeCustomizationHandler.gameObject.SetActive(true);
                pipeCustomizationHandler.StartPipeCustomizationHandler();
                //shop.TypeClicked(3);
                break;
            case 3: //backgrounds
                shop.TypeClicked(4);
                break;
            case 4: //graves
                graveCustomizationHandler.gameObject.SetActive(true);
                graveCustomizationHandler.StartGraveCustomizationHandler();
                break;
        }
        gameObject.SetActive(false);
    }
}
