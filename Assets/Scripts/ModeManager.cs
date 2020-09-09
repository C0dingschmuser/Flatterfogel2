using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using UnityEngine.Video;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum GameModes
{
    Classic,
    Hardcore,
    Royale,
    Mining,
    Destruction
}

public class ModeManager : MonoBehaviour
{
    public Sprite[] modeSprites;
    public MainMode[] mainModes;
    public static ObscuredInt currentIndex = 0;

    public static ModeManager Instance;

    public RenderTexture[] renderTextures;
    public RawImage rawImage;

    public GameObject goBtn;

    public SwipeDetector swDetector;
    public TextMeshProUGUI modeText, modeBGText, oldModeText, oldModeBGText, 
        descriptionText, oldDescriptionText;
    public GameObject[] displayParents;
    public VideoClip[] modeClips;
    public Image previewImage, previewOldImage;
    public VideoPlayer normalPlayer;
    public Vector3[] previewPositions;
    public Vector3 startPosition, endPosition;

    public LocalizedString unlockText;
    private string unlockTextString;

    public static Color modeColor;

    private ObscuredInt oldIndex = 0;
    private bool changeRunning = false, modeDisplayActive = false, moveDone = false,
        prepareDone = false;
    public bool IsMDisplayActive { get => modeDisplayActive; }

    private Coroutine modeAnimation = null;

    private int modeFrame = 0;

