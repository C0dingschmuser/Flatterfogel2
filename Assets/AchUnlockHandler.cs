using System.Collections;
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
    public TextMeshProUGUI[] unlockText;
    public TextMeshProUGUI titleText;

    public static AchUnlockHandler Instance;

    [SerializeField]
    private Vector3 defaultStartPos;

    //liste an achs die noch abgearbeitet werden muss
    private List<Achievement> unlockedAchievements = new List<Achievement>();

    private Coroutine achRoutine = null;

    private void Awake()
    {
        Instance = this;
        unlockObj.SetActive(false);
    }

    public void StartAchievementUnlock(Achievement ach)
    {
        if(!unlockedAchievements.Contains(ach))
        {
            unlockedAchievements.Add(ach);
        }

        if(achRoutine == null)
        {
            StartCoroutine(AchievementUnlock());
        }
    }

    private IEnumerator AchievementUnlock()
    {
        Coroutine temp = StartCoroutine(FlashColor());

        bool ok = false;

        unlockObj.SetActive(true);

        while(!ok)
        {
            Achievement latest = unlockedAchievements.Last();

            unlockedAchievements.Remove(latest);

            unlockImage.sprite = latest.mainSprite[latest.upgradeStep];
            titleText.text = latest.titleString;

            unlockObj.transform.position = defaultStartPos;
            unlockObj.transform.DOMoveX(-502, 0.5f).SetEase(Ease.OutExpo);

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

        unlockObj.SetActive(false);
        StopCoroutine(temp);

        yield return achRoutine = null;
    }

    private IEnumerator FlashColor()
    {
        while(true)
        {
            Color c = unlockText[0].color;
            unlockText[0].color = unlockText[1].color;
            unlockText[1].color = c;

            yield return new WaitForSeconds(0.2f);
        }
    }
}
