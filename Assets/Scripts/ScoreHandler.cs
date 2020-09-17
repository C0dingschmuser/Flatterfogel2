using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using CodeStage.AntiCheat.Storage;
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITY_ANDROID
using UnityEngine.SocialPlatforms;
#endif
using Firebase.Analytics;
using Random = UnityEngine.Random;

public class ScoreHandler : MonoBehaviour
{
    public AchievementHandler achHandler;
    public ShopHandler shop;
    public PipeCustomizationHandler pipeCustomizationHandler;

    public SwipeDetector swDetector = null;

    public Canvas windowCanvas;
    public Material medalMat;
    public GameObject[] medalObjs;
    public GameObject scoreObj, perfectHitObj, highscoreObj, coinObj, tapObj, highscoreList, okButton, personalCoinsObj,
        continueEffects, xpParent, goAgainButton, menuButton;
    public GameObject inputParent, hParent, eventSystem, achParent, fullParent, smallPipePrefab, playerObj, levelTextObj;

    public ObjectPooler objPooler;
    public GameObject inAppUpdate, inAppDialogue, inAppProgress, inAppProgressButton, inAppError;
    public TextMeshProUGUI progressText, dialogueSize, timeButtonText;
    public Slider progressSlider;

    public Color[] prestigeColors;
    public Transform nameParent, scoreParent, postParent, levelParent, highscoreDataParent, updateParent,
        positionParent, playersParent, pipeParent, arrowParent, windowParent, accountWindowParent,
        otherChangelogParent;

    public GameObject playerObjPrefab, loadingObj, updateText, otherChangelogText, otherChangelogVersiontext;
    public GraphicRaycaster raycaster;

    public TMP_InputField nameInput, backupInput;
    public TextMeshProUGUI nameInfoText, backupText, 
        highscoreModeText, highscoreModeBGText;

    public AccountHandler accountHandler;
    public LevelHandler lvlHandler;
    public FlatterFogelHandler ffHandler;
    public List<ScoreHolder> scoreData = new List<ScoreHolder>();

    public Vector3 highscoreStartPos, defaultHighscorePos;
    public Transform modeButtonParent, diffButtonParent;

    public LocalizedString perfectHit, loading, day, week, global;
    public string perfectHitString, loadingString;
    public string[] timeString = new string[3];

    public static float moveTime = 0.25f;
    public static ObscuredInt personalCoins = 0;
    public static ScoreHandler Instance;

    private float mainTimer = 0;
    private Coroutine timerRoutine = null;

    private Tween moveTween = null;
    private Coroutine highscoreDisplay = null;
    private int diffClicked = -1, modeSelected = 1, timeSelected = 0, newBundleCode = 0, bundleSize = 18000000, highscorePageIndex = 0, highscoreMaxPage = 1;
    private bool registerRunning = false, fetchRunning = false, closing = false, highscoreActive = false,
        opening = false, dataFetched = false, highscoreDisplayRunning = false, moveRunning = false;
    private Vector3 originalPipeParentPos, originalPlayerParentPos;
    private HsStringData[] hsData;

    ulong ceiling = 1000, bottom = 0;
    int scoreDiff = 1000;
    float lastP = 1;

    int temp = 0;

    public class HsStringData
    {
        //0 = Global
        //1 = Weekly
        //2 = Daily
        public string[] dataString = new string[3];
    }

    public class ScoreHolder
    {
        public string username;
        public GraveTop gTop;
        public GraveSide gSide;
        public GraveBottom gBottom;
        public ulong score;
        public float offset = 0;
    }

    private void Awake()
    {
        hsData = new HsStringData[3];

        for(int i = 0; i < 3; i++)
        {
            hsData[i] = new HsStringData();
        }

        accountHandler.Initialize();

        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;

        originalPipeParentPos = pipeParent.position;
        originalPlayerParentPos = playersParent.transform.position;

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //UpdateScoreboard(16, 600);

        personalCoins = ObscuredPrefs.GetInt("Coins", 0); //CoinPers gefarmte spendencoins

        StartCoroutine(GetStatus());
    }

    public void StartLoadLocalization()
    {
        StartCoroutine(LoadLocalization());
    }

    private IEnumerator LoadLocalization()
    {
        AsyncOperationHandle handle;

        yield return handle = perfectHit.GetLocalizedString();
        perfectHitString = (string)handle.Result;

        yield return handle = loading.GetLocalizedString();
        loadingString = (string)handle.Result;

        yield return handle = day.GetLocalizedString();
        timeString[1] = (string)handle.Result;

        yield return handle = week.GetLocalizedString();
        timeString[2] = (string)handle.Result;

        yield return handle = global.GetLocalizedString();
        timeString[0] = (string)handle.Result;

        yield return handle = accountHandler.connecting.GetLocalizedString();
        accountHandler.connectionString = (string)handle.Result;

        yield return handle = accountHandler.connectingFailed.GetLocalizedString();
        accountHandler.connectionFailedString = (string)handle.Result;

        yield return handle = accountHandler.invalidName.GetLocalizedString();
        accountHandler.invalidNameString = (string)handle.Result;

        yield return handle = accountHandler.invalidPassword.GetLocalizedString();
        accountHandler.invalidPasswordString = (string)handle.Result;

        yield return handle = accountHandler.checking.GetLocalizedString();
        accountHandler.checkingString = (string)handle.Result;

        yield return handle = accountHandler.length.GetLocalizedString();
        accountHandler.lengthString = (string)handle.Result;

        yield return handle = accountHandler.invalidEmail.GetLocalizedString();
        accountHandler.invalidEmailString = (string)handle.Result;

        yield return handle = accountHandler.nameExists.GetLocalizedString();
        accountHandler.nameExistsString = (string)handle.Result;

        yield return handle = accountHandler.emailRegistered.GetLocalizedString();
        accountHandler.emailRegisteredString = (string)handle.Result;

        yield return handle = accountHandler.loginFailed.GetLocalizedString();
        accountHandler.loginFailedString = (string)handle.Result;

        yield return handle = accountHandler.invalidCode.GetLocalizedString();
        accountHandler.invalidCodeString = (string)handle.Result;

        yield return handle = accountHandler.lookInSpam.GetLocalizedString();
        accountHandler.lookInSpamString = (string)handle.Result;

        yield return handle = accountHandler.resetPassword.GetLocalizedString();
        accountHandler.resetPasswordString = (string)handle.Result;
    }

    public void ChangeTimeClicked()
    {
        if (fetchRunning || !dataFetched || highscoreDisplayRunning || moveRunning)
        {
            return;
        }

        timeSelected++;
        if(timeSelected > 2)
        {
            timeSelected = 0;
        }

        timeButtonText.text = timeString[timeSelected];

        playersParent.position = originalPlayerParentPos;
        pipeParent.position = originalPipeParentPos;

        highscorePageIndex = 0;

        //StartCoroutine(FetchHighscores());

        highscoreDisplay = 
            StartCoroutine(FetchHighscoreData(RealModeToArrayID(modeSelected), timeSelected));
    }

