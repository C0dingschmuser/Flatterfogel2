using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
    private Image skinImage = null, wingImage = null;
    [SerializeField]
    private Image miningSkinImage = null, minerImage = null;
    [SerializeField]
    private Image pipeImage = null;

    [SerializeField]
    private Material fontMat = null, imageMat = null;

    public static float anTime = 0.25f;

    private float dissolveAmount = 1;
    private bool anRunning = false;

    public void OpenMenu(bool start = false)
    {
        gameObject.SetActive(true);

        if(start)
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
    }

    public void TypeClicked(int id)
    {
        if(anRunning)
        {
            return;
        }

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
        }
        gameObject.SetActive(false);
    }
}
