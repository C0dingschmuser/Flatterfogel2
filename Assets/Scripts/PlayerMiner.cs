using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

public class PlayerMiner : MonoBehaviour
{
    public List<MinerTool> miningTools = new List<MinerTool>();

    public static MinerTool currentMiner;

    public GameObject miner, effect, wing, heatEffect, persistentParent;
    public UnityEngine.Experimental.Rendering.Universal.Light2D laserLight, miningLight, playerLight;
    public ParticleSystem[] pSystem;
    //private int pSystemCount = 0;

    [SerializeField]
    private Vector3[] heatPositions = null; //rechts / links / unten

    private bool isMining = false;

    public void InitializeMiner()
    {
        Color laserColor = currentMiner.laserColor;

        miningLight.enabled = true;
        playerLight.enabled = false;

        Vector3 pos = transform.position;
        pos.x += heatPositions[0].x;
        pos.y += heatPositions[0].y;

        heatEffect.transform.position = pos;

        miner.GetComponent<SpriteRenderer>().sprite = currentMiner.main;
        miner.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = currentMiner.effect;
        effect.GetComponent<SpriteRenderer>().color = laserColor;
        laserLight.color = laserColor;
    }

    public void EndMining()
    {
        miningLight.enabled = false;
        playerLight.enabled = true;
    }

    public bool IsMining()
    {
        return isMining;
    }

    public void ChangeDir(int dir)
    {
        Vector3 pos = transform.position;

        persistentParent.transform.rotation = Quaternion.identity;

        miner.transform.rotation = Quaternion.identity;
        miner.GetComponent<SpriteRenderer>().flipX = false;
        miner.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = false;

        switch (dir)
        {
            case 2: //runter
                pos.x += heatPositions[2].x;
                pos.y += heatPositions[2].y;

                persistentParent.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                miner.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                break;
            case 3: //links
                miner.GetComponent<SpriteRenderer>().flipX = true;
                miner.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = true;

                pos.x += heatPositions[1].x;
                pos.y += heatPositions[1].y;

                persistentParent.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                break;
        }

        if (dir == 1 || dir == 0)
        { //rechts oder hoch
            wing.transform.rotation = Quaternion.identity;

            pos.x += heatPositions[0].x;
            pos.y += heatPositions[0].y;
        }

        heatEffect.transform.position = pos;
    }

    public void StartStopMine(int dir = 0)
    {

        if(!isMining)
        {
            //wasDown = false;

            switch(dir)
            {
                case 2: //runter
                    //wasDown = true;

                    break;
            }

            isMining = true;
            effect.SetActive(true);

            persistentParent.transform.GetChild(0).gameObject.SetActive(true);
            persistentParent.transform.GetChild(1).gameObject.SetActive(true);

            ParticleSystem.EmissionModule main = pSystem[0].emission;
            main.rateOverTime = 30;

            /*if(pSystemCount == 0)
            {
                pSystemCount++;
                pSystem[0].Play();
            } else
            {
                pSystemCount = 0;
                pSystem[1].Play();
            }*/

        } else
        {
            /*if (wasDown)
            {
                if(!GetComponent<FF_PlayerData>().IsDownPressed())
                {
                    //wing.transform.rotation = Quaternion.identity;
                }
            }*/

            isMining = false;
            effect.SetActive(false);

            //nur licht ausschalten
            persistentParent.transform.GetChild(1).gameObject.SetActive(false);

            ParticleSystem.EmissionModule main = pSystem[0].emission;
            main.rateOverTime = 0;
        }
    }
}
