using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using DG.Tweening;
using System;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;
using UnityEngine.SocialPlatforms.Impl;

public class AchievementHandler : MonoBehaviour
{
    public AchUnlockHandler achUnlockHandler;
    public GameObject achPrefab, achCoin, menuUnclaimed, windowUnclaimed;
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
    private Material[] completedMats;

    [SerializeField]
    private TextMeshProUGUI mainText;

    [SerializeField]
    private Color lockedColor, unlockedColor;

    private int flashDir = 0, dissolveStep = 0;
    private float sliderValue = 0;
    private float[] dissolveAmounts;

    private CoroutineHandle mainHandle;

    private ObscuredInt achLvl = 0;
    private ObscuredLong newAchCoins = 0, defaultDiff = 50;
    private ObscuredLong achCoins = 0, achMaxCoins = 50;

    private Tween achCoinTween = null, achCollTween = null, achColorTween = null;
    private bool handleRunning = false;

    private AchHolder[] allAchHolders;

    public Color flashColor;

    private List<GameObject> achMoveObjs = new List<GameObject>();
    private List<GameObject> delList = new List<GameObject>();

    public static AchievementHandler Instance;

    private void Awake()
    {
        Instance = this;
        //achUnlockHandler.Startup();
    }

    private void Start()
    {
        dissolveAmounts = new float[completedMats.Length];
        for(int i = 0; i < dissolveAmounts.Length; i++)
        {
            dissolveAmounts[i] = 1;
        }

        achLvl = ObscuredPrefs.GetInt("AchLevel", 0);
        achCoins = ObscuredPrefs.GetLong("AchCoins", 0);
        achMaxCoins = ObscuredPrefs.GetLong("AchMaxCoins", defaultDiff);

        mainSlider.value = (float)achCoins / achMaxCoins;
        mainText.text = achCoins.ToString() + "/" + achMaxCoins.ToString();
    }

    public void UpdateMenuUnclaimed()
    {
        int unclaimed = 0;

        for(int i = 0; i < allAchievements.Length; i++)
        {
            if(allAchievements[i].completed && !
                allAchievements[i].rewardCollected)
            {
                unclaimed++;
            }
        }

        if(unclaimed > 0)
        {
            menuUnclaimed.SetActive(true);
            windowUnclaimed.SetActive(true);

            string s = unclaimed.ToString();

            if(unclaimed > 9)
            {
                s = "9+";
            }

            menuUnclaimed.transform.GetChild(0).
                GetComponent<TextMeshProUGUI>().text = s;

            windowUnclaimed.transform.GetChild(0).
                GetComponent<TextMeshProUGUI>().text = s;
        } else
        {
            menuUnclaimed.SetActive(false);
            windowUnclaimed.SetActive(false);
        }
    }

    public void CollectClicked(GameObject caller)
    {
        caller.GetComponent<AchHolder>().completedSprite.material =
            completedMats[dissolveStep];

        caller.GetComponent<Image>().color = new Color32(188, 188, 188, 255);

        caller.GetComponent<AchHolder>().achievement.rewardCollected = true;
        UpdateMenuUnclaimed();

        caller.GetComponent<AchHolder>().uncollected.SetActive(false);
        caller.GetComponent<AchHolder>().collected.SetActive(true);
        caller.GetComponent<AchHolder>().collectButton.interactable = false;
        caller.GetComponent<AchHolder>().sliderFiller.color = unlockedColor;

        completedMats[dissolveStep].SetFloat("_DissolveAmount", 1);
        dissolveAmounts[dissolveStep] = 1;

        int oldStep = dissolveStep;

        Tween temp = DOTween.To(() => dissolveAmounts[oldStep], x => dissolveAmounts[oldStep] = x, 0, 0.5f);
        temp.OnUpdate(() =>
        {
            completedMats[oldStep].SetFloat("_DissolveAmount", dissolveAmounts[oldStep]);
        });

        dissolveStep++;
        if (dissolveStep > 3)
        {
            dissolveStep = 0;
        }
    }

    public void AddAchCoins(long newCoins)
    {
        newAchCoins += newCoins;

        if (!handleRunning)
        { //coroutine starten wenn sie noch nicht läuft
            handleRunning = true;
            Timing.RunCoroutine(HandleAddAchCoins());
        }
    }

    public void StartSpawnAchObjs(GameObject caller, Vector3 startPos, long newCoins)
    {
        CollectClicked(caller);

        Timing.RunCoroutine(_SpawnAchObjs(startPos, newCoins));
    }

