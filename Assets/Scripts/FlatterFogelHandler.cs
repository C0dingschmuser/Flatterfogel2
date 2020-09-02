using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEditor;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using Destructible2D;
#if UNITY_ANDROID || UNITY_IOS
using Firebase.Analytics;
#endif
using System.Linq;

public class FlatterFogelHandler : MonoBehaviour
{
    public GameObject player, pipePrefab, flash, scoreText, goBtn, backbtn, menu,
        cameraObj, particleParent, pipeDestructionPrefab, deathText, backgroundHandler, blusPrefab,
        pipeDestructionParent, modeChangeInfo, startTimerObj, ovenButton, highscoreLineObj,
        topCollider, bossWarning;

    public GameObject[] playerScoreEffect;
    public GameObject[] cameraColliders;
    private int scoreEffectCounter = 0;

    public Transform pipeParent, visualObstacleParent, miningItemParent;

    public bool gameActive = false, modeCurrentlyChanged = false, battleRoyale = false,
        destructablePipeOnScreen = false, hardcore = false, destructionMode = false, miningMode = false,
        zigZag = false;

    public AnimationCurve flashCurve;
    public GameObject[] housePrefabs;
    public GameObject[] otherDestructionPrefabs;

    public Sprite[] blusSprites, coinSprites;

    private List<GameObject> pipes = new List<GameObject>();
    private List<GameObject> otherObjs = new List<GameObject>();
    private List<GameObject> gravestoneObjs = new List<GameObject>();
    private List<GameObject> aiObjs = new List<GameObject>();
    private List<GameObject> effects = new List<GameObject>();
    private List<GameObject> coins = new List<GameObject>();

    public List<GameObject> bottomObjs = new List<GameObject>();
    public AchievementHandler achHandler;
    public GroundHandler groundHandler;
    public MineHandler mineHandler;
    public DestructionHandler destructionHandler;
    public IngameMenuHandler ingameMenu;
    public ZigZagHandler zigZagHandler;
    public SplatterHandler splatterHandler;
    public ShootingPipeHandler shootingPipehandler;
    public TutorialHandler tutHandler;

    public float minPipeY = 500, maxPipeY = 1065;

    private ObscuredInt internalScoreCount = 0, tunnelRemaining = 0, tunnelDir = 0, tunnelMax = 10;
    private ObscuredULong score = 0, taps = 0, lastScore = 0, roundCoins = 0, perfectHits = 0;

    public class HighscoreHolder
    {
        public ObscuredULong[] highscore = new ObscuredULong[3];
    }

    private HighscoreHolder[] highscore = new HighscoreHolder[6];
    private bool modeChanging = false, isStarting = false, highscoreLineShowed = false, highscoreLineMode = false,
        pipeSpawnAllowed = false, holdingDown = false, nextPipeTunnel = false, inTunnel = false;
    
    public static Vector3 currentCameraPos;

    public GameObject aiTest;
    public ModeManager modeManager;

    public Vector3 playerStartPos, playerPlayPos;
    public ScoreHandler scoreHandler;

    [SerializeField] private PostProcessVolume defaultVolume = null;
    [SerializeField] private Sprite[] flyCloudSprites = null;
    [SerializeField] private Sprite[] pauseSprites = null;
    [SerializeField] public Image pauseImage = null;
    [SerializeField] private Color[] defaultColors = null;
    [SerializeField] private GameObject[] blusEffects = null;
    [SerializeField] private Material[] blusEffectsMats = null;
    private float[] blusEffectsDissolve = new float[3];
    [SerializeField] private CameraShake cameraShake = null;
    [SerializeField] private Camera mainCamera = null;

    private int startTimer = 3, d2dLayerCounter = 0, blusEffectCounter = 0;
    private List<GameObject> delList = new List<GameObject>();

    public static Color32[] pr0Farben = 
        new Color32[4] { new Color32(238, 77, 46, 255), 
            new Color32(29, 185, 146, 255), 
            new Color32(191, 188, 6, 255), 
            new Color32(0, 143, 255, 255) };

    public static Color32 currentColor = new Color32(0, 0, 0, 255);

    private float lastTimeScale = 1f;
    private Tween scoreScaleTween = null, scoreColorTween = null;

    public static int state = 0, gameState = 0, waitingState = -1;
    public static bool cullingEnabled = false, gamePaused = false, clicked = false,
        nextCompleteDestruction = false;
    public static float scrollSpeed = 175f;
    public const float defaultScrollSpeed = 175f; //150 def
    
    ObjectPooler objectPooler;
    public static FlatterFogelHandler Instance;
    public static float fixedStep = 0.02f;

    public static Color32 GetTandomPr0Color()
    {
        return pr0Farben[Random.Range(0, pr0Farben.Length)];
    }

    public enum FF_States {
        Idle = 0,
        Playing = 1,
        End = 2,
    }

    private void Awake()
    {
        for(int i = 0; i < highscore.Length; i++)
        {
            HighscoreHolder h = new HighscoreHolder();
            highscore[i] = h;
        }

        //Application.targetFrameRate = 60;
        objectPooler = ObjectPooler.Instance;
        Instance = this;

        SetScoreColor(defaultColors[Random.Range(0, defaultColors.Length)]);
        ModeManager.modeColor = currentColor;
    }

    public void SetInternalScore(int newScore)
    {
        internalScoreCount = newScore;
    }

    public void AddRoundCoin()
    {
        roundCoins++;
    }

    public void AddPerfectHit()
    {
        perfectHits++;
    }

    private void FlashHighscoreObj()
    {
        if(highscoreLineMode)
        {
            highscoreLineMode = false;

            highscoreLineObj.GetComponent<TextMeshProUGUI>().color = new Color32(0, 255, 255, 255);
            highscoreLineObj.transform.GetChild(0).
                GetComponent<TextMeshProUGUI>().color = new Color32(255, 0 , 0, 255);
        } else
        {
            highscoreLineMode = true;

            highscoreLineObj.GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, 255); 
            highscoreLineObj.transform.GetChild(0).
                GetComponent<TextMeshProUGUI>().color = new Color32(0, 255, 255, 255);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        lastScore = ObscuredPrefs.GetULong("Player_LastScore", 0);

        for(int i = 0; i < highscore.Length; i++)
        { //loop durch modes
            for(int a = 0; a < 3; a++)
            { //loop durch diffs
                ulong score = ObscuredPrefs.GetULong("Player_Highscore_" + i.ToString() + "_" + a.ToString(), 0);

#if UNITY_EDITOR
                if(score == 0)
                {
                    score = 1;
                }
#endif

                highscore[i].highscore[a] = score;
            }
        }

#if UNITY_ANDROID || UNITY_IOS
        cullingEnabled = true;
        FirebaseAnalytics.SetCurrentScreen("MainMenu", "UnityPlayerActivity");

#endif

        Timing.RunCoroutine(Util._EmulateUpdate(_MainUpdate, this));
    }

    public void UpdateTap()
    {
        if(tutHandler.mainTut == 0)
        {
            if(tutHandler.mainTutStep == 0)
            {
                if(taps >= 3)
                {
                    tutHandler.StartMainTutGreat();
                }
            }
        }

        taps++;
    }

    private void DeadRestart()
    {
        StartGame();
    }

    public ObscuredULong GetScore()
    {
        return score;
    }

    public ObscuredULong GetHighscore(int mode = -1, int diff = -1)
    {
        if(diff == -1)
        {
            diff = OptionHandler.GetDifficulty();
        }

        if(mode == -1)
        {
            mode = ModeManager.currentIndex;
        }

        return highscore[mode].highscore[diff];
    }

    public void SetHighscore(int mode, int diff, ulong score)
    {
        highscore[mode].highscore[diff] = score;

        ObscuredPrefs.SetULong("Player_Highscore_" + mode.ToString() + "_" + diff.ToString(), score);
    }

    public ObscuredULong GetLastScore()
    {
        return lastScore;
    }

    public void DissolvePipes(float time)
    {
        if (pipes.Count > 0)
        {
            Color c = pipes[0].GetComponent<SpriteRenderer>().color;

            for (int i = 0; i < pipes.Count; i++)
            {
                PipeData pipeData = pipes[i].GetComponent<PipeData>();
                if (pipeData.isChecked)
                {
                    pipeData.SetLightIntensity(0);
                }

                pipes[i].transform.parent.GetComponent<PipeHolder>().StopMove();
                /*if (OptionHandler.enhancedPipeDestruction == 1)
                {
                    if(!pipeData.isChecked || !pipeData.IsDestructionStarted(true))
                    {
                        pipeData.isChecked = true;
                        pipeData.StartDestruction(0.4f, 0, true);
                    }
                }*/
            }

            //if(OptionHandler.enhancedPipeDestruction == 0)
            //{
            pipeParent.GetComponent<Dissolver>().StartDissolve(c, time);
            //}
        }
    }

    public void DisableOtherObjs()
    {
        for (int i = 0; i < otherObjs.Count; i++)
        {
            otherObjs[i].SetActive(false);
        }
        otherObjs.Clear();
    }

