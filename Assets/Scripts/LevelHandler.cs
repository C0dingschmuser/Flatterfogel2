using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using CodeStage.AntiCheat.Storage;
using CodeStage.AntiCheat.ObscuredTypes;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelHandler : MonoBehaviour
{
    public static LevelHandler Instance;

    public LocalizedString levelLocalized;
    private string levelString;

    private int effectID = 0;
    private ObscuredInt defaultDiff = 50, currentPrestige = 0;
    private ObscuredLong currentXP, currentLVL = 1, currentLVLDiff = 50, newXP = 0;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentXP = ObscuredPrefs.GetLong("CurrentXP", 0);
        currentLVL = ObscuredPrefs.GetLong("CurrentLVL", 1);
        currentPrestige = ObscuredPrefs.GetInt("CurrentPrestige", 0);
        newXP = ObscuredPrefs.GetLong("NewXP", 0);
        currentLVLDiff = ObscuredPrefs.GetLong("CurrentDiff", defaultDiff);
    }

    public void StartLoadLocalization()
    {
        StartCoroutine(LoadLocalization());
    }

    private IEnumerator LoadLocalization()
    {
        AsyncOperationHandle handle;

        yield return handle = levelLocalized.GetLocalizedString();

        levelString = (string)handle.Result;
    }

    private void SaveXP()
    {
        ObscuredPrefs.SetInt("CurrentPrestige", currentPrestige);
        ObscuredPrefs.SetLong("CurrentXP", currentXP);
        ObscuredPrefs.SetLong("CurrentLVL", currentLVL);
        ObscuredPrefs.SetLong("CurrentDiff", currentLVLDiff);
        ObscuredPrefs.SetLong("NewXP", newXP);
    }

    public int GetPrestige()
    {
        return currentPrestige;
    }

    public long GetXP()
    {
        return currentXP;
    }

    public long GetLVL()
    {
        return currentLVL;
    }

    public void AddNewXP(int amount)
    {
        newXP += amount;
    }

    public void UpdateXP(GameObject parent, GameObject[] deactivateObjs = null)
    {
        if(deactivateObjs != null)
        {
            for(int i = 0; i < deactivateObjs.Length; i++)
            {
                deactivateObjs[i].GetComponent<Button>().interactable = false;
            }
        }

        StartCoroutine(XPRoutine(parent, deactivateObjs));
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            SaveXP();
        }
    }

    private void OnApplicationQuit()
    {
        SaveXP();
    }

    IEnumerator XPRoutine(GameObject parent, GameObject[] deactivateObjs = null)
    {
        if(newXP <= 0)
        {
            SetXPText(parent);

            if (deactivateObjs != null)
            {
                for (int i = 0; i < deactivateObjs.Length; i++)
                {
                    deactivateObjs[i].GetComponent<Button>().interactable = true;
                }
            }

            yield return null;
        }

        Slider xpSlider = parent.transform.GetChild(0).GetComponent<Slider>();
        TextMeshProUGUI xpText = parent.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>();
        //TextMeshProUGUI lvltext = parent.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        bool ok = false;
        while(!ok)
        { //jeder durchlauf tweent auf das nächste lvl bis newXP "leer" ist
            long newLVLXP = newXP;
            if(newLVLXP > currentLVLDiff)
            { //wenn noch mehr xp als im aktuellen lvl dann auf xp des aktuellen lvls beschränken
                newLVLXP = currentLVLDiff;
            }

            newXP -= newLVLXP;

            newLVLXP += currentXP;
            if(newLVLXP > currentLVLDiff)
            {
                long diff = newLVLXP - currentLVLDiff;

                newXP += diff;

                newLVLXP = currentLVLDiff;
            }

            if(newXP <= 0)
            { //loop beenden
                ok = true;
            }

            float waitTime = 0.3f;

            if(ok)
            {
                waitTime = 0.5f;
            }

            Tween xpNumTween = DOTween.To(() => currentXP, x => currentXP = x, newLVLXP, waitTime - 0.01f);
            xpNumTween.OnUpdate(() =>
            {
                xpSlider.value = (float)currentXP / currentLVLDiff;
                xpText.text = currentXP.ToString() + "/" + currentLVLDiff.ToString();
            });

            yield return new WaitForSeconds(waitTime);

            if (currentXP >= currentLVLDiff)
            { //neues lvl
                currentXP = 0;
                currentLVL++;

                UpdateLevel();

                currentLVLDiff += defaultDiff;

                SetXPText(parent, true, waitTime);
            }
        }

        if (deactivateObjs != null)
        {
            for (int i = 0; i < deactivateObjs.Length; i++)
            {
                deactivateObjs[i].GetComponent<Button>().interactable = true;
            }
        }

        SaveXP();
    }

    private void UpdateLevel()
    {
        AchievementHandler.Instance.UpdateStep("destructionUnlock");
        AchievementHandler.Instance.UpdateStep("miningUnlock");
    }

    public void ResetEffects(GameObject parent)
    {
        int count = parent.transform.GetChild(1).childCount;
        for(int i = 0; i < count; i++)
        {
            parent.transform.GetChild(1).GetChild(i).gameObject.SetActive(false);
        }
    }

    public void SetXPText(GameObject parent, bool juice = false, float waitTime = 0.5f)
    {
        if(juice)
        {
            parent.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;
            parent.transform.GetChild(1).localScale = new Vector3(1.4f, 1.4f, 1.4f);

            parent.transform.GetChild(1).GetComponent<TextMeshProUGUI>().DOColor(Color.black, waitTime - 0.05f);
            parent.transform.GetChild(1).DOScale(1, waitTime - 0.05f);

            GameObject effect = parent.transform.GetChild(1).GetChild(effectID).gameObject;
            effect.SetActive(false);
            effect.SetActive(true);
            effectID++;
            if(effectID > 3)
            {
                effectID = 0;
            }

            SoundManager.Instance.PlaySound(SoundManager.Sound.LevelUp);
        }

        parent.transform.GetChild(0).GetComponent<Slider>().value = currentXP / (float)currentLVLDiff;
        parent.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text =
            currentXP.ToString() + "/" + currentLVLDiff.ToString();
        parent.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = levelString + " " + currentLVL.ToString();

        MenuData.Instance.UpdatePlayerLevelText();
    }
}
