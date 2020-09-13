using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class MenuData : MonoBehaviour
{
    public Canvas menuCanvas;
    public GameObject inputName, singleplayerParent, multiplayerParent, colorParent, benitrat0r,
        benitrat0rDigits, options, resButton, blusObj, notifications, ffHandler, ffGoBtn, ffOptions,
        physicsButton, shopButton, title, highscoreButton, playerLvlText;

    public bool locked = false, buttonsMoving = false;
    public AnimationCurve curve;
    public static int mode = 0;
    public static int state = 0;

    public static MenuData Instance;

    private List<GameObject> scalingButtons = new List<GameObject>();
    private GameObject nextButton = null;

    private Vector3[] goPositions = new Vector3[24];
    private Vector3[] optionPositions = new Vector3[24];
    private Vector3[] shopPositions = new Vector3[24];
    private Vector3[] highscorePositions = new Vector3[24];

    public static float scaleTime = 0.25f;

    [SerializeField]
    private Transform deadMenuParent = null;
    [SerializeField]
    private Sprite[] menuSprites = null;
    [SerializeField]
    private Vector3[] defaultPositions = null;
    [SerializeField]
    private GraphicRaycaster menuRaycaster = null, ffRaycaster = null;
    [SerializeField]
    private GameObject eventSystem = null;

    public enum Modes
    {
        versus = 0,
        battleRoyale = 1,
        sort = 2,
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdatePlayerLevelText();

        ffHandler.SetActive(true);

        for(int i = 0; i < 24; i++)
        {
            goPositions[i] = deadMenuParent.GetChild(0).GetChild(i).position;
            shopPositions[i] = deadMenuParent.GetChild(1).GetChild(i).position;
            highscorePositions[i] = deadMenuParent.GetChild(2).GetChild(i).position;
            optionPositions[i] = deadMenuParent.GetChild(3).GetChild(i).position;
        }

        StartCoroutine(SetPosition());
    }

    public float GetScaleTime()
    {
        return scaleTime;
    }

    public void UpdatePlayerLevelText()
    {
        playerLvlText.GetComponent<TextMeshProUGUI>().text =
            "Level " + LevelHandler.Instance.GetLVL().ToString();
    }

    public void PushNotificationsClicked()
    {
#if UNITY_ANDROID
        int wants = PlayerPrefs.GetInt("WantsMessages", 1);

        string message = "";

        if(wants == 1)
        {
            wants = 0;
            message = "Neim";
            //Firebase.Analytics.FirebaseAnalytics.SetUserProperty("WantsMessages", "0");
        } else
        {
            wants = 1;
            message = "Ja";
            //Firebase.Analytics.FirebaseAnalytics.SetUserProperty("WantsMessages", "1");
        }

        PlayerPrefs.SetInt("WantsMessages", wants);
        notifications.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
#endif
    }

    private void OnApplicationQuit()
    {
#if UNITY_ANDROID
        //Firebase.Analytics.FirebaseAnalytics.LogEvent("MenuBenis", "Benis", menuBenisCount);
#endif
    }

    public void Benitrat0rClicked()
    {
        benitrat0r.SetActive(true);
        Camera.main.transform.DOMove(new Vector3(-381, 1943, -10), 0.75f);

        state = 4;

        Invoke("CheckBenitrat0rMusic", 0.25f);
    }

    public void FFClicked()
    { //inaktiv
        ffHandler.SetActive(true);

        //Camera.main.transform.DOMove(new Vector3(-1122, 642, -10), 0.75f);
    }

    public void DoScaleDown()
    {
        title.transform.DOScale(0, scaleTime);
        ffGoBtn.transform.DOScale(0, scaleTime);
        ffOptions.transform.DOScale(0, scaleTime);
        shopButton.transform.DOScale(0, scaleTime);
        highscoreButton.transform.DOScale(0, scaleTime);
        playerLvlText.transform.DOScale(0, scaleTime);
    }

    public IEnumerator DoMoveAway()
    {
        buttonsMoving = true;

        menuCanvas.sortingOrder = 10;

        ffGoBtn.transform.DOMoveX(-1131, scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.1f);

        shopButton.transform.DOMoveX(369, scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.1f);

        highscoreButton.transform.DOMoveX(-1131, scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.1f);

        ffOptions.transform.DOMoveX(369, scaleTime, true).SetEase(Ease.InBack);

        StartCoroutine(ResetMovingState(scaleTime + 0.01f, true));
    }

    /*public IEnumerator DoMoveDown()
    {
        menuCanvas.sortingOrder = 10;
    }*/

    public IEnumerator DoMoveIn()
    {
        buttonsMoving = true;

        OptionHandler.Instance.MenuZoomIn();

        ffGoBtn.SetActive(true);
        shopButton.SetActive(true);
        highscoreButton.SetActive(true);
        ffOptions.SetActive(true);

        menuCanvas.sortingOrder = 11;

        ffOptions.transform.DOMoveX(-321, scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.1f);

        highscoreButton.transform.DOMoveX(-321, scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.1f);

        shopButton.transform.DOMoveX(-321, scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.1f);

        ffGoBtn.transform.DOMoveX(-321, scaleTime, true).SetEase(Ease.OutBack);

        StartCoroutine(ResetMovingState(scaleTime + 0.01f));
    }

    private IEnumerator ResetMovingState(float waitTime, bool disable = false)
    {
        buttonsMoving = true;

        yield return new WaitForSeconds(waitTime);

        if(disable)
        {
            ffGoBtn.SetActive(false);
            shopButton.SetActive(false);
            highscoreButton.SetActive(false);
            ffOptions.SetActive(false);
        }

        buttonsMoving = false;
    }

    public IEnumerator SetPosition()
    {
        buttonsMoving = true;

        scalingButtons.Clear();

        ffGoBtn.SetActive(true);
        shopButton.SetActive(true);
        highscoreButton.SetActive(true);
        ffOptions.SetActive(true);

        ffGoBtn.transform.position = new Vector3(-321, 2000);
        ffGoBtn.transform.localScale = Vector3.one;

        ffOptions.transform.position = new Vector3(-321, 2000);
        ffOptions.transform.localScale = Vector3.one;

        shopButton.transform.position = new Vector3(-321, 2000);
        shopButton.transform.localScale = Vector3.one;

        highscoreButton.transform.position = new Vector3(-321, 2000);
        highscoreButton.transform.localScale = Vector3.one;

        bool snapping = true;

        ffOptions.transform.DOMove(defaultPositions[3], scaleTime * 3, snapping).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.1f);

        highscoreButton.transform.DOMove(defaultPositions[2], scaleTime * 3, snapping).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.1f);

        shopButton.transform.DOMove(defaultPositions[1], scaleTime * 3, snapping).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.1f);

        ffGoBtn.transform.DOMove(defaultPositions[0], scaleTime * 3, snapping).SetEase(Ease.OutBack);

        StartCoroutine(ResetMovingState((scaleTime * 3) + 0.01f));
    }

    public void DoScaleUp(bool setPosition = false)
    {
        float mainTime = scaleTime;

        if(setPosition)
        {
            SetPosition();
            mainTime /= 4;
        }

        title.transform.DOScale(1, scaleTime);

        ffGoBtn.transform.DOScale(1, mainTime);
        ffOptions.transform.DOScale(1, mainTime);
        shopButton.transform.DOScale(1, mainTime);

        highscoreButton.transform.DOScale(1, scaleTime);
        playerLvlText.transform.DOScale(1, scaleTime);
    }

    public void ButtonPressed(GameObject button)
    {
        if(!scalingButtons.Contains(button) && !buttonsMoving)
        {
            bool ok = true;

            if (TutorialHandler.Instance.mainTut == 0 &&
                !button.CompareTag("GoBtn"))
            {
                ok = false;
            }

            if(ok)
            {
                scalingButtons.Add(button);
                button.transform.DOScale(0.8f, 0.05f);
            }
        }
    }

    public void ButtonReleased(GameObject button)
    {
        if(scalingButtons.Contains(button))
        {
            scalingButtons.Remove(button);

            bool ok = true;
            bool go = false;

            switch (button.tag)
            {
                case "GoBtn":

                    go = true;
                    if (ModeManager.Instance.GetModeDisplayActive())
                    {
                        ok = false;
                    }

                    ModeManager.Instance.ModeGoClicked();
                    break;
                case "OptionBtn":
                    OptionHandler.Instance.ShowOptions();
                    break;
                case "ShopBtn":
                    ShopHandler.Instance.OpenShop();
                    break;
                case "ScoreBtn":
                    ScoreHandler.Instance.ShowHighscores();
                    break;
            }

            if(ok)
            {
                button.transform.DOScale(1f, 0.05f);

                if(!go)
                {
                    OptionHandler.Instance.MenuZoomOut();
                }
            }
        }
    }

    public void StartFF(bool again = false)
    {
        ffRaycaster.enabled = false;
        menuRaycaster.enabled = false;
        eventSystem.SetActive(false);

        if(!again)
        {
            //OptionHandler.Instance.UpdateResolution(1);
            PlayDestruction();
        } else
        {
            ffGoBtn.SetActive(false);
            shopButton.SetActive(false);
            highscoreButton.SetActive(false);
            ffOptions.SetActive(false);
        }

        if(!again)
        {
            OptionHandler.Instance.MenuZoomOut();
            ffHandler.GetComponent<FlatterFogelHandler>().StartGame(true, true);
        } else
        {
            ffHandler.GetComponent<FlatterFogelHandler>().StartGame(true);
        }

        StartCoroutine(DisableFF(scaleTime + 0.025f, false));
    }

    public void PlayDestruction()
    {
        ffGoBtn.SetActive(false);
        shopButton.SetActive(false);
        highscoreButton.SetActive(false);
        ffOptions.SetActive(false);

        for (int i = 0; i < deadMenuParent.childCount; i++)
        {
            deadMenuParent.GetChild(i).gameObject.SetActive(true);
        }

        for (int i = 0; i < 24; i++)
        {
            for(int a = 0; a < 4; a++)
            {
                deadMenuParent.GetChild(a).GetChild(i).localScale = Vector3.one;

                deadMenuParent.GetChild(a).GetChild(i).rotation = Quaternion.identity;
                deadMenuParent.GetChild(a).GetChild(i).GetComponent<Rigidbody2D>().velocity = Vector2.zero;

                switch (a)
                {
                    case 0: //go
                        deadMenuParent.GetChild(a).GetChild(i).position = goPositions[i];
                        deadMenuParent.GetChild(a).GetChild(i).GetComponent<Rigidbody2D>().
                            AddExplosionForce(1000, deadMenuParent.GetChild(a).position, 300);
                        break;
                    case 1: //shop
                        deadMenuParent.GetChild(a).GetChild(i).position = shopPositions[i];
                        break;
                    case 2: //highscores
                        deadMenuParent.GetChild(a).GetChild(i).position = highscorePositions[i];
                        break;
                    case 3: //options
                        deadMenuParent.GetChild(a).GetChild(i).position = optionPositions[i];
                        break;
                }
            }
        }

        StartCoroutine(ScaleDown(2f));
    }

    private IEnumerator ScaleDown(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        Vector3 newScale = Vector3.one;

        DOTween.To(() => newScale, x => newScale = x, Vector3.zero, 0.25f);

        while(newScale.x > 0.01f)
        {
            for (int i = 0; i < 24; i++)
            {
                for (int a = 0; a < 4; a++)
                {
                    deadMenuParent.GetChild(a).GetChild(i).localScale =
                        newScale;
                }
            }
            yield return new WaitForSeconds(0.01f);
        }

        for (int i = 0; i < 4; i++)
        {
            deadMenuParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    private IEnumerator DisableFF(float waitTime, bool all = true)
    {
        yield return new WaitForSeconds(waitTime);

        title.SetActive(false);
        if(all)
        {
            ffGoBtn.SetActive(false);
            ffOptions.SetActive(false);
            shopButton.SetActive(false);
            highscoreButton.SetActive(false);
        }
        playerLvlText.SetActive(false);
    }

    public void DeathFF(bool activate = true)
    {
        ffRaycaster.enabled = true;
        menuRaycaster.enabled = true;

        if(activate)
        {
            //OptionHandler.Instance.UpdateResolution(0);

            OptionHandler.Instance.MenuZoomIn();

            shopButton.SetActive(true);
            ffGoBtn.SetActive(true);
            ffOptions.SetActive(true);
            highscoreButton.SetActive(true);
            playerLvlText.SetActive(true);
            StartCoroutine(SetPosition());
        }

        eventSystem.SetActive(true);
    }

    private void CheckBenitrat0rMusic()
    {

        benitrat0r.GetComponent<Benitrat0r>().sound.GetComponent<AudioScript>().BeginPlayAmbient();
    }

    public void OptionsClicked()
    {
        options.SetActive(true);
        state = 1;
        Camera.main.transform.DOMove(new Vector3(-381, -665, -10), 0.5f);
    }

    public void OptionsBackClicked()
    {
        state = 0;
        Camera.main.transform.DOMove(new Vector3(-381, 790, -500), 0.5f);
        Invoke("DisableOptions", 0.5f);
    }

    private void DisableOptions()
    {
        options.SetActive(false);
    }

    public void Benitrat0rBack()
    {
        state = 0;
        Camera.main.transform.DOMove(new Vector3(-381, 642, -10), 0.5f);
        Invoke("DisableBenitrat0r", 0.5f);
    }

    private void DisableBenitrat0r()
    {
        benitrat0r.SetActive(false);
    }
}
