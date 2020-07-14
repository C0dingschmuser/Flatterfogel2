using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialHandler : MonoBehaviour
{
    public GameObject[] menuButtons;

    public GameObject mainTutObj;

    public Material completedMaterial, failedMaterial;

    public static TutorialHandler Instance;

    public int mainTut = 0, mainTutStep = 0;

    private float dissolveAmount = 0f;

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

    public void Completed(bool failed = false)
    {
        dissolveAmount = 0f;

        Tween dTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, 1f, 0.5f);
        dTween.OnUpdate(() =>
        {
            if(!failed)
            {
                completedMaterial.SetFloat("_DissolveAmount", dissolveAmount);
            }
            else
            {
                failedMaterial.SetFloat("_DissolveAmount", dissolveAmount);
            }
        });
    }

    public void ResetCompleted()
    {
        dissolveAmount = 0;
        completedMaterial.SetFloat("_DissolveAmount", dissolveAmount);
        failedMaterial.SetFloat("_DissolveAmount", dissolveAmount);
    }

    public void StartMainTut()
    {
        ResetCompleted();

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
        mainTutStep = 1;

        StartCoroutine(MainTutGreat());
    }

    private IEnumerator MainTutGreat()
    {
        Completed();

        yield return new WaitForSeconds(0.51f);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);
        ResetCompleted();

        mainTutObj.transform.GetChild(0).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(1).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(2).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(3).gameObject.SetActive(true); //great

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(3f);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);

        mainTutObj.transform.GetChild(3).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(4).gameObject.SetActive(true); //navigate pipe tut

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(1);

        FlatterFogelHandler.Instance.SpawnPipes(false, false);
    }

    public void StartMainTutGreat2()
    {
        StartCoroutine(MainTutGreat2());
    }

    private IEnumerator MainTutGreat2()
    {
        Completed();

        yield return new WaitForSeconds(0.51f);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);
        ResetCompleted();

        mainTutObj.transform.GetChild(0).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(1).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(2).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(3).gameObject.SetActive(true); //great
        mainTutObj.transform.GetChild(4).gameObject.SetActive(false);

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(4);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);

        mainTutStep = 2;

        StartPerfectHit();
        //FlatterFogelHandler.Instance.SpawnPipes(false, false, 9999, false, false, true);
    }

    public void StartPerfectHit()
    {
        mainTutStep = 3;

        mainTutObj.transform.GetChild(3).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(5).gameObject.SetActive(true); //perfect hit info

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        StartCoroutine(PerfectHit());
    }

    private IEnumerator PerfectHit()
    {
        yield return new WaitForSeconds(2f);

        FlatterFogelHandler.Instance.SpawnPipes(false, false);
    }

    public void StartAlmostHit()
    {
        StartCoroutine(AlmostHit()); 
    }

    private IEnumerator AlmostHit()
    {
        Completed(true);

        yield return new WaitForSeconds(0.51f);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);
        ResetCompleted();

        mainTutObj.transform.GetChild(0).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(1).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(2).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(3).gameObject.SetActive(false); //great
        mainTutObj.transform.GetChild(4).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(5).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(6).gameObject.SetActive(true);

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(3);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);

        mainTutObj.transform.GetChild(6).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(5).gameObject.SetActive(true); //perfect hit info

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        FlatterFogelHandler.Instance.SpawnPipes(false, false);
    }

    public void StartRealPerfectHit()
    {
        mainTutStep = 4;

        StartCoroutine(RealPerfectHit());
    }

    private IEnumerator RealPerfectHit()
    {
        Completed();

        yield return new WaitForSeconds(0.51f);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);
        ResetCompleted();

        mainTutObj.transform.GetChild(0).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(1).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(2).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(3).gameObject.SetActive(true); //great
        mainTutObj.transform.GetChild(4).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(5).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(6).gameObject.SetActive(false);

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(3);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);

        mainTutObj.transform.GetChild(0).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(1).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(2).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(3).gameObject.SetActive(false); //great
        mainTutObj.transform.GetChild(4).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(5).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(6).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(7).gameObject.SetActive(true); //coin info

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(2);

        FlatterFogelHandler.Instance.SpawnPipes(false, false, 9999, false, false, true);
    }

    public void StartCoinGreat()
    {
        StartCoroutine(MainTutCoinGreat());
    }

    private IEnumerator MainTutCoinGreat()
    {
        Completed();

        yield return new WaitForSeconds(0.51f);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);
        ResetCompleted();

        mainTutObj.transform.GetChild(0).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(1).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(2).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(3).gameObject.SetActive(false); //great
        mainTutObj.transform.GetChild(4).gameObject.SetActive(false);
        mainTutObj.transform.GetChild(5).gameObject.SetActive(false); 
        mainTutObj.transform.GetChild(6).gameObject.SetActive(false); 
        mainTutObj.transform.GetChild(7).gameObject.SetActive(false); 
        mainTutObj.transform.GetChild(8).gameObject.SetActive(true); //perfekt

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(3);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        yield return new WaitForSeconds(MenuData.scaleTime + 0.01f);

        mainTutObj.transform.GetChild(8).gameObject.SetActive(false); //perfect
        mainTutObj.transform.GetChild(9).gameObject.SetActive(true); //bereit

        mainTutObj.transform.DOMoveY(1375, MenuData.scaleTime, true).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(2);

        FlatterFogelHandler.Instance.SpawnPipes(false, false);

        yield return new WaitForSeconds(1);

        mainTutObj.transform.DOMoveY(2000, MenuData.scaleTime, true).SetEase(Ease.InBack);

        mainTut = 1;
    }
}
