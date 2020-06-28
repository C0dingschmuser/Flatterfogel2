using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using UnityEditor;

public class GDPRHandler : MonoBehaviour
{
    public static string privacyLinkDE = "https://pastebin.com/HsEfaLCt";
    public static string usageLinkDE = "https://pastebin.com/BQcicxpA";

    public static bool isActive = false;
    public int gdpr_accepted = 0, alpha_unlocked = 0;
    public GraphicRaycaster raycaster;

    public GameObject alphaHandler, inAppUpdate;

    // Start is called before the first frame update
    void Start()
    {
        /*string alp = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string full = "";
        for(int i = 0; i < 100; i++)
        {
            string a = "";
            for(int b = 0; b < 10; b++)
            {
                a += alp[Random.Range(0, alp.Length)];
            }
            full += a + "\n";
        }
        EditorGUIUtility.systemCopyBuffer = full;*/

        float currentVersion =
            float.Parse(Application.version, System.Globalization.CultureInfo.InvariantCulture);

        if(currentVersion < 0.6f)
        { //Complete Reset
            PlayerPrefs.DeleteAll();
            ObscuredPrefs.DeleteAll();
        }

        gdpr_accepted = ObscuredPrefs.GetInt("GDPR_ACCEPTED", 0);
        if (gdpr_accepted == 0)
        {
            isActive = true;
            ActivateGDPR();
        }
        else
        {
            isActive = false;
            ActivateGDPR(false);
        }
    }

    private void ActivateGDPR(bool enable = true)
    {
        raycaster.enabled = enable;

        isActive = enable;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(enable);
        }

        if(!enable)
        { //bei deaktivierung gplay signin
            AccountHandler.Instance.StartGplayLogin();

            if(inAppUpdate.activeSelf)
            {
                raycaster.enabled = true;
            }
        }
    }

    public void DataClicked(int type)
    {
        if (type == 0)
        { //Nutzungsbedingungen
            OptionHandler.Instance.OpenTOS();
        }
        else
        { //Datenschutzerklärung
            OptionHandler.Instance.OpenDataPrivacy();
        }
    }

    public void AcceptClicked(int type)
    {
        ObscuredPrefs.SetInt("GDPR_ACCEPTED", type);

        if (type == 1)
        { //akzeptiert
            ActivateGDPR(false);

            raycaster.enabled = true;
            /*for(int i = 0; i < alphaHandler.transform.childCount; i++)
            {
                alphaHandler.transform.GetChild(i).gameObject.SetActive(true);
            }*/
        }
        else
        {
            Application.Quit();
        }
    }
}
