﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using CodeStage.AntiCheat.Storage;
using UnityEngine.Networking;
#if UNITY_ANDROID
using UnityEngine.SocialPlatforms;
#endif
using Firebase.Analytics;
using DG.Tweening;

public class OptionHandler : MonoBehaviour
{
    public IngameMenuHandler ingameMenu;
    public DiscordHandler discord;
    public Canvas menuCanvas, windowCanvas;
    public GameObject mainCamera;
    public GameObject introParent, introHold;
    public GameObject aspectRatioForcer;
    public GameObject versionDisplay;
    public GameObject startVersionDisplay;
    public GameObject eventSystem;
    public CanvasScaler uiCanvasScaler;
    public GameObject optionObj;
    public Transform topUiParent, bottomUiParent;
    public Vector3 startPos, defaultPos;

    [Header("Kategorien")]
    public GameObject allgObj;
    public GameObject graphicsObj;
    public GameObject soundObj;
    public GameObject accountObj;
    public GameObject dataPrivacy;
    public GameObject credits;
    [Header("Allgemein")]
    public GameObject languageButton;
    public GameObject difficultyButton;
    public GameObject introButton;
    public GameObject kreuzPosButton;
    public GameObject kreuzSizeButton;
    public GameObject mineModeButton;
    [Header("Grafik")]
    public int[] allResolutions;
    public GameObject physicButton;
    public GameObject lightButton;
    public GameObject renderButton;
    public GameObject backgroundButton;
    public GameObject destructionButton;
    public GameObject framerateButton;
    public GameObject vsyncButton;
    public GameObject energySaveButton;
    public GameObject extendParent;
    public GameObject normalMapButton;
    [Header("Sound")]
    public GameObject jumpEffectButton;
    public Slider volumeSlider, effevtVolumeSlider;
    [Header("Account")]
    public TextMeshProUGUI usernameText, infoText;
    public GameObject logOutButton;
    public GameObject requestDataButton, requestDataFullButton;
    public GameObject mainAccountPage, requestAccountPage;
    public TMP_InputField emailInput, passwordInput;

    //Grafik
    public static int physicsResolution = 0, screenResolution = 1, lightEnabled = 1,
        parallaxMode = 0, enhancedPipeDestruction = 1, cameraShake = 1, particleEffects = 1,
        enhancedFramerate = 0, stretchImage = 1, firstLaunch = 1, vSyncEnabled = 1, energySaveMode = 0,
        normalMaps = 1;

    public Light2D[] allLights;

    //Sound
    public static int jumpEffectMode = 0;

    //Allgemein
    public static string currentPost = "4141210";
    public static OptionHandler Instance;
    public static int kreuzPos = 0, kreuzSize = 0, mineMode = 2, noPush = 0;
    public static bool hardcoreActive = false, normalAspect = false,
        destructionEnlargeActive = false, destructionTransition = false;

    public static bool playStore = false, pr0 = true;

    public static Vector3 defaultCameraPos = new Vector3(-381, 790, -400);
    public static float defaultOrthoSize = 640, moveTime = 0.25f;
    public static Bounds cameraBounds;

    private float defAspect = 0;
    private static int difficulty = 1;
    private bool closing = false, optionsActive = false, ingame = false, dataRequestRunning = false,
        languageLoaded = false, loadingFinished = false;

    private List<Locale> allLocales = new List<Locale>();
    private int selectedLocaleIndex = -1;

    private string usernameString, emailSentString, emailNotSentString;

    public GameObject languageItemPrefab, languageParent, languageOkButton, gdprObj;

    public LocalizedString mineHalten, mineGesten, mineKreuz;
    public LocalizedString mineItemLeft, mineItemRight;
    public LocalizedString normal, gross, hoch;
    public LocalizedString yes, no, on, off, language;
    public LocalizedString tosLink, privacyLink;
    public LocalizedString noNameSelected, emailSent, emailNotSent;

    [Header("Handler")]
    [SerializeField]
    FlatterFogelHandler ffHandler = null;
    [SerializeField]
    ShopHandler shopHandler = null;
    [SerializeField]
    BackgroundHandler backgroundHandler = null;
    [SerializeField]
    FF_PlayerData ffPlayerData = null;
    [SerializeField]
    MineHandler mineHandler = null;

    private void Awake()
    {
        Instance = this;
        firstLaunch = ObscuredPrefs.GetInt("FirstLaunch", 1);

        currentPost = PlayerPrefs.GetString("Post", currentPost);

        int noPush = PlayerPrefs.GetInt("Player_NoPush", 0);

        if(noPush == 0)
        { //will benachrichtigungen
            FirebaseHandler.SetUserProperty("WantsMessages", "1");
        } else
        {
            FirebaseHandler.SetUserProperty("WantsMessages", "0");
        }

        if(pr0)
        {
            FirebaseHandler.SetUserProperty("ProVersion", "1");
        } else
        {
            FirebaseHandler.SetUserProperty("ProVersion", "0");
        }

        if (firstLaunch == 1)
        {
            if(pr0)
            {
                jumpEffectButton.SetActive(true);
            } else
            {
                jumpEffectButton.SetActive(false);
            }
        }

        DOTween.SetTweensCapacity(500, 50);

        discord.Setup();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadLocalization());

        versionDisplay.GetComponent<TextMeshProUGUI>().text = 
            "BETA " + Application.version;
        startVersionDisplay.GetComponent<TextMeshProUGUI>().text =
            "v" + Application.version;

