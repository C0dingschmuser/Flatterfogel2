using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class InfoText : MonoBehaviour
{
    private int flash = 0;

    public bool moving = false, endWaiting = false;

    [SerializeField]
    TextMeshProUGUI text = null;

    public void StartFlashing(string text, float xOffset = 0, float yOffset = 100, float time = 2f, bool moving = false, float scale = 1, float endScale = 1)
    {
        transform.localScale = new Vector3(scale, scale, scale);

        flash = 0;
        this.text.color = Color.red;
        this.text.text = text;

        if(!moving)
        {
            transform.DOMove(
                new Vector3(transform.position.x + xOffset, transform.position.y + yOffset), time);
        } else
        {
            if(scale != endScale)
            {
                transform.DOScale(endScale, 1f);
            }
        }

        this.moving = moving;

        InvokeRepeating("Flash", 0.25f, 0.25f);

        if(transform.position.x < 18)
        { //auf bild
            endWaiting = false;

            Invoke("FadeOut", 0.75f);
            Invoke("EndFlash", 1.5f);
        } else
        {
            endWaiting = true;
        }

    }

    private void Flash()
    {
        float alpha = text.color.a;
        Color newColor = Color.red;
        
        if(flash == 0)
        {
            flash = 1;
            newColor = Color.yellow;
        } else
        {
            flash = 0;
        }

        newColor.a = alpha;
        text.color = newColor;
    }

    private void FadeOut()
    {
        text.DOFade(0, 1f);
    }

    private void EndFlash()
    {
        CancelInvoke("Flash");
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(moving)
        {
            if(FlatterFogelHandler.Instance.gameActive)
            {
                transform.Translate(-FlatterFogelHandler.scrollSpeed * Time.deltaTime, + 2 * Time.deltaTime, 0);

                if(endWaiting)
                {
                    if(transform.position.x < 18)
                    {
                        endWaiting = false;

                        Invoke("FadeOut", 0.5f);
                        Invoke("EndFlash", 1f);
                    }
                }
            } else
            {
                if(endWaiting)
                {
                    endWaiting = false;

                    Invoke("FadeOut", 0.5f);
                    Invoke("EndFlash", 1f);
                }
            }
        }
    }
}