    public void StartGame(bool fullReset = true)
    {
        startTimer = 2;

        isStarting = true;

        FF_PlayerData pD = player.GetComponent<FF_PlayerData>();
        if (pD.dead)
        {
            pD.PlayerGo(false);

            //battleRoyale = false;
            for(int i = 0; i < aiObjs.Count; i++)
            {
                aiObjs[i].SetActive(false);
            }
            aiObjs.Clear();

            DissolvePipes(0.6f);

            destructionHandler.ClearAll();

            groundHandler.DissolveGround(gameState);

            DisableOtherObjs();

            for(int i = 0; i < coins.Count; i++)
            {
                coins[i].SetActive(false);
            }
            coins.Clear();

            for (int i = 0; i < gravestoneObjs.Count; i++)
            {
                gravestoneObjs[i].GetComponent<GravestoneHandler>().FadeOutGravestone(0.5f);
            }

            Invoke(nameof(BeginPipeDestruction), 0.7f);
            Invoke(nameof(BeginGroundDestruction), 0.7f);
            Invoke(nameof(DeadRestart), 0.75f);
            return;
        } else
        {
            pD.PlayerGo(false);
        }

        pipeParent.GetComponent<Dissolver>().ResetDissolve();
        groundHandler.ResetDissolve(gameState);

        ovenButton.SetActive(false);

        clicked = false;

        nextCompleteDestruction = false;

        state = (int)FF_States.Playing;
        gameState = 0;
        waitingState = -1;
        scrollSpeed = defaultScrollSpeed;

        if(zigZag)
        {
            scrollSpeed = ZigZagHandler.scrollSpeed;
        }

        if(!destructionMode && !battleRoyale && !miningMode)
        {
            /*switch(OptionHandler.GetDifficulty())
            {
                case 0:
                    scrollSpeed -= 25;
                    break;
                case 2:
                    scrollSpeed += 35;
                    break;
            }*/

            scrollSpeed = defaultScrollSpeed + 75;
        }

        if(hardcore)
        {
            scrollSpeed += 40;
        }

        player.GetComponent<CircleCollider2D>().isTrigger = true;
        player.GetComponent<BoxCollider2D>().isTrigger = true;

        FF_PlayerData.Instance.ResetRotation();
        FF_PlayerData.Instance.ResetMine(false);

        FF_PlayerData.Instance.SetPlayerDepth(1);

        for(int i = 0; i < otherObjs.Count; i++)
        {
            otherObjs[i].SetActive(false);
        }

        otherObjs.Clear();

        //if(OptionHandler.enhancedPipeDestruction == 0)
        //{
            for(int i = 0; i < pipeParent.childCount; i++)
            {
                pipeParent.GetChild(i).gameObject.SetActive(false);
            }
            pipes.Clear();
        //}

        //bool hardcore = false;
        if(hardcore)
        {
            //hardcoreLast = true;
            backgroundHandler.GetComponent<BackgroundHandler>().DisableEnableLight(false);
        }

        OptionHandler.hardcoreActive = hardcore;
        player.GetComponent<FF_PlayerData>().EnableDisableHardcore(hardcore);

        pipeParent.GetComponent<Dissolver>().ResetDissolve();

        gravestoneObjs.Clear();

        destructionHandler.ClearAll();
        mineHandler.ResetMines();

        ShopHandler.Instance.UpdateMineItemUI();

        backgroundHandler.GetComponent<BackgroundHandler>().SetScrolling(true);

        cameraObj.transform.position = OptionHandler.defaultCameraPos; //kamera zurücksetzen

        int diff = OptionHandler.GetDifficulty();

        if(hardcore)
        {
            diff = 1;
        }

        if(zigZag)
        {
            diff = 0;
        }

        switch(diff)
        {
            case 0:
                playerPlayPos = new Vector3(-540, 790, -0.5f);
                break;
            case 1:
                playerPlayPos = new Vector3(-468, 790, -0.5f);
                break;
            case 2:
                playerPlayPos = new Vector3(-381, 790, -0.5f);
                break;
        }

        cameraColliders[0].transform.position = new Vector3(mainCamera.transform.position.x - 372,
            mainCamera.transform.position.y, 100);

        cameraColliders[1].transform.position = new Vector3(mainCamera.transform.position.x + 372,
            mainCamera.transform.position.y, 100);

        if (destructionMode)
        {
            playerPlayPos = new Vector3(-648, 790, -0.5f);
            cameraColliders[0].SetActive(false);
            cameraColliders[1].SetActive(false);

            BackgroundHandler.Instance.ScaleLights(false);
            OptionHandler.Instance.DestructionEnlarge();
        } else
        {
            BackgroundHandler.Instance.ScaleLights(true);

            cameraColliders[0].SetActive(true);
            cameraColliders[1].SetActive(true);

            if(miningMode)
            {
                cameraColliders[0].transform.position = new Vector3(mainCamera.transform.position.x - 283.1f,
                    mainCamera.transform.position.y, 100);
                cameraColliders[1].transform.position = new Vector3(mainCamera.transform.position.x + 283.1f,
                    mainCamera.transform.position.y, 100);
            }
        }

        player.transform.position = playerStartPos;
        player.transform.DOMove(playerPlayPos, 1f);

        if(fullReset)
        {
#if UNITY_EDITOR
            SetScore(0, 0);
            //internalScoreCount = 38;
#else
            SetScore(0, 0);
#endif
            scoreText.SetActive(false);

            taps = 0;
            roundCoins = 0;
            perfectHits = 0;

            highscoreLineShowed = false;

            CancelInvoke(nameof(FlashHighscoreObj));
            highscoreLineObj.SetActive(false);
        }

        player.GetComponent<Rigidbody2D>().simulated = false;

        ResetTimerObj();

        startTimerObj.SetActive(true);

        if(battleRoyale)
        {
            int maxAI = 10;

            for(int i = 0; i < maxAI; i++)
            {
                float percent = (float)i / maxAI;

                int endPoint = 0;

                #region CalcEndPoint
                if (percent <= 0.2f)
                {
                    switch(OptionHandler.GetDifficulty())
                    {
                        case 0:
                            if(Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(1, 3);
                            } else
                            {
                                endPoint = Random.Range(0, 3);
                            }
                            break;
                        case 1:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(1, 4);
                            }
                            else
                            {
                                endPoint = Random.Range(0, 6);
                            }
                            break;
                        case 2:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(2, 6);
                            }
                            else
                            {
                                endPoint = Random.Range(0, 10);
                            }
                            break;
                    }
                } else if(percent <= 0.4f)
                {
                    switch (OptionHandler.GetDifficulty())
                    {
                        case 0:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(2, 6);
                            }
                            else
                            {
                                endPoint = Random.Range(2, 4);
                            }
                            break;
                        case 1:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(3, 7);
                            }
                            else
                            {
                                endPoint = Random.Range(3, 8);
                            }
                            break;
                        case 2:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(6, 12);
                            }
                            else
                            {
                                endPoint = Random.Range(5, 12);
                            }
                            break;
                    }
                } else if(percent <= 0.6f)
                {
                    switch (OptionHandler.GetDifficulty())
                    {
                        case 0:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(5, 15);
                            }
                            else
                            {
                                endPoint = Random.Range(7, 15);
                            }
                            break;
                        case 1:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(10, 20);
                            }
                            else
                            {
                                endPoint = Random.Range(10, 17);
                            }
                            break;
                        case 2:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(15, 30);
                            }
                            else
                            {
                                endPoint = Random.Range(15, 23);
                            }
                            break;
                    }
                } else if(percent <= 0.8f)
                {
                    switch (OptionHandler.GetDifficulty())
                    {
                        case 0:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(20, 40);
                            }
                            else
                            {
                                endPoint = Random.Range(15, 35);
                            }
                            break;
                        case 1:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(20, 50);
                            }
                            else
                            {
                                endPoint = Random.Range(20, 45);
                            }
                            break;
                        case 2:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(25, 60);
                            }
                            else
                            {
                                endPoint = Random.Range(25, 55);
                            }
                            break;
                    }
                }
                else if (percent <= 1f)
                {
                    switch (OptionHandler.GetDifficulty())
                    {
                        case 0:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(30, 50);
                            }
                            else
                            {
                                endPoint = Random.Range(25, 45);
                            }
                            break;
                        case 1:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(30, 60);
                            }
                            else
                            {
                                endPoint = Random.Range(35, 55);
                            }
                            break;
                        case 2:
                            if (Random.Range(0, 3) == 0)
                            {
                                endPoint = Random.Range(35, 85);
                            }
                            else
                            {
                                endPoint = Random.Range(35, 75);
                            }
                            break;
                    }
                }
                #endregion

                GameObject newAi = 
                    objectPooler.SpawnFromPool("RoyaleAI", new Vector3(Random.Range(-699, -242), 1476), Quaternion.identity);
                newAi.GetComponent<AIHandler>().StartAI(Random.Range(497, 1235), Random.Range(0, 2.5f), endPoint);

                aiObjs.Add(newAi);
            }

            StartCoroutine(HandleAI());
        }

#if UNITY_STANDALONE_WIN

#else
        if (OptionHandler.enhancedFramerate == 1 /*&& !destructionMode*/)
        {
            QualitySettings.vSyncCount = 1;
        } else
        {
            QualitySettings.vSyncCount = 2;
        }
#endif
        fixedStep = 0.02f;

        if(miningMode)
        { //setzt licht runter und ground cover
            //backgroundHandler.GetComponent<BackgroundHandler>().StartMining(); aufgerufen in minehandler
            groundHandler.EnableGroundCover(false);
        } else
        {
            groundHandler.EnableGroundCover(true);
            mineHandler.DisableBackground();
        }

#if UNITY_ANDROID || UNITY_IOS
        if (destructionMode)
        {
            fixedStep = 0.03f;
        }
#endif

#if UNITY_ANDROID || UNITY_IOS
        if(fullReset)
        {
            if (!hardcore && !destructionMode && !miningMode && !battleRoyale && !zigZag)
            {
                StatHandler.classicCount++;

                FirebaseAnalytics.SetCurrentScreen("Classic", "UnityPlayerActivity");
                FirebaseAnalytics.LogEvent("ClassicPlayed");
            }
            else if (hardcore && !destructionMode && !miningMode && !battleRoyale && !zigZag)
            {
                FirebaseAnalytics.SetCurrentScreen("Hardcore", "UnityPlayerActivity");
                FirebaseAnalytics.LogEvent("HardcorePlayed");
            }
            else if (!hardcore && destructionMode && !miningMode && !battleRoyale && !zigZag)
            {
                FirebaseAnalytics.SetCurrentScreen("Destruction", "UnityPlayerActivity");
                FirebaseAnalytics.LogEvent("DestructionPlayed");
            }
            else if (!hardcore && !destructionMode && miningMode && !battleRoyale && !zigZag)
            {
                FirebaseAnalytics.SetCurrentScreen("Mining", "UnityPlayerActivity");
                FirebaseAnalytics.LogEvent("MiningPlayed");
            }
            else if (!hardcore && !destructionMode && !miningMode && battleRoyale && !zigZag)
            {
                FirebaseAnalytics.SetCurrentScreen("Royale", "UnityPlayerActivity");
                FirebaseAnalytics.LogEvent("RoyalePlayed");
            }
            else if (!hardcore && !destructionMode && !miningMode && !battleRoyale && zigZag)
            {
                FirebaseAnalytics.SetCurrentScreen("ZigZag", "UnityPlayerActivity");
                FirebaseAnalytics.LogEvent("ZigZagPlayed");
            }
        }