    private void ReactivateEventSystem()
    {
        ReactivateEventSystemFull();

        highscoreActive = true;

        if (closing)
        {
            highscoreActive = false;

            closing = false;
            hParent.SetActive(false);
            achParent.SetActive(false);

            swDetector.enabled = false;

            windowCanvas.sortingOrder = 10;

            playersParent.transform.position = originalPlayerParentPos;
            pipeParent.transform.position = originalPipeParentPos;

            DisableHighscoreObjs();

            FF_PlayerData.Instance.inHighscores = false;

            playerObj.SetActive(true);
            levelTextObj.SetActive(true);

            FlatterFogelHandler.Instance.SpawnMainGravestone();

            FirebaseHandler.SetCurrentScreen("MainMenu", "UnityPlayerActivity");

            //MenuData.Instance.DoScaleUp();
            StartCoroutine(MenuData.Instance.DoMoveIn());
        } else if(opening)
        {
            opening = false;
        }

        if(!moveRunning)
        {
            playersParent.SetParent(highscoreList.transform);

            playersParent.SetAsLastSibling();
            playersParent.SetSiblingIndex(playersParent.GetSiblingIndex() - 1);
        } else
        {
            moveRunning = false;
        }

    }

    public void DisableHighscoreObjs()
    {
        for (int a = 0; a < playersParent.childCount; a++)
        { //alte playerobjs löschen
            Destroy(playersParent.GetChild(a).gameObject);
        }

        List<Transform> smallPipes = new List<Transform>();

        for (int a = 0; a < pipeParent.childCount; a++)
        { //alte smallpipes deaktivieren
            //if (pipeParent.GetChild(a).CompareTag("SmallPipe"))
            //{
                pipeParent.GetChild(a).gameObject.SetActive(false);

            //    smallPipes.Add(pipeParent.GetChild(a));
            //} else
            //{
            //    pipeParent.GetChild(a).gameObject.SetActive(true);
            //}
        }

        //FlatterFogelHandler.Instance.EnableDisableGravestones(true);

        for(int a = 0; a < smallPipes.Count; a++)
        {
            smallPipes[a].SetParent(objPooler.transform);
        }
    }

    private void ReactivateEventSystemFull()
    {
        eventSystem.SetActive(true);
    }

    public void ShowHighscores()
    {
        if (hParent.activeSelf || opening || closing) return;

        FlatterFogelHandler.Instance.DespawnGravestone();

        //windowCanvas.sortingOrder = 11;
        StartCoroutine(MenuData.Instance.DoMoveAway());

        ModeManager.Instance.BackClicked();

        eventSystem.SetActive(false);

        fullParent.SetActive(true);

        achParent.SetActive(false);
        achParent.transform.position = defaultHighscorePos;

        hParent.SetActive(true);
        hParent.transform.position = highscoreStartPos;
        hParent.transform.localScale = Vector3.one;

        swDetector.enabled = false;

        transform.localScale = Vector3.one;

        hParent.transform.DOMove(defaultHighscorePos, moveTime);
        //hParent.transform.DOScale(new Vector3(1, 1, 1), moveTime);

        FirebaseHandler.SetCurrentScreen("Highscores", "UnityPlayerActivity");

        opening = true;
        Invoke(nameof(ReactivateEventSystem), moveTime + 0.01f);

        FF_PlayerData.Instance.inHighscores = true;

        playersParent.SetParent(hParent.transform.parent);

        playersParent.SetAsLastSibling();
        playersParent.SetSiblingIndex(playersParent.GetSiblingIndex() - 1);

        windowParent.gameObject.SetActive(true);
        accountWindowParent.gameObject.SetActive(false);

        if (accountHandler.accountState != AccountStates.LoggedOut)
        {
            ForceHighscoreStart();
        }
        else
        {
            bool overrideAuth = false; //DEBUG da test
#if UNITY_EDITOR
            overrideAuth = true;
#endif
            inputParent.SetActive(true);

            accountHandler.ResetMenu();

            highscoreList.SetActive(false);

            bool ok = false;

            /*if (Social.localUser.authenticated || overrideAuth)
            { //registrierung mit test daten
#if UNITY_ANDROID || UNITY_IOS
                AccountHandler.Instance.StartGplayRegister();
#endif          
                ok = true;
            }*/

            if (!ok)
            { //öffnet login fenster
                windowParent.gameObject.SetActive(false);
                accountWindowParent.gameObject.SetActive(true);
                windowCanvas.sortingOrder = 11;
            }

            accountHandler.ResetMenu(ok);
        }
    }

    public void ForceHighscoreStart()
    {
        inputParent.SetActive(false);
        highscoreList.SetActive(true);

        windowParent.gameObject.SetActive(true);
        accountWindowParent.gameObject.SetActive(false);
        windowCanvas.sortingOrder = 10;

        HandleColor(0, modeSelected);
        HandleColor(1, OptionHandler.GetDifficulty());

        timeButtonText.text = timeString[timeSelected];

        //if(!dataFetched)
        //{
            if (!fetchRunning)
            {
                StartCoroutine(FetchHighscores());
            }
        //}
    }

    public void CloseHighscores()
    {
        if(registerRunning || AccountHandler.running || moveRunning || highscoreDisplayRunning || fetchRunning) //fetchrunning alt
        {
            return;
        }

        closing = true;

        eventSystem.SetActive(false);

        loadingObj.SetActive(false);

        playersParent.SetParent(hParent.transform.parent);

        playersParent.SetAsLastSibling();
        playersParent.SetSiblingIndex(playersParent.GetSiblingIndex() - 1);

        hParent.transform.DOMove(highscoreStartPos, moveTime);
        //hParent.transform.DOScale(0, moveTime);

        if(highscoreActive)
        {
            for(int i = 0; i < playersParent.childCount; i++)
            {
                playersParent.GetChild(i).DOMoveY(-200, 0.2f);
            }

            for(int i = 0; i < pipeParent.childCount; i++)
            {
                if(pipeParent.GetChild(i).CompareTag("SmallPipe"))
                {
                    pipeParent.GetChild(i).DOMoveY(-200, 0.2f);
                }
            }
        }

        Invoke(nameof(ReactivateEventSystem), moveTime + 0.01f);
    }

    public void CloseAchievements()
    {
        closing = true;

        eventSystem.SetActive(false);

        achParent.transform.DOMove(highscoreStartPos, moveTime).SetEase(Ease.InBack);
        //hParent.transform.DOScale(0, moveTime);

        Invoke(nameof(ReactivateEventSystem), moveTime + 0.01f);
    }

    public void NameTextChanged()
    {
        backupInput.text = "";
    }

    public void BackupTextChanged()
    {
        nameInput.text = "";
    }

    public void InputOkayClicked()
    {
        inputParent.SetActive(false);
        ShowHighscores();
    }

    private string GenerateBackup()
    {
        string b = "";

        string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        for(int i = 0; i < 16; i++)
        {
            b += alphabet[Random.Range(0, alphabet.Length)];
        }

        return b;
    }

    public void SetHSDiff(int diff)
    {
        if(fetchRunning)
        {
            return;
        }

        diffClicked = diff;

        HandleColor(1, diff);

        StartCoroutine(FetchHighscores());
    }