    private IEnumerator<float> _SpawnAchObjs(Vector3 startPos, long newCoins)
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
            ach.target = 
                new Vector3(-648.2f + Random.Range(-28, 28), 1200 + Random.Range(-28, 28), 0);
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
            yield return Timing.WaitForSeconds(0.05f);
        }
    }

    private IEnumerator<float> HandleAddAchCoins()
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

            yield return Timing.WaitForSeconds(waitTime);

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

        handleRunning = false;
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

        for (int a = 0; a < allAchievements.Length; a++)
        {
            allAchievements[a].step = 0;
            allAchievements[a].completed = false;
            allAchievements[a].rewardCollected = false;
        }

        if (achString.Length > 0)
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
                    if(allAchievements[a].identifier.Contains(identifier))
                    { //assigned achievement found
                        allAchievements[a].step = currentStep;
                        allAchievements[a].completed = completed;
                        allAchievements[a].rewardCollected = rewardCollected;
                        //break; kein break da reset für alle erfolgen muss
                    }
                }
            }

        }

        allAchHolders = new AchHolder[allAchievements.Length];

        UpdateUI(true);
    }

    public void UpdateUI(bool create = false)
    {
        for (int i = 0; i < allAchievements.Length; i++)
        { //position wird automatisch angepasst
            AchHolder holder;
            Achievement ach = allAchievements[i];

            if (create)
            {
                holder = Instantiate(achPrefab, contentParent).GetComponent<AchHolder>();
            }
            else
            {
                holder = allAchHolders[i];
            }

            holder.achievement = ach;

            holder.titleText.text = ach.titleString;
            holder.descriptionText.text = ach.descriptionString;

            holder.sprite.sprite = ach.mainSprite[ach.upgradeStep];

            holder.progressText.text =
                ach.step.ToString() + "/" + ach.maxStep.ToString();

            holder.rewardText.text = ach.rewards[ach.upgradeStep].amount.ToString();

            float val = (float)ach.step / ach.maxStep;
            holder.progressSlider.value = val;

            Color32 collectColor = lockedColor;
            holder.collectButton.interactable = false;

            if (ach.completed)
            {
                holder.completedSprite.gameObject.SetActive(true);

                if (ach.rewardCollected)
                {
                    holder.gameObject.GetComponent<Image>().color = new Color32(188, 188, 188, 255);

                    collectColor = Color.white;
                    holder.collectButton.interactable = false;

                    holder.completedSprite.gameObject.SetActive(false);
                }
                else
                {
                    collectColor = unlockedColor;
                    holder.collectButton.interactable = true;

                    Color c = new Color32(200, 252, 255, 255);

                    holder.completedSprite.color = c;
                    holder.completedSprite.gameObject.SetActive(true);
                }
            }
            else
            {
                holder.completedSprite.gameObject.SetActive(false);
            }

            byte alphaVal = 255;
            if (!holder.collectButton.interactable)
            {
                alphaVal = 180;
            }

            collectColor.a = alphaVal;

            Color32 temp = holder.rewardSprite.color;
            temp.a = alphaVal;
            holder.rewardSprite.color = temp;

            temp = holder.rewardText.color;
            temp.a = alphaVal;
            holder.rewardText.color = temp;

            holder.collectSprite.color = collectColor;

            if(create)
            {
                allAchHolders[i] = holder;
            }
        }

        SortAchievements();
    }

    public void SaveAchievements()
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

    public Achievement GetAchievementByString(string identifier)
    {
        for(int i = 0; i< allAchievements.Length; i++)
        {
            if(allAchievements[i].identifier.Equals(identifier))
            {
                return allAchievements[i];
            }
        }

        Debug.LogError("Achievement '" + identifier + "' not found!");
        return null;
    }

    public void UnlockComplete(string identifier)
    {
        UnlockComplete(GetAchievementByString(identifier));
    }

    public void UnlockComplete(Achievement ach)
    {
        if(ach.completed)
        {
            return;
        }

        ach.step = ach.maxStep;
        ach.completed = true;

        UpdateMenuUnclaimed();

        AchUnlockHandler.Instance.StartAchievementUnlock(ach);
    }

    public void UpdateStep(string identifier, int amount = 1)
    {
        UpdateStep(GetAchievementByString(identifier), amount);
    }

    public void UpdateStep(Achievement ach, int amount = 1)
    {
        if(ach.completed)
        {
            return;
        }

        ach.step += amount;
        if(ach.step >= ach.maxStep)
        {
            ach.step = ach.maxStep;

            ach.completed = true;
            ach.rewardCollected = false;

            UpdateMenuUnclaimed();

            AchUnlockHandler.Instance.StartAchievementUnlock(ach);
        }
    }

    public void OpenAchievements()
    {
        hParent.SetActive(false);

        //Update UI
        UpdateUI();

        mainHandle = Timing.RunCoroutine(Util._EmulateUpdate(_MainUpdate, this));

        achParent.SetActive(true);
    }

    public void OpenHighscores()
    {
        hParent.SetActive(true);
        achParent.SetActive(false);

        Timing.KillCoroutines(mainHandle);
    }

    public void CollisionEnter(GameObject achObj)
    {
        AchMoveObj obj = achObj.GetComponent<AchMoveObj>();

        if (obj.value > 0)
        {
            AddAchCoins(obj.value);
        }

        achObj.SetActive(false);
        achMoveObjs.Remove(achObj);

        if(achCollTween != null)
        {
            if(achCollTween.IsActive())
            {
                achCollTween.Kill();
            }
        }

        if (achColorTween != null)
        {
            if (achColorTween.IsActive())
            {
                achColorTween.Kill();
            }
        }

        achCoin.transform.localScale = new Vector3(2f, 2f, 2f);

        Color32 c = Color.white;
        c.a = 180;

        achCoin.transform.GetChild(0).GetComponent<Image>().color = c;

        achCollTween = achCoin.transform.DOScale(1, 0.25f);
        achColorTween = achCoin.transform.GetChild(0).GetComponent<Image>().DOFade(0, 0.25f);
    }

    public void SortAchievements()
    { //alle erreichten achs nach oben an die liste -> dann completed & reward uncollected -> rest darunter

        List<Transform> completedAchs = new List<Transform>();
        List<Transform> notCollectedAchs = new List<Transform>();

        for (int i = 0; i < allAchHolders.Length; i++)
        {
            allAchHolders[i].transform.SetParent(transform);

            if(allAchHolders[i].achievement.completed)
            {
                if(allAchHolders[i].achievement.rewardCollected)
                {
                    completedAchs.Add(allAchHolders[i].transform);
                }
                else
                {
                    notCollectedAchs.Add(allAchHolders[i].transform);
                }
            }
        }

        for(int i = 0; i < completedAchs.Count; i++)
        {
            completedAchs[i].SetParent(contentParent);
        }

        for(int i = 0; i < notCollectedAchs.Count; i++)
        {
            notCollectedAchs[i].SetParent(contentParent);
        }

        for(int i = 0; i < allAchHolders.Length; i++)
        {
            if(!completedAchs.Contains(allAchHolders[i].transform) &&
                !notCollectedAchs.Contains(allAchHolders[i].transform))
            {
                allAchHolders[i].transform.SetParent(contentParent);
            }
        }
    }

    public void CloseClicked()
    {
        Timing.KillCoroutines(mainHandle);
        ScoreHandler.Instance.CloseAchievements();
    }

    // Update is called once per frame
    void _MainUpdate()
    {
        if(flashDir == 0)
        { //hoch
            flashColor.a += Time.deltaTime;

            if(flashColor.a >= 0.99f)
            {
                flashDir = 1;
            }
        } else
        {
            flashColor.a -= Time.deltaTime;

            if(flashColor.a <= 0.01f)
            {
                flashColor.a = 0;
                flashDir = 0;
            }
        }

        if(allAchHolders != null)
        {
            for (int i = 0; i < allAchHolders.Length; i++)
            {
                if (allAchHolders[i].achievement.completed &&
                    !allAchHolders[i].achievement.rewardCollected)
                {
                    allAchHolders[i].collectFlashSprite.color = flashColor;
                    allAchHolders[i].sliderFiller.color = flashColor;
                }
            }
        }

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

            /*if(obj.currentTime >= obj.scaleDownTime &&
                !obj.scaleDownDone)
            {
                achMoveObjs[i].transform.DOScale(0, obj.scaleDownTime - 0.01f);
                obj.scaleDownDone = true;
            }*/

            /*if(obj.currentTime >= obj.maxTime)
            {
                if(obj.value > 0)
                {
                    AddAchCoins(obj.value);
                }

                achMoveObjs[i].SetActive(false);
                delList.Add(achMoveObjs[i]);
            }*/
        }

        for(int i = 0; i < delList.Count; i++)
        {
            achMoveObjs.Remove(delList[i]);
        }
        delList.Clear();
    }
}
