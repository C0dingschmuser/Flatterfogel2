﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class AchUnlockHandler : MonoBehaviour
{
    private bool unlockActive = false;

    public GameObject unlockObj, effectObj;
    public Image unlockImage;

    public static AchUnlockHandler Instance;

    [SerializeField]
    private Vector3 defaultStartPos;

    //liste an achs die noch abgearbeitet werden muss
    private List<Achievement> unlockedAchievements = new List<Achievement>();

    private Coroutine achRoutine = null;

    private void Awake()
    {
        Instance = this;
        unlockObj.transform.parent.gameObject.SetActive(false);
    }

    public void StartAchievementUnlock(Achievement ach)
    {
        if(!unlockedAchievements.Contains(ach))
        {
            if(ach.unlockMode != null)
            { //Achievement schaltet Modus frei -> modemanager aufrufen
                ModeManager.Instance.UnlockMode(ach.unlockMode.identifier);
            }

            unlockedAchievements.Add(ach);
        }

        if (achRoutine == null)
        {
            //Debug.Log("start");
            StartCoroutine(AchievementUnlock());
        }
        else Debug.Log("running!");
    }

    private IEnumerator AchievementUnlock()
    {
        Coroutine temp = StartCoroutine(FlashColor());

        bool ok = false;

        unlockObj.transform.parent.gameObject.SetActive(true);

        while(!ok)
        {
            Achievement latest = unlockedAchievements.Last();

            unlockedAchievements.Remove(latest);

            unlockImage.sprite = latest.mainSprite[latest.upgradeStep];

            unlockObj.transform.position = defaultStartPos;
            unlockObj.transform.DOMoveX(-138, 0.5f).SetEase(Ease.OutExpo);

            yield return new WaitForSeconds(0.51f);

            effectObj.SetActive(false);
            effectObj.SetActive(true);

            yield return new WaitForSeconds(3f);

            unlockObj.transform.DOMoveX(defaultStartPos.x, 0.5f).SetEase(Ease.InExpo);

            yield return new WaitForSeconds(0.51f);

            if (unlockedAchievements.Count == 0)
            { //verlassen wenn keine achievements mehr
                ok = true;
            }
        }

        unlockObj.transform.parent.gameObject.SetActive(false);
        StopCoroutine(temp);

        yield return achRoutine = null;
    }

    private IEnumerator FlashColor()
    {
        while(true)
        {
            

            yield return new WaitForSeconds(0.2f);
        }
    }
}
