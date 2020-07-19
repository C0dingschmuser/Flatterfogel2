using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using DG.Tweening;
using System;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class AchievementHandler : MonoBehaviour
{
    public GameObject achPrefab;
    public Transform contentParent;

    public Achievement[] allAchievements;

    [SerializeField]
    private Vector3 achStartPos;

    [SerializeField]
    private GameObject hParent, achParent;

    [SerializeField]
    private Transform achMoveParent;

    [SerializeField]
    private ObjectPooler objPooler;

    [SerializeField]
    private Slider mainSlider;

    [SerializeField]
    private TextMeshProUGUI mainText;

    [SerializeField]
    private Color lockedColor, unlockedColor;

    private float sliderValue = 0;

    private ObscuredInt achLvl = 0;
    private ObscuredLong newAchCoins = 0, defaultDiff = 25;
    private ObscuredLong achCoins = 0, achMaxCoins = 25;

    private Tween achCoinTween = null;
    private bool handleRunning = false;

    private List<GameObject> achMoveObjs = new List<GameObject>();
    private List<GameObject> delList = new List<GameObject>();

    public static AchievementHandler Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        achLvl = ObscuredPrefs.GetInt("AchLevel", 0);
        achCoins = ObscuredPrefs.GetLong("AchCoins", 0);
        achMaxCoins = ObscuredPrefs.GetLong("AchMaxCoins", defaultDiff);

        mainSlider.value = (float)achCoins / achMaxCoins;
        mainText.text = achCoins.ToString() + "/" + achMaxCoins.ToString();
    }

    public void AddAchCoins(long newCoins)
    {
        newAchCoins += newCoins;

        if (!handleRunning)
        { //coroutine starten wenn sie noch nicht läuft
            handleRunning = true;
            StartCoroutine(HandleAddAchCoins());
        }
    }

    public void StartSpawnAchObjs(Vector3 startPos, long newCoins)
    {
        StartCoroutine(SpawnAchObjs(startPos, newCoins));
    }

    private IEnumerator SpawnAchObjs(Vector3 startPos, long newCoins)
    {
        long amount = 10;

        if (newCoins < 10)
        {
            amount = newCoins;
        }

        for (long i = 0; i < amount; i++)
        {
            GameObject newAchObj = objPooler.SpawnFromPool("AchObj", startPos, Quaternion.identity);

            newAchObj.transform.localScale = Vector3.zero;
            newAchObj.transform.SetParent(achMoveParent);

            AchMoveObj ach = newAchObj.GetComponent<AchMoveObj>();
            ach.currentTime = 0;
            ach.scaleUpTime = 0.2f; //zeit aber der sich zum ziel bewegt wird
            ach.scaleDownTime = 0.625f; //zeit ab der runtergescaled wird
            ach.maxTime = 0.8f;
            ach.target = new Vector3(-648.2f, 1200, 0);
            ach.scaleUpDone = false;
            ach.scaleDownDone = false;

            if (i == 0)
            {
                ach.value = newCoins;
            }
            else
            {
                ach.value = 0;
            }

            float range = 70;

            newAchObj.transform.DOMove(new Vector3(startPos.x + Random.Range(-range, range),
                startPos.y + Random.Range(-range, range),
                startPos.z), ach.scaleUpTime - 0.01f);

            newAchObj.transform.DOScale(1, ach.scaleUpTime - 0.01f);

            achMoveObjs.Add(newAchObj);
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator HandleAddAchCoins()
    {
        bool ok = false;
        while(!ok)
        {
            long newLVLXP = newAchCoins;
            if (newLVLXP > achMaxCoins)
            { //wenn noch mehr xp als im aktuellen lvl dann auf xp des aktuellen lvls beschränken
                newLVLXP = achMaxCoins;
            }

            newAchCoins -= newLVLXP;

            newLVLXP += achCoins;
            if (newLVLXP > achMaxCoins)
            {
                long diff = newLVLXP - achMaxCoins;

                newAchCoins += diff;

                newLVLXP = achMaxCoins;
            }

            if (newAchCoins <= 0)
            { //loop beenden
                ok = true;
            }

            float waitTime = 0.3f;

            achCoinTween = DOTween.To(() => achCoins, x => achCoins = x, newLVLXP, waitTime - 0.01f);
            achCoinTween.OnUpdate(() =>
            {
                mainSlider.value = (float)achCoins / achMaxCoins;
                mainText.text = achCoins.ToString() + "/" + achMaxCoins.ToString();
            });

            yield return new WaitForSeconds(waitTime);

            if (achCoins >= achMaxCoins)
            { //neues lvl
                achCoins = 0;
                achLvl++;
                achMaxCoins+= defaultDiff;
                LevelUp();
            }

            if(newAchCoins > 0)
            {
                ok = false;
            }
        }

        Debug.Log(newAchCoins);

        handleRunning = false;

        yield return null;
    }

    private void LevelUp()
    {
        mainSlider.value = 0;
        mainText.text = achCoins.ToString() + "/" + achMaxCoins.ToString();
    }

    public void StartLoadLocalization()
    {
        StartCoroutine(LoadLocalization());
    }

    private IEnumerator LoadLocalization()
    {
        AsyncOperationHandle handle;

        for(int i = 0; i < allAchievements.Length; i++)
        {
            yield return handle = allAchievements[i].title.GetLocalizedString();

            allAchievements[i].titleString = (string)handle.Result;

            yield return handle = allAchievements[i].description.GetLocalizedString();

            allAchievements[i].descriptionString = (string)handle.Result;
        }

        LoadAchievements();
    }

    private void LoadAchievements()
    {
        for(int i = 0; i < contentParent.childCount; i++)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        string achString = ObscuredPrefs.GetString("AchievementString");

        if(achString.Length > 0)
        {
            string[] rawSplit = achString.Split('|');
            for(int i = 0; i < rawSplit.Length - 1; i++)
            { //durch alle gespeicherten achievements loopen
                string[] split = rawSplit[i].Split(',');

                /* 0 = identifier
                 * 1 = step
                 * 2 = completed (false/true)
                 * 3 = reward collected (false/true)
                 */

                string identifier = split[0];
                int currentStep = Int32.Parse(split[1]);
                bool completed = bool.Parse(split[2]);
                bool rewardCollected = bool.Parse(split[3]);

                for(int a = 0; a < allAchievements.Length; a++)
                {
                    allAchievements[a].step = 0;
                    allAchievements[a].completed = false;
                    allAchievements[a].rewardCollected = false;

                    if(allAchievements[a].identifier.Equals(identifier))
                    { //assigned achievement found
                        allAchievements[a].step = currentStep;
                        allAchievements[a].completed = true;//completed;
                        allAchievements[a].rewardCollected = rewardCollected;
                        //break; kein break da reset für alle erfolgen muss
                    }
                }
            }
        }

        for(int i = 0; i < allAchievements.Length; i++)
        { //position wird automatisch angepasst
            AchHolder ach = Instantiate(achPrefab, contentParent).GetComponent<AchHolder>();

            ach.achievement = allAchievements[i];

            ach.titleText.text = allAchievements[i].titleString;
            ach.descriptionText.text = allAchievements[i].descriptionString;

            ach.sprite.sprite = allAchievements[i].mainSprite;

            ach.progressText.text =
                allAchievements[i].step.ToString() + "/" + allAchievements[i].maxStep.ToString();

            ach.rewardText.text = allAchievements[i].rewards[0].amount.ToString();

            float val = (float)allAchievements[i].step / allAchievements[i].maxStep;
            ach.progressSlider.value = val;

            Color32 collectColor = lockedColor;
            ach.collectButton.interactable = false;

            if(allAchievements[i].completed)
            {
                if(allAchievements[i].rewardCollected)
                {
                    collectColor = Color.white;
                    ach.collectButton.interactable = false;
                } else
                {
                    collectColor = unlockedColor;
                    ach.collectButton.interactable = true;
                }
            }

            byte alphaVal = 255;
            if(!ach.collectButton.interactable)
            {
                alphaVal = 180;
            }

            collectColor.a = alphaVal;

            Color32 temp = ach.rewardSprite.color;
            temp.a = alphaVal;
            ach.rewardSprite.color = temp;

            temp = ach.rewardText.color;
            temp.a = alphaVal;
            ach.rewardText.color = temp;

            ach.collectSprite.color = collectColor;
        }
    }

    private void SaveAchievements()
    {
        string achString = "";

        for(int i = 0; i < allAchievements.Length; i++)
        {
            achString += allAchievements[i].identifier + "," +
                allAchievements[i].step.ToString() + "," +
                allAchievements[i].completed.ToString() + "," +
                allAchievements[i].rewardCollected.ToString() + "|";
        }

        ObscuredPrefs.SetString("AchievementString", achString);

        ObscuredPrefs.SetInt("AchLevel", achLvl);
        ObscuredPrefs.SetLong("AchCoins", achCoins);
        ObscuredPrefs.SetLong("AchMaxCoins", achMaxCoins);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveAchievements();
        }
    }

    private void OnApplicationQuit()
    {
        SaveAchievements();
    }

    private void SetContentPos(int row)
    {
        float startY = 1201.980f;

        int startPos = row;

        float endY = startY + (180 * startPos);

        contentParent.position = new Vector3(contentParent.position.x,
            endY, contentParent.position.z);
    }

    public void OpenAchievements()
    {
        hParent.SetActive(false);
        achParent.SetActive(true);
    }

    public void OpenHighscores()
    {
        hParent.SetActive(true);
        achParent.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        for(int i = 0; i < achMoveObjs.Count; i++)
        {
            AchMoveObj obj = achMoveObjs[i].GetComponent<AchMoveObj>();
            obj.currentTime += Time.deltaTime;

            if(obj.currentTime >= obj.scaleUpTime &&
                !obj.scaleUpDone)
            {
                Ease endEase = Ease.OutCubic;
                switch(Random.Range(0, 5))
                {
                    case 0:
                        endEase = Ease.InCubic;
                        break;
                    case 1:
                        endEase = Ease.InOutCubic;
                        break;
                    case 2:
                        endEase = Ease.InOutQuart;
                        break;
                    case 3:
                        endEase = Ease.InOutQuad;
                        break;
                }

                achMoveObjs[i].transform.
                    DOMove(obj.target, obj.maxTime - obj.scaleUpTime).SetEase(endEase);
                obj.scaleUpDone = true;
            }

            if(obj.currentTime >= obj.scaleDownTime &&
                !obj.scaleDownDone)
            {
                achMoveObjs[i].transform.DOScale(0, obj.scaleDownTime - 0.01f);
                obj.scaleDownDone = true;
            }

            if(obj.currentTime >= obj.maxTime)
            {
                if(obj.value > 0)
                {
                    AddAchCoins(obj.value);
                }

                achMoveObjs[i].SetActive(false);
                delList.Add(achMoveObjs[i]);
            }
        }

        for(int i = 0; i < delList.Count; i++)
        {
            achMoveObjs.Remove(delList[i]);
        }
        delList.Clear();
    }
}