    private void HandleColor(int type, int pos)
    {
        if(type == 0)
        { //mode
            for (int i = 0; i < modeButtonParent.childCount; i++)
            {
                Color c = Color.white;

                if (pos - 1 == i)
                {
                    c = FlatterFogelHandler.pr0Farben[0];
                }

                modeButtonParent.GetChild(i).GetComponent<Image>().color =
                    c;
            }
        } else
        { //diff

            for (int i = 0; i < diffButtonParent.childCount; i++)
            {
                Color c = Color.white;

                if (pos == i)
                {
                    c = FlatterFogelHandler.pr0Farben[0];
                }

                diffButtonParent.GetChild(i).GetComponent<Image>().color =
                    c;
            }
        }
    }

    public void ModeClicked(int mode)
    {
        if (fetchRunning || !dataFetched || highscoreDisplayRunning || moveRunning ||
            modeSelected == mode)
        {
            return;
        }

        playersParent.position = originalPlayerParentPos;
        pipeParent.position = originalPipeParentPos;

        highscorePageIndex = 0;

        modeSelected = mode;

        HandleColor(0, mode);

        //StartCoroutine(FetchHighscores());

        highscoreDisplay = StartCoroutine(FetchHighscoreData(RealModeToArrayID(mode), timeSelected));
    }

    IEnumerator StartTimer()
    {
        mainTimer = 0;
        while(true)
        {
            yield return new WaitForSeconds(0.05f);
            mainTimer += 0.05f;
        }
    }

