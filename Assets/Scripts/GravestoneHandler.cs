using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GravestoneHandler : MonoBehaviour
{
    public GameObject player;
    public Text[] stoneName;
    private float opacity = 0;

    private const float maxDistance = 150;

    public void StartGravstone(GameObject player, float scrollSpeed, string name)
    {
        this.player = player;

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
