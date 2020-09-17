using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class RarityFilter
{
    public bool enabled = true;
}

public class FilterHandler : MonoBehaviour
{
    public static bool purchased = true, animated = true;
    public static RarityFilter[] raritys = new RarityFilter[5];

    public CustomizationHandler customizationHandler;

    public GraphicRaycaster raycaster;
    public Transform purchasedParent, animatedParent, rarityParent, filter;

    public Vector3 defaultPos;

    public static void SetupFilter()
    {
        for (int i = 0; i<raritys.Length; i++)
        {
            RarityFilter f = new RarityFilter
            {
                enabled = true
            };

            raritys[i] = f;
        }
    }

    public void OpenFilter()
    {
        gameObject.SetActive(true);
        raycaster.enabled = true;

        Vector3 startPos = defaultPos;
        startPos.y = 2500;

        filter.position = startPos;

        filter.DOMoveY(defaultPos.y, MenuData.scaleTime).SetEase(Ease.OutBack);
    }

    public void CloseFilter()
    {
        filter.DOMoveY(2500, MenuData.scaleTime).SetEase(Ease.InBack);
        raycaster.enabled = false;

        customizationHandler.FilterUpdate();

        Invoke("EndClose", MenuData.scaleTime + 0.01f);
    }

    private void EndClose()
    {
        gameObject.SetActive(false);
    }

    public void PurchaseClicked()
    {
        if (purchased)
        {
            purchasedParent.GetChild(0).gameObject.SetActive(false);

            purchased = false;
        } else
        {
            purchasedParent.GetChild(0).gameObject.SetActive(true);

            purchased = true;
        }
    }

    public void AnimatedClicked()
    {
        if (animated)
        {
            animatedParent.GetChild(0).gameObject.SetActive(false);

            animated = false;
        }
        else
        {
            animatedParent.GetChild(0).gameObject.SetActive(true);

            animated = true;
        }
    }

    public void RarityClicked(int id)
    {
        if (raritys[id].enabled)
        {
            rarityParent.GetChild(id + 1).GetChild(0).gameObject.SetActive(false);

            raritys[id].enabled = false;
        }
        else
        {
            rarityParent.GetChild(id + 1).GetChild(0).gameObject.SetActive(true);

            raritys[id].enabled = true;
        }
    }
}
