using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;

public class AlphaHandler : MonoBehaviour
{
    public TextMeshProUGUI submitText;
    public TextMeshProUGUI buttonText;
    public GameObject keyInput;
    public GraphicRaycaster raycaster;

    private bool checkRunning = false;

    public void SubmitClicked()
    {
        if (checkRunning) return;

        string key = keyInput.GetComponent<TMP_InputField>().text;

        bool ok = true;
        for(int i = 0; i < key.Length; i++)
        {
            if((key[i] >= 'a' && key[i] <= 'z') ||
                (key[i] >= 'A' && key[i] <= 'Z') ||
                (key[i] >= '0' && key[i] <= '9'))
            {
                ok = true;
            } else
            {
                ok = false;
                break;
            }
        }

        if(key.Length != 10)
        {
            ok = false;
        }

        if(!ok)
        {
            submitText.text = "Fehlerhafter Key!";
            return;
        } else
        {
            buttonText.text = "Überprüfe...";
            StartCoroutine(CheckKey(key));
        }
    }

    private void HideAlphaHandler()
    {
        ObscuredPrefs.SetInt("GDPR_ACCEPTED", 1);
        raycaster.enabled = false;
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    IEnumerator CheckKey(string key)
    {
        checkRunning = true;

        string link = "https://bruh.games/unlock.php?unlockkey=" + key;

        UnityWebRequest www = UnityWebRequest.Get(link);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            submitText.text = "Verbindung fehlgeschlagen!";
        } else
        {
            string wwwData = www.downloadHandler.text;
            if(wwwData.Length == 1)
            {
                int success = Int32.Parse(wwwData);
                if(success == 1)
                {
                    HideAlphaHandler();
                } else
                {
                    submitText.text = "Ungültiger Key!";
                    buttonText.text = "Überprüfen";
                }
            }
        }

        checkRunning = false;
    }
}