    IEnumerator FetchHighscores()
    {
        timerRoutine = StartCoroutine(StartTimer());

        fetchRunning = true;

        if(highscoreDisplay != null)
        {
            StopCoroutine(highscoreDisplay);
        }

        for(int i = 0; i < nameParent.childCount; i++)
        {
            if(i > 0)
            {
                nameParent.GetChild(i).gameObject.SetActive(false);
            }
            scoreParent.GetChild(i).gameObject.SetActive(false);
            levelParent.GetChild(i).gameObject.SetActive(false);
            positionParent.GetChild(i).gameObject.SetActive(false);
        }

        nameParent.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            accountHandler.connectionString; 

        string mName = ModeManager.Instance.mainModes[modeSelected - 1].modeName;
        highscoreModeText.text = mName;
        highscoreModeBGText.text = mName;

        int diff = OptionHandler.GetDifficulty();

        if (diffClicked > -1)
        {
            diff = diffClicked;
        }

        ulong lastS = ffHandler.GetLastScore();

        //nameList.text = "Lade Daten...";

        WWWForm form = new WWWForm();

        for (int i = 0; i < 5; i++)
        {
            //Global
            form.AddField("highscore" + (i + 1).ToString(), ffHandler.GetHighscore(i).score.ToString());

            //Daily
            form.AddField("highscore" + (i + 1).ToString() + "_daily",
                ffHandler.GetHighscore(i, HighscoreType.Daily).score.ToString());

            //Daily-Time
            form.AddField("highscore" + (i + 1).ToString() + "_daily_t",
                ffHandler.GetHighscore(i, HighscoreType.Daily).recordTime.ToString("yyyy-MM-dd HH:mm:ss"));

            //Weekly
            form.AddField("highscore" + (i + 1).ToString() + "_weekly",
                ffHandler.GetHighscore(i, HighscoreType.Weekly).score.ToString());

            //Weekly-Time
            form.AddField("highscore" + (i + 1).ToString() + "_weekly_t",
                ffHandler.GetHighscore(i, HighscoreType.Weekly).recordTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        long currentLvl = lvlHandler.GetLVL();
        int currentPrestige = lvlHandler.GetPrestige();

        string username = AccountHandler.Instance.username;

        ShopHandler shop = ShopHandler.Instance;

        string gTop = shop.allGraveTops[shop.GetSelected(CustomizationType.GraveTop)].identifier;
        string gSide = shop.allGraveSides[shop.GetSelected(CustomizationType.GraveSide)].identifier;
        string gBottom = shop.allGraveBottoms[shop.GetSelected(CustomizationType.GraveBottom)].identifier;

        Skin currentSkin = shop.allSkins[shop.GetSelected(CustomizationType.Skin)];

        string skinString = currentSkin.identifier;
        string pipeString = shop.allPipes[shop.GetSelected(CustomizationType.Pipe)].identifier;
        string pipeColorID = shop.GetPipeColorID().ToString();
        string wingString = "null";

        if (currentSkin.overrideWing == null)
        { //wenn kein override
            wingString = shop.allWings[shop.GetSelected(CustomizationType.Wing)].identifier;
        }

        string hatString = shop.allHats[shop.GetSelected(CustomizationType.Hat)].identifier;

        string authHash = AccountHandler.Md5Sum(username + Auth.authKey);

        form.AddField("diff", diff.ToString());
        form.AddField("setscore", "1");
        form.AddField("lastscore", lastS.ToString());

        form.AddField("lvl", currentLvl.ToString());
        form.AddField("pr", currentPrestige.ToString());

        form.AddField("classicAvg", Math.Round(StatHandler.classicAvg, 2).ToString());
        form.AddField("miningAvg", Math.Round(StatHandler.miningAvg, 2).ToString());
        form.AddField("destructionAvg", Math.Round(StatHandler.destructionAvg, 2).ToString());

        form.AddField("classicMAX", StatHandler.classicCount.ToString());
        form.AddField("miningMAX", StatHandler.miningCount.ToString());
        form.AddField("destructionMAX", StatHandler.destructionCount.ToString());

        form.AddField("hs", "1");
        form.AddField("mode", "-1");

        form.AddField("setgrave", "1");
        form.AddField("gtop", gTop);
        form.AddField("gside", gSide);
        form.AddField("gbottom", gBottom);

        form.AddField("setskin", "1");
        form.AddField("skin", skinString);
        form.AddField("wing", wingString);
        form.AddField("hat", hatString);

        form.AddField("pipe", pipeString);
        form.AddField("pipecolor", pipeColorID);

        form.AddField("name", username);
        form.AddField("hash", authHash);

        loadingObj.SetActive(true);
        loadingObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
        loadingObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            loadingString;

        using (UnityWebRequest www = UnityWebRequest.Post("https://bruh.games/manager.php", form))
        {
#pragma warning disable CS0618 // Typ oder Element ist veraltet
            www.chunkedTransfer = false;
#pragma warning restore CS0618 // Typ oder Element ist veraltet

            www.timeout = 10;

            yield return www.SendWebRequest(); //wartet das abfrage fertig ist

            StopCoroutine(timerRoutine);
            float remaining = 0.2f - mainTimer;

            if (remaining > 0.01)
            {
                yield return new WaitForSeconds(remaining);
            }

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                nameParent.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    AccountHandler.Instance.connectionFailedString;

                loadingObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
                loadingObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    accountHandler.connectionFailedString;
            }
            else
            {
                loadingObj.SetActive(false);

                //FetchString(www.downloadHandler.text)

                string wwwData = www.downloadHandler.text;

                if (!wwwData.Contains("#") || !wwwData.Contains(",") || !wwwData.Contains("|"))
                {
                    nameParent.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                        "Error while Parsing:\n" + wwwData;

                    fetchRunning = false;

                    yield return null;
                }
                else
                {
                    Debug.Log("UString: " + wwwData);
                }

                string[] rawData = wwwData.Split('|'); //split nach modi

                if (rawData.Length > 2)
                {
                    for(int a = 0; a < 3; a++)
                    { //loop durch modi

                        string[] time = rawData[a].Split('~');

                        for (int i = 0; i < 3; i++)
                        { //loop durch time

                            if(a == 0)
                            {
                                Debug.Log(i + " " + time[i]);
                            }

                            hsData[a].dataString[i] = time[i];
                        }
                    }

                    int arrayID = RealModeToArrayID(modeSelected);

                    highscoreDisplay = StartCoroutine(FetchHighscoreData(arrayID, timeSelected));

                    dataFetched = true;
                }
            }
        }

        fetchRunning = false;
    }

    private int RealModeToArrayID(int mode)
    {
        int arrayID = 0;

        switch (mode) //real id
        {
            case 1: //classic
                arrayID = 0;
                break;
            case 5: //destruction
                arrayID = 1;
                break;
            case 4: //mining
                arrayID = 2;
                break;
        }

        return arrayID;
    }

    private IEnumerator FetchHighscoreData(int arrayID, int timeID, bool wait = false)
    { //fetcht die bereits erhaltenen daten und zeigt sie an

        if(wait)
        {
            yield return new WaitForSeconds(0.4f);
        }

        string mName = ModeManager.Instance.mainModes[modeSelected - 1].modeName;
        highscoreModeText.text = mName;
        highscoreModeBGText.text = mName;

        highscoreDisplayRunning = true;
        swDetector.enabled = false;

        string username = AccountHandler.Instance.username;

        ShopHandler shop = ShopHandler.Instance;

        nameParent.GetChild(0).gameObject.SetActive(true);
        scoreParent.GetChild(0).gameObject.SetActive(true);
        levelParent.GetChild(0).gameObject.SetActive(true);
        positionParent.GetChild(0).gameObject.SetActive(true);

        string[] rawData = hsData[arrayID].dataString[timeID].Split('#');

        for (int i = 0; i < nameParent.childCount; i++)
        {
            positionParent.GetChild(i).GetComponent<TextMeshProUGUI>().text = "";
            positionParent.GetChild(i).GetComponent<TextMeshProUGUI>().color = Color.black;
            nameParent.GetChild(i).GetComponent<TextMeshProUGUI>().text = "";
            nameParent.GetChild(i).GetComponent<TextMeshProUGUI>().color = Color.black;
            scoreParent.GetChild(i).GetComponent<TextMeshProUGUI>().text = "";
            scoreParent.GetChild(i).GetComponent<TextMeshProUGUI>().color = Color.black;
            levelParent.GetChild(i).GetComponent<Image>().color = prestigeColors[0];
            levelParent.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = "1";
        }

        for (int a = 0; a < playersParent.childCount; a++)
        { //alte playerobjs löschen
            Destroy(playersParent.GetChild(a).gameObject);
        }

        List<Transform> smallPipes = new List<Transform>();

        for (int a = 0; a < pipeParent.childCount; a++)
        { //alte smallpipes deaktivieren
            if (pipeParent.GetChild(a).CompareTag("SmallPipe"))
            {
                pipeParent.GetChild(a).gameObject.SetActive(false);

                smallPipes.Add(pipeParent.GetChild(a));
            } else
            {
                pipeParent.GetChild(a).gameObject.SetActive(false);
            }
        }

        for (int a = 0; a < smallPipes.Count; a++)
        {
            smallPipes[a].SetParent(objPooler.transform);
        }

        DestructionHandler.Instance.ClearAll();

        FlatterFogelHandler.Instance.DisableOtherObjs();
        FlatterFogelHandler.Instance.EnableDisableGravestones(false);

        playerObj.SetActive(false);
        levelTextObj.SetActive(false);

        bool userInHS = false;

        if(hsData[arrayID].dataString[timeID].Contains(username))
        { //user in hs
            userInHS = true;
        }

        ceiling = 1000;
        lastP = 1;
        temp = 0;

        int playerCount = rawData.Length - 1;

        int p = 0;

        if (playerCount > 0)
        {
            string[] scoreData = rawData[playerCount - 1].Split(',');

            bottom = ulong.Parse(scoreData[0]);

            for (p = 0; p < playerCount; p++)
            {
                scoreData = rawData[p].Split(',');

                CreatePlayerObj(p, ulong.Parse(scoreData[0]), scoreData[1],
                    scoreData[4], scoreData[5], scoreData[6], scoreData[7],
                    Int32.Parse(scoreData[8]));

                if (p < 8)
                {
                    yield return new WaitForSeconds(0.075f);
                }
                else
                {
                    yield return null;
                }
            }
        }


        if(!userInHS)
        { //user am ende einfügen wenn nicht in highscore
            p++; //extra abstand
            playerCount += 2;

            //long lvl = lvlHandler.GetLVL();
            //int prestige = lvlHandler.GetPrestige();

            Skin currentSkin = shop.allSkins[shop.GetSelected(CustomizationType.Skin)];

            string wingString = "null";
            string hatString = shop.allHats[shop.GetSelected(CustomizationType.Hat)].identifier;
            string pipeString = shop.allPipes[shop.GetSelected(CustomizationType.Pipe)].identifier;

            if (currentSkin.overrideWing == null)
            { //wenn kein override
                wingString = shop.allWings[shop.GetSelected(CustomizationType.Wing)].identifier;
            }

            /*HighscoreType hsType = HighscoreType.Global;

            if (timeSelected == 0)
            {//global 
                hsType = HighscoreType.Global;
            } else if(timeSelected == 1)
            { //daily
                hsType = HighscoreType.Daily;
            } else if(timeSelected == 2)
            { //weekly
                hsType = HighscoreType.Weekly;
            }*/

            CreatePlayerObj(p, ffHandler.GetHighscore(modeSelected - 1, (HighscoreType)timeSelected).score, username,
                currentSkin.identifier, wingString, hatString, pipeString, shop.GetPipeColorID());
        }

        highscorePageIndex = 0;
        highscoreMaxPage = (int)Math.Ceiling(playerCount / 6f);

        arrowParent.GetChild(0).GetComponent<Button>().interactable = false;

        if (highscoreMaxPage > 1)
        {
            arrowParent.GetChild(1).GetComponent<Button>().interactable = true;
        }
        else
        {
            arrowParent.GetChild(1).GetComponent<Button>().interactable = false;
        }

        nameParent.GetChild(nameParent.childCount - 1).gameObject.SetActive(false);
        scoreParent.GetChild(nameParent.childCount - 1).gameObject.SetActive(false);
        levelParent.GetChild(levelParent.childCount - 1).gameObject.SetActive(false);
        positionParent.GetChild(levelParent.childCount - 1).gameObject.SetActive(false);

        highscoreDisplayRunning = false;
        swDetector.enabled = true;
    }

    private void CreatePlayerObj(int i, ulong score, string username,
        string skin, string wing, string hat, string pipe, int pipeColor, bool player = false)
    {
        int num = i + 1;

        GameObject newPlayer = Instantiate(playerObjPrefab, playersParent);
        Vector3 pos = new Vector3(-655 + (100 * i), 264, 20f);

        //1015 max y
        //320 min y
        //diff 695
        if (i == 0)
        {
            ceiling = score;
            scoreDiff = (int)(ceiling - bottom);
        }

        //float p = (score / (float)ceiling); alt

        int tempDiff = (int)(ceiling - score);

        float scoreDiff_float = (float)scoreDiff;

        if(scoreDiff_float < 0.0001f)
        {
            scoreDiff_float = 0.0001f;
        }

        //Höhe in Prozent
        float p = Mathf.Clamp(((float)scoreDiff - tempDiff) / scoreDiff_float, 0, 1);

        /*if(i > 0)
        {
            if(lastP - p > 0.45f)
            { //wenn abstand zu groß glätten
                p = Mathf.Abs(lastP - 0.15f);

                ceiling = (ulong)((float)ceiling * 0.85f);
            }
        }*/
        lastP = p;

        float yPos = 320 + (p * 695);

        pos.y = yPos;

        newPlayer.transform.position = new Vector3(pos.x, -200, pos.z);
        newPlayer.transform.DOMoveY(pos.y, 0.2f);

        GameObject smallPipe = objPooler.SpawnFromPool("SmallPipe", new Vector3(pos.x, -200, pos.z), Quaternion.identity);
        smallPipe.transform.SetParent(pipeParent);
        //Instantiate(smallPipePrefab, pipeParent);
        smallPipe.SetActive(true);

        pos.y -= 443; //pipe 443 unter skin

        //smallPipe.transform.position = new Vector3(pos.x, -200, pos.z);
        smallPipe.transform.DOMoveY(pos.y, 0.2f);

        Skin playerSkin = (Skin)shop.GetItemByString(CustomizationType.Skin, skin);
        Wing playerWing = (Wing)shop.GetItemByString(CustomizationType.Wing, wing);
        Hat playerHat = (Hat)shop.GetItemByString(CustomizationType.Hat, hat);
        Pipe playerPipe = (Pipe)shop.GetItemByString(CustomizationType.Pipe, pipe);
        Color playerPipeColor = pipeCustomizationHandler.GetPipeColor(pipeColor);

        if (!playerPipe.colorChangeSupported)
        {
            playerPipeColor = Color.white;
        }

        smallPipe.GetComponent<SpriteRenderer>().sprite = playerPipe.sprite[0];
        smallPipe.GetComponent<SpriteRenderer>().color = playerPipeColor;

        smallPipe.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = playerPipe.endSprite[0];
        smallPipe.transform.GetChild(0).GetComponent<SpriteRenderer>().color = playerPipeColor;

        bool isTop = true;

        temp++;
        if (temp == 2)
        {
            isTop = false;
            temp = 0;
        }

        newPlayer.GetComponent<PlayerHolder>().LoadPlayer(playerSkin, playerWing, playerHat, playerPipe,
            playerPipeColor, username, score, isTop);

        if (AccountHandler.Instance.username.Equals(username))
        { //spieler ist in highscores drin
            newPlayer.GetComponent<PlayerHolder>().SetPlayer();
        }

        newPlayer.SetActive(true);
    }

    public void SwipeDetector_OnSwipe(SwipeData data)
    { //1392 max y

        if(data.StartPosition.y > 1392 || data.EndPosition.y > 1392)
        { //swipen nur unten möglich
            return;
        }

        switch (data.Direction)
        {
            case SwipeDirection.Left:
                DirClicked(1);
                break;
            case SwipeDirection.Right:
                DirClicked(0);
                break;
        }
    }

    public void DirClicked(int dir)
    {
        if (fetchRunning || !dataFetched || highscoreDisplayRunning || moveRunning || !highscoreActive)
        {
            return;
        }

        //distance to move: 597

        float amount = 597;
        int indexA = -1;

        if (dir == 1)
        { //rechts
            amount = -amount;
            indexA = 1;
        }

        int newIndex = highscorePageIndex + indexA;

        if(newIndex >= 0 && newIndex < highscoreMaxPage)
        {
            highscorePageIndex = newIndex;

            if(newIndex < highscoreMaxPage - 1)
            { //geht noch weiter nach rechts
                arrowParent.GetChild(1).GetComponent<Button>().interactable = true;
            } else
            {
                arrowParent.GetChild(1).GetComponent<Button>().interactable = false;
            }

            if(newIndex > 0)
            { //geht noch weiter nach links
                arrowParent.GetChild(0).GetComponent<Button>().interactable = true;
            } else
            {
                arrowParent.GetChild(0).GetComponent<Button>().interactable = false;
            }

            eventSystem.SetActive(false);

            moveRunning = true;

            pipeParent.DOMoveX(pipeParent.transform.position.x + amount, 0.29f);
            playersParent.DOMoveX(playersParent.transform.position.x + amount, 0.29f);

            Invoke(nameof(ReactivateEventSystem), 0.3f);
        }
    }

    [Obsolete]
    IEnumerator GetStatus()
    {
        //ulong hs = ffHandler.GetHighscore().score;
        ulong lastS = ffHandler.GetLastScore();

        int diff = OptionHandler.GetDifficulty();

        string username = accountHandler.username;

        WWWForm form = new WWWForm();

        for(int i = 0; i < 5; i++)
        {
            //Global
            form.AddField("highscore" + (i + 1).ToString(), ffHandler.GetHighscore(i).score.ToString());

            //Daily
            form.AddField("highscore" + (i + 1).ToString() + "_daily",
                ffHandler.GetHighscore(i, HighscoreType.Daily).score.ToString());

            //Daily-Time
            form.AddField("highscore" + (i + 1).ToString() + "_daily_t",
                ffHandler.GetHighscore(i, HighscoreType.Daily).recordTime.ToString("yyyy-MM-dd HH:mm:ss"));

            //Weekly
            form.AddField("highscore" + (i + 1).ToString() + "_weekly",
                ffHandler.GetHighscore(i, HighscoreType.Weekly).score.ToString());

            //Weekly-Time
            form.AddField("highscore" + (i + 1).ToString() + "_weekly_t",
                ffHandler.GetHighscore(i, HighscoreType.Weekly).recordTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        //int tempCoins = ObscuredPrefs.GetInt("CoinTmp", 0);

        long currentLvl = lvlHandler.GetLVL();
        int currentPrestige = lvlHandler.GetPrestige();

        string authHash = AccountHandler.Md5Sum(username + Auth.authKey);

        ShopHandler shop = ShopHandler.Instance;

        string gTop = shop.allGraveTops[shop.GetSelected(CustomizationType.GraveTop)].identifier;
        string gSide = shop.allGraveSides[shop.GetSelected(CustomizationType.GraveSide)].identifier;
        string gBottom = shop.allGraveBottoms[shop.GetSelected(CustomizationType.GraveBottom)].identifier;

        Skin currentSkin = shop.allSkins[shop.GetSelected(CustomizationType.Skin)];

        string skinString = currentSkin.identifier;
        string pipeString = shop.allPipes[shop.GetSelected(CustomizationType.Pipe)].identifier;
        string pipeColorID = shop.GetPipeColorID().ToString();
        string wingString = "null";

        if(currentSkin.overrideWing == null)
        { //wenn kein override
            wingString = shop.allWings[shop.GetSelected(CustomizationType.Wing)].identifier;
        }

        string hatString = shop.allHats[shop.GetSelected(CustomizationType.Hat)].identifier;

        form.AddField("last50", "1");
        form.AddField("v", Application.version);
        form.AddField("diff", diff.ToString());

        if (accountHandler.accountState == AccountStates.LoggedOut ||
            accountHandler.accountState != AccountStates.Synced)
        { //username nicht gesetzt bzw nicht gesynced-> abfrage ohne score set
            username = AccountHandler.tempNames[Random.Range(0, 4)];
            authHash = AccountHandler.Md5Sum(username + Auth.authKey);
        } else
        {
            form.AddField("setscore", "1");
            form.AddField("lastscore", lastS.ToString());

            form.AddField("classicAvg", Math.Round(StatHandler.classicAvg, 2).ToString());
            form.AddField("miningAvg", Math.Round(StatHandler.miningAvg, 2).ToString());
            form.AddField("destructionAvg", Math.Round(StatHandler.destructionAvg, 2).ToString());

            form.AddField("classicMAX", StatHandler.classicCount.ToString());
            form.AddField("miningMAX", StatHandler.miningCount.ToString());
            form.AddField("destructionMAX", StatHandler.destructionCount.ToString());

            form.AddField("lvl", currentLvl.ToString());
            form.AddField("pr", currentPrestige.ToString());

            form.AddField("setgrave", "1");
            form.AddField("gtop", gTop);
            form.AddField("gside", gSide);
            form.AddField("gbottom", gBottom);

            form.AddField("setskin", "1");
            form.AddField("skin", skinString);
            form.AddField("wing", wingString);
            form.AddField("hat", hatString);

            form.AddField("pipe", pipeString);
            form.AddField("pipecolor", pipeColorID);
        }

        form.AddField("name", username);
        form.AddField("hash", authHash);

        string os = "0";

#if UNITY_IOS
        os = "1";
#endif

        form.AddField("os", os);

        bool otherChangelog = false;

#if UNITY_IOS
        if(!PlayerPrefs.GetString("LastChangelogVersion", Application.version).Equals(Application.version))
        {
            otherChangelog = true;
        }
#elif UNITY_ANDROID
        if (PlayerPrefs.GetInt("OtherChangelog", 0) == 0)
        {
            otherChangelog = true;
        }
#endif

        if(otherChangelog)
        { //force changelog request
            form.AddField("changelogforce", "1");
        } else
        {
            form.AddField("changelogforce", "0");
        }

        #region ErrorHandler
        string exceptionString;

        exceptionString = "Username: " + username + "\n";

        exceptionString += "Version: " + Application.version + "\n";

        exceptionString += ExceptionHandler.exceptionString;

        exceptionString += "--------------------------------------------------------";

#if UNITY_EDITOR
        exceptionString = "";
#endif

        if (exceptionString.Contains("LogType.Error"))
        {
            byte[] encodedByte = System.Text.ASCIIEncoding.ASCII.GetBytes(exceptionString);
            string base64Encoded = Convert.ToBase64String(encodedByte);

            form = new WWWForm();
            form.AddField("name", username);
            form.AddField("hash", authHash);
            form.AddField("stack", base64Encoded);

            using (UnityWebRequest wwwError = UnityWebRequest.Post("https://bruh.games/errorhandler.php", form))
            {
                wwwError.chunkedTransfer = false;

                yield return wwwError.SendWebRequest();

                if(wwwError.isNetworkError || wwwError.isHttpError)
                {
                    Debug.Log(wwwError.error);
                } else
                { //stacktrace übermittelt, kann gelöscht werden
                    ExceptionHandler.exceptionString = "";
                    ObscuredPrefs.SetString("ExceptionString", "");
                }
            }
        }
        #endregion 

        using (UnityWebRequest www = UnityWebRequest.Post("https://bruh.games/manager.php", form))
        {
            www.chunkedTransfer = false;
            www.timeout = 10;

            yield return www.SendWebRequest(); //wartet das abfrage fertig ist

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                FetchString(www.downloadHandler.text);
            }
        }

        //Server Sync abgeschlossen
        //Start Intro

        OptionHandler.Instance.SetIntro();
    }

    public void OpenAchievements()
    {
        if(highscoreDisplayRunning || fetchRunning)
        {
            return;
        }

        achHandler.OpenAchievements();
    }

    public void OpenUpdate(bool open)
    {
        updateParent.gameObject.SetActive(false);
        raycaster.enabled = false;

        if(open)
        {
            string url = "https://google.com";

            if(OptionHandler.playStore)
            {
                url = "https://play.google.com/store/apps/details?id=com.BruhGames.Flatterfogel2";
            } else
            {
#if UNITY_ANDROID
                url = "https://drive.google.com/file/d/1xAwEW3CvlLEcvWRG_zxMCq8w28-_T5RA/view?usp=sharing";
#elif UNITY_IOS
                url = "https://drive.google.com/file/d/1L-RXllM7KcJiirRIh29kV_xLSjGHC8Sp/view?usp=sharing";
#endif
            }

            Application.OpenURL(url);
        }
    }

    public void CloseOtherChangelog()
    {
        otherChangelogParent.gameObject.SetActive(false);

        if(!postParent.gameObject.activeSelf && !GDPRHandler.isActive)
        {
            raycaster.enabled = false;
        }
    }

    public void OpenPost(bool open)
    {
        postParent.gameObject.SetActive(false);
        raycaster.enabled = false;

        string post = PlayerPrefs.GetString("Post");

        if(open)
        {
            Application.OpenURL("https://pr0gramm.com/new/" + post);
        }
    }

    public void InAppClicked(int code)
    {
        switch(code)
        {
            case 0: //dl starten
                inAppDialogue.SetActive(false);
                inAppProgress.SetActive(true);
                StartCoroutine(HandleBundleUpdate(newBundleCode));
                break;
            case 1: //fenster schließen
            case 3: //error
                inAppUpdate.SetActive(false);
                raycaster.enabled = false;
                break;
            case 2: //dl complete -> restart
                SceneManager.LoadScene("FF");
                break;
        }
    }

    private bool FetchString(string s)
    {
        string wwwData = s;

        if (!wwwData.Contains("#") || !wwwData.Contains(","))
        {
            Debug.LogWarning("Update string is in wrong format! -> " + wwwData);
            return false;
        }
        else
        {
            //Debug.Log("UString: " + wwwData);
        }

        string[] rawTypes = wwwData.Split('|');

        string[] vData = rawTypes[1].Split('#');

        string version = vData[0];
        string latestPost = vData[1];
        string bundleVersion = vData[2];
        bundleSize = Int32.Parse(vData[3]);

        string encodedChangelog = vData[4];

        bool otherChangelog = false;

        if (encodedChangelog.Length > 0)
        {
            byte[] bytes = Convert.FromBase64String(encodedChangelog);

            string changelog = System.Text.ASCIIEncoding.UTF8.GetString(bytes);

            updateText.GetComponent<TextMeshProUGUI>().text = changelog;

            otherChangelogVersiontext.GetComponent<TextMeshProUGUI>().text = "Version " + Application.version;
            otherChangelogText.GetComponent<TextMeshProUGUI>().text = changelog;

#if UNITY_IOS
        if(!PlayerPrefs.GetString("LastChangelogVersion", Application.version).Equals(Application.version))
        { //Changelog nach Updates aber nicht bei firstlaunch anzeigen
            otherChangelog = true;
            PlayerPrefs.SetString("LastChangelogVersion", Application.version);
        }
#elif UNITY_ANDROID
            if (PlayerPrefs.GetInt("OtherChangelog", 0) == 0)
            {
                otherChangelog = true;
                PlayerPrefs.SetInt("OtherChangelog", 1);
            }
#endif
        }

        int currentBundleCode = PlayerPrefs.GetInt("DownloadedBundle", PlayerPrefs.GetInt("InstalledBundle"));

        int newBundleCode = Int32.Parse(bundleVersion);

        if(currentBundleCode < newBundleCode)
        {
            Debug.Log("New Bundle Found!");

            this.newBundleCode = newBundleCode;

            int mbytes = (bundleSize / 1024) / 1024; //byte to mbyte conversion
            dialogueSize.text = mbytes.ToString() + "MB";

            raycaster.enabled = true;
            inAppDialogue.SetActive(true);
            inAppProgress.SetActive(false);
            inAppUpdate.SetActive(true);
        }

        if(latestPost != OptionHandler.currentPost)
        {
            if(!GDPRHandler.isActive && OptionHandler.pr0 && !inAppUpdate.activeSelf)
            {
                PlayerPrefs.SetString("Post", latestPost);
                postParent.gameObject.SetActive(true);
                raycaster.enabled = true;
            }
        }

        if(version.Contains("."))
        {
            float newVersion = 
                float.Parse(version, System.Globalization.CultureInfo.InvariantCulture);
            float oldVersion = 
                float.Parse(Application.version, System.Globalization.CultureInfo.InvariantCulture);

            if (newVersion > oldVersion)
            {
#if UNITY_EDITOR
                Debug.Log(newVersion + " " + oldVersion);
#endif

                if (!GDPRHandler.isActive && !postParent.gameObject.activeSelf)
                {
                    updateParent.gameObject.SetActive(true);
                    raycaster.enabled = true;
                }
            } else
            { //other changelog check
                if(otherChangelog)
                {
                    if(!GDPRHandler.isActive)
                    {
                        raycaster.enabled = true;
                        otherChangelogParent.gameObject.SetActive(true);
                    }
                }
            }
        }

        string[] rawData = rawTypes[0].Split('#');

        for (int i = 0; i < rawData.Length - 1; i++)
        {
            string[] scoreData = rawData[i].Split(',');

            string topIdentifier = scoreData[2];
            string sideIdentifier = scoreData[3];
            string bottomIdentifier = scoreData[4];

            //Debug.Log(scoreData[1] + " " + topIdentifier);

            ScoreHolder nH = new ScoreHolder
            {
                score = ulong.Parse(scoreData[0]),
                username = scoreData[1],
                gTop = ShopHandler.Instance.GetGraveTop(topIdentifier),
                gSide = ShopHandler.Instance.GetGraveSide(sideIdentifier),
                gBottom = ShopHandler.Instance.GetGraveBottom(bottomIdentifier)
            };

            int count = 0;
            for (int a = 0; a < this.scoreData.Count; a++)
            { //duplikat-scores zählen so dass max 2 tote spieler pro score angezeigt werden
                if (this.scoreData[a].score == nH.score)
                { //min 123f nach pipe, max 366 nach pipe
                    count++;
                }
            }

            if (count < 2)
            {

                float range = Random.Range(0, 121.5f);

                if (count == 1)
                {
                    range = Random.Range(121.5f, 243);
                }

                nH.offset = 123f + range;
                this.scoreData.Add(nH);
            }
        }

        return true;
    }

    public IEnumerator HandleBundleUpdate(int bundleCode)
    {
        string link = "https://bruh.games/assetbundles/android/mainbundle";

#if UNITY_IOS
        link = "https://bruh.games/assetbundles/ios/mainbundle";
#endif

        Debug.Log("Begin Update");

        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(link, (uint)bundleCode, 0);

        www.SendWebRequest();
        while (!www.isDone)
        {
            float downloaded = (float)www.downloadedBytes;

            float p = downloaded / bundleSize;

            int result = (int)(100 * p);

            progressText.text = result.ToString() + "%";
            progressSlider.value = p;

            yield return null;
        }

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);

            progressText.text = "Fehler!";

            progressSlider.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.red;
            inAppProgressButton.SetActive(false);
            inAppError.SetActive(true);
        }
        else
        {
            Debug.Log("Download Complete - Begin Load");

            progressText.text = "Installiere...";

            PlayerPrefs.SetInt("DownloadedBundle", bundleCode);

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

            progressText.text = "Installation erfolgreich!";

            inAppError.SetActive(false);
            inAppProgressButton.SetActive(true);
        }
    }