        introParent.SetActive(false);

#if UNITY_EDITOR
        introHold.SetActive(true); //debug
#else
        introHold.SetActive(true);
#endif
    }

    public void KekDetected()
    {

    }

    /*void OnEnable() Deaktiviert da von Crashlytics gehandelt
    {
        Application.logMessageReceived += LogCallback;
    }

    //Called when there is an exception
    void LogCallback(string condition, string stackTrace, LogType type)
    {
        Debug.Log("CALLBACK " + condition + " " + stackTrace + " " + type);
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }*/

    IEnumerator LoadLocalization()
    {
        string selectedLocaleName = PlayerPrefs.GetString("SelectedLocale", "");

        if (selectedLocaleName.Equals(""))
        { //system default
            languageParent.transform.parent.parent.parent.parent.gameObject.SetActive(true);

            languageOkButton.GetComponent<Button>().interactable = false;

            for (int i = 0; i < gdprObj.transform.childCount; i++)
            {
                gdprObj.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        yield return LocalizationSettings.InitializationOperation;

        selectedLocaleIndex = -1;

        if (selectedLocaleName.Equals(""))
        { //system default

            selectedLocaleName = "en";

            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
            {
                Locale locale = LocalizationSettings.AvailableLocales.Locales[i];

                GameObject obj = Instantiate(languageItemPrefab, languageParent.transform);

                obj.GetComponent<LanguageItemHandler>().languageID = locale.name;

                obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    locale.name;
            }

            /*switch(Application.systemLanguage)
            {
                default:
                case SystemLanguage.English:
                    selectedLocaleName = "en";
                    break;
                case SystemLanguage.German:
                    selectedLocaleName = "de";
                    break;
                case SystemLanguage.French:
                    selectedLocaleName = "fr";
                    break;
                case SystemLanguage.Spanish:
                    selectedLocaleName = "es";
                    break;
            }*/
        }

        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
        {
            Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
            allLocales.Add(locale);

            if(locale.name.Contains(selectedLocaleName))
            {
                selectedLocaleIndex = i;
            }
        }

        StartCoroutine(SetLocalization(selectedLocaleIndex, true));
    }

    public void LanguageClicked(string language)
    {
        languageOkButton.GetComponent<Button>().interactable = true;

        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
        {
            Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
            allLocales.Add(locale);

            if (locale.name.Contains(language))
            {
                selectedLocaleIndex = i;
            }
        }
    }

    public void LanguageOkayClicked()
    {
        languageOkButton.GetComponent<Button>().interactable = false;

        languageLoaded = false;

        StartCoroutine(WaitForLangaugeLoad());
    }

    private IEnumerator WaitForLangaugeLoad()
    {
        StartCoroutine(SetLocalization(selectedLocaleIndex, false, false));

        while (!languageLoaded)
        {
            yield return new WaitForSeconds(0.25f);
        }

        languageParent.transform.parent.parent.parent.parent.gameObject.SetActive(false);

        for (int i = 0; i < gdprObj.transform.childCount; i++)
        {
            gdprObj.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private IEnumerator SetLocalization(int index, bool load = false, bool resolution = true)
    {
        selectedLocaleIndex = index;
        string localeName = allLocales[index].name;

        if(!load)
        {
            PlayerPrefs.SetString("SelectedLocale", localeName);
        }

        LocalizationSettings.SelectedLocale = allLocales[index];
        yield return LocalizationSettings.InitializationOperation;

        Debug.Log("LOADED LOCALE: " + localeName);

        languageLoaded = true;

        if(load)
        {
#if UNITY_EDITOR
            //introParent.SetActive(false);
#else
            //introParent.SetActive(true);
#endif

            Debug.Log("Finished Language");
            Debug.Log("Fetching Server");

            //introHold.SetActive(false);
        }
        //intro erst starten wenn laden fertig

        UpdateAll(false, resolution);
        ModeManager.Instance.StartLoadLocalization();
        LevelHandler.Instance.StartLoadLocalization();
        ScoreHandler.Instance.StartLoadLocalization();
        ShopHandler.Instance.StartLoadLocalization();
        MineHandler.Instance.StartLoadLocalozation();
        ThinkHandler.Instance.StartLoadLocalization();
        FF_PlayerData.Instance.StartLoadLocalization();
        AchievementHandler.Instance.StartLoadLocalization(load);

        if(load)
        {
            BackgroundHandler.Instance.SpawnCloud(true);
        }

        loadingFinished = true;

        //AccountHandler.Instance.StartLoadLocalization();
    }

    public void SetIntro()
    {
        StartCoroutine(WaitLoad());
    }

    private IEnumerator WaitLoad()
    {
        while(!loadingFinished)
        {
            yield return new WaitForSeconds(0.05f);
        }

        introHold.SetActive(false);

        MenuZoomIn(false);
#if UNITY_EDITOR
        introParent.SetActive(false);
#else
        introParent.SetActive(true);
#endif
    }

    public static int GetDifficulty()
    {
        /*if(hardcoreActive)
        {
            return 2;
        }*/

        return difficulty;
    }

    public void LoadOptions()
    {
        physicsResolution = PlayerPrefs.GetInt("Player_PhysicsResolution", 0);
        screenResolution = PlayerPrefs.GetInt("Player_Resolution", 2);
        lightEnabled = PlayerPrefs.GetInt("Player_Light", 1);
        parallaxMode = 1;//PlayerPrefs.GetInt("Player_ParallaxMode", 1);
        enhancedPipeDestruction = PlayerPrefs.GetInt("Player_EnhancedDestruction", 1);

        jumpEffectMode = PlayerPrefs.GetInt("Player_JumpEffectMode", 0);

        difficulty = 1;//PlayerPrefs.GetInt("Player_Difficulty", 1);
        noPush = PlayerPrefs.GetInt("Player_NoPush", 0);
        kreuzPos = PlayerPrefs.GetInt("Player_KreuzPos", 0);
        kreuzSize = PlayerPrefs.GetInt("Player_KreuzSize", 0);
        mineMode = PlayerPrefs.GetInt("Player_MineMode", 2);

        enhancedFramerate = PlayerPrefs.GetInt("Player_EnhancedFramerate", 1);
        vSyncEnabled = 1;//PlayerPrefs.GetInt("Player_VSync", 1);
        energySaveMode = PlayerPrefs.GetInt("Player_EnergySave", 0);
        normalMaps = 1;// PlayerPrefs.GetInt("Player_NormalMap", 1);
    }

    public void UpdateAll(bool excludeEnergy = false, bool resolution = true)
    {
        LoadOptions();
        StartCoroutine(UpdateLanguageText());
        StartCoroutine(UpdateLight());

        if(resolution)
        {
            UpdateResolution();
        }

        if (energySaveMode == 0)
        {
            StartCoroutine(UpdatePhysics());
            StartCoroutine(UpdateParallax());
            StartCoroutine(UpdateDestruction());
            StartCoroutine(UpdateEnhancedFramerate());
        }

        UpdateJumpEffect();
        StartCoroutine(UpdatePush());
        StartCoroutine(UpdateKreuzPos());
        StartCoroutine(UpdateKreuzSize());
        UpdateDifficulty();
        UpdateVSync();
        StartCoroutine(UpdateMineMode());
        UpdateMusicVolume();
        UpdateEffectVolume();

        if(!excludeEnergy)
        {
            StartCoroutine(UpdateEnergySaveMode(true));
        }
    }

    public void ShowOptions(bool ingame = false)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }

        this.ingame = ingame;

        if(!ingame)
        {
            windowCanvas.sortingOrder = 11;

            ModeManager.Instance.BackClicked();
            //MenuData.Instance.DoScaleDown();

            StartCoroutine(MenuData.Instance.DoMoveAway());
        } else
        {
            windowCanvas.sortingOrder = 14;
        }

        UpdateCategory(0);

        eventSystem.SetActive(false);

        defaultPos = new Vector3(-381, 791, -400);//mainCamera.transform.position;
        defaultPos.y += 220;
        defaultPos.z = 0;

        startPos = defaultPos;
        startPos.y += 1990;

        optionObj.transform.position = startPos;
        //optionObj.transform.localScale = new Vector3(0, 0, 0);

        optionObj.transform.DOMove(defaultPos, moveTime, true).SetEase(Ease.OutBack).
            SetUpdate(UpdateType.Normal, ingame);
        //optionObj.transform.DOScale(new Vector3(1, 1, 1), moveTime);

        StartCoroutine(ReactivateEventSystem(moveTime + 0.01f));
    }

    public void CloseOptions()
    {
        if (dataRequestRunning) return;

        SoundManager.Instance.PlaySound(Sound.MenuError);

        closing = true;

        eventSystem.SetActive(false);

        optionObj.transform.DOMove(startPos, moveTime, true).SetEase(Ease.InBack).
            SetUpdate(UpdateType.Normal, ingame);
        //optionObj.transform.DOScale(new Vector3(0, 0, 0), moveTime);

        StartCoroutine(ReactivateEventSystem(moveTime + 0.01f));
    }

    private IEnumerator ReactivateEventSystem(float waitTime)
    {
        yield return new WaitForSecondsRealtime(waitTime);

        eventSystem.SetActive(true);

        optionsActive = true;

        if(closing)
        {
            optionsActive = false;

            closing = false;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            windowCanvas.sortingOrder = 10;

            if (!ingame)
            {
                //MenuData.Instance.DoScaleUp();
                StartCoroutine(MenuData.Instance.DoMoveIn());
            } else
            {
                ingameMenu.OptionClose();
                ingameMenu.OpenMenu(true, false, false, true);
            }
        }
    }

    public void SetButtonText(Transform button, string text)
    {
        TextMeshProUGUI textObj = button.GetComponent<TextMeshProUGUI>();

        if(text.Length > 7)
        {
            textObj.fontSize = 28;
        } else
        {
            textObj.fontSize = 36;
        }

        textObj.text = text;
    }

    public void LanguageClicked()
    {
        selectedLocaleIndex++;

        if(selectedLocaleIndex >= allLocales.Count)
        {
            selectedLocaleIndex = 0;
        }

        StartCoroutine(SetLocalization(selectedLocaleIndex));
    }

    private IEnumerator UpdateLanguageText()
    {
        AsyncOperationHandle handle;

        yield return handle = language.GetLocalizedString();

        SetButtonText(languageButton.transform.GetChild(0).GetChild(0), (string)handle.Result);

        yield return handle = noNameSelected.GetLocalizedString();

        usernameString = (string)handle.Result;

        yield return handle = emailSent.GetLocalizedString();

        emailSentString = (string)handle.Result;

        yield return handle = emailNotSent.GetLocalizedString();

        emailNotSentString = (string)handle.Result;
    }

    public void UpdateUsernameString(bool loggedIn)
    {
        string s = usernameString;
        Color c = Color.red;

        if(loggedIn)
        {
            s = AccountHandler.Instance.username;
            c = Color.black;
        }

        usernameText.text = s;
        usernameText.color = c;
    }

    public void OpenDataPrivacy()
    {
        StartCoroutine(OpenPrivacyLink());
    }

    public void OpenTOS()
    {
        StartCoroutine(OpenTOSLink());
    }

    private IEnumerator OpenPrivacyLink()
    {
        AsyncOperationHandle handle;
        yield return handle = privacyLink.GetLocalizedString();

        Application.OpenURL((string)handle.Result);
    }

    private IEnumerator OpenTOSLink()
    {
        AsyncOperationHandle handle;
        yield return handle = tosLink.GetLocalizedString();

        Application.OpenURL((string)handle.Result);
    }

    public void OpenPrivacySettings()
    {
        allgObj.SetActive(false);
        graphicsObj.SetActive(false);
        soundObj.SetActive(false);
        accountObj.SetActive(false);
        credits.SetActive(false);

        dataPrivacy.SetActive(true);
    }

    public void OpenCredits()
    {
        allgObj.SetActive(false);
        graphicsObj.SetActive(false);
        soundObj.SetActive(false);
        accountObj.SetActive(false);
        dataPrivacy.SetActive(false);

        credits.SetActive(true);
    }

    #region EnergySaveMode
    public void EnergySaveModeClicked()
    {
        if(energySaveMode == 0)
        {
            energySaveMode = 1;
        } else
        {
            energySaveMode = 0;
        }

        PlayerPrefs.SetInt("Player_EnergySave", energySaveMode);
        StartCoroutine(UpdateEnergySaveMode());
    }

    public IEnumerator UpdateEnergySaveMode(bool updateAll = false)
    {
        bool enable = false;

        AsyncOperationHandle handle;

        if(energySaveMode == 1)
        {
            screenResolution = 1; //540p
            enhancedPipeDestruction = 0; //pipe zerstörung deaktivieren
            enhancedFramerate = 0; //30fps
            physicsResolution = 0; //niedrigste physik-resolution
            parallaxMode = 0; //bewegende hintergründe deaktiveren

            UpdateResolution();
            StartCoroutine(UpdateDestruction());
            StartCoroutine(UpdateEnhancedFramerate());
            StartCoroutine(UpdatePhysics());
            StartCoroutine(UpdateParallax());

            BackgroundHandler.Instance.SetNewBackground(
                ShopHandler.Instance.GetCurrentBackground());

            yield return handle = on.GetLocalizedString();
        } else
        {
            enable = true;

            if(!updateAll)
            {
                LoadOptions();
                UpdateAll(true);
            }

            yield return handle = off.GetLocalizedString();
        }

        SetButtonText(energySaveButton.transform.GetChild(0).GetChild(0), (string)handle.Result);

        renderButton.transform.GetChild(0).GetComponent<Button>().interactable = enable;
        destructionButton.transform.GetChild(0).GetComponent<Button>().interactable = enable;
        framerateButton.transform.GetChild(0).GetComponent<Button>().interactable = enable;
        physicButton.transform.GetChild(0).GetComponent<Button>().interactable = enable;
        backgroundButton.transform.GetChild(0).GetComponent<Button>().interactable = enable;
        lightButton.transform.GetChild(0).GetComponent<Button>().interactable = enable;
    }
#endregion

#region Physics
    public void PhysicsClicked()
    {
        if (physicsResolution == 0)
        { //auf hoch setzen
            physicsResolution = 1;
        }
        else
        { //auf normal setzen
            physicsResolution = 0;
        }

        PlayerPrefs.SetInt("Player_PhysicsResolution", physicsResolution);

        shopHandler.SavePurchasedItems();
        StartCoroutine(UpdatePhysics());
        //SceneManager.LoadScene(0);
    }

    private IEnumerator UpdatePhysics()
    {
        bool high = false;

        AsyncOperationHandle handle;

        if (physicsResolution == 0)
        {
            yield return handle = normal.GetLocalizedString();
        }
        else
        {
            yield return handle = hoch.GetLocalizedString();

            high = true;
        }

        physicButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
            (string)handle.Result;

        ffHandler.ChangePhysicsResolution(high);
    }
#endregion

#region Resolution
    public void ResolutionClicked()
    {
        screenResolution++;
        if(screenResolution > allResolutions.Length - 1)
        {
            screenResolution = 0;
        }

        PlayerPrefs.SetInt("Player_Resolution", screenResolution);

#if UNITY_ANDROID || UNITY_IOS
        UpdateResolution();
#else
        Screen.SetResolution(720, 1280, false);
#endif
    }

    public void UpdateResolution(int type = 0)
    {
#if UNITY_STANDALONE_WIN
        return;
#endif

        int resolutionID = screenResolution;

        /*if(type == 0)
        { //menü -> im Menü 1080p
            resolutionID = 0;
        }*/

        defAspect = Screen.height / (float)Screen.width;

        bool overrideOrtho = false;

        aspectRatioForcer.SetActive(false);
        if (defAspect < 1.8f)
        {
            if(defAspect < 1.5f)
            {
                aspectRatioForcer.SetActive(true);
            }

            defAspect = 1.7778f;
            overrideOrtho = true;
            normalAspect = true;
        }

        int width = 720;
        string resolutionText = "720p";

        width = allResolutions[resolutionID];
        int height = (int)(width * defAspect);

        Screen.SetResolution(width, height, true);

        if (height < width)
        {
            resolutionText = height.ToString() + "p";
        }
        else if (width < height)
        {
            resolutionText = width.ToString() + "p";
        }

        //if(type > 0)
        //{
            renderButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                resolutionText;
        //}

        uiCanvasScaler.scaleFactor = width / 720f;

        Camera c = mainCamera.GetComponent<Camera>();

        c.orthographicSize = 720 * Screen.height / Screen.width * 0.5f;

        float cameraHeight = c.orthographicSize * 2;
        cameraBounds = new Bounds(
            c.transform.position,
            new Vector3(cameraHeight * defAspect, cameraHeight, 0));

        //border unten 150

        Vector3 pos = c.transform.position;
        pos.y = 150 + cameraBounds.extents.y;

        c.transform.position = pos;

        defaultCameraPos = pos;
        defaultOrthoSize = c.orthographicSize;

        if(overrideOrtho)
        {
            defaultCameraPos = new Vector3(-381, 790, -400);
            defaultOrthoSize = 640;

            c.transform.position = defaultCameraPos;
            c.orthographicSize = defaultOrthoSize;

            cameraBounds = new Bounds(defaultCameraPos, new Vector3(1137.8f, 640f));
        }

        ShopHandler.Instance.UIScaleFinished();

        //Obere Grenze
        float topY = 1434;
        float bottomY = 150;

        if (!normalAspect)
        {
            topY = defaultCameraPos.y + cameraBounds.extents.y;
        }

        float center = (topY + bottomY) / 2;
        pos.y = center;

        backgroundHandler.UpdateTopExtent(pos, cameraHeight);
        //DestructionEnlarge();
    }

    public void MenuZoomIn(bool tween = true)
    {
        Camera c = mainCamera.GetComponent<Camera>();

        float newOrtho = defaultOrthoSize * 0.75f; //500 * Screen.height / Screen.width * 0.5f; //720 - 200 = 520

        if(tween)
        {
            DOTween.To(() => c.orthographicSize, x => c.orthographicSize = x, newOrtho, 0.5f);
        } else
        {
            c.orthographicSize = newOrtho;
        }

        float cameraHeight = newOrtho * 2;

        Bounds cameraBounds = new Bounds(
            c.transform.position,
            new Vector3(cameraHeight * defAspect, cameraHeight, 0));

        if(tween)
        {
            mainCamera.transform.DOMove(new Vector3(-470, 201 + cameraBounds.extents.y, -400), 0.5f);
        } else
        {
            mainCamera.transform.position = new Vector3(-470, 201 + cameraBounds.extents.y, -400);
        }
    }

    public void MenuZoomOut()
    {
        Camera c = mainCamera.GetComponent<Camera>();

        float newOrtho = defaultOrthoSize;

        DOTween.To(() => c.orthographicSize, x => c.orthographicSize = x, newOrtho, 0.5f);

        float cameraHeight = newOrtho * 2;

        Bounds cameraBounds = new Bounds(
            c.transform.position,
            new Vector3(cameraHeight * defAspect, cameraHeight, 0));

        mainCamera.transform.DOMove(new Vector3(-381, defaultCameraPos.y, -400), 0.5f);
    }

    public void MiningZoomIn()
    {
        MineHandler.cameraOK = false;
        FlatterFogelHandler.Instance.DisableCameraShake();

        Camera c = mainCamera.GetComponent<Camera>();

        float newOrtho = 540 * Screen.height / Screen.width * 0.5f; //720 - 180 = 540

        DOTween.To(() => c.orthographicSize, x => c.orthographicSize = x, newOrtho, 0.5f);
        c.transform.position = OptionHandler.defaultCameraPos;

        float cameraHeight = newOrtho * 2;

        Bounds cameraBounds = new Bounds(
            c.transform.position,
            new Vector3(cameraHeight * defAspect, cameraHeight, 0));

        //Vector3 pos = c.transform.position;

        //pos.x = FlatterFogelHandler.Instance.player.transform.position.x;
        //pos.y = FlatterFogelHandler.Instance.player.transform.position.y;

        //c.transform.DOMove(pos, 0.5f);

        Invoke(nameof(ReEnableMineCam), 0.2f);
        Invoke(nameof(ReEnableColl), 0.51f);
    }

    public void DestructionEnlarge()
    {
        //if(destructionEnlargeActive)
        //{
        //    return;
        //}

        FlatterFogelHandler.Instance.DisableCameraShake();
        destructionEnlargeActive = true;

        destructionTransition = true;

        Camera c = mainCamera.GetComponent<Camera>();

        float newOrtho = (720 + 360.125f) * Screen.height / Screen.width * 0.5f;

        DOTween.To(() => c.orthographicSize, x => c.orthographicSize = x, newOrtho, 1);

        float cameraHeight = newOrtho * 2;

        Bounds cameraBounds = new Bounds(
            c.transform.position,
            new Vector3(cameraHeight * defAspect, cameraHeight, 0));

        Vector3 pos = c.transform.position;

        pos.x = defaultCameraPos.x + 360.125f / 2;
        pos.y = 150 + cameraBounds.extents.y;

        c.transform.DOMove(pos, 1f);

        backgroundHandler.EnlargeTopExtent(pos);

        Invoke(nameof(ReEnableColl), 1.01f);
    }

    public void DestructionReduce()
    {
        destructionEnlargeActive = false;

        backgroundHandler.ReduceTopExtent();
    }

    private void ReEnableMineCam()
    {
        MineHandler.cameraOK = true;
    }

    private void ReEnableColl()
    {
        destructionTransition = false;
    }
#endregion

#region Light
    public void LightClicked()
    {
        if(lightEnabled == 1)
        {
            lightEnabled = 0;
        } else
        {
            lightEnabled = 1;
        }

        PlayerPrefs.SetInt("Player_Light", lightEnabled);

        shopHandler.SavePurchasedItems();
        SceneManager.LoadScene(0);
    }

    private IEnumerator UpdateLight()
    {
        AsyncOperationHandle handle;

        if(lightEnabled == 1)
        {
            backgroundHandler.moonLight.enabled = true;
            backgroundHandler.sunLight.enabled = true;
            ffPlayerData.playerLightObj.enabled = true;

            yield return handle = on.GetLocalizedString();
        } else
        {
            backgroundHandler.moonLight.enabled = false;
            backgroundHandler.sunLight.enabled = false;
            ffPlayerData.playerLightObj.enabled = false;

            yield return handle = off.GetLocalizedString();
        }

        SetButtonText(lightButton.transform.GetChild(0).GetChild(0), (string)handle.Result);
    }
#endregion

#region Parallax
    public void ParallaxClicked()
    {
        if(parallaxMode == 0)
        {
            parallaxMode = 1;
        } else
        {
            parallaxMode = 0;
        }

        PlayerPrefs.SetInt("Player_ParallaxMode", parallaxMode);
        shopHandler.SavePurchasedItems();

        BackgroundHandler.Instance.SetNewBackground(
            ShopHandler.Instance.GetCurrentBackground());

        StartCoroutine(UpdateParallax());
    }

    private IEnumerator UpdateParallax()
    {
        AsyncOperationHandle handle;

        if (parallaxMode == 0)
        {
            yield return handle = off.GetLocalizedString();
        }
        else
        {
            yield return handle = on.GetLocalizedString();
        }

        SetButtonText(backgroundButton.transform.GetChild(0).GetChild(0), (string)handle.Result);
    }
#endregion

#region EnhancedDestruction
    public void DestructionClicked()
    {
        if (enhancedPipeDestruction == 0)
        {
            enhancedPipeDestruction = 1;
        }
        else
        {
            enhancedPipeDestruction = 0;
        }

        PlayerPrefs.SetInt("Player_EnhancedDestruction", enhancedPipeDestruction);
        StartCoroutine(UpdateDestruction());
    }

    private IEnumerator UpdateDestruction()
    {
        AsyncOperationHandle handle;

        if (enhancedPipeDestruction == 0)
        {
            yield return handle = off.GetLocalizedString();
        }
        else
        {
            yield return handle = on.GetLocalizedString();
        }

        SetButtonText(destructionButton.transform.GetChild(0).GetChild(0), (string)handle.Result);
    }
#endregion

#region JumpEffect
    public void JumpEffectClicked()
    {
        if (jumpEffectMode == 0)
        { //auf original setzen
            jumpEffectMode = 1;
        }
        else
        { //auf normal setzen
            jumpEffectMode = 0;
        }

        PlayerPrefs.SetInt("Player_JumpEffectMode", jumpEffectMode);
        UpdateJumpEffect();
    }

    private void UpdateJumpEffect()
    {
        if (jumpEffectMode == 0)
        {
            jumpEffectButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "Neu";
        }
        else
        {
            jumpEffectButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "Original";
        }
    }
#endregion

#region Difficulty
    public void DifficultyClicked()
    {
        if(difficulty == 0)
        {
            difficulty = 1;
        } else if(difficulty == 1)
        {
            difficulty = 2;
        } else if(difficulty == 2)
        {
            difficulty = 0;
        }

        PlayerPrefs.SetInt("Player_Difficulty", difficulty);
        UpdateDifficulty();
    }

    private void UpdateDifficulty()
    {
        switch(difficulty)
        {
            case 0:
                difficultyButton.transform.GetChild(0).GetComponent<Image>().color = new Color32(225, 8, 233, 255);
                difficultyButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    "Leicht";
                break;
            case 1:
                difficultyButton.transform.GetChild(0).GetComponent<Image>().color = new Color32(173, 220, 141, 255);
                difficultyButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    "Mittel";
                break;
            case 2:
                difficultyButton.transform.GetChild(0).GetComponent<Image>().color = new Color32(255, 153, 0, 255);
                difficultyButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    "Schwer";
                break;
        }
    }
#endregion

#region Intro
    public void PushClicked()
    {
        if(noPush == 0)
        {
            noPush = 1;
#if UNITY_ANDROID || UNITY_IOS
            FirebaseHandler.SetUserProperty("WantsMessages", "0");
#endif
        } else
        {
            noPush = 0;
#if UNITY_ANDROID || UNITY_IOS
            FirebaseHandler.SetUserProperty("WantsMessages", "1");
#endif
        }

        PlayerPrefs.SetInt("Player_NoPush", noPush);
        StartCoroutine(UpdatePush());
    }

    private IEnumerator UpdatePush()
    {
        AsyncOperationHandle handle;

        if (noPush == 0)
        {
            yield return handle = yes.GetLocalizedString();
        } else
        {
            yield return handle = no.GetLocalizedString();
        }

        introButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
            (string)handle.Result;
    }
#endregion

#region KreuzPos
    public void KreuzPosClicked()
    {
        if (kreuzPos == 0)
        { //auf links setzen
            kreuzPos = 1;
        }
        else
        { //auf rechts setzen
            kreuzPos = 0;
        }

        PlayerPrefs.SetInt("Player_KreuzPos", kreuzPos);
        StartCoroutine(UpdateKreuzPos());
    }

    private IEnumerator UpdateKreuzPos()
    {
        AsyncOperationHandle handle;

        if (kreuzPos == 0)
        {
            yield return handle = mineItemLeft.GetLocalizedString();
        }
        else
        {
            yield return handle = mineItemRight.GetLocalizedString();
        }

        string text = (string)handle.Result;

        SetButtonText(kreuzPosButton.transform.GetChild(0).GetChild(0), text);

        mineHandler.UpdateMovementSizePos();
    }
#endregion

#region KreuzSize
    public void KreuzSizeClicked()
    {
        if(kreuzSize == 0)
        {
            kreuzSize = 1;
        } else
        {
            kreuzSize = 0;
        }

        PlayerPrefs.SetInt("Player_KreuzSize", kreuzSize);
        StartCoroutine(UpdateKreuzSize());
    }

    private IEnumerator UpdateKreuzSize()
    {
        AsyncOperationHandle handle;

        if (kreuzSize == 0)
        {
            yield return handle = normal.GetLocalizedString();
        }
        else
        {
            yield return handle = gross.GetLocalizedString();
        }

        kreuzSizeButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
            (string)handle.Result;

        mineHandler.UpdateMovementSizePos();
    }
#endregion

#region MineMode
    public void MineModeClicked()
    {
        switch(mineMode)
        {
            case 0:
                mineMode = 1;
                break;
            case 1:
                mineMode = 2;
                break;
            case 2:
                mineMode = 0;
                break;
        }

        PlayerPrefs.SetInt("Player_MineMode", mineMode);
        StartCoroutine(UpdateMineMode());
    }

    private IEnumerator UpdateMineMode()
    {
        string text;

        AsyncOperationHandle handle;

        switch(mineMode)
        {
            default:
                yield return handle = mineHalten.GetLocalizedString();
                break;
            case 1:
                yield return handle = mineGesten.GetLocalizedString();
                break;
            case 2:
                yield return handle = mineKreuz.GetLocalizedString();
                break;
        }

        text = (string)handle.Result;

        SetButtonText(mineModeButton.transform.GetChild(0).GetChild(0), text);
    }

#endregion

#region Framerate
    public void FramerateClicked()
    {
        if (enhancedFramerate == 0)
        { //auf links setzen
            enhancedFramerate = 1;
        }
        else
        { //auf rechts setzen
            enhancedFramerate = 0;
        }

        PlayerPrefs.SetInt("Player_EnhancedFramerate", enhancedFramerate);
        StartCoroutine(UpdateEnhancedFramerate());
    }

    private IEnumerator UpdateEnhancedFramerate()
    {
#if UNITY_STANDALONE_WIN
        yield return null;
#endif

        AsyncOperationHandle handle;

        if (enhancedFramerate == 0)
        {
            yield return handle = on.GetLocalizedString();

#if UNITY_ANDROID
            QualitySettings.vSyncCount = 2;
#endif
            Application.targetFrameRate = 30;
//#endif
        }
        else
        {
            yield return handle = off.GetLocalizedString();

#if UNITY_ANDROID
            QualitySettings.vSyncCount = 1;
#endif
            Application.targetFrameRate = 60;
//#endif
        }

        SetButtonText(framerateButton.transform.GetChild(0).GetChild(0), (string)handle.Result);
    }
#endregion

#region musicvolume

    public void MusicVolumeSlider()
    {
        float val = volumeSlider.value;

        PlayerPrefs.SetFloat("Player_MusicVolume", val);
        UpdateMusicVolume(val);
    }

    private void UpdateMusicVolume(float val = -1)
    {
        if(val < 0)
        {
            val = PlayerPrefs.GetFloat("Player_MusicVolume", 1);
            volumeSlider.value = val;
        }
        SoundManager.Instance.SetMusicVolume(val);
    }

#endregion

#region effectvolume

    public void EffectVolumeSlider()
    {
        float val = effevtVolumeSlider.value;

        PlayerPrefs.SetFloat("Player_EffectVolume", val);
        UpdateEffectVolume(val);
    }

    private void UpdateEffectVolume(float val = -1)
    {
        if(val < 0)
        {
            val = PlayerPrefs.GetFloat("Player_EffectVolume", 1);
            effevtVolumeSlider.value = val;
        }
        SoundManager.Instance.SetEffectVolume(val);
    }

#endregion

#region vsync
    public void VSyncClicked()
    {
        if(vSyncEnabled == 0)
        {
            vSyncEnabled = 1;
        } else
        {
            vSyncEnabled = 0;
        }

        PlayerPrefs.SetInt("Player_VSync", vSyncEnabled);
        UpdateVSync();
    }

    private void UpdateVSync()
    {
#if UNITY_STANDALONE_WIN
        return;
#endif

        if (vSyncEnabled == 1)
        {
            vsyncButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "An";

            if (enhancedFramerate == 1)
            {
                QualitySettings.vSyncCount = 1;
            } else
            {
                QualitySettings.vSyncCount = 2;
            }
        } else
        {
            vsyncButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "Aus";

            QualitySettings.vSyncCount = 0;
        }

        StartCoroutine(UpdateEnhancedFramerate());
    }
#endregion

#region normalmap
    public void NormalMapClicked()
    {
        if(normalMaps == 0)
        {
            normalMaps = 1;
        } else
        {
            normalMaps = 0;
        }

        PlayerPrefs.SetInt("Player_NormalMap", normalMaps);
        UpdateNormalMap();
    }

    public void UpdateNormalMap()
    {
        if(normalMaps == 0)
        {
            normalMapButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "Aus";
        } else
        {
            normalMapButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "An";
        }

        for(int i = 0; i < allLights.Length; i++)
        {
            //allLights[i].ble
        }
    }
#endregion

    public void UpdateCategory(int id)
    {
        if (dataRequestRunning) return;

        dataPrivacy.SetActive(false);
        credits.SetActive(false);

        if(id == 0)
        { //Allgemein
            allgObj.SetActive(true);
            graphicsObj.SetActive(false);
            soundObj.SetActive(false);
            accountObj.SetActive(false);
        } else if(id == 1)
        { //Grafik
            graphicsObj.SetActive(true);
            soundObj.SetActive(false);
            allgObj.SetActive(false);
            accountObj.SetActive(false);
        } else if(id == 2)
        { //Sound
            graphicsObj.SetActive(false);
            soundObj.SetActive(true);
            allgObj.SetActive(false);
            accountObj.SetActive(false);
        } else if(id == 3)
        { //Account
            graphicsObj.SetActive(false);
            soundObj.SetActive(false);
            allgObj.SetActive(false);

            accountObj.SetActive(true);
            mainAccountPage.SetActive(true);
            requestAccountPage.SetActive(false);

            if(AccountHandler.Instance.accountState == AccountStates.LoggedOut)
            {
                logOutButton.GetComponent<Button>().interactable = false;
                requestDataButton.GetComponent<Button>().interactable = false;
            } else
            {
                logOutButton.GetComponent<Button>().interactable = true;
                requestDataButton.GetComponent<Button>().interactable = true;
            }
        }
    }

    public void LogoutClicked()
    {
        logOutButton.GetComponent<Button>().interactable = false;
        requestDataButton.GetComponent<Button>().interactable = false;

        UpdateUsernameString(false);

        AccountHandler.Instance.LogoutUser();
    }

    public void OpenDataRequest()
    {
        mainAccountPage.SetActive(false);
        requestAccountPage.SetActive(true);

        emailInput.text = "";
        passwordInput.text = "";

        dataRequestRunning = false;

        infoText.gameObject.SetActive(false);
        requestDataFullButton.GetComponent<Button>().interactable = true;
    }

    public void DataRequestClicked()
    {
        if (dataRequestRunning) return;

        dataRequestRunning = true;

        string email = emailInput.text;
        string pw = passwordInput.text;
        string name = AccountHandler.Instance.username;

        requestDataFullButton.GetComponent<Button>().interactable = false;

        infoText.gameObject.SetActive(true);
        infoText.color = Color.black;
        infoText.text = AccountHandler.Instance.connectionString;

        StartCoroutine(HandleDataRequest(email, pw, name));
    }

    IEnumerator HandleDataRequest(string email, string pw, string name)
    {
        string authHash = AccountHandler.Md5Sum(name + Auth.dataAuthKey);

        pw = AccountHandler.Md5Sum(pw);

        WWWForm form = new WWWForm();
        form.AddField("name", name);
        form.AddField("pw", pw);
        form.AddField("email", email);
        form.AddField("hash", authHash);

        using (UnityWebRequest www = UnityWebRequest.Post("https://bruh.games/datarequest.php", form))
        {
#pragma warning disable CS0618 // Typ oder Element ist veraltet
            www.chunkedTransfer = false;
#pragma warning restore CS0618 // Typ oder Element ist veraltet

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                infoText.color = Color.red;
                infoText.text = AccountHandler.Instance.connectionFailedString;
                requestDataFullButton.GetComponent<Button>().interactable = true;
            }
            else
            { //datenabfrage erfolgreich
                string response = www.downloadHandler.text;

                if(response.Contains("1"))
                { //erfolgreich
                    emailInput.text = "";
                    passwordInput.text = "";

                    infoText.color = new Color32(0, 130, 0, 255);
                    infoText.text = emailSentString;
                } else
                {
                    infoText.color = Color.red;
                    infoText.text = emailNotSentString;

                    requestDataFullButton.GetComponent<Button>().interactable = true;
                }
            }

            dataRequestRunning = false;
        }
    }

    public void ExtendGraphicsClicked()
    {
        bool enable = true;

        if(extendParent.activeSelf)
        {
            enable = false;
        }

        extendParent.SetActive(enable);

    }

    // Update is called once per frame
    void Update()
    {
        if (optionsActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !closing)
            {
                CloseOptions();
            }
        }
    }
}
