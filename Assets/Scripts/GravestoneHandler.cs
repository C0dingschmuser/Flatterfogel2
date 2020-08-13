using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GravestoneHandler : MonoBehaviour
{
    public GameObject player, topSprite, sideLeftSprite, sideRightSprite, bottomSprite;
    public Text[] stoneName;

    private float opacity = 0;

    private const float maxDistance = 150;

    private void ApplyOffset(GameObject obj, Vector3 offset)
    {
        Vector3 pos = transform.position;

        pos.x += offset.x;
        pos.y += offset.y;
        pos.z += offset.z;

        obj.transform.position = pos;
    }

    public void StartGravstone(GameObject player, GraveTop top, GraveSide side, GraveBottom bottom,
        float scrollSpeed, string name)
    {
        this.player = player;

        ApplyOffset(topSprite, top.offset);
        ApplyOffset(sideLeftSprite, side.offset[0]);
        ApplyOffset(sideRightSprite, side.offset[1]);
        ApplyOffset(bottomSprite, bottom.offset);

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

        StartCoroutine(UpdateAlpha());
    }

    private Color SetAlpha(Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }

    public void StopGravestone()
    {
        StopCoroutine(UpdateAlpha());
        gameObject.SetActive(false);
    }

    IEnumerator UpdateAlpha()
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
            yield return new WaitForSeconds(0.1f);
        }
    }
}