    public IEnumerator OpenScoreboard(float waitTime, ulong score, ulong highscore, ulong taps, ulong perfectHits, ulong roundCoins, bool showScore = true)
    {
        if(showScore)
        {
            GetMax(score);
            GetComponent<Dissolver>().ResetDissolve();
            continueEffects.SetActive(true);

            lvlHandler.ResetEffects(xpParent);
            lvlHandler.SetXPText(xpParent);

            achHandler.ResetScoreAchievementDisplay();
            achHandler.UpdateScoreAchievementDisplay();

            scoreObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "0";
            perfectHitObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "0";
            highscoreObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "0";
            coinObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "0";
            tapObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "0";

            goAgainButton.GetComponent<Button>().interactable = false;
            menuButton.GetComponent<Button>().interactable = false;

            ScaleUp();
        }

        yield return new WaitForSeconds(waitTime);
        UpdateScoreboard(score, highscore, taps, perfectHits, roundCoins, showScore);
    }

    public void UpdateScoreboard(ulong score, ulong highscore, ulong taps, ulong perfectHits, ulong roundCoins, bool showScore = true)
    {
        scoreObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "0";
        perfectHitObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "0";
        highscoreObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "0";
        coinObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "0";
        tapObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "0";

        GameObject[] buttons = new GameObject[2];
        buttons[0] = goAgainButton;
        buttons[1] = menuButton;

        lvlHandler.UpdateXP(xpParent, buttons);
        achHandler.StartAnimateScoreAchievementDisplay();

        TextMeshProUGUI scoreText = scoreObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI perfectHitText = perfectHitObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI highscoreText = highscoreObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI coinText = coinObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI tapText = tapObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        ulong tempScore = 0;

        Tween scoreTween = DOTween.To(() => tempScore, x => tempScore = x, score, 1f);
        scoreTween.OnUpdate(() =>
        {
            scoreText.text = tempScore.ToString();
        });

        ulong tempHighscore = 0;

        Tween highscoreTween = DOTween.To(() => tempHighscore, x => tempHighscore = x, highscore, 1f);
        highscoreTween.OnUpdate(() =>
        {
            highscoreText.text = tempHighscore.ToString();
        });

        ulong tempTaps = 0;

        Tween tapTween = DOTween.To(() => tempTaps, x => tempTaps = x, taps, 1f);
        tapTween.OnUpdate(() =>
        {
            tapText.text = tempTaps.ToString();
        });

        ulong tempPerfectHit = 0;

        Tween perfectHitTween = DOTween.To(() => tempPerfectHit, x => tempPerfectHit = x, perfectHits, 1f);
        perfectHitTween.OnUpdate(() =>
        {
            perfectHitText.text = tempPerfectHit.ToString();
        });

        ulong tempRoundCoins = 0;

        Tween roundCoinTween = DOTween.To(() => tempRoundCoins, x => tempRoundCoins = x, roundCoins, 1f);
        roundCoinTween.OnUpdate(() =>
        {
            coinText.text = tempRoundCoins.ToString();
        });

        int max = GetMax(score, true);

        if (max >= 0)
        {
            GetComponent<Dissolver>().StartDissolve(Color.blue);
        }
    }