#endif

        Time.fixedDeltaTime = fixedStep;

        if(miningMode)
        {
            SoundManager.Instance.PlayMusicFade(MusicID.Mining);
        } else
        {
            SoundManager.Instance.PlayMusicFade((MusicID)Random.Range(1, 3));
        }

        InvokeRepeating(nameof(StartTimerStep), 0f, 1f);
        //Invoke("EndStart", 2f);
        //Invoke("StartGround", 5f);
    }

    public void SetModes(bool battleRoyale, bool destructionMode, bool hardcore, bool miningMode, bool zigzag)
    {
        this.battleRoyale = battleRoyale;
        this.destructionMode = destructionMode;
        this.hardcore = hardcore;
        this.miningMode = miningMode;
        this.zigZag = zigzag;
    }

    public Vector3 LatestPipePos(GameObject caller, float xPos = -381)
    {
        Vector3 start = new Vector3(xPos, 790);

        float dist = 2000;

        GameObject nearestBlus = null;

        for(int i = 0; i < pipes.Count; i += 2)
        {
            GameObject blus = pipes[i].transform.parent.GetComponent<PipeHolder>().GetAssignedBlus();

            if(blus.transform.position.x > start.x - 75)
            { //check das blus in range
                float newDist = Mathf.Abs(blus.transform.position.x - start.x);

                if (newDist < dist)
                {
                    dist = newDist;
                    nearestBlus = blus;
                }
            }
        }

        if (nearestBlus == null)
        {
            return start;
        } else
        {
            caller.GetComponent<AIHandler>().SetCurrentBlus(nearestBlus);
            return nearestBlus.transform.position;
        }
    }

    public void PlayerShoot()
    {
        if(destructionMode)
        {
            //dest
            destructionHandler.PlayerShootD2D();
            return;
        }

        if (!destructablePipeOnScreen) return;

        Vector3 playerPos = player.transform.position;
        playerPos.x += 53;

        GameObject newP = objectPooler.SpawnFromPool("Projectile", playerPos, Quaternion.identity);
        newP.GetComponent<Rigidbody2D>().velocity = new Vector2(750, 0);

        newP.GetComponent<ProjectileHandler>().ResetProjectile();
        //newP.GetComponent<BoxCollider2D>().isTrigger = false;
    }

    private void ResetTimerObj()
    {
        Color c = startTimerObj.GetComponent<TextMeshProUGUI>().color;
        c.a = 0;

        startTimerObj.GetComponent<TextMeshProUGUI>().color = c;
    }

    private void StartTimerStep()
    {
        if(startTimer == 0)
        {
            CancelInvoke(nameof(StartTimerStep));

            startTimerObj.SetActive(false);

            EndStart();
        } else
        {
            startTimerObj.GetComponent<TextMeshProUGUI>().text = startTimer.ToString();
            startTimerObj.transform.localScale = new Vector3(9, 9, 9);

            ResetTimerObj();

            startTimerObj.GetComponent<TextMeshProUGUI>().DOFade(1, 0.25f);
            startTimerObj.transform.DOScale(2, 0.75f);

            startTimer--;
        }
    }

    private void BeginPipeDestruction()
    {
        for (int i = 0; i < pipes.Count; i++)
        { //i ist immer top und i+1 = bottom
            if (i == 0 || i % 2 == 0)
            { //middleobj deaktivieren
                pipes[i].transform.parent.GetChild(3).gameObject.SetActive(false);
            }

            if (OptionHandler.enhancedPipeDestruction == 0)
            {
                pipes[i].GetComponent<PipeData>().ScaleDown();
                pipes[i].SetActive(false);
            }

        }

        if (OptionHandler.enhancedPipeDestruction == 0)
        {
            pipes.Clear();
        }
    }

    private void BeginGroundDestruction()
    {
        groundHandler.DestroyGround(gameState);
    }

    public void EndGame()
    {
        //bottomObjs[1].SetActive(false);

        //Camera.main.transform.DOMove(new Vector3(-381, 642, -10), 0.5f);

        Invoke(nameof(EndEndGame), 0.5f);
    }

    private void EndEndGame()
    {
        gameObject.SetActive(false);
    }

    private void EndStart()
    {
        if(!zigZag)
        {
            player.GetComponent<Rigidbody2D>().simulated = true;
        } else
        { //fallen deaktiviert
            player.GetComponent<Rigidbody2D>().simulated = false;
            zigZagHandler.StartZigZag();
        }

        gameActive = true;
        player.GetComponent<FF_PlayerData>().PlayerGo(true);

        scoreText.SetActive(true);

        if(miningMode)
        {
            ThinkHandler.Instance.StartThink(ThinkHandler.Instance.mineBeginString, 2f);
        }

        pauseImage.gameObject.SetActive(true);

        isStarting = false;

        InvokeRepeating(nameof(HandleRotation), 0f, 0.05f);
        if(!destructionMode && !miningMode)
        {
            pipeSpawnAllowed = true;

            if(tutHandler.mainTut != 0)
            {
                Timing.RunCoroutine(SpawnPipesWait(1f, false)); //noDEBUG
                //SpawnTunnel(10, true);
            }
        }
        
        if(destructionMode)
        {
            destructionHandler.DisableEnable(true);
        }


        //StartGround();
    }

    private void StartPipe(int type)
    { //aufgerufen nach wechsel von anderem modus zu pipe
        switch(type)
        { //typ des aktuellen modus
            case 1: //platformer

                modeChangeInfo.SetActive(true);
                modeChangeInfo.GetComponent<TextMeshProUGUI>().text = "FLIEG!";
                Invoke(nameof(DisableModeChangeText), 2f);

                break;
        }
        waitingState = 0;
    }

    private void DisableModeChangeText()
    {
        modeChangeInfo.SetActive(false);
    }

    private void StartPipeFull()
    {
        groundHandler.DestroyGround(1);
        groundHandler.GenerateStartGroundObjs();

        mineHandler.gameObject.SetActive(false);

        waitingState = -1;
        gameState = 0;
        //scrollSpeed = defaultScrollSpeed;
        player.GetComponent<CircleCollider2D>().isTrigger = true;
        player.GetComponent<BoxCollider2D>().isTrigger = true;

        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        SoundManager.Instance.PlayMusicFade((MusicID)Random.Range(1, 3));

        Vector3 pos = player.transform.position;
        pos.x = playerPlayPos.x;

        player.transform.position = pos;

        FF_PlayerData.Instance.ResetGravityScale();
        FF_PlayerData.Instance.ResetMine();

        pipeSpawnAllowed = true;
        Timing.RunCoroutine(SpawnPipesWait(1f, false));
    }

    public Vector3[] GetAllPipePositions(float overrideHeight = -9999)
    {
        Vector3[] posArray = new Vector3[pipes.Count / 2];
        for(int i = 0; i < pipes.Count; i += 2)
        {
            posArray[i / 2] = pipes[i].transform.position;
        }

        if(overrideHeight > -9999)
        {
            for(int i = 0; i < posArray.Length; i++)
            {
                posArray[i].y = overrideHeight;
            }
        }

        return posArray;
    }

    public void StartMineFull()
    {
        mineHandler.gameObject.SetActive(true);
        mineHandler.StartMining();

        CancelInvoke(nameof(FlashHighscoreObj));
        highscoreLineObj.SetActive(false);

        groundHandler.StartMiningGround();
        gameState = 2;

        for(int i = 0; i < pipes.Count; i++)
        {
            /*if(!pipes[i].GetComponent<PipeData>().isTop)
            { //untere pipes resizen so dass sie nicht mehr
                //in minefeld hereinragen
                pipes[i].transform.parent.GetComponent<PipeHolder>().StopMove();

                Vector3 endPos = pipes[i].transform.GetChild(0).position;

                Vector3 middlePos = 
                    pipes[i].transform.parent.GetComponent<PipeHolder>().GetAssignedBlus().transform.position;
                middlePos.y -= pipes[i].GetComponent<PipeData>().abstand;

                float diff = middlePos.y - 225;

                pipes[i].transform.position = new Vector3(middlePos.x, middlePos.y - (diff / 2));
                pipes[i].GetComponent<SpriteRenderer>().size = new Vector2(1, diff / 75);
                pipes[i].transform.GetChild(0).position = endPos; //endstück vorherige position zuweisen
            }*/
            pipes[i].GetComponent<SpriteRenderer>().enabled = false;
            pipes[i].GetComponent<BoxCollider2D>().enabled = false;
        }
        pipes.Clear();

        //FF_PlayerData.Instance.ResetStamina(true);

        backgroundHandler.GetComponent<BackgroundHandler>().SetScrolling(false);
        pipeSpawnAllowed = false;

        destructionHandler.DisableEnable(false);
    }

    private void StartGroundFull()
    {
        pipeSpawnAllowed = false;
        destructionHandler.DisableEnable(false);

        for (int i = 0; i < pipes.Count; i++)
        {
            pipes[i].SetActive(false);
        }
        pipes.Clear();

        waitingState = -1;
        gameState = 1;
        //CancelInvoke("HandleRotation");

        groundHandler.StartGround();

        scrollSpeed = 375;

        player.GetComponent<CircleCollider2D>().isTrigger = false;
        player.GetComponent<BoxCollider2D>().isTrigger = false;

        player.GetComponent<Rigidbody2D>().gravityScale = 200;
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        //player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        //player.transform.DOMoveX(-568, 0.5f);
        player.transform.position = new Vector3(-568, player.transform.position.y);

        Invoke(nameof(EnablePlayerConstraints), 0.55f);
    }

    private void EnablePlayerConstraints()
    {
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
    }

    private IEnumerator<float> CheckBlusCircle(int current)
    {
        int newCounter = current; //zuweisung da evtl mehrmals aufgerufen
        int len = 0;

        yield return Timing.WaitForSeconds(0.125f); //warte bis particle system effekt anfängt

        float currentRadius = 10;
        DOTween.To(() => currentRadius, x => currentRadius = x, 400, 0.7f);

        while (true)
        {
            //int num = pSystem.GetParticles(pParticles);
            if(currentRadius > 399)
            {
                //Debug.Log("over");
                break;
            }

            Collider2D[] objs =
                Physics2D.OverlapCircleAll(blusEffects[newCounter].transform.position, currentRadius);/*(pParticles[0].GetCurrentSize(pSystem) / 10) * 400f);*/

            //objs = objs.OrderBy(
            //    x => Vector2.Distance(this.transform.position, x.transform.position))
            //    .ToArray();

            len = objs.Length;

            for(int i = 0; i < len; i++)
            {
                if(objs[i].CompareTag("PipeDestructionPrefab"))
                {
                    if(objs[i].GetComponent<Rigidbody2D>().isKinematic)
                    {
                        //objs[i].GetComponent<SpriteRenderer>().color = Color.black;

                        Color c = objs[i].GetComponent<SpriteRenderer>().color;
                        objs[i].GetComponent<SpriteRenderer>().color = Color.black;

                        Vector3 oScale = objs[i].transform.localScale;

                        objs[i].transform.localScale *= 3;

                        float H, S, V;

                        Color.RGBToHSV(c, out H, out S, out V);

                        S *= 0.5f;

                        Color nC = Color.HSVToRGB(H, S, V);

                        objs[i].GetComponent<SpriteRenderer>().DOColor(nC, 0.4f);
                        objs[i].transform.DOScale(oScale, 0.25f);

                        objs[i].GetComponent<Rigidbody2D>().isKinematic = false;
                        objs[i].GetComponent<Rigidbody2D>().AddForce(
                                        new Vector2(Random.Range(-12500, 12500), Random.Range(-2500, 2500)));

                        //yield return new WaitForSeconds(0.001f);
                    }
                }
            }

            yield return Timing.WaitForSeconds(0.03f);
        }
    }

    private void SetScore(ulong score)
    {
        this.score = score;
    }

    public void SetScore(int score, int add = 0, int effect = 0, GameObject blus = null)
    {
        if(add == 0)
        {//setzen
            SetScore((ulong)score);
            internalScoreCount = 0;
        } else if(add == 1)
        { //addieren
            SetScore(this.score + (ulong)score);
            ResetModeChangedBool();

            internalScoreCount += score;

            float xp = (float)score * 5;

            if(destructionMode)
            {
                xp /= 4;
            }
            
            if(zigZag)
            {
                xp /= 3;
            }

            if(!miningMode)
            {
                LevelHandler.Instance.AddNewXP((int)xp);
            }

            float am = 7;

            if (hardcore)
            {
                am = 5;
            }

            if(score < 15 || tutHandler.mainTut == 0)
            {
                am = 0;
            }

            if (!destructionMode && !miningMode && 
                (!shootingPipehandler.shootingPipesActive && shootingPipehandler.endComplete))
            {
                scrollSpeed = Mathf.Clamp(scrollSpeed + am, defaultScrollSpeed, 400);
            }

        } else if(add == -1)
        { //subtrahieren
            if ((ulong)Mathf.Abs(score) >= this.score)
            {
                SetScore((ulong)0);
            }
            else
            {
                ulong a = this.score;

                internalScoreCount = Mathf.Clamp(internalScoreCount - score, 0, 9999);

                long final = (long)a - score;

                SetScore((ulong)final);
            }
        }

        scoreText.GetComponent<TextMeshProUGUI>().text = this.score.ToString();
        scoreText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = this.score.ToString();

        if(effect > 0)
        {
            Color orig = scoreText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;

            if(effect > 2)
            {
                blusEffectCounter++;
                if(blusEffectCounter > 2)
                {
                    blusEffectCounter = 0;
                }

                blusEffects[blusEffectCounter].transform.position = FF_PlayerData.Instance.lastBlusPosition;

                ParticleSystem.MainModule psmain = blusEffects[blusEffectCounter].GetComponent<ParticleSystem>().main;
                psmain.startColor = orig;

                blusEffects[blusEffectCounter].SetActive(false);

                Timing.RunCoroutine(HandleBlusEffectFade(0.4f));

                IEnumerator<float> HandleBlusEffectFade(float wait)
                {
                    int old = blusEffectCounter;

                    Material mat = blusEffectsMats[old];

                    blusEffectsDissolve[old] = 1;
                    mat.SetFloat("_DissolveAmount", 1);

                    blusEffects[old].SetActive(true);

                    yield return Timing.WaitForSeconds(wait);

                    Tween bTween = DOTween.To(() => blusEffectsDissolve[old],
                        x => blusEffectsDissolve[old] = x, 0, 0.3f);
                    bTween.OnUpdate(() =>
                    {
                        mat.SetFloat("_DissolveAmount", blusEffectsDissolve[old]);
                    });
                }

                if (!destructionMode)
                {
                    Timing.RunCoroutine(CheckBlusCircle(blusEffectCounter));
                }
            }

            playerScoreEffect[scoreEffectCounter].SetActive(false);
            playerScoreEffect[scoreEffectCounter].SetActive(true);

            scoreEffectCounter++;
            if(scoreEffectCounter >= playerScoreEffect.Length)
            {
                scoreEffectCounter = 0;
            }

            defaultVolume.profile.TryGetSettings(out Bloom bloomLayer); 
            defaultVolume.profile.TryGetSettings(out ChromaticAberration chromeLayer);
            defaultVolume.profile.TryGetSettings(out ColorGrading colorGr);

            //bloomLayer.intensity.value = 15f;
            //bloomLayer.color.value = orig;

            chromeLayer.intensity.value = 0.65f;
            colorGr.hueShift.value = -100f;

            DOTween.To(() => chromeLayer.intensity.value, x => chromeLayer.intensity.value = x, 0, 0.4f);
            //DOTween.To(() => bloomLayer.intensity.value, x => bloomLayer.intensity.value = x, 2f, 0.4f);
            //DOTween.To(() => bloomLayer.color.value, x => bloomLayer.color.value = x, Color.white, 0.4f);
            DOTween.To(() => colorGr.hueShift.value, x => colorGr.hueShift.value = x, 0, 0.3f);

            if (blus != null)
            { //pipe zerstörung initalisieren
                GameObject[] tempPipes = blus.transform.parent.GetComponent<BlusData>().assignedPipes;

                blus.transform.parent.GetComponent<BlusData>().DestroyBlus(effect);
                SoundManager.Instance.PlaySound(SoundManager.Sound.Blus);

                for (int i = 0; i < 2; i++)
                {
                    if(tempPipes[i] != null)
                    {
                        PipeData pData = tempPipes[i].GetComponent<PipeData>();

                        if (i == 0)
                        {
                            PipeHolder pH = tempPipes[i].transform.parent.GetComponent<PipeHolder>();

                            if(tutHandler.mainTut == 1)
                            { //achs erst updaten wenn tutorial abgeschlossen
                                if (pH.isMoving)
                                {
                                    achHandler.QueueStep("movingPipe", 1);
                                }
                                else
                                {
                                    if(pH.tunnel)
                                    {
                                        if(pH.lastInTunnel)
                                        {
                                            achHandler.QueueStep("tunnelCompleted", 1);
                                        }
                                    } else
                                    {
                                        achHandler.QueueStep("normalPipe", 1);
                                    }
                                }
                            }

                            pH.StopMove();
                        }

                        if(tutHandler.mainTut == 0)
                        { //im tutorial collision box deaktivieren
                            tempPipes[i].GetComponent<BoxCollider2D>().enabled = false;
                            tempPipes[i].transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;
                        }

                        pData.isChecked = true;
                        if(pData.highscorePipe)
                        {
                            CancelInvoke(nameof(FlashHighscoreObj));
                            highscoreLineObj.SetActive(false);
                        }

                        if(!pData.isTop)
                        { //flak bei zerstörung deaktivieren
                            //tempPipes[i].transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                            tempPipes[i].transform.GetChild(0).GetChild(0).GetComponent<Flakhandler>().enabled = false;
                        }

                        if (OptionHandler.enhancedPipeDestruction == 0 || battleRoyale || !nextCompleteDestruction)
                        { //Aufleuchten
                            if (OptionHandler.lightEnabled == 1)
                            {
                                //pData.StartFlash(1f); Überarbeiten
                            }
                        }
                        else
                        { //"Zerstörung"
                            if(!pData.flakEnabled)
                            { //nur obere pipe zerstören wenn keine flak da ist & kein tunnel
                                if (!FF_PlayerData.Instance.IsStaminaLow() && !pData.pHolder.tunnel)
                                {
                                    pData.StartDestruction(0.2f, i);
                                }
                            }
                        }

                        CheckDestructablePipes();
                    }
                }

                if(nextCompleteDestruction)
                {
                    nextCompleteDestruction = false;
                }
            }

            bool scaleOK = false;

            if(scoreScaleTween == null)
            {
                scaleOK = true;
            } else if(!scoreScaleTween.active)
            {
                scaleOK = true;
            }

            if(!scaleOK)
            { //tween läuft noch
                scoreScaleTween.Kill();
            }

            scoreText.transform.localScale = new Vector3(4f, 4f, 4f);
            scoreScaleTween = scoreText.transform.DOScale(1, 0.5f);

            bool colorOk = false;

            if(scoreColorTween == null)
            {
                colorOk = true;
            } else if(!scoreColorTween.active)
            {
                colorOk = true;
            }

            if(!colorOk)
            {
                scoreColorTween.Kill();
            }

            SetScoreColor(Color.white);
            scoreColorTween = scoreText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOColor(orig, 0.5f);
        }
    }

    private void CheckDestructablePipes()
    { //prüft ob noch unzerstörte sichtbar sind
        bool ok = false;
        
        for(int i = 0; i < pipes.Count; i++)
        {
            if(pipes[i].GetComponent<PipeData>().destructable)
            {
                ok = true;
                break;
            }
        }

        destructablePipeOnScreen = ok;
    }

    public void StartCameraShake(float duration = 0.2f, float shakeAmount = 5)
    {
        if (!gameActive || OptionHandler.destructionTransition) return;

        if (OptionHandler.cameraShake == 1 &&
            !cameraShake.enabled)
        {
            cameraShake.GetComponent<CameraShake>().defaultShakeDuration = duration;
            cameraShake.GetComponent<CameraShake>().shakeAmount = shakeAmount;
            cameraShake.enabled = true;

            /*StartCoroutine(ResetTimePause());
            Time.timeScale = 0;*/

            //Invoke("ResetTimePause", 0.02f);
        }
    }

    private IEnumerator ResetTimePause()
    {
        //wartet 20ms
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(0.02f));

        Time.timeScale = 1;

        yield return null;
    }

    private void SetScoreColor(Color c)
    {
        scoreText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = c;
        currentColor = c;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void StartZoomOnBoss(Vector3 pos, float zoomInTime, float waitTime)
    {
        Timing.RunCoroutine(ZoomOnBoss(pos, zoomInTime, waitTime));
    }

    private IEnumerator<float> ZoomOnBoss(Vector3 pos, float zoomInTime, float waitTime)
    {
        //reinzoomen
        player.GetComponent<Rigidbody2D>().simulated = false;

        bool circle = player.GetComponent<CircleCollider2D>().enabled;
        bool box = player.GetComponent<BoxCollider2D>().enabled;

        player.GetComponent<CircleCollider2D>().enabled = false;
        player.GetComponent<BoxCollider2D>().enabled = false; ;

        DOTween.To(() => cameraObj.GetComponent<Camera>().orthographicSize,
            x => cameraObj.GetComponent<Camera>().orthographicSize = x, 300, zoomInTime);

        pos.z = -500;

        pos.x = Mathf.Clamp(pos.x, -572, -192);

        if (pos.y > 1128)
        {
            pos.y = 1128;
        }
        else if (pos.y < 452)
        {
            if (!miningMode)
            {
                pos.y = 452;
            }
        }

        cameraObj.transform.DOMove(pos, zoomInTime);

        yield return Timing.WaitForSeconds(zoomInTime);

        if(shootingPipehandler.shootingPipesActive)
        {
            bossWarning.SetActive(true);

            pos.z = 0;
            bossWarning.transform.position = pos;

            for(int i = 2; i < bossWarning.transform.childCount; i++)
            {
                bossWarning.transform.GetChild(i).gameObject.SetActive(false);
            }

            bossWarning.transform.GetChild(1).gameObject.SetActive(true);

            Vector3 lineEndPos = Vector3.zero;

            for(int i = 0; i < pipes.Count; i++)
            { //erste pipe mit flak finden
                if(pipes[i].GetComponent<PipeData>().flakEnabled)
                {
                    lineEndPos = pipes[i].transform.GetChild(0).GetChild(0).position;
                    break;
                }
            }

            lineEndPos.x -= 75;
            lineEndPos.z = 100;

            bossWarning.transform.GetChild(1).GetChild(0).
                GetComponent<LineRenderer>().SetPosition(0,
                bossWarning.transform.GetChild(1).GetChild(0).position);

            bossWarning.transform.GetChild(1).GetChild(0).
                GetComponent<LineRenderer>().SetPosition(1, lineEndPos);
        }

        //warten dass waittime vorbei ist

        yield return Timing.WaitForSeconds(waitTime);

        bossWarning.SetActive(false);

        //rauszoomen

        DOTween.To(() => cameraObj.GetComponent<Camera>().orthographicSize,
            x => cameraObj.GetComponent<Camera>().orthographicSize = x, OptionHandler.defaultOrthoSize, zoomInTime);

        cameraObj.transform.DOMove(OptionHandler.defaultCameraPos, zoomInTime);

        if(shootingPipehandler.shootingPipesActive)
        {
            for(int i = 0; i < pipes.Count; i++)
            { //pipes nach rechts
                pipes[i].transform.DOMoveX(pipes[i].transform.position.x + 300, zoomInTime * 0.95f);
            }

            for(int i = 0; i < otherObjs.Count; i++)
            {
                otherObjs[i].transform.DOMoveX(otherObjs[i].transform.position.x + 300, zoomInTime * 0.95f);
            }
        }

        yield return Timing.WaitForSeconds(zoomInTime);

        //zoomout complete

        player.GetComponent<CircleCollider2D>().enabled = circle;
        player.GetComponent<BoxCollider2D>().enabled = box;

        player.GetComponent<Rigidbody2D>().simulated = true;

        if(shootingPipehandler.shootingPipesActive)
        {
            shootingPipehandler.ZoomComplete();
        }
    }

    private IEnumerator<float> SpawnPipesWait(float wait, bool empty = false, bool moveAllowed = true)
    {
        yield return Timing.WaitForSeconds(wait);

        SpawnPipes(empty, moveAllowed);
    }

    public void SpawnPipes(bool empty = false, bool moveAllowed = true, 
        float overrideY = 9999, bool spawnClose = false, bool overrideDistance = false,
        bool overrideCoin = false)
    {
        if(!pipeSpawnAllowed)
        {
            return;
        }

        if(shootingPipehandler.shootingPipesActive || !shootingPipehandler.endComplete)
        {
            moveAllowed = false;
        }

        Pipe selectedPipe = ShopHandler.Instance.GetCurrentPipe();

        float lastX = 298;

        if(pipes.Count > 0 && tutHandler.mainTut != 0)
        {
            lastX = pipes[pipes.Count - 1].transform.position.x + 500;
        }

        if(spawnClose)
        {
            lastX = pipes[pipes.Count - 1].transform.position.x + 93.3f;
        }

        GameObject pipeHolder = objectPooler.SpawnFromPool("Pipe",
            new Vector3(1000, 0), Quaternion.identity);

        pipeHolder.GetComponent<PipeHolder>().ResetPH();

        if(nextPipeTunnel)
        {
            nextPipeTunnel = false;
            pipeHolder.GetComponent<PipeHolder>().tunnel = true;
        }

        pipeHolder.transform.SetParent(pipeParent);

        GameObject pipeTop = pipeHolder.transform.GetChild(0).gameObject;
        pipeTop.SetActive(true);

        GameObject pipeBottom = pipeHolder.transform.GetChild(1).gameObject;
        pipeBottom.SetActive(true);

        pipeTop.GetComponent<PipeData>().ResetPipe(true);

        bool flakActive =
            shootingPipehandler.shootingPipesActive;

        if(flakActive && shootingPipehandler.endComplete)
        {
            flakActive = false;
        }

        if(empty || overrideDistance)
        { //im tunnel keine flaks auf pipes spawnen
            flakActive = false;
        }

        if(flakActive)
        {
            if(!shootingPipehandler.firstPipeSpawned)
            { //das hier ist die erste pipe -> zuweisung
                shootingPipehandler.FirstPipeSpawn(pipeBottom);
            }
        }

        pipeBottom.GetComponent<PipeData>().ResetPipe(false, false, false, flakActive);

        float minY = minPipeY, maxY = maxPipeY;

        float yPos = 565 + (25 * Random.Range(0, 20));

        if(overrideY < 9998)
        {
            yPos = overrideY;
        }

        pipeHolder.GetComponent<PipeHolder>().SetEmpty(yPos, empty);

        float xPos = lastX;

        int abstand; /*= 150;//Random.Range(130, 160);

        //Abstand variiert je nach Schwierigkeit
        if(OptionHandler.GetDifficulty() == 0)
        {
            abstand = 175;//Random.Range(145, 185);
        } else if(OptionHandler.GetDifficulty() == 2)
        {
            abstand = 125;//Random.Range(115, 135);
        }*/

        int maxChance = 4;

        if(Random.Range(0, maxChance) == 0 && score > 80)
        {
            abstand = 175;
        } else
        {
            abstand = 200;
        }

        if(score < 30)
        {
            abstand = 275;
        } else if(score < 50)
        {
            abstand = 225;
        }

        if (overrideDistance) {
            if(score < 100)
            {
                abstand = 250;
            } else if(score < 130)
            {
                abstand = 225;
            } else
            {
                abstand = 200;
            }
        } else if(flakActive)
        {
            abstand = 350;
        }

        GameObject middleObj = pipeHolder.transform.GetChild(3).gameObject;
        middleObj.GetComponent<SpriteRenderer>().size = new Vector2(1,  2 + (abstand / 75f));
        middleObj.GetComponent<SpriteRenderer>().color = ShopHandler.Instance.pipeColor;

        middleObj.GetComponent<PipeMiddleHandler>().abstand = abstand;

        pipeTop.GetComponent<PipeData>().middleObj = middleObj;

        pipeHolder.transform.position = new Vector3(xPos, yPos);
        pipeTop.transform.position = new Vector3(xPos, yPos + abstand);
        pipeBottom.transform.position = new Vector3(xPos, yPos - abstand);

        const float pipeSize = 1200;

        float newTopY = pipeTop.transform.position.y + (pipeSize / 2);
        float newBottomY = pipeBottom.transform.position.y - (pipeSize / 2);

        pipeTop.GetComponent<PipeData>().abstand = abstand;
        pipeTop.GetComponent<PipeData>().thisPipe = selectedPipe;

        pipeTop.transform.localScale = new Vector3(75, 75, 75);
        pipeTop.GetComponent<SpriteRenderer>().size = new Vector2(1, pipeSize / 75);

        pipeTop.GetComponent<SpriteRenderer>().color = ShopHandler.Instance.pipeColor;
        pipeTop.GetComponent<SpriteRenderer>().sprite = 
            selectedPipe.sprite[Random.Range(0, selectedPipe.sprite.Length)];
        pipeTop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite =
            selectedPipe.endSprite[Random.Range(0, selectedPipe.endSprite.Length)];
        pipeTop.transform.GetChild(0).GetComponent<SpriteRenderer>().color =
            ShopHandler.Instance.pipeColor;

        pipeTop.transform.position = new Vector3(xPos, newTopY);

        pipeBottom.GetComponent<PipeData>().abstand = abstand;
        pipeBottom.GetComponent<PipeData>().thisPipe = selectedPipe;

        pipeBottom.transform.localScale = new Vector3(75, 75, 75);
        pipeBottom.GetComponent<SpriteRenderer>().size = new Vector2(1, pipeSize / 75);

        pipeBottom.GetComponent<SpriteRenderer>().color = ShopHandler.Instance.pipeColor;
        pipeBottom.GetComponent<SpriteRenderer>().sprite = 
            selectedPipe.sprite[Random.Range(0, selectedPipe.sprite.Length)];
        pipeBottom.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite =
            selectedPipe.endSprite[Random.Range(0, selectedPipe.endSprite.Length)];
        pipeBottom.transform.GetChild(0).GetComponent<SpriteRenderer>().color =
            ShopHandler.Instance.pipeColor;

        pipeBottom.transform.position = new Vector3(xPos, newBottomY);

        pipeTop.transform.GetChild(0).position = new Vector3(xPos, yPos + abstand - 37.5f);// + 37.5f);
        pipeBottom.transform.GetChild(0).position = new Vector3(xPos, yPos - abstand + 37.5f);// - 37.5f);

        //BoxCollider2D pTB2D = pipeTop.GetComponent<BoxCollider2D>();
        //BoxCollider2D pBB2D = pipeBottom.GetComponent<BoxCollider2D>();

        //pTB2D.size = new Vector2(75, topDiff);
        //pTB2D.offset = new Vector2(0, topDiff / 2);

        //pBB2D.size = new Vector2(75, bottomDiff);
        //pBB2D.offset = new Vector2(0, -(bottomDiff / 2));

        float maxYDiff = 50;

        if(abstand == 200)
        {
            maxYDiff = 65;
        }

        //spawn moving pipes
        bool ok = false, destOk = false;
        float speed = 100;

        if(score < 15)
        {
            ok = false;
        } else if(score < 30)
        {
            if(Random.Range(0, 7) == 0)
            {
                ok = true;
            }

            speed = 75;
        } else if(score < 70)
        {
            if(Random.Range(0, 5) == 0)
            {
                ok = true;
            }

            speed = 100;
        } else
        {
            if(Random.Range(0, 2) == 0)
            {
                ok = true;
            }

            speed = 125;
        }

        #region oldMove
        /*switch (OptionHandler.GetDifficulty())
        {
            case 0: //leicht

                if (Random.Range(0, 10) == 0)
                {
                    speed = 75;
                    ok = true;
                }

                if ((ok && Random.Range(0, 12) == 0) ||
                    (!ok && Random.Range(0, 10) == 0))
                {
                    destOk = true;
                }

                break;
            case 1: //mittel

                if (Random.Range(0, 5) == 0)
                {
                    speed = 100;
                    ok = true;
                }

                if ((ok && Random.Range(0, 8) == 0) ||
                    (!ok && Random.Range(0, 6) == 0))
                {
                    destOk = true;
                }

                break;
            case 2: //schwer

                if (Random.Range(0, 2) == 0)
                {
                    speed = 150;
                    ok = true;
                }

                if ((ok && Random.Range(0, 6) == 0) ||
                    (!ok && Random.Range(0, 4) == 0))
                {
                    destOk = true;
                }

                break;
        }*/
        #endregion

        if (!moveAllowed)
        {
            ok = false;
        }

        if (!empty)
        {
            SpawnBlus(new Vector3(xPos, yPos, -1.1f), pipeTop, pipeBottom, maxYDiff, ok, overrideCoin); //temp
        } else
        {
            pipeBottom.GetComponent<PipeData>().emptyPipe = true;
            pipeBottom.GetComponent<PipeData>().isChecked = true;

            pipeTop.GetComponent<PipeData>().emptyPipe = true;
            pipeTop.GetComponent<PipeData>().isChecked = true;
        }

        int pipeType = 0;

        if(!FF_PlayerData.Instance.IsStaminaLow() && !spawnClose)
        {
            //spawn gravestones wenn vorhanden

            int count = scoreHandler.scoreData.Count;

            float lastPipeX = pipeTop.transform.position.x;

            for (int i = 0; i < count; i++)
            {
                if(scoreHandler.scoreData[i].score == score + 1)
                {
                    float lastGraveX = lastPipeX + 123f + Random.Range(0, 50);

                    if(gravestoneObjs.Count > 0)
                    {
                        if (gravestoneObjs[gravestoneObjs.Count - 1].transform.position.x >
                                lastPipeX)
                        {
                            lastGraveX = gravestoneObjs[gravestoneObjs.Count - 1].transform.position.x;

                            lastGraveX += Random.Range(121.5f, 243);
                        }
                    }

                    ScoreHandler.ScoreHolder temp = scoreHandler.scoreData[i];

                    GameObject nG = 
                        SpawnGravestone(lastGraveX, temp.username, temp.gTop, temp.gSide, temp.gBottom);

                    gravestoneObjs.Add(nG);
                }
            }

            if (ok)
            {
                pipeHolder.GetComponent<PipeHolder>().StartMove(minY, maxY, speed);
                pipeType = 1;
            }

            destOk = false;

            if(destOk) {
                middleObj.SetActive(true);
                
                Vector3 pos = middleObj.transform.position;
                pos.x = pipeTop.transform.position.x;

                middleObj.transform.position = pos;

                pipeTop.GetComponent<PipeData>().destructable = true;

                pipeHolder.GetComponent<PipeHolder>().GetAssignedBlus().
                    transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;

                destructablePipeOnScreen = true;
                pipeType = 2;
            }
        }

        if(!empty)
        {
            //pipeHolder.GetComponent<PipeHolder>().GetAssignedBlus().
            //    GetComponent<BlusData>().SetBlusPipeType(pipeType);

            ulong bCount = 0;
            for (int i = 0; i < pipes.Count; i++)
            {
                if (pipes[i].GetComponent<PipeData>().isTop &&
                    !pipes[i].GetComponent<PipeData>().isChecked)
                { //um zu zählen wie viele blus schon gespawnt sind
                    bCount++;
                }
            }

            ulong newScore = score + bCount + 1;
            if (newScore > highscore[ModeManager.currentIndex].highscore[OptionHandler.GetDifficulty()] &&
                highscore[ModeManager.currentIndex].highscore[OptionHandler.GetDifficulty()] != 0 && !highscoreLineShowed)
            { //highscorelinie aktivieren
                pipeTop.GetComponent<PipeData>().highscorePipe = true;
                highscoreLineShowed = true;

                highscoreLineObj.SetActive(true);
                InvokeRepeating(nameof(FlashHighscoreObj), 0f, 0.25f);

                Vector3 hObjPos = highscoreLineObj.transform.position;
                hObjPos.x = pipeTop.transform.position.x + 50f;

                highscoreLineObj.transform.position = hObjPos;
            }
        }


        /*if(Random.Range(0, 65) == 44 && newScore > 30)
        { //spawn coin
            GameObject newCoin = objectPooler.SpawnFromPool("Coin", new Vector3(
                pipeTop.transform.position.x + Random.Range(160, 225), Random.Range(425, 1372), -300),
                Quaternion.identity);

            newCoin.GetComponent<CoinHandler>().ResetCoin();

            coins.Add(newCoin);
        }*/

        pipes.Add(pipeTop);
        pipes.Add(pipeBottom);
    }

    public void EnableDisableGravestones(bool enable)
    {
        for(int i = 0; i < gravestoneObjs.Count; i++)
        {
            gravestoneObjs[i].SetActive(enable);
        }
    }

    private GameObject SpawnGravestone(float xPos, string name, GraveTop top, GraveSide side, GraveBottom bottom)
    {
        float yPos = 278.1f;

        GameObject nG = objectPooler.SpawnFromPool("Gravestone", new Vector3(xPos, yPos, 100), Quaternion.identity);
        nG.GetComponent<GravestoneHandler>().StartGravstone(player, top, side, bottom, scrollSpeed, name);

        return nG;
    }

    public bool SpawnBlus(Vector3 pos, GameObject pipeTop = null, GameObject pipeBot = null, float maxY = 50, bool moving = false,
        bool overrideCoin = false)
    {
        bool modeChangeBlus = false;

        GameObject newBlus = null;

        bool duplicate = true;
        while(duplicate)
        {
            duplicate = false;

            newBlus = objectPooler.SpawnFromPoolCheck("Blus");
            if(otherObjs.Contains(newBlus))
            {
                duplicate = true;
            }
        }

        newBlus = objectPooler.SpawnFromPoolEnd(newBlus, pos, Quaternion.identity);

        newBlus.transform.SetParent(particleParent.transform);

        BlusData bData = newBlus.GetComponent<BlusData>();

        bData.ResetBlus(pipeTop, pipeBot);

        if(pipeTop != null)
        {
            pipeTop.transform.parent.GetComponent<PipeHolder>().SetAssignedBlus(newBlus);
        }

        bool overrideModeChange = false;

        //alt war score % 30 == 0
        if ((internalScoreCount >= 40 && waitingState == -1 && score > 0 &&
            !modeCurrentlyChanged && !destructionMode &&!battleRoyale && !hardcore && !zigZag) || overrideModeChange)
        { //wenn internalscore >= 40 & gerade kein waitingstate & kein destructionmode / hardcore (einbauen)
            if(!splatterHandler.splatterActive &&
                !shootingPipehandler.shootingPipesActive &&
                shootingPipehandler.endComplete)
            {
                internalScoreCount = 0;

                modeCurrentlyChanged = true;
                bData.modeChangeBlus = true;

                newBlus.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.blue;
                bData.lightObj.color = Color.white;

                modeChangeBlus = true;
            }
        }

        bool tut = false;
        if(TutorialHandler.Instance.mainTut == 0)
        { //keine coins im tut spawnen außer bei override
            tut = true;
        }

        if((Random.Range(0, 20) == 0 && !modeChangeBlus && !tut) ||
            overrideCoin)
        { //coin spawnen
            bData.isCoin = true;
            bData.SetSprites(coinSprites);
        } else
        {
            bData.isCoin = false;
            bData.SetSprites(blusSprites);
        }

        if(!moving)
        {
            bData.StartMove(25, maxY);
        }

        newBlus.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = true;

        otherObjs.Add(newBlus);

        return modeChangeBlus;
    }

    public void SpawnCoin(Vector3 pos)
    {
        GameObject newBlus = null;

        bool duplicate = true;
        while (duplicate)
        {
            duplicate = false;

            newBlus = objectPooler.SpawnFromPoolCheck("Blus");
            if (otherObjs.Contains(newBlus))
            {
                duplicate = true;
            }
        }

        newBlus = objectPooler.SpawnFromPoolEnd(newBlus, pos, Quaternion.identity);

        newBlus.transform.SetParent(particleParent.transform);

        BlusData bData = newBlus.GetComponent<BlusData>();

        bData.SpawnCoin(pos, coinSprites);
        //ShopHandler.Instance.UpdateBlus(1, 1, true);

        otherObjs.Add(newBlus);
    }

    public void PlayerDeath()
    {
        state = (int)FF_States.End;

        gameActive = false;
        cameraShake.enabled = false;

        //flash.GetComponent<Image>().DOFade(.75f, 0.2f).SetEase(flashCurve);

        if(hardcore)
        {
            backgroundHandler.GetComponent<BackgroundHandler>().DisableEnableLight(true);
            player.GetComponent<FF_PlayerData>().EnableDisableHardcore(false);
        }

        if(zigZag)
        {
            zigZagHandler.StopZigZag();
        }

        backgroundHandler.GetComponent<BackgroundHandler>().SetScrolling(false);

        CancelInvoke(nameof(HandleRotation));
        pipeSpawnAllowed = false;

        destructionHandler.DisableEnable(false);

        shootingPipehandler.EndShootingPipes();

        if(splatterHandler.splatterActive)
        {
            splatterHandler.EndSplatter(true);
        }

        float anTime = 0.2f;
        float newT = 0.1f;
        float newFD = newT * fixedStep;

        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, newT, anTime);
        DOTween.To(() => Time.fixedDeltaTime, x => Time.fixedDeltaTime = x, newFD, anTime);

        SoundManager.Instance.SetMusicSpeed(0.1f, true, true);

        scoreText.SetActive(false);

        //originalCameraPos = cameraObj.transform.position;
        DOTween.To(() => cameraObj.GetComponent<Camera>().orthographicSize, 
            x => cameraObj.GetComponent<Camera>().orthographicSize = x, 300, anTime);

        for(int i = 0; i < playerScoreEffect.Length; i++)
        {
            playerScoreEffect[i].SetActive(false);
        }

        pauseImage.gameObject.SetActive(false);

        Vector3 pos = player.transform.position;
        pos.z = -500;

        if(pos.x < -572)
        {
            pos.x = -572;
        }

        if(pos.y > 1128)
        {
            pos.y = 1128;
        } else if(pos.y < 452)
        {
            if(!miningMode)
            {
                pos.y = 452;
            }
        }

        cameraObj.transform.DOMove(pos, anTime);

        //cameraObj.GetComponent<PostProcessVolume>().enabled = true;

        deathText.SetActive(true);
        pos.z = deathText.transform.position.z;
        pos.y += 125;
        deathText.transform.position = pos;

        if (!destructionMode && !zigZag && !miningMode && !battleRoyale && !hardcore && score > 0)
        { //Eigenen Grabstein bei Classic spawnen
            ShopHandler temp = ShopHandler.Instance;

            GameObject nG = SpawnGravestone(player.transform.position.x, AccountHandler.Instance.username,
                temp.allGraveTops[temp.GetSelected(CustomizationType.GraveTop)],
                temp.allGraveSides[temp.GetSelected(CustomizationType.GraveSide)],
                temp.allGraveBottoms[temp.GetSelected(CustomizationType.GraveBottom)]);

            Vector3 pPos = nG.transform.position;

            for (int i = 0; i < gravestoneObjs.Count; i++)
            {
                float dist = Vector2.Distance(pPos, gravestoneObjs[i].transform.position);

                if (dist < 121.5f)
                { //zu nah -> würde überlappen -> nach unten verlegen
                    gravestoneObjs[i].GetComponent<GravestoneHandler>().FadeOutGravestone(0.3f);
                    gravestoneObjs[i].transform.DOMoveY(gravestoneObjs[i].transform.position.y - 200, 0.3f);
                }
            }

            pPos.y -= 200;

            nG.transform.DOMoveY(pPos.y + 200, 0.3f);

            gravestoneObjs.Add(nG);
        }

        Invoke(nameof(ResetTime), 0.35f);
        //Invoke("ResetCamera", 0.75f);
        Invoke(nameof(CallMenuDeath), 0.75f);
    }

    private void ResetTime()
    {
        float anTime = 0.15f;

        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, anTime);
        DOTween.To(() => Time.fixedDeltaTime, x => Time.fixedDeltaTime = x, fixedStep, anTime);

        deathText.SetActive(false);

        if(destructionMode)
        {
            OptionHandler.Instance.DestructionReduce();
        }

        //SoundManager.Instance.SetMusicSpeed(1);
        //SoundManager.Instance.PlayMusicFade(MusicID.Menu);

        DOTween.To(() => cameraObj.GetComponent<Camera>().orthographicSize,
            x => cameraObj.GetComponent<Camera>().orthographicSize = x, OptionHandler.defaultOrthoSize, anTime);

        cameraObj.transform.DOMove(OptionHandler.defaultCameraPos, anTime);

        //cameraObj.GetComponent<PostProcessVolume>().enabled = false;
    }

    private void CallMenuDeath()
    {
        //ovenButton.SetActive(true);

        lastScore = score;
        ObscuredPrefs.SetULong("Player_LastScore", lastScore);

        int diff = OptionHandler.GetDifficulty();

        if(score > highscore[ModeManager.currentIndex].highscore[diff])
        {
            highscore[ModeManager.currentIndex].highscore[diff] = score;

            ObscuredPrefs.SetULong("Player_Highscore_" + ModeManager.currentIndex.ToString() + "_" + diff.ToString(), score);
        }

        StartCoroutine(scoreHandler.OpenScoreboard(
            OptionHandler.moveTime, score, highscore[ModeManager.currentIndex].highscore[diff], taps,
            perfectHits, roundCoins));

#if UNITY_ANDROID || UNITY_IOS
        FirebaseAnalytics.SetCurrentScreen("MainMenu", "UnityPlayerActivity");
#endif
        menu.GetComponent<MenuData>().DeathFF(false);
    }

    private void HandleRotation()
    {
        if (!gameActive || zigZag) return;

        player.GetComponent<FF_PlayerData>().HandleRotation();
    }

    private void ResetModeChangedBool()
    {
        modeCurrentlyChanged = false;
    }

    public void ChangeMode()
    {
        player.GetComponent<Rigidbody2D>().simulated = false;

        modeChanging = true;

        //flash.GetComponent<Image>().DOFade(1, 0.25f);

        defaultVolume.profile.TryGetSettings(out LensDistortion lensDisto);

        DOTween.To(() => lensDisto.intensity.value, x => lensDisto.intensity.value = x, -100, 0.25f);
        DOTween.To(() => lensDisto.scale.value, x => lensDisto.scale.value = x, 0.05f, 0.25f);

        Invoke(nameof(ChangeModeFull), 0.251f);
    }

    public void ChangeModeFull()
    {
        switch(gameState)
        {
            case 0:
                int newMode = Random.Range(0, 3);

                switch(newMode)
                {
                    default:
                        StartBossFull();
                        break;
                    case 1:
                        StartSplatter();
                        break;
                    case 2:
                        StartShootingPipes();
                        break;
                }

                //StartGroundFull();
                break;
            case 1: //von ground zu pipe
            case 3: //von boss zu pipe
                StartPipeFull();
                break;
            case 2: //von mine modus zu pipe
                groundHandler.ResetMine();
                StartGame(false);
                break;

        }

        player.transform.rotation = Quaternion.identity;

        //flash.GetComponent<Image>().DOFade(0, 0.25f);

        defaultVolume.profile.TryGetSettings(out LensDistortion lensDisto);

        DOTween.To(() => lensDisto.intensity.value, x => lensDisto.intensity.value = x, 0, 0.25f);
        DOTween.To(() => lensDisto.scale.value, x => lensDisto.scale.value = x, 1, 0.25f);

        Invoke(nameof(ChangeModeOver), 0.251f);
    }
    private void ChangeModeOver()
    {
        if(BossHandler.Instance.GetActive())
        { //bei boss bleibt deaktiviert da zoomIn
            player.GetComponent<Rigidbody2D>().simulated = false;
        } else
        {
            player.GetComponent<Rigidbody2D>().simulated = true;
        }

        modeChanging = false;
    }

    private void StartSplatter()
    {
        for (int i = 0; i < pipes.Count; i++)
        {
            pipes[i].GetComponent<PipeData>().ResetAll();
            pipes[i].SetActive(false);
        }
        pipes.Clear();

        for (int i = 0; i < otherObjs.Count; i++)
        {
            if (!otherObjs[i].GetComponent<BlusData>().modeChangeBlus)
            {
                otherObjs[i].SetActive(false);
            }
        }

        CancelInvoke(nameof(FlashHighscoreObj));
        highscoreLineObj.SetActive(false);

        splatterHandler.StartSplatter();

        SpawnPipes();
    }

    private void StartShootingPipes()
    {
        for (int i = 0; i < pipes.Count; i++)
        {
            pipes[i].GetComponent<PipeData>().ResetAll();
            pipes[i].SetActive(false);
        }
        pipes.Clear();

        for (int i = 0; i < otherObjs.Count; i++)
        {
            if (!otherObjs[i].GetComponent<BlusData>().modeChangeBlus)
            {
                otherObjs[i].SetActive(false);
            }
        }

        CancelInvoke(nameof(FlashHighscoreObj));
        highscoreLineObj.SetActive(false);

        shootingPipehandler.StartShootingPipes();

        SpawnPipes();
    } 

    public void StartBossFull()
    {
        pipeSpawnAllowed = false;

        destructionHandler.DisableEnable(false);

        for (int i = 0; i < pipes.Count; i++)
        {
            pipes[i].GetComponent<PipeData>().ResetAll();
            pipes[i].SetActive(false);
        }
        pipes.Clear();

        for(int i = 0; i < otherObjs.Count; i++)
        {
            if(!otherObjs[i].GetComponent<BlusData>().modeChangeBlus)
            {
                otherObjs[i].SetActive(false);
            }
        }

        CancelInvoke(nameof(FlashHighscoreObj));
        highscoreLineObj.SetActive(false);

        Vector3 newPos = playerStartPos;

        newPos.y = 790;

        player.transform.position = newPos;
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        BossHandler.Instance.StartBoss();
        SoundManager.Instance.PlayMusicFade(MusicID.Boss);

        waitingState = -1;
        gameState = 3;
    }

    public void ChangePhysicsResolution(bool high)
    {
#region newBlusData

        List<GameObject> blusList = new List<GameObject>();

        GameObject newBlus;

        int blusPoolSize = 12;
        for(int i = 0; i < blusPoolSize; i++)
        {
            newBlus = objectPooler.SpawnFromPool("Blus", new Vector3(1000, 0), Quaternion.identity);
            blusList.Add(newBlus);
        }

        int amount = 4;

        if(high)
        {
            amount = 8;
        }

        for(int i = 0; i < blusList.Count; i++)
        {
            blusList[i].SetActive(true);
            blusList[i].GetComponent<BlusData>().GenerateDestroyedParts(amount);
            blusList[i].SetActive(false);
        }

#endregion

        player.GetComponent<FF_PlayerData>().RegeneratePhysics(high);
    }

    IEnumerator HandleAI()
    {
        int len = aiObjs.Count;

        List<GameObject> delList = new List<GameObject>();

        int delLen = 0;

        while(battleRoyale)
        {
            for(int i = 0; i < aiObjs.Count; i++)
            {
                bool ok = aiObjs[i].GetComponent<AIHandler>().HandleAI(scrollSpeed);

                if(!ok)
                { //out of bild
                    delList.Add(aiObjs[i]);
                }
            }

            delLen = delList.Count;

            for(int i = 0; i < delLen; i++)
            {
                aiObjs.Remove(delList[i]);
                delList[i].SetActive(false);
            }

            delList.Clear();

            yield return new WaitForSeconds(0.05f);
        }
    }

    public void D2D_Hit(Vector3 pos, GameObject hitObj)
    {
        GameObject newHit = objectPooler.SpawnFromPool("D2D_HitEffect", pos, Quaternion.identity);
        effects.Add(newHit);

        pos.x += 30;

        D2D_HouseHandler hHandler = hitObj.transform.parent.parent.GetComponent<D2D_HouseHandler>();

        hHandler.wasHit = true;

        if (hHandler.NotFractured() && hHandler.FireSpawnOk())
        {
            GameObject newFire = objectPooler.SpawnFromPool("D2D_FireEffect", pos, Quaternion.identity);
            effects.Add(newFire);

            hHandler.AddFireEffect(newFire); //damit hHandler den effekt deaktivieren kann bei fracture
        }

        SoundManager.Instance.PlaySound(SoundManager.Sound.PipeHit);

        StartCameraShake(0.1f);
    }

    public void DisableCameraShake()
    {
        cameraShake.enabled = false;
    }

    public void AddFlakEffect(Vector3 pos)
    {
        GameObject effect = objectPooler.SpawnFromPool("FlakEffect", pos, Quaternion.identity);

        effects.Add(effect);
    }

    public static bool IsInBounds(Vector2 pos, RectTransform transform)
    {
        bool ok = false;

        if(pos.x > transform.position.x - transform.rect.width / 2 &&
            pos.x < transform.position.x + transform.rect.width / 2)
        {
            if(pos.y > transform.position.y - transform.rect.height / 2 &&
                pos.y < transform.position.y + transform.rect.height / 2)
            {
                ok = true;
            }
        }

        return ok;
    }

    public void PauseGame(bool pause, bool suicide = false)
    {
        if(pause)
        { //pausieren
            gamePaused = true;
            lastTimeScale = Time.timeScale;

            pauseImage.sprite = pauseSprites[1];
            pauseImage.gameObject.SetActive(false);

            ingameMenu.OpenMenu(true, true);

            Time.timeScale = 0;
        } else
        { //fortsetzen
            gamePaused = false;

            pauseImage.sprite = pauseSprites[0];
            pauseImage.gameObject.SetActive(true);

            Time.timeScale = lastTimeScale;

            if(suicide)
            {
                player.GetComponent<FF_PlayerData>().Die(DeathCause.Suicide);
            }
        }
    }

    public void SpawnTunnel(int max = 10, bool start = false)
    {
        if(start)
        {
            nextPipeTunnel = true; //zurückgesetzt in spawnpipe
            inTunnel = true;

            tunnelMax = max * 2;

            tunnelRemaining = max * 2;

            tunnelDir = Random.Range(0, 2);

            SpawnPipes(true, false);
        }

        //Debug.Log("spawnT " + start + " rem: " + tunnelRemaining);

        float lastY = 
            pipes[pipes.Count - 1].transform.parent.GetComponent<PipeHolder>().GetStartY();

        if(tunnelRemaining == tunnelMax / 2)
        { //neuzuweisung der richtung bei hälfte
            tunnelDir = Random.Range(0, 2);
        }

        int i = tunnelMax - tunnelRemaining;

        float newY;

        if (tunnelDir == 0)
        { //hoch
            newY = Random.Range(lastY + 5, lastY + 30);
        }
        else
        { //runter
            newY = Random.Range(lastY - 30, lastY - 5);
        }

        if (newY > maxPipeY)
        {
            newY = maxPipeY;
            tunnelDir = 1;
        }
        else if (newY < minPipeY)
        {
            newY = minPipeY;
            tunnelDir = 0;
        }

        lastY = newY;

        bool empty = true;

        if (i == 0 || i == 5 || i == 10 || i == 15)
        {
            empty = false;
        }

        nextPipeTunnel = true;

        SpawnPipes(empty, false, newY, true, true);

        tunnelRemaining--;
        if(tunnelRemaining <= 0)
        {
            inTunnel = false;
        }

        pipes[pipes.Count - 1].transform.parent.
            GetComponent<PipeHolder>().lastInTunnel = true;
    }

    // Update is called once per frame
    void _MainUpdate()
    {
        float realSpeed = scrollSpeed * Time.deltaTime;

        Touch[] touches = Input.touches;
        bool touchOK = false;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetMouseButton(0))
        {
            touchOK = true;
            holdingDown = true;
        } else
        {
            touchOK = false;
        }
