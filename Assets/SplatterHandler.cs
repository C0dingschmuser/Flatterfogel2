﻿using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using DG.Tweening;

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
            minTempDissolve = 0.1f;
            dissolveEndValue = 0.2f;

            diff = 0;
        } else if(score < 100)
        {
            minTempDissolve = 0.13f;
            dissolveEndValue = 0.23f;

            diff = 1;
        } else if(score < 150)
        {
            minTempDissolve = 0.16f;
            dissolveEndValue = 0.25f;

            diff = 2;
        } else
        {
            minTempDissolve = 0.25f;
            dissolveEndValue = 0.35f;

            diff = 3;
        }

        tempDissolveEnd = Random.Range(minTempDissolve, dissolveEndValue);

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
                int coins = 1;

                switch(diff)
                {
                    case 1:
                        coins = 2;
                        break;
                    case 2:
                        coins = 5;
                        break;
                    case 3:
                        coins = 10;
                        break;
                }

                Timing.RunCoroutine(SpawnEndCoins(0.5f, coins));
            }
        });

        Timing.KillCoroutines(mainHandle);
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
                        float speed = 0.05f;

                        if(!dir)
                        { //kleiner
                            dissolveAmount -= speed * Time.deltaTime;

                            if(dissolveAmount < tempDissolveEnd)//0.25f)
                            {
                                dir = true;
                                tempDissolveEnd = Random.Range(dissolveAmount + 0.01f, dissolveEndValue);
                            }
                        } else
                        {
                            dissolveAmount += speed * Time.deltaTime;

                            if(dissolveAmount > tempDissolveEnd)//0.375f)
                            {
                                dir = false;
                                tempDissolveEnd = Random.Range(dissolveAmount - 0.01f, minTempDissolve);
                            }
                        }

                        splatterMat.SetFloat("_DissolveAmount", dissolveAmount);

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
