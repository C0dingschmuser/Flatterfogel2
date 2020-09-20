using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditHandler : MonoBehaviour
{
    private Coroutine routine;

    public TextMeshProUGUI[] titleText;

    private void OnEnable()
    {
        routine = StartCoroutine(HandleColor());
    }

    private void OnDisable()
    {
        StopCoroutine(routine);
    }

    IEnumerator HandleColor()
    {
        while(true)
        {
            for(int i = 0; i < titleText.Length; i++)
            {
                Color c;

                if(Random.Range(0, 3) > 0)
                {
                    c = FlatterFogelHandler.pr0Farben[Random.Range(0, FlatterFogelHandler.pr0Farben.Length)];
                } else
                {
                    c = new Color32(22, 22, 24, 255);
                }

                titleText[i].color = c;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