#else
        if (touches.Length > 0)
        {
            if (touches[0].phase >= TouchPhase.Began && touches[0].phase < TouchPhase.Ended)
            {
                touchOK = true;
                holdingDown = true;
            }
        }
#endif

        if (!touchOK)
        {
            if(holdingDown && zigZag)
            {
                Vector3 pos;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                pos = Input.mousePosition;
#else
                pos = touches[0].position;
#endif
                pos.z = 500f;
                pos = mainCamera.ScreenToWorldPoint(pos);

                player.GetComponent<FF_PlayerData>().PlayerFly(pos, zigZag, true);
            }

            holdingDown = false;
            clicked = false;
        }

        int len = gravestoneObjs.Count;

        for (int i = 0; i < len; i++)
        {
            if (gameActive && gameState != 2)
            {
                gravestoneObjs[i].transform.Translate(Vector3.left * realSpeed);

                if(gravestoneObjs[i].transform.position.x < -879)
                {
                    delList.Add(gravestoneObjs[i]);
                }
            }
        }

        len = delList.Count;
        for (int i = 0; i < len; i++)
        {
            gravestoneObjs.Remove(delList[i]);
            delList[i].GetComponent<GravestoneHandler>().StopGravestone();
        }
        delList.Clear();

        len = aiObjs.Count;
        for(int i = 0; i < len; i++)
        {
            GameObject obj = aiObjs[i];
            AIHandler objHandler = obj.GetComponent<AIHandler>();

            if(player.GetComponent<FF_PlayerData>().dead)
            {
                if(!objHandler.IsGroundHit() && !objHandler.IsDead())
                {
                    if(!objHandler.playerDead)
                    {
                        objHandler.playerDead = true;
                        obj.GetComponent<Rigidbody2D>().gravityScale *= 3;
                    }

                    obj.transform.Translate(Vector3.right * realSpeed, Space.World);
                }
            } else
            {

                if (objHandler.IsGroundHit())
                {
                    obj.transform.Translate(Vector3.left * realSpeed, Space.World);
                }
            }
        }

        len = otherObjs.Count;
        for (int i = 0; i < len; i++)
        {
            BlusData b = otherObjs[i].GetComponent<BlusData>();

            if (gameActive && gameState != 2) {
                //otherObjs[i].transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

                Vector3 pos = otherObjs[i].transform.position;
                pos.x -= realSpeed;

                otherObjs[i].transform.position = pos;
                
                if(pos.x < 21 && b.renderDisabled)
                {
                    b.renderDisabled = false;

                    b.DisableEnbleSprite(true);

                    if(OptionHandler.lightEnabled == 1)
                    {
                        b.DisableEnableLight(true);
                    }
                }

            }

            if(OptionHandler.hardcoreActive)
            {
                b.UpdateHardcoreLight();
            }

            if(b.isDestroyed)
            {
                b.timer -= 1 * Time.deltaTime;
                if (b.timer <= 0)
                {
                    delList.Add(otherObjs[i]);
                }
            }

            /*if(pos.x < -819)
            {
                if(!(b.timer > 0))
                {
                    delList.Add(otherObjs[i]);
                }
            } else
            {
                if (b.isDestroyed)
                {
                    if (b.timer > 0f)
                    {
                        b.timer -= 1 * Time.deltaTime;
                        if (b.timer < 1f && !b.endAnimation)
                        {
                            b.endAnimation = true;
                            b.DoDownscale();
                        }
                    }
                    else
                    {
                        delList.Add(otherObjs[i]);
                    }
                }
            }*/
        }

        len = delList.Count;
        for(int i = 0; i < len; i++)
        {
            otherObjs.Remove(delList[i]);
            delList[i].SetActive(false);
        }
        delList.Clear();

        if (gameActive)
        {
            delList.Clear();

            len = effects.Count;
            for(int i = 0; i < len; i++)
            {
                effects[i].transform.Translate(-realSpeed, 0, 0);

                if(effects[i].transform.position.x < -920)
                {
                    delList.Add(effects[i]);
                }
            }

            len = delList.Count;
            for(int i = 0; i < len; i++)
            {
                effects.Remove(delList[i]);
                delList[i].SetActive(false);
            }
            delList.Clear();

            if(holdingDown && !clicked)
            {
                Vector3 pos;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                pos = Input.mousePosition;
#else
                pos = touches[0].position;
#endif
                pos.z = 500f;
                pos = mainCamera.ScreenToWorldPoint(pos);

                if(IsInBounds(pos, pauseImage.rectTransform))
                { //pause clicked
                    clicked = true;

                    if(!gamePaused)
                    {
                        PauseGame(true);
                    }
                } else if(miningMode)
                {
                    for(int i = 0; i < miningItemParent.childCount; i++)
                    {
                        if(IsInBounds(pos, 
                            miningItemParent.GetChild(i).GetComponent<Image>().rectTransform))
                        {
                            clicked = true;
                            mineHandler.MineItemClicked(i);
                        }
                    }
                }
            }

            if(!modeChanging && !isStarting && !gamePaused)
            {
                if (gameState == 0 || waitingState == 0 || gameState == 3)
                {

                    if (holdingDown && !clicked)
                    {
                        clicked = true;

                        Vector3 pos;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                        pos = Input.mousePosition;
#else
                        pos = touches[0].position;
#endif
                        pos.z = 500f;
                        pos = mainCamera.ScreenToWorldPoint(pos);

                        player.GetComponent<FF_PlayerData>().PlayerFly(pos, zigZag);

                        UpdateTap();

                        float abstandX = 0, abstandY = 20f;

                        GameObject flyCloud = objectPooler.SpawnFromPool("FlyCloud",
                                            new Vector3(player.transform.position.x - abstandX, 
                                            player.transform.position.y - abstandY),
                                            Quaternion.identity);

                        flyCloud.GetComponent<FlyCloudHandler>().StartCloud(new Vector2(-scrollSpeed * 75, Random.Range(-3000, -2000)),
                            flyCloudSprites[Random.Range(0, flyCloudSprites.Length)],
                            Random.Range(-10, 10),
                            0.5f);
                    }

                    if(gameState == 0)
                    {
                        if (player.transform.position.y < 337)
                        { //landung initiieren
                            bool ok = miningMode;//player.GetComponent<FF_PlayerData>().CheckStamina();

                            if (ok)
                            {
                                player.GetComponent<FF_PlayerData>().StartMining();
                                StartMineFull();
                            } else if(tutHandler.mainTut == 0 &&
                                player.GetComponent<Rigidbody2D>().velocity.y <= 0.01f)
                            {
                                player.transform.position = new Vector3(player.transform.position.x,
                                    337, player.transform.position.z);
                                player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                            }
                        }
                    } else if(gameState == 3)
                    {
                        /*if(player.transform.position.y < 300) DEBUG
                        { //out of world -> death
                            player.GetComponent<FF_PlayerData>().Die(DeathCause.OutOfWorld);
                        }*/
                    }
                }
            }

            groundHandler.GroundUpdate(gameState, scrollSpeed);

            /*if(score % 15 == 0 && waitingState == -1 && score > 0 && !modeCurrentlyChanged)
            { //wenn score glatt durch 15 teilbar & gerade kein waitingstate

            }

            if(waitingState == 1 && gameState == 0)
            { //switch von main zu platformer
                if (pipes.Count > 0)
                {
                    if (pipes[pipes.Count - 1].transform.position.x <= -500)
                    {
                        StartGroundFull();
                    }
                }
                else
                { //direkt ground start
                    StartGroundFull();
                }
            } else if(waitingState == 0 && gameState == 1)
            { //switch von platformer zu main
                float height = 
                    (FF_PlayerData.Instance.GetGroundHeight() * 90) + 195;
                
                if(player.transform.position.y - height > 90)
                { //player 90 Pixel über aktuellem Boden -> beende ground & starte flug
                    StartPipeFull();
                }
            }*/

            if(gameState != 2)
            {
                len = pipes.Count;
                for (int i = 0; i < len; i++)
                {
                    Vector3 pos = pipes[i].transform.position;

                    pos.x -= realSpeed;

                    PipeData pData = pipes[i].GetComponent<PipeData>();

                    pData.UpdateDestroyedParts();

                    bool del = false;

                    if(pos.x < -795 && !pData.destructionStarted ||
                        pos.x < -1500 && pData.destructionStarted)
                    {
                        del = true;
                        delList.Add(pipes[i]);
                    }

                    if(!del)
                    {
                        if (pos.x < -795 && pData.renderDeactivated == 1)
                        { //Deaktiviert Sprite Renderer out of bounds links
                            pData.renderDeactivated = 0;

                            pipes[i].GetComponent<SpriteRenderer>().enabled = false;
                            pipes[i].GetComponent<BoxCollider2D>().enabled = false;
                        }
                        else if (pos.x < 32 && pData.renderDeactivated == 2)
                        { //Aktiviert Sprite Renderer wenn in bounds rechts
                            pData.renderDeactivated = 1;

                            pipes[i].GetComponent<SpriteRenderer>().enabled = true;
                            pipes[i].transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                        }
                    }

                    pipes[i].transform.position = pos;
                    if(pData.isTop)
                    {
                        if(((pos.x < -14 && inTunnel) || (pos.x < -202 && !inTunnel)) 
                            && (i == len - 2))
                        { //wenn letzte pipe x in liste < -202
                            if(!pData.nextSpawned)
                            {
                                pData.nextSpawned = true;

                                if(tutHandler.mainTut != 0)
                                { //neue pipes nur spawnen wenn tutorial abgeschlossen
                                    if (Random.Range(0, 15) == 0
                                        && !shootingPipehandler.shootingPipesActive
                                        && shootingPipehandler.endComplete
                                        && score > 50 && !inTunnel)
                                    {
                                        SpawnTunnel(10, true);
                                    }
                                    else
                                    {
                                        if(inTunnel)
                                        {
                                            SpawnTunnel(10, false);
                                        } else
                                        {
                                            SpawnPipes();
                                        }
                                    }
                                }

                                //SpawnPipes(false);
                            }
                        } 

                        if (pData.highscorePipe)
                        {
                            Vector3 hPos = highscoreLineObj.transform.position;
                            hPos.x = pos.x + 50;

                            highscoreLineObj.transform.position = hPos;

                            if (del)
                            {
                                CancelInvoke(nameof(FlashHighscoreObj));
                                highscoreLineObj.SetActive(false);
                            }
                        }

                        if(pData.destructable)
                        {
                            Vector3 newMiddlePos = 
                                pipes[i].transform.parent.GetComponent<PipeHolder>().GetAssignedBlus().transform.position;

                            newMiddlePos.x += pipes[i].transform.parent.GetChild(3).GetComponent<PipeMiddleHandler>().xOffset;

                            pData.middleObj.transform.position = newMiddlePos;
                        }
                    }
                }

                for (int i = 0; i < delList.Count; i++)
                {
                    pipes.Remove(delList[i]);
                    delList[i].transform.parent.gameObject.SetActive(false);
                }

                delList.Clear();
                len = coins.Count;

                for(int i = 0; i < len; i++)
                {
                    bool ok = coins[i].GetComponent<CoinHandler>().UpdateCoin(scrollSpeed);

                    if(!ok)
                    {
                        delList.Add(coins[i]);
                    }
                }

                for (int i = 0; i < delList.Count; i++)
                {
                    coins.Remove(delList[i]);
                    delList[i].SetActive(false);
                }
            }
            delList.Clear();

            destructionHandler.HandleScrolling(scrollSpeed);
        }
    }
}
