using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class IngameMenuHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject continueBtn = null, settingsBtn = null, exitBtn = null, eventSystem = null, mainCamera = null,
        bgImage = null, quitParent = null, quitImage = null;

    [SerializeField]
    private Canvas uiCanvas = null;

    [SerializeField]
    private GraphicRaycaster raycaster = null, optionRaycaster = null;

    [SerializeField]
    private FlatterFogelHandler ffHandler = null;

    private bool buttonsMoving = false, bgEnabled = true;
    private List<GameObject> scalingButtons = new List<GameObject>();

    private float originalCameraOrtho;
    private Vector3 originalCameraPos;

    private void SetStartPosition()
    {
        Vector3 old = continueBtn.transform.position;
        old.x = -1131;
        continueBtn.transform.position = old;

        old = settingsBtn.transform.position;
        old.x = 369;
        settingsBtn.transform.position = old;

        old = exitBtn.transform.position;
        old.x = -1131;
        exitBtn.transform.position = old;
    }

    private void OpenQuit()
    {
        eventSystem.SetActive(false);

        float scaleTime = MenuData.Instance.GetScaleTime();

        Vector3 pos = mainCamera.transform.position;
        pos.z = 0;
        pos.y += 1900;

        quitParent.transform.position = pos;
        quitParent.gameObject.SetActive(true);

        pos.y -= 1900;

        Color c = Color.black;
        c.a = 0;
        quitImage.GetComponent<Image>().color = c;
        quitImage.GetComponent<Image>().DOFade(0.826f, scaleTime).SetUpdate(UpdateType.Normal, true);

        quitParent.transform.DOMove(pos, scaleTime).
            SetEase(Ease.OutBack).SetUpdate(UpdateType.Normal, true);

        StartCoroutine(EndQuitMove(scaleTime + 0.01f, false, false));
    }

    public void CloseQuit(bool quit)
    {
        eventSystem.SetActive(false);

        float scaleTime = MenuData.Instance.GetScaleTime();

        Vector3 newPos = quitParent.transform.position;
        newPos.y += 1900;

        quitImage.GetComponent<Image>().DOFade(0, scaleTime).SetUpdate(UpdateType.Normal, true);

        quitParent.transform.DOMove(newPos, scaleTime).
            SetEase(Ease.OutBack).SetUpdate(UpdateType.Normal, true);

        StartCoroutine(EndQuitMove(scaleTime + 0.01f, quit, true));
    }

    IEnumerator EndQuitMove(float waitTime, bool end, bool close)
    {
        yield return new WaitForSecondsRealtime(waitTime);

        eventSystem.SetActive(true);

        if(close)
        {
            if (end)
            {
                quitParent.SetActive(false);
                OpenMenu(false, false, true);
            }
            else
            {
                OpenMenu(true, false, false, false, true);
            }
        }
    }

    public void OpenMenu(bool open, bool bg = false, bool quit = false, bool fromOption = false, bool fromQuit = false)
    {
        float scaleTime = MenuData.Instance.GetScaleTime();

        if (open)
        {
            if((ffHandler.miningMode || ffHandler.destructionMode) && !fromOption && !fromQuit)
            { //kamera raus/rein zoomen wenn nicht aus optionen kommt
                if(bg)
                {
                    Color c = Color.black;
                    c.a = 0;
                    bgImage.GetComponent<Image>().color = c;

                    bgImage.GetComponent<Image>().DOFade(0.826f, scaleTime).SetUpdate(UpdateType.Normal, true);
                }

                originalCameraPos = mainCamera.transform.position;
                originalCameraOrtho = mainCamera.GetComponent<Camera>().orthographicSize;

                float newOrtho = OptionHandler.defaultOrthoSize;
                Vector3 newPos = OptionHandler.defaultCameraPos;

                if (ffHandler.miningMode)
                {
                    if(!BackgroundHandler.Instance.layersEnabled)
                    {
                        bgEnabled = false;

                        BackgroundHandler.Instance.EnableBackground(true);
                    }

                    DOTween.To(() => mainCamera.GetComponent<Camera>().orthographicSize, x => mainCamera.GetComponent<Camera>().orthographicSize = x,
                        newOrtho, scaleTime).SetUpdate(UpdateType.Normal, true);

                    newPos = originalCameraPos;
                    newPos.x = -381;

                    DOTween.To(() => mainCamera.transform.position, x => mainCamera.transform.position = x,
                        newPos, scaleTime).SetUpdate(UpdateType.Normal, true);

                } else
                {
                    DOTween.To(() => mainCamera.GetComponent<Camera>().orthographicSize, x => mainCamera.GetComponent<Camera>().orthographicSize = x,
                        newOrtho, MenuData.Instance.GetScaleTime()).SetUpdate(UpdateType.Normal, true);

                    DOTween.To(() => mainCamera.transform.position, x => mainCamera.transform.position = x,
                        newPos, MenuData.Instance.GetScaleTime()).SetUpdate(UpdateType.Normal, true);
                }
            }

            gameObject.SetActive(true);
            eventSystem.SetActive(true);
            raycaster.enabled = true;

            if(ffHandler.tutHandler.mainTut == 0)
            {
                settingsBtn.GetComponent<Button>().interactable = false;
                exitBtn.GetComponent<Button>().interactable = false;
            } else
            {
                settingsBtn.GetComponent<Button>().interactable = true;
                exitBtn.GetComponent<Button>().interactable = true;
            }

            SetStartPosition();
            StartCoroutine(DoMoveIn());
        } else
        {
            float waitTime = MenuData.Instance.GetScaleTime();
            StartCoroutine(CloseMenu(waitTime * 2, quit));
        }
    }

    private IEnumerator CloseMenu(float waitTime, bool quit)
    {
        bgImage.GetComponent<Image>().DOFade(0, waitTime).SetUpdate(UpdateType.Normal, true);

        yield return new WaitForSecondsRealtime(waitTime);

        eventSystem.SetActive(false);

        if(ffHandler.miningMode || ffHandler.destructionMode)
        {
            if(ffHandler.miningMode)
            {
                if(!bgEnabled)
                {
                    BackgroundHandler.Instance.EnableBackground(false);
                }
            }
            
            DOTween.To(() => mainCamera.GetComponent<Camera>().orthographicSize, x => mainCamera.GetComponent<Camera>().orthographicSize = x,
                originalCameraOrtho, MenuData.Instance.GetScaleTime()).SetUpdate(UpdateType.Normal, true);

            DOTween.To(() => mainCamera.transform.position, x => mainCamera.transform.position = x,
                originalCameraPos, MenuData.Instance.GetScaleTime()).SetUpdate(UpdateType.Normal, true);
            
        }

        raycaster.enabled = false;

        yield return new WaitForSecondsRealtime(MenuData.Instance.GetScaleTime());

        if(quit)
        {
            ffHandler.PauseGame(false, quit);
        } else
        {
            ffHandler.StartResumeTimer();
        }

        gameObject.SetActive(false);
    }

    public IEnumerator DoMoveAway()
    {
        buttonsMoving = true;

        float scaleTime = MenuData.Instance.GetScaleTime();

        continueBtn.transform.DOMoveX(-1131, scaleTime, true).SetEase(Ease.InBack).SetUpdate(UpdateType.Normal, true);

        yield return new WaitForSecondsRealtime(0.1f);

        settingsBtn.transform.DOMoveX(369, scaleTime, true).SetEase(Ease.InBack).SetUpdate(UpdateType.Normal, true);

        yield return new WaitForSecondsRealtime(0.1f);

        exitBtn.transform.DOMoveX(-1131, scaleTime, true).SetEase(Ease.InBack).SetUpdate(UpdateType.Normal, true);

        StartCoroutine(ResetMovingState(scaleTime + 0.01f));
    }

    public IEnumerator DoMoveIn()
    {
        buttonsMoving = true;

        float scaleTime = MenuData.Instance.GetScaleTime();

        exitBtn.transform.DOMoveX(-381, scaleTime, true).SetEase(Ease.InBack).SetUpdate(UpdateType.Normal, true);

        yield return new WaitForSecondsRealtime(0.1f);

        settingsBtn.transform.DOMoveX(-381, scaleTime, true).SetEase(Ease.InBack).SetUpdate(UpdateType.Normal, true);

        yield return new WaitForSecondsRealtime(0.1f);

        continueBtn.transform.DOMoveX(-381, scaleTime, true).SetEase(Ease.InBack).SetUpdate(UpdateType.Normal, true);

        StartCoroutine(ResetMovingState(scaleTime + 0.01f));
    }

    IEnumerator ResetMovingState(float waitTime)
    {
        yield return new WaitForSecondsRealtime(waitTime);

        buttonsMoving = false;
    }

    public void OptionClose()
    {
        optionRaycaster.enabled = false;
    }

    public void ButtonPressed(GameObject btn)
    {
        if (!scalingButtons.Contains(btn) && !buttonsMoving && 
            btn.GetComponent<Button>().interactable == true)
        {
            bool ok = true;

            if (ok)
            {
                scalingButtons.Add(btn);
                btn.transform.DOScale(0.8f, 0.05f).SetUpdate(UpdateType.Normal, true);
            }
        }
    }

    public void ButtonReleased(GameObject btn)
    {
        if (scalingButtons.Contains(btn))
        {
            scalingButtons.Remove(btn);

            bool ok = true;

            StartCoroutine(DoMoveAway());

            switch (btn.tag)
            {
                case "GoBtn":
                    OpenMenu(false);
                    break;
                case "OptionBtn":
                    optionRaycaster.enabled = true;

                    OptionHandler.Instance.ShowOptions(true);
                    break;
                case "BackBtn":
                    OpenQuit();
                    break;
            }

            if (ok)
            {
                btn.transform.DOScale(1f, 0.05f).SetUpdate(UpdateType.Normal, true);
            }
        }
    }
}
