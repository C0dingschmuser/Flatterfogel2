using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using DG.Tweening;
using Firebase.Analytics;

public class SplatterHandler : MonoBehaviour
{
    [SerializeField]
    private BackgroundHandler bgHandler = null;

    [SerializeField]
    private GameObject[] splatterObjs = null;

    [SerializeField]
    private Material splatterMat = null;

    public bool splatterActive = false;
    private bool dir = false, transition = false;

    private CoroutineHandle mainHandle;

    [SerializeField]
    private Vector3 startPos;

    [SerializeField]
    private float dissolveEndValue = 0.325f, minTempDissolve = 0.25f;

    private float dissolveAmount = 0, tempDissolveEnd;

    private int diff = 0;

    private Tween anTween = null;

    public void StartSplatter(ulong score)
    {
        splatterActive = true;
        transition = false;

        dissolveAmount = 0;
        splatterMat.SetFloat("_DissolveAmount", dissolveAmount);

        if(score < 50)
        {
            minTempDissolve = 0.20f;
            dissolveEndValue = 0.28f;

            diff = 0;
        } else if(score < 100)
        {
            minTempDissolve = 0.30f;
            dissolveEndValue = 0.36f;

            diff = 1;
        } else if(score < 150)
        {
            minTempDissolve = 0.36f;
            dissolveEndValue = 0.42f;

            diff = 2;
        } else
        {
            minTempDissolve = 0.42f;
            dissolveEndValue = 0.48f;

            diff = 3;
        }

        //dissolveEndValue = 0.425f;
        //minTempDissolve = 0.3f;

        //tempDissolveEnd = Random.Range(minTempDissolve, dissolveEndValue/* + 0.025f*/);

        dissolveEndValue = Random.Range(minTempDissolve, dissolveEndValue);

        anTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, dissolveEndValue, 1f);
        anTween.OnUpdate(() =>
        {
            splatterMat.SetFloat("_DissolveAmount", dissolveAmount);
        });

        for(int i = 0; i < splatterObjs.Length; i++)
        {
            splatterObjs[i].transform.position = new Vector3(startPos.x + (2925 * i), startPos.y, startPos.z);
            splatterObjs[i].SetActive(true);
        }

        mainHandle = Timing.RunCoroutine(Util._EmulateUpdate(_MainUpdate, this));
    }

    public void EndSplatter(bool force = false)
    {
        transition = true;

        if(anTween != null)
        {
            anTween.Kill();
        }

        Timing.KillCoroutines(mainHandle);

        dissolveAmount = splatterMat.GetFloat("_DissolveAmount"); //dissolveEndValue;
        //splatterMat.SetFloat("_DissolveAmount", dissolveAmount);

        anTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, 0, 1f);
        anTween.OnUpdate(() =>
        {
            splatterMat.SetFloat("_DissolveAmount", dissolveAmount);
        });

        anTween.OnComplete(() =>
        {
            splatterActive = false;
            for(int i = 0; i < splatterObjs.Length; i++)
            {
                splatterObjs[i].SetActive(false);
                splatterActive = false;
            }

            FlatterFogelHandler.Instance.SetInternalScore(0);

            if(!force)
            {
                int coins = 3;

                switch(diff)
                {
                    case 1:
                        coins = 4;
                        break;
                    case 2:
                        coins = 7;
                        break;
                    case 3:
                        coins = 14;
                        break;
                }

                Timing.RunCoroutine(SpawnEndCoins(0.5f, coins));

                FirebaseHandler.LogEvent("Splatter_Finish");
                FirebaseHandler.LogEvent("Boss_Finish");
            }
        });
    }

    private IEnumerator<float> SpawnEndCoins(float time, int coins)
    {
        if(!FF_PlayerData.Instance.dead)
        { //nur ausführen wenn alive
            while (coins > 0)
            {
                Vector3 newPos = new Vector3(Random.Range(-438, -85),
                    Random.Range(223, 1052), 0);

                FlatterFogelHandler.Instance.SpawnCoin(newPos);

                coins--;
                yield return Timing.WaitForSeconds(time);
            }
        }
    }

    // Update is called once per frame
    void _MainUpdate()
    {
        if(splatterActive)
        {
            if(bgHandler.GetScrolling())
            {
                Vector3 pos;
                for(int i = 0; i < splatterObjs.Length; i++)
                {
                    pos = splatterObjs[i].transform.position;
                    pos.x -= FlatterFogelHandler.scrollSpeed * Time.deltaTime;

                    splatterObjs[i].transform.position = pos;

                    if(!transition)
                    {
                        /*float speed = 0.05f;

                        if(!dir)
                        { //kleiner
                            dissolveAmount -= speed * Time.deltaTime;

                            if(dissolveAmount < tempDissolveEnd)//0.25f)
                            {
                                dir = true;
                                tempDissolveEnd = dissolveEndValue;//Random.Range(dissolveAmount + 0.01f, dissolveEndValue);
                            }
                        } else
                        { //größer
                            dissolveAmount += speed * Time.deltaTime;

                            if(dissolveAmount > tempDissolveEnd)//0.375f)
                            {
                                dir = false;
                                tempDissolveEnd = minTempDissolve;//Random.Range(dissolveAmount - 0.01f, minTempDissolve);
                            }
                        }

                        splatterMat.SetFloat("_DissolveAmount", dissolveAmount);*/

                        if (i == 0 && pos.x < -2200)
                        { //ende erreicht, begin dissolve
                            EndSplatter();
                        }
                    }
                }
            }
        }
    }
}
