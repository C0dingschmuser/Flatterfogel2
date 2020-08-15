using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class GravestoneHandler : MonoBehaviour
{
    public GameObject player, topSprite, sideLeftSprite, sideRightSprite, bottomSprite;
    public Text[] stoneName;

    public CoroutineHandle handle;

    private float opacity = 0;

    private const float maxDistance = 150;

    private void ApplyOffset(GameObject obj, Transform parent, Vector3 offset)
    {
        Vector3 pos = transform.position;

        if(parent != null)
        {
            pos = parent.position;
        }

        pos.x += offset.x;
        pos.y += offset.y;
        pos.z += offset.z;

        obj.transform.position = pos;
    }

    public void StartGravstone(GameObject player, GraveTop top, GraveSide side, GraveBottom bottom,
        float scrollSpeed, string name)
    {
        this.player = player;

        //Bodenposition ist standard
        //toppart ist von bodenposition + offset abhängig
        //sides sind von topposition & jeweiligem offset abhängig

        ApplyOffset(bottomSprite, null, bottom.offset);
        ApplyOffset(topSprite, bottomSprite.transform, top.offset);
        ApplyOffset(sideLeftSprite, topSprite.transform, top.wingOffset[0]);
        ApplyOffset(sideRightSprite, topSprite.transform, top.wingOffset[1]);

        topSprite.GetComponent<SpriteRenderer>().sprite = top.sprite;
        sideLeftSprite.GetComponent<SpriteRenderer>().sprite = side.sprite;
        sideRightSprite.GetComponent<SpriteRenderer>().sprite = side.sprite;
        bottomSprite.GetComponent<SpriteRenderer>().sprite = bottom.sprite;

        opacity = 0;

        stoneName[0].GetComponent<Text>().color =
            SetAlpha(stoneName[0].GetComponent<Text>().color, 0);
        stoneName[0].GetComponent<Text>().text = name;

        stoneName[1].GetComponent<Text>().color =
            SetAlpha(stoneName[1].GetComponent<Text>().color, 0);
        stoneName[1].GetComponent<Text>().text = name;

        handle = Timing.RunCoroutine(_UpdateAlpha());
    }

    private Color SetAlpha(Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }

    public void FadeOutGravestone(float time)
    {
        Timing.KillCoroutines(handle);
        Timing.RunCoroutine(_WaitFadeOut(time));
    }

    IEnumerator<float> _WaitFadeOut(float time = 0.3f)
    {
        float opacity = stoneName[0].GetComponent<Text>().color.a;
        float maxTime = time;
        while(time > 0)
        {
            opacity = time / maxTime;

            Color c1 = stoneName[0].GetComponent<Text>().color;
            c1.a = opacity;

            Color c2 = stoneName[1].GetComponent<Text>().color;
            c2.a = opacity;

            stoneName[0].GetComponent<Text>().color = c1;
            stoneName[1].GetComponent<Text>().color = c2;

            time -= 0.1f;
            yield return Timing.WaitForSeconds(0.1f);
        }

        StopGravestone();
    }

    public void StopGravestone()
    {
        Timing.KillCoroutines(handle);
        gameObject.SetActive(false);
    }

    IEnumerator<float> _UpdateAlpha()
    {
        while(true)
        {
            float xDist =
                Mathf.Abs(transform.position.x - player.transform.position.x);
            if (xDist < maxDistance)
            {
                opacity = 1 - (xDist / maxDistance);
                stoneName[0].GetComponent<Text>().color =
                    SetAlpha(stoneName[0].GetComponent<Text>().color, opacity);
                stoneName[1].GetComponent<Text>().color =
                    SetAlpha(stoneName[1].GetComponent<Text>().color, opacity);
            } else if(opacity <= 0.1f && opacity > 0)
            {
                opacity = 0;
                stoneName[0].GetComponent<Text>().color =
                    SetAlpha(stoneName[0].GetComponent<Text>().color, opacity);
                stoneName[1].GetComponent<Text>().color =
                    SetAlpha(stoneName[1].GetComponent<Text>().color, opacity);
            }
            yield return Timing.WaitForSeconds(0.1f);
        }
    }
}
