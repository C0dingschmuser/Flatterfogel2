using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class ThinkHandler : MonoBehaviour
{
    public GameObject thinkObj;
    public static ThinkHandler Instance;

    private float length;

    private void Awake()
    {
        Instance = this;
    }

    public void StartThink(string text, float length)
    {
        thinkObj.SetActive(true);

        Color c = Color.white;
        c.a = 0;

        Color c2 = Color.black;
        c2.a = 0;

        this.length = length;

        thinkObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        thinkObj.GetComponent<Image>().color = c;
        thinkObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = c2;

        thinkObj.GetComponent<Image>().DOFade(1, length / 6);
        thinkObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(1, length / 6);

        Invoke("FadeOut", length - (length / 6));
        Invoke("EndThink", length + 0.01f);
    }

    private void FadeOut()
    {
        thinkObj.GetComponent<Image>().DOFade(0, length / 6);
        thinkObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(0, length / 6);
    }

    private void EndThink()
    {
        thinkObj.SetActive(false);
    }
}
