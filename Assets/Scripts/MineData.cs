using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MineData : MonoBehaviour
{
    //public uint id;
    public int arrayPos, xPos, depth;
    public float vibrateDiff = 4, lavaEndInten = 0.2f;
    public bool mined = false, blussizin = false, unmineable = false, bigstone = false,
        vibrate = false, deathActive = true, lava = false;

    public string deathName = "";

    public Item mineral;
    public Transform[] blussizinLightParent;
    public BlussizinLightHolder[] blussizinLights;
    public UnityEngine.Experimental.Rendering.Universal.Light2D lavaLight;

    private float mP = 0.1f, duration = 0.5f;
    private int blussizinParentID = 0;

    public class BlussizinLightHolder {
        public UnityEngine.Experimental.Rendering.Universal.Light2D light;
        public int blendMode = 0;
    }

    private void LoadLights()
    {
        blussizinLights = 
            new BlussizinLightHolder[blussizinLightParent[blussizinParentID].childCount];

        for (int i = 0; i < blussizinLightParent[blussizinParentID].childCount; i++)
        {
            BlussizinLightHolder bH = new BlussizinLightHolder
            {
                light = blussizinLightParent[blussizinParentID].GetChild(i).
                    GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>()
            };
            blussizinLights[i] = bH;
        }
    }

    public void ResetMineData()
    {
        transform.GetChild(2).gameObject.SetActive(false); //lava deaktiveren

        lava = false;
    }

    public void StartBlussizinLight(int id = 0)
    {
        blussizinParentID = id;
        LoadLights();

        duration = 0.5f;

        blussizin = true;
        blussizinLightParent[blussizinParentID].gameObject.SetActive(true);

        for(int i = 0; i < blussizinLights.Length; i++)
        {
            blussizinLights[i].blendMode = Random.Range(0, 2);
            blussizinLights[i].light.intensity = Random.Range(0.05f, 0.25f);
        }
    }

    public void StartLava()
    {
        transform.GetChild(2).gameObject.SetActive(true);

        duration = 0.1f;

        lava = true;
    }

    public void StopBlussizinLights()
    {
        blussizin = false;
        blussizinLightParent[blussizinParentID].gameObject.SetActive(false);
    }

    public bool IsOkayToMine()
    {
        bool ok = true;

        if(mined || unmineable)
        {
            ok = false;
        }

        return ok;
    }

    public void StartVibrate()
    {
        vibrate = true;
    }

    public void EnableFall()
    {
        vibrate = false;
        transform.GetChild(1).GetComponent<Rigidbody2D>().simulated = true;
    }

    public void CollisionEnter2D(GameObject caller, Collision2D collision)
    {
        GameObject obj = collision.gameObject;

        if(bigstone)
        {
            if(obj.CompareTag("MineGround") && deathActive)
            {
                if(obj.transform.position.y < caller.transform.position.y)
                {
                    deathActive = false;
                    Invoke(nameof(StartStoneFade), 2f);
                }
            }
        }
    }

    private void StartStoneFade()
    {
        transform.GetChild(1).GetComponent<SpriteRenderer>().DOFade(0, 0.25f);
        Invoke(nameof(EndStoneFade), 0.26f);
    }

    private void EndStoneFade()
    {
        Destroy(transform.GetChild(1).gameObject);
    }

    private void Update()
    {
        if(blussizin)
        {
            for(int i = 0; i < blussizinLights.Length; i++)
            {
                if(blussizinLights[i].blendMode == 0)
                {
                    blussizinLights[i].light.intensity += mP * Time.deltaTime * duration;
                    if(blussizinLights[i].light.intensity >= 0.3f)
                    {
                        blussizinLights[i].blendMode = 1;
                    }
                } else
                {
                    blussizinLights[i].light.intensity -= mP * Time.deltaTime * duration;
                    if (blussizinLights[i].light.intensity <= 0)
                    {
                        blussizinLights[i].light.intensity = 0;
                        blussizinLights[i].blendMode = 0;
                    }
                }
            }
        } else if(vibrate)
        {
            if(bigstone)
            {
                Vector3 defPos = transform.position;
                defPos.x += Random.Range(-vibrateDiff, vibrateDiff);
                defPos.y += Random.Range(-vibrateDiff, vibrateDiff);

                transform.GetChild(1).position = defPos;
            }
        } else if(lava)
        {
            lavaLight.intensity -= Time.deltaTime * duration;
            if(lavaLight.intensity < lavaEndInten)
            {
                lavaLight.intensity = Random.Range(0.4f, 0.6f);
                lavaEndInten = lavaLight.intensity - Random.Range(0.1f, 0.4f);
            }
        }
    }
}
