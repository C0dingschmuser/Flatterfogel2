﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SplatterHandler : MonoBehaviour
{
    [SerializeField]
    private BackgroundHandler bgHandler;

    [SerializeField]
    private GameObject[] splatterObjs = null;

    [SerializeField]
    private Material splatterMat = null;

    public bool splatterActive = false;
    private bool dir = false, transition = false;

    [SerializeField]
    private Vector3 startPos;

    [SerializeField]
    private float dissolveEndValue = 0.325f, minTempDissolve = 0.25f;

    private float dissolveAmount = 0, tempDissolveEnd;

    // Start is called before the first frame update
    void Start()
    {
        //Invoke("StartSplatter", 3f);
    }

    public void StartSplatter()
    {
        splatterActive = true;
        transition = false;

        dissolveAmount = 0;
        splatterMat.SetFloat("_DissolveAmount", dissolveAmount);

        tempDissolveEnd = Random.Range(0.25f, 0.35f);

        Tween anTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, dissolveEndValue, 1f);
        anTween.OnUpdate(() =>
        {
            splatterMat.SetFloat("_DissolveAmount", dissolveAmount);
        });

        for(int i = 0; i < splatterObjs.Length; i++)
        {
            splatterObjs[i].transform.position = new Vector3(startPos.x + (2925 * i), startPos.y, startPos.z);
            splatterObjs[i].SetActive(true);
        }
    }

    public void EndSplatter()
    {
        transition = true;

        dissolveAmount = splatterMat.GetFloat("_DissolveAmount"); //dissolveEndValue;
        //splatterMat.SetFloat("_DissolveAmount", dissolveAmount);

        Tween anTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, 0, 1f);
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
        });
    }

    // Update is called once per frame
    void Update()
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