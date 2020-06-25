using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenuHandler : MonoBehaviour
{
    [SerializeField]
    private ShopHandler shop = null;
    [SerializeField]
    private CustomizationHandler customizationHandler = null;
    [SerializeField]
    private PipeCustomizationHandler pipeCustomizationHandler = null;
    [SerializeField]
    private Image skinImage = null, wingImage = null;
    [SerializeField]
    private Image miningSkinImage = null, minerImage = null;
    [SerializeField]
    private Image pipeImage = null;

    public void OpenMenu()
    {
        gameObject.SetActive(true);

        skinImage.sprite = shop.GetSkinSprite(shop.GetSelectedSkin());
        if(shop.HasWingSupport(shop.GetSelectedSkin()))
        {
            wingImage.sprite = shop.GetWingSprite(shop.GetSelectedWing());
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
        switch(id)
        {
            case 0: //skins
                customizationHandler.SetType(CustomizationType.Skin);
                customizationHandler.gameObject.SetActive(true);
                break;
            case 1: //mining
                shop.TypeClicked(2);
                break;
            case 2: //pipes
                pipeCustomizationHandler.SetType(CustomizationType.Pipe);
                pipeCustomizationHandler.gameObject.SetActive(true);
                //shop.TypeClicked(3);
                break;
            case 3: //backgrounds
                shop.TypeClicked(4);
                break;
        }
        gameObject.SetActive(false);
    }
}
