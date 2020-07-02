using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BuyInfoHandler : MonoBehaviour
{
    public Image mainImage;
    public Material buyMat;

    private Sprite[] anSprites;

    private bool isAnimated = false;

    private float dissolveAmount;

    private Coroutine anRoutine = null;

    public void SetBuyData(Sprite newSprite)
    {
        mainImage.sprite = newSprite;

        isAnimated = false;
    }

    public void SetBuyData(Sprite[] newSpriteAn)
    {
        mainImage.sprite = newSpriteAn[0];

        anSprites = newSpriteAn;

        isAnimated = true;
    }

    public void OpenBuyInfo()
    {
        transform.parent.GetComponent<GraphicRaycaster>().enabled = true;

        buyMat.SetFloat("_DissolveAmount", 0);
        dissolveAmount = 0;

        if(isAnimated)
        {
            StartCoroutine(HandleAnimation());
        }

        Tween temp = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, 1, 2);
        temp.OnUpdate(() =>
        {
            buyMat.SetFloat("_DissolveAmount", dissolveAmount);
        });
    }

    private IEnumerator HandleAnimation()
    {
        int step = 0;

        while(true)
        {
            mainImage.sprite = anSprites[step];

            step++;
            if(step > 2)
            {
                step = 0;
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    public void CloseBuyInfo()
    {
        if(anRoutine != null)
        {
            StopCoroutine(anRoutine);
        }

        transform.parent.GetComponent<GraphicRaycaster>().enabled = false;
        ShopHandler.Instance.BuyInfoOkayClicked();
    }
}
