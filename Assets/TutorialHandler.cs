using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialHandler : MonoBehaviour
{
    public GameObject[] menuButtons;

    public GameObject mainTutObj;

    public static TutorialHandler Instance;

    public int mainTut = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void Startup()
    {
        mainTut = PlayerPrefs.GetInt("TutorialPlayed", 0);

        if (mainTut == 0)
        { //tutorial noch nicht gespielt -> starten
            StartMainTut();
        }
    }

    public void StartMainTut()
    {
        for(int i = 0; i < menuButtons.Length; i++)
        {
            menuButtons[i].GetComponent<Button>().interactable = false;
        }

        mainTutObj.transform.position = new Vector3(-381, 2000, -300);
        mainTutObj.transform.GetChild(0).gameObject.SetActive(true);

        mainTutObj.transform.DOMoveY(987, MenuData.scaleTime, true).SetEase(Ease.OutBack);
    }

    public void MainTutModes()
    {
        mainTutObj.transform.GetChild(0).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(1).gameObject.SetActive(true);

        mainTutObj.transform.DOMoveY(330, MenuData.scaleTime, true).SetEase(Ease.OutBack);
    }

    public void MainTutBegin()
    {
        mainTutObj.transform.GetChild(0).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(1).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(2).gameObject.SetActive(true);

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);
    }

    public void StartMainTutGreat()
    {
        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        StartCoroutine(MainTutGreat());
    }

    private IEnumerator MainTutGreat()
    {
        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);

        mainTutObj.transform.GetChild(0).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(1).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(2).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(3).gameObject.SetActive(true);

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(4f);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);

        //pipe spawn starten

        mainTutObj.transform.GetChild(3).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(4).gameObject.SetActive(true); //navigate pipe tut

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);
    }
}