    private void Awake()
    {
        Instance = this;

        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;

        currentIndex = ObscuredPrefs.GetInt("Player_SelectedMode", 0);

        for(int i = 0; i < mainModes.Length; i++)
        {
            bool unlocked = ObscuredPrefs.GetBool("Player_Mode_" + mainModes[i].identifier, false);
            if(mainModes[i].identifier.Equals("classic"))
            { //classic standardmäßig unlocked
                unlocked = true;
            }

            mainModes[i].unlocked = unlocked;
        }

        Invoke(nameof(DisableSwipe), 0.25f);
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            SaveModes();
        }
    }

    private void OnApplicationQuit()
    {
        SaveModes();
    }

    private void SaveModes()
    {
        for(int i = 0; i < mainModes.Length; i++)
        {
            ObscuredPrefs.SetBool("Player_Mode_" + mainModes[i].identifier, mainModes[i].unlocked);
        }
    }

    public void UnlockMode(string identifier)
    {
        for(int i = 0; i < mainModes.Length; i++)
        {
            if(mainModes[i].identifier.Contains(identifier))
            {
                mainModes[i].unlocked = true;
                break;
            }
        }

        SaveModes();
    }

    public void StartLoadLocalization()
    {
        StartCoroutine(LoadLocalization());
    }

    private IEnumerator LoadLocalization()
    {
        AsyncOperationHandle handle;

        for (int i = 0; i < mainModes.Length; i++)
        {
            if(mainModes[i].modeNameLocalized != null)
            {
                yield return handle = mainModes[i].modeNameLocalized.GetLocalizedString();
                mainModes[i].modeName = (string)handle.Result;
            }
        }

        yield return handle = unlockText.GetLocalizedString();
        unlockTextString = (string)handle.Result;

        SetModeText(mainModes[currentIndex].modeName, modeColor);
    }

    private void DisableSwipe()
    { //Verspätet aufrufen 
        swDetector.enabled = false;
        SetIndexData(true);
    }

    public bool GetModeDisplayActive()
    {
        return modeDisplayActive;
    }

    private void CheckUnlocked()
    {
        for(int i = 0; i < mainModes.Length; i++)
        {
            if(LevelHandler.Instance.GetLVL() >= mainModes[i].unlockLevel)
            {
                mainModes[i].unlocked = true;
            }
        }
        SaveModes();

        if (mainModes[currentIndex].unlocked)
        {
            goBtn.GetComponent<Image>().raycastTarget = true;
            goBtn.GetComponent<Button>().interactable = true;
            displayParents[0].transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            goBtn.GetComponent<Image>().raycastTarget = false;
            goBtn.GetComponent<Button>().interactable = false;

            Transform lockParent = displayParents[0].transform.GetChild(1).GetChild(1);
            string temp = unlockTextString + mainModes[currentIndex].unlockLevel.ToString();

            lockParent.gameObject.SetActive(true);
            lockParent.GetChild(1).GetComponent<TextMeshProUGUI>().text = temp;
            lockParent.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = temp;
        }
    }

    public void ActivateModeChange()
    {
        swDetector.enabled = true;
        modeDisplayActive = true;

        float moveTime = MenuData.Instance.GetScaleTime();

        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).localScale = new Vector3(1, 1, 1);
        transform.GetChild(0).position = startPosition;

        transform.GetChild(0).DOMove(endPosition, moveTime).SetEase(Ease.OutQuad);

        CheckUnlocked();

        normalPlayer.clip = modeClips[currentIndex];
        normalPlayer.Play();

        //modeAnimation = StartCoroutine(HandleModeAnimation());
    }

    public void DisableModeChange()
    {
        swDetector.enabled = false;
        modeDisplayActive = false;

        if (modeAnimation != null)
        {
            StopCoroutine(modeAnimation);
        }

        float moveTime = MenuData.Instance.GetScaleTime();

        transform.GetChild(0).DOMove(startPosition, moveTime).SetEase(Ease.OutQuad);

        goBtn.GetComponent<Image>().raycastTarget = true;
        goBtn.GetComponent<Button>().interactable = true;

        Invoke(nameof(DisableAllChilds), moveTime + 0.01f);
    }

    /*IEnumerator HandleModeAnimation()
    {
        while (true)
        {
            previewImage.sprite = modeSprites[currentIndex].sprites[modeFrame];

            modeFrame++;
            if(modeFrame >= 126)
            {
                modeFrame = 0;
            }
            yield return new WaitForSeconds(0.04f);
        }
    }*/

    private void DisableAllChilds()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void SwipeDetector_OnSwipe(SwipeData data)
    {
        if (!modeDisplayActive) return;

        if(data.Direction == SwipeDirection.Left)
        { //links
            ChangeMode(false);
        } else if(data.Direction == SwipeDirection.Right)
        { //rechts
            ChangeMode(true);
        }
    }

    private void SetIndexData(bool start = false)
    {
        //oldPlayer.frame = normalPlayer.frame;
        //previewOldImage.sprite = previewImage.sprite;

        if(!start)
        {
            previewOldImage.sprite = modeSprites[oldIndex];

            if(!mainModes[oldIndex].unlocked)
            {
                Transform lockParent = displayParents[1].transform.GetChild(1).GetChild(0);
                string temp = unlockTextString + mainModes[oldIndex].unlockLevel.ToString();

                lockParent.gameObject.SetActive(true);
                lockParent.GetChild(1).GetComponent<TextMeshProUGUI>().text = temp;
                lockParent.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = temp;
            } else
            {
                displayParents[1].transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
            }
        }

        previewImage.gameObject.SetActive(true);
        previewImage.sprite = modeSprites[currentIndex];

        moveDone = false;
        prepareDone = false;

        normalPlayer.Stop();

        normalPlayer.GetComponent<VideoPlayer>().targetTexture =
            renderTextures[currentIndex];
        rawImage.texture = renderTextures[currentIndex];

        normalPlayer.clip = modeClips[currentIndex];
        normalPlayer.prepareCompleted += DisablePreview;
        normalPlayer.Play();

        void DisablePreview(VideoPlayer vp)
        {
            prepareDone = true;
        }

        //oldDescriptionText.color = FlatterFogelHandler.currentColor;
        oldDescriptionText.text = descriptionText.text;

        //oldDescriptionText.transform.parent.GetComponent<TextMeshProUGUI>().color =
        //    FlatterFogelHandler.currentColor;
        oldDescriptionText.transform.parent.GetComponent<TextMeshProUGUI>().text =
            oldDescriptionText.text;

        //previewImage.sprite = mainModes[currentIndex].previewSprite;

        SetModeText(mainModes[currentIndex].modeName, modeColor);

        //descriptionText.color = FlatterFogelHandler.currentColor;
        //descriptionText.text = mainModes[currentIndex].modeDescription;

        //descriptionText.transform.parent.GetComponent<TextMeshProUGUI>().color =
        //    FlatterFogelHandler.currentColor;
        descriptionText.transform.parent.GetComponent<TextMeshProUGUI>().text =
            descriptionText.text;

        if(!start)
        {
            CheckUnlocked();
        }

        Invoke(nameof(SetIndexDataLate), 0.25f);
    }

    private void SetModeText(string newText, Color modeColor)
    {
        oldModeText.text = modeText.text;
        oldModeText.color = modeColor;
        oldModeBGText.text = modeText.text;

        modeText.text = newText;
        modeText.color = modeColor;
        modeBGText.text = modeText.text;
    }

    private void SetIndexDataLate()
    {
        switch (currentIndex)
        {
            case 0: //klassisch
                FlatterFogelHandler.Instance.SetModes(false, false, false, false, false);
                break;
            case 1: //hardcore
                FlatterFogelHandler.Instance.SetModes(false, false, true, false, false);
                break;
            case 2: //royale
                FlatterFogelHandler.Instance.SetModes(true, false, false, false, false);
                break;
            case 3: //mining
                FlatterFogelHandler.Instance.SetModes(false, false, false, true, false);
                break;
            case 4: //destruction
                FlatterFogelHandler.Instance.SetModes(false, true, false, false, false);
                break;
            case 5: //zigzag
                FlatterFogelHandler.Instance.SetModes(false, false, false, false, true);
                break;
        }

        moveDone = true;
    }

    public void ChangeMode(bool direction)
    {
        if (changeRunning) return;

        if(TutorialHandler.Instance.mainTut == 0)
        { //Modewechsel disabled im Tutorial
            return;
        }

        changeRunning = true;

        oldIndex = currentIndex;

        if(direction == false)
        {
            currentIndex--;
            displayParents[0].transform.position = previewPositions[2];
        } else
        {
            currentIndex++;
            displayParents[0].transform.position = previewPositions[0];
        }

        if (currentIndex < 0) currentIndex = mainModes.Length - 1;
        if (currentIndex >= mainModes.Length) currentIndex = 0;

        while(currentIndex >= 1 && currentIndex <= 2 || currentIndex == 5)
        { //hardcore + royale + zickzack ausschließen da überarbeitung
            if (direction == false)
            {
                currentIndex--;
                displayParents[0].transform.position = previewPositions[2];
            }
            else
            {
                currentIndex++;
                displayParents[0].transform.position = previewPositions[0];
            }

            if (currentIndex < 0) currentIndex = mainModes.Length - 1;
            if (currentIndex >= mainModes.Length) currentIndex = 0;
        }

        modeFrame = 0;

        ObscuredPrefs.SetInt("Player_SelectedMode", currentIndex);

        SetIndexData();

        displayParents[1].transform.position = previewPositions[1];

        float mTime = 0.2f;

        Invoke(nameof(ResetChangeRunning), mTime + 0.01f);

        if(direction == false)
        { //links

            displayParents[0].transform.DOMoveX(previewPositions[1].x, mTime).SetEase(Ease.InOutSine);
            displayParents[1].transform.DOMoveX(previewPositions[0].x, mTime).SetEase(Ease.InOutSine);
        } else
        { //rechts
            displayParents[0].transform.DOMoveX(previewPositions[1].x, mTime).SetEase(Ease.InOutSine);
            displayParents[1].transform.DOMoveX(previewPositions[2].x, mTime).SetEase(Ease.InOutSine);
        }
    }

    private void ResetChangeRunning()
    {
        changeRunning = false;
    }

    public void ModeGoClicked()
    {
        //MenuData.Instance.DoScaleDown();
        if(!modeDisplayActive)
        {
            if(TutorialHandler.Instance.mainTut == 0)
            {
                TutorialHandler.Instance.MainTutModes();
            }

            ActivateModeChange();
        } else
        {
            if (TutorialHandler.Instance.mainTut == 0)
            {
                TutorialHandler.Instance.MainTutBegin();
            }

            GoClicked();
        }

    }

    public void GoClicked()
    {
        if (changeRunning) return;

        DisableModeChange();

        //float scaleTime = MenuData.Instance.GetScaleTime();

        //Invoke("StartFF", scaleTime + 0.02f);
        StartFF();
    }

    private void StartFF()
    {
        MenuData.Instance.StartFF();
    }

    public void BackClicked()
    {
        if (changeRunning || !modeDisplayActive) return;

        //MenuData.Instance.DoScaleUp();
        DisableModeChange();
    }

    // Update is called once per frame
    void Update()
    {
        if(modeDisplayActive)
        {
            if(prepareDone && moveDone)
            {
                if(normalPlayer.frameCount > 1)
                {
                    previewImage.gameObject.SetActive(false);
                    prepareDone = false;

                }
            }

            if(Input.GetKeyDown(KeyCode.Escape) && !changeRunning)
            {
                if(TutorialHandler.Instance.mainTut != 0)
                {
                    BackClicked();
                }
            }
        }
    }
}