    private IEnumerator StartParticle(float delay, int max)
    {
        yield return new WaitForSeconds(delay);
        medalObjs[max].transform.GetChild(0).gameObject.SetActive(true);
    }

    private int GetMax(ulong score, bool particles = false)
    {
        GameModes mode = (GameModes)(int)ModeManager.currentIndex;

        int max = -1;

        switch (mode)
        {
#region classic & royale
            case GameModes.Classic:
            case GameModes.Royale:
                if (score >= 1000)
                {
                    max = 6;
                }
                else if (score >= 500)
                {
                    max = 5;
                }
                else if (score > 200)
                {
                    max = 4;
                }
                else if (score > 100)
                {
                    max = 3;
                }
                else if (score > 50)
                {
                    max = 2;
                }
                else if (score > 0) //15
                {
                    max = 1;
                }
                break;
#endregion
#region hardcore
            case GameModes.Hardcore:
                if (score >= 800)
                {
                    max = 6;
                }
                else if (score >= 400)
                {
                    max = 5;
                }
                else if (score > 125)
                {
                    max = 4;
                }
                else if (score > 75)
                {
                    max = 3;
                }
                else if (score > 30)
                {
                    max = 2;
                }
                else if (score > 10)
                {
                    max = 1;
                }
                break;
#endregion
#region mining
            case GameModes.Mining:
                if (score >= 4000)
                {
                    max = 6;
                }
                else if (score >= 1500)
                {
                    max = 5;
                }
                else if (score > 800)
                {
                    max = 4;
                }
                else if (score > 400)
                {
                    max = 3;
                }
                else if (score > 150)
                {
                    max = 2;
                }
                else if (score > 50)
                {
                    max = 1;
                }
                break;
#endregion
#region destruction
            case GameModes.Destruction:
                if (score >= 1300)
                {
                    max = 6;
                }
                else if (score >= 650)
                {
                    max = 5;
                }
                else if (score > 300)
                {
                    max = 4;
                }
                else if (score > 175)
                {
                    max = 3;
                }
                else if (score > 60)
                {
                    max = 2;
                }
                else if (score > 25)
                {
                    max = 1;
                }
                break;
#endregion
        }

        max--;

        for (int i = 0; i < 6; i++)
        {
            if(particles)
            {
                if (max == i)
                { //partikel aktivieren
                    StartCoroutine(StartParticle(1f, max));
                }
                else
                {
                    medalObjs[i].transform.GetChild(0).gameObject.SetActive(false);
                }
            } else
            {
                medalObjs[i].transform.GetChild(0).gameObject.SetActive(false);
            }

            if (max >= i)
            {
                medalObjs[i].transform.GetChild(1).GetComponent<Image>().material = medalMat;
                medalObjs[i].transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                medalObjs[i].transform.GetChild(1).GetComponent<Image>().material = null;
                medalObjs[i].transform.GetChild(2).gameObject.SetActive(true);
            }
        }

        return max;
    }

    public void OkayClicked()
    { //score verstecken & menü anzeigen
        continueEffects.SetActive(false);
        ScaleDown();

        //Reset player obj pos
        FF_PlayerData.Instance.StartGoDead();

        FlatterFogelHandler.Instance.ResetAllObjs(0.49f, true);
        FlatterFogelHandler.Instance.SpawnMainGravestone();

        Invoke(nameof(CallMenuDeath), moveTime + 0.01f);
        Invoke(nameof(Restore), 0.54f);
    }

    private void Restore()
    {
        FlatterFogelHandler.Instance.pipeParent.GetComponent<Dissolver>().ResetDissolve();
        FlatterFogelHandler.Instance.groundHandler.ResetDissolve(FlatterFogelHandler.gameState);
    }

    private void CallMenuDeath()
    {
        MenuData.Instance.DeathFF(true);
    }

    private void CallMenuGo()
    {
        MenuData.Instance.StartFF(true);
    }

    public void GoAgainClicked()
    {
        ScaleDown();

        Invoke(nameof(CallMenuGo), moveTime + 0.01f);
    }

    private void ScaleDown()
    {
        transform.DOScale(0, moveTime);
        eventSystem.SetActive(false);

        Invoke("DisableAll", moveTime + 0.01f);
        Invoke("ReactivateEventSystemFull", moveTime + 0.01f);
    }

