﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartupHandler : MonoBehaviour
{
    public GameObject parentObj, skipObj, copyrightObj;
    public Material[] dissolveMat;
    public Animator animator;
    public Dissolver logoDissolver;
    public ParticleSystem minus;

    private int tapCount = 0;

    public void Awake()
    {
        for (int i = 0; i < dissolveMat.Length; i++)
        {
            dissolveMat[i].SetFloat("_DissolveAmount", 1);
        }
    }

    public void StartDissolve(bool force = false)
    {
        if(force)
        {
            minus.gameObject.SetActive(false);
            copyrightObj.SetActive(false);
        }

        for(int i = 0; i < dissolveMat.Length; i++)
        {
            dissolveMat[i].SetFloat("_DissolveAmount", 1);
        }

        logoDissolver.enabled = true;

        //if(OptionHandler.firstLaunch != 1) //weil dann gdpr & language active

        Invoke("DisableStartup", 1.01f);
    }

    private void DisableStartup()
    {
        parentObj.SetActive(false);
    }

    private void Update()
    {
        bool touchOK = false;

        if (Input.GetMouseButtonDown(0))
        { //check ob grounded
            touchOK = true;
        }

        if(touchOK)
        {
            tapCount++;

            if(tapCount == 1)
            {
                skipObj.SetActive(true);
            } else if(tapCount == 2)
            {
                skipObj.SetActive(false);
                animator.enabled = false;
                StartDissolve(true);
            }
        }
    }
}
