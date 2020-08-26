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
using System.Net.Configuration;
#if UNITY_ANDROID
using UnityEngine.SocialPlatforms;
using Firebase.Analytics;
#endif
using Random = UnityEngine.Random;

public class ScoreHandler : MonoBehaviour
{
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
    public TextMeshProUGUI progressText, dialogueSize;
    public Slider progressSlider;

    public Color[] prestigeColors;
    public Transform nameParent, scoreParent, postParent, levelParent, highscoreDataParent, updateParent,
        positionParent, playersParent, pipeParent;

    public GameObject playerObjPrefab, loadingObj;
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

    public LocalizedString perfectHit, loading;
    public string perfectHitString, loadingString;

    public static float moveTime = 0.25f;
    public static ObscuredInt personalCoins = 0;
    public static ScoreHandler Instance;

    private float mainTimer = 0;
    private Coroutine timerRoutine = null;

    private Tween moveTween = null;
    private Coroutine highscoreDisplay = null;
    private int diffClicked = -1, modeSelected = 1, newBundleCode = 0, bundleSize = 18000000, highscorePageIndex = 0, highscoreMaxPage = 1;
    private bool registerRunning = false, fetchRunning = false, closing = false, highscoreActive = false,
        opening = false, dataFetched = false, highscoreDisplayRunning = false, moveRunning = false;
    private Vector3 originalPipeParentPos, originalPlayerParentPos;
    private string[] hsDataString;

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
        hsDataString = new string[3];
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
            if (pipeParent.GetChild(a).CompareTag("SmallPipe"))
            {
                pipeParent.GetChild(a).gameObject.SetActive(false);

                smallPipes.Add(pipeParent.GetChild(a));
            }
        }

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

        opening = true;
        Invoke(nameof(ReactivateEventSystem), moveTime + 0.01f);

        FF_PlayerData.Instance.inHighscores = true;

        playersParent.SetParent(hParent.transform.parent);
        playersParent.SetAsLastSibling();

        if(accountHandler.accountState == AccountStates.LoggedOut)
        {
            bool overrideAuth = true; //DEBUG da test
#if UNITY_EDITOR
            overrideAuth = true;
#endif
            inputParent.SetActive(true);

            accountHandler.ResetMenu();

            highscoreList.SetActive(false);

            bool ok = false;

            if (Social.localUser.authenticated || overrideAuth)
            { //registrierung mit test daten
#if UNITY_ANDROID || UNITY_IOS
                AccountHandler.Instance.StartGplayRegister();
#endif          
                ok = true;
            }

            accountHandler.ResetMenu(ok);
        } else
        {
            ForceHighscoreStart();
        }
    }

    public void ForceHighscoreStart()
    {
        inputParent.SetActive(false);
        highscoreList.SetActive(true);

        HandleColor(0, modeSelected);
        HandleColor(1, OptionHandler.GetDifficulty());

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
        if(registerRunning || fetchRunning || AccountHandler.running || moveRunning)
        {
            return;
        }

        closing = true;

        eventSystem.SetActive(false);

        loadingObj.SetActive(false);

        playersParent.SetParent(hParent.transform.parent);
        playersParent.SetAsLastSibling();

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

        highscoreDisplay = StartCoroutine(FetchHighscoreData(RealModeToArrayID(mode)));
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

        string hsString = "";
        for (int i = 0; i < 5; i++)
        {
            hsString += "&highscore" + (i + 1).ToString() + "=" + ffHandler.GetHighscore(i, diff).ToString();
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

        string link = "https://bruh.games/manager.php?setscore=1&lastscore=" +
                        lastS.ToString() + hsString + "&name=" + username + "&hs=1&diff=" +
                        diff.ToString() + "&mode=-1" +
                        "&lvl=" + currentLvl.ToString() + "&pr=" + currentPrestige.ToString() +
                        "&setgrave=1&gtop=" + gTop + "&gside=" + gSide + "&gbottom=" + gBottom +
                        "&setskin=1&skin=" + skinString + "&wing=" + wingString + "&hat=" + hatString +
                        "&pipe=" + pipeString + "&pipecolor=" + pipeColorID +
                        "&hash=" + authHash;

        loadingObj.SetActive(true);
        loadingObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
        loadingObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            loadingString;

        UnityWebRequest www = UnityWebRequest.Get(link);
        yield return www.SendWebRequest();

        StopCoroutine(timerRoutine);
        float remaining = 0.4f - mainTimer;

        if(remaining > 0.01)
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
                //Debug.Log("UString: " + wwwData);
            }

            string[] rawData = wwwData.Split('|'); //split nach modi

            if(rawData.Length > 2)
            {
                hsDataString[0] = rawData[0]; //classic
                hsDataString[1] = rawData[1]; //destruction
                hsDataString[2] = rawData[2]; //mining

                int arrayID = RealModeToArrayID(modeSelected);

                highscoreDisplay = StartCoroutine(FetchHighscoreData(arrayID));

                dataFetched = true;
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

    private IEnumerator FetchHighscoreData(int arrayID, bool wait = false)
    { //fetcht die bereits erhaltenen daten und zeigt sie an

        if(wait)
        {
            yield return new WaitForSeconds(0.4f);
        }

        highscoreDisplayRunning = true;
        swDetector.enabled = false;

        string username = AccountHandler.Instance.username;

        ShopHandler shop = ShopHandler.Instance;

        nameParent.GetChild(0).gameObject.SetActive(true);
        scoreParent.GetChild(0).gameObject.SetActive(true);
        levelParent.GetChild(0).gameObject.SetActive(true);
        positionParent.GetChild(0).gameObject.SetActive(true);

        string[] rawData = hsDataString[arrayID].Split('#');

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
            }
        }

        for (int a = 0; a < smallPipes.Count; a++)
        {
            smallPipes[a].SetParent(objPooler.transform);
        }

        FlatterFogelHandler.Instance.DisableOtherObjs();

        playerObj.SetActive(false);
        levelTextObj.SetActive(false);

        bool userInHS = false;

        ulong ceiling = 1000;
        float lastP = 1;

        int temp = 0;

        int playerCount = rawData.Length - 1;

        highscoreMaxPage = playerCount / 6;

        for (int i = 0; i < playerCount; i++)
        {
            string[] scoreData = rawData[i].Split(',');

            int num = i + 1;

            GameObject newPlayer = Instantiate(playerObjPrefab, playersParent);
            Vector3 pos = new Vector3(-655 + (100 * i), 264, 20f);

            ulong score = ulong.Parse(scoreData[0]);

            //1015 max y
            //320 min y
            //diff 695
            if (i == 0)
            {
                ceiling = score;
            }

            float p = (score / (float)ceiling);

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

            Skin playerSkin = (Skin)shop.GetItemByString(CustomizationType.Skin, scoreData[4]);
            Wing playerWing = (Wing)shop.GetItemByString(CustomizationType.Wing, scoreData[5]);
            Hat playerHat = (Hat)shop.GetItemByString(CustomizationType.Hat, scoreData[6]);
            Pipe playerPipe = (Pipe)shop.GetItemByString(CustomizationType.Pipe, scoreData[7]);
            Color playerPipeColor = pipeCustomizationHandler.GetPipeColor(Int32.Parse(scoreData[8]));

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
                playerPipeColor, scoreData[1], scoreData[0], isTop);

            newPlayer.SetActive(true);

            yield return new WaitForSeconds(0.075f);
        }

        if (!userInHS)
        { //name nicht in top 15 -> unten anzeigen
            nameParent.GetChild(nameParent.childCount - 1).GetComponent<TextMeshProUGUI>().text =
                username;
            nameParent.GetChild(nameParent.childCount - 1).GetComponent<TextMeshProUGUI>().color =
                Color.red;

            ulong score = ffHandler.GetHighscore(modeSelected - 1, 1);

            scoreParent.GetChild(nameParent.childCount - 1).GetComponent<TextMeshProUGUI>().text =
                score.ToString();
            scoreParent.GetChild(nameParent.childCount - 1).GetComponent<TextMeshProUGUI>().color =
                Color.red;

            long lvl = lvlHandler.GetLVL();
            int prestige = lvlHandler.GetPrestige();

            levelParent.GetChild(levelParent.childCount - 1).GetComponent<Image>().color =
                prestigeColors[prestige];
            levelParent.GetChild(levelParent.childCount - 1).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                lvl.ToString();
        }

        nameParent.GetChild(nameParent.childCount - 1).gameObject.SetActive(false);
        scoreParent.GetChild(nameParent.childCount - 1).gameObject.SetActive(false);
        levelParent.GetChild(levelParent.childCount - 1).gameObject.SetActive(false);
        positionParent.GetChild(levelParent.childCount - 1).gameObject.SetActive(false);

        highscoreDisplayRunning = false;
        swDetector.enabled = true;
    }

    public void SwipeDetector_OnSwipe(SwipeData data)
    {
        switch (data.Direction)
        {
            case SwipeDirection.Left:
                DirClicked(0);
                break;
            case SwipeDirection.Right:
                DirClicked(1);
                break;
        }
    }

    public void DirClicked(int dir)
    {
        if (fetchRunning || !dataFetched || highscoreDisplayRunning || moveRunning)
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

            eventSystem.SetActive(false);

            moveRunning = true;

            pipeParent.DOMoveX(pipeParent.transform.position.x + amount, 0.29f);
            playersParent.DOMoveX(playersParent.transform.position.x + amount, 0.29f);

            Invoke(nameof(ReactivateEventSystem), 0.3f);
        }
    }

    IEnumerator GetStatus()
    {
        ulong hs = ffHandler.GetHighscore();
        ulong lastS = ffHandler.GetLastScore();

        int diff = OptionHandler.GetDifficulty();

        string username = accountHandler.username;

        string hsString = "";
        for(int i = 0; i < 5; i++)
        {
            hsString += "&highscore" + (i + 1).ToString() + "=" + ffHandler.GetHighscore(i).ToString();
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

        string link = "https://bruh.games/manager.php?setscore=1&lastscore=" +
                        lastS.ToString() + hsString + "&name=" + username + "&last50=1&diff=" + diff.ToString() +
                        "&v=" + Application.version + "&lvl=" + currentLvl.ToString() + "&pr=" + currentPrestige.ToString() +
                        "&setgrave=1&gtop=" + gTop + "&gside=" + gSide + "&gbottom=" + gBottom +
                        "&setskin=1&skin=" + skinString + "&wing=" + wingString + "&hat=" + hatString +
                        "&pipe=" + pipeString + "&pipecolor=" + pipeColorID +
                        "&hash=" + authHash;

        if (accountHandler.accountState == AccountStates.LoggedOut ||
            accountHandler.accountState != AccountStates.Synced)
        { //username nicht gesetzt bzw nicht gesynced-> abfrage ohne score set
            string tempName = AccountHandler.tempNames[Random.Range(0, 4)];
            authHash = AccountHandler.Md5Sum(tempName + Auth.authKey);

            link = "https://bruh.games/manager.php?last50=1&diff=" + diff.ToString() +
                        "&v=" + Application.version + "&name=" + tempName + "&hash=" + authHash;
        }

        string os = "0";

#if UNITY_IOS
        os = "1";
#endif

        link += "&os=" + os;

        UnityWebRequest www = UnityWebRequest.Get(link);
        yield return www.SendWebRequest(); //wartet das abfrage fertig ist

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            ObscuredPrefs.SetInt("CoinTmp", 0);

            FetchString(www.downloadHandler.text);
        }
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
                Debug.Log(newVersion + " " + oldVersion);

                if (!GDPRHandler.isActive && !postParent.gameObject.activeSelf)
                {
                    updateParent.gameObject.SetActive(true);
                    raycaster.enabled = true;
                }
            }
        }

        string[] rawData = rawTypes[0].Split('#');

        Debug.Log(wwwData);

        for (int i = 0; i < rawData.Length - 1; i++)
        {
            string[] scoreData = rawData[i].Split(',');

            string topIdentifier = scoreData[2];
            string sideIdentifier = scoreData[3];
            string bottomIdentifier = scoreData[4];

            Debug.Log(scoreData[1] + " " + topIdentifier);

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

        Invoke("CallMenuDeath", moveTime + 0.01f);
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

        Invoke("CallMenuGo", moveTime + 0.01f);
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