    private void ScaleUp()
    {
        transform.localScale = new Vector3(0, 0, 0);
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        transform.DOScale(1, moveTime);

        eventSystem.SetActive(false);
        Invoke("ReactivateEventSystemFull", moveTime + 0.01f);
    }

    private void DisableAll()
    {
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        transform.localScale = new Vector3(1, 1, 1);
    }

    private void HandleItemAnimation()
    {
        Skin pSkin;
        Hat cHat;

        PlayerHolder pH = null;

        List<Skin> doneSkins = new List<Skin>();
        List<Hat> doneHats = new List<Hat>();

        for (int i = 0; i < playersParent.childCount; i++)
        {
            pH = playersParent.GetChild(i).GetComponent<PlayerHolder>();

            pSkin = pH.skin;
            cHat = pH.hat;

            bool ok = false;

            if (!doneSkins.Contains(pSkin))
            {
                doneSkins.Add(pSkin);
                ok = true;
            }

            if (pSkin.animated)
            {   
                if(ok)
                {
                    pSkin.shopTime += Time.deltaTime;

                    if (pSkin.shopTime >= pSkin.animationSpeed)
                    { //sprite update
                        pSkin.shopTime = 0;

                        pSkin.shopStep++;
                        if (pSkin.shopStep >= pSkin.animatedSprites.Length)
                        {
                            pSkin.shopStep = 0;
                        }
                    }
                }

                pH.skinImage.sprite = pSkin.animatedSprites[pSkin.shopStep];
            }

            ok = false;

            if (!doneHats.Contains(cHat))
            {
                doneHats.Add(cHat);
                ok = true;
            }

            if (cHat.animated)
            {
                if(ok)
                {
                    cHat.shopTime += Time.deltaTime;

                    if (cHat.shopTime >= cHat.animationSpeed)
                    { //sprite update
                        cHat.shopTime = 0;

                        cHat.shopStep++;
                        if (cHat.shopStep >= cHat.animatedSprites.Length)
                        {
                            cHat.shopStep = 0;
                        }

                        if (i == shop.GetSelectedHat())
                        {
                            FF_PlayerData.Instance.OverrideHatSprite(cHat.animatedSprites[cHat.shopStep]);
                        }
                    }
                }

                pH.hatImage.sprite = cHat.animatedSprites[cHat.shopStep];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(highscoreActive)
        {
            if(Input.GetKeyDown(KeyCode.Escape) && !closing)
            {
                CloseHighscores();
            }

            HandleItemAnimation();
        }
    }
}
