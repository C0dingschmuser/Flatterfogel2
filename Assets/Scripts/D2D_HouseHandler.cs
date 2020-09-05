using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using Destructible2D;

public class D2DDestructionEvent {
    public int houseID = 0;
    public int frameWait = 0;
}

public class D2D_HouseHandler : MonoBehaviour
{
    public DestructionMode stats;
    public float yStartPos, deathY, lowestY, lifeTime = 0;
    public bool destroyed = false, endInvoked = false, completeDestroyed = false, wasHit = false, scoreAdded = false, weaponDone = false,
        rdyEnable = false;
    public Vector3 lastHitPos;
    public int defChildCount = 3, currentWeaponDamage = 0, maxWeaponDamage = 0, basePoint;

    private GameObject[] fireEffects = new GameObject[3];

    private List<D2DDestructionEvent> destructionEvents = new List<D2DDestructionEvent>();
    private List<D2DDestructionEvent> delList = new List<D2DDestructionEvent>();

    [SerializeField]
    private Transform d2dParent = null;
    [SerializeField]
    private Transform particleParent = null;

    private bool childsCalled = false, childsKinetic = false, fractureCalled = false;
    private int fireEffectCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Transform GetD2DObjParent()
    {
        return d2dParent;
    }

    public void WeaponHit(Vector3 lastPos, int newDamage, GameObject hitObj)
    {
        lastHitPos = lastPos;
        if (weaponDone) return;

        currentWeaponDamage += newDamage;

        if(currentWeaponDamage >= maxWeaponDamage)
        {
            weaponDone = true;

            if(stats.instantFracture)
            {
                hitObj.GetComponent<D2dFracturer>().Fracture();
            } else
            {
                hitObj.GetComponent<Rigidbody2D>().isKinematic = false;
            }

            FractureScore(hitObj);
        }
    }

    public bool UpdateHouseHandler(float scrollSpeed)
    {

        transform.Translate((-scrollSpeed - stats.speed) * Time.deltaTime, 0, 0);

        bool ok = true;

        if(d2dParent.childCount == 0)
        {
            ok = false;
        }

        if(ok)
        {
            if (transform.position.x < -877 && !childsCalled)
            {
                childsCalled = true;

                for (int i = 0; i < d2dParent.childCount; i++)
                {
                    d2dParent.transform.GetChild(i).GetComponent<D2dDestroyer>().enabled = true;
                }
            }
        }

        if(!ok && !endInvoked)
        {
            endInvoked = true;
            Invoke(nameof(End), 2f);
        }

        return ok;
    }

    public void FractureScore(GameObject caller = null)
    {
        if (scoreAdded) return;

        scoreAdded = true;

        int score = basePoint;

        if(!(lastHitPos.x != 0) && basePoint > 0)
        {
            lastHitPos = transform.GetChild(0).GetChild(0).position;

            score += 1; //indirekter hit -> extra punkt
        }

        deathY = lastHitPos.y;

        if (stats.type == DestructionMode.Type.Flak)
        {
            GetComponent<Flakhandler>().enabled = false;
        }
        else if(stats.type == DestructionMode.Type.EnemyPlaneSmall1)
        {
            GetComponent<EnemyPlaneHandler>().enabled = false;
        }

        GameObject infoText =
                ObjectPooler.Instance.SpawnFromPool("InfoText", lastHitPos, Quaternion.identity);

        string sym = "+";

        if(score < 0)
        {
            sym = "";
        }
        infoText.GetComponent<InfoText>().StartFlashing(sym + score.ToString(), 0, 0, 1, true, 1, 5);

        if(score > 0)
        {
            FlatterFogelHandler.Instance.SetScore(score, 1, 1);
        } else if(score < 0)
        {
            FlatterFogelHandler.Instance.SetScore(score, -1, 1);
        }

    }

    public void FractureStarted()
    {
        if (fractureCalled) return;

        if (stats.type == DestructionMode.Type.Flak)
        {
            GetComponent<Flakhandler>().enabled = false;
        } else if(stats.type == DestructionMode.Type.EnemyPlaneSmall1)
        {
            GetComponent<EnemyPlaneHandler>().enabled = false;
        }

        MiddleDestroyed();

        fractureCalled = true;

        for(int i = 0; i < fireEffectCount; i++)
        {
            fireEffects[i].transform.SetParent(ObjectPooler.Instance.transform);

            //fireEffects[i].SetActive(false);

            fireEffects[i].transform.GetChild(0).GetComponent<ParticleSystem>().Stop();

            fireEffects[i].transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
            fireEffects[i].transform.GetChild(1).GetComponent<ParticleSystem>().Clear();
            fireEffects[i] = null;
        }

        SoundManager.Instance.PlaySound(SoundManager.Sound.DModeExplosion);

        if(defChildCount > 1)
        {
            FF_PlayerData.Instance.lastBlusPosition =
                d2dParent.transform.GetChild(1).position;
        } else
        {
            FF_PlayerData.Instance.lastBlusPosition =
                d2dParent.transform.GetChild(0).position;
        }

        FractureScore();

        if(particleParent != null)
        {
            particleParent.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        }
    }

    public bool NotFractured()
    {
        if(d2dParent.childCount > defChildCount)
        {
            return false;
        } else
        {
            return true;
        }
    }

    public void AddFireEffect(GameObject fireEffect)
    {
        fireEffects[fireEffectCount] = fireEffect;
        fireEffectCount++;
    }

    public bool FireSpawnOk()
    {
        if(fireEffectCount >= 3)
        {
            return false;
        } else
        {
            return true;
        }
    }

    private void End()
    {
        Destroy(this.gameObject);
    }

    public void MiddleDestroyed()
    {
        if (childsKinetic) return;

        childsKinetic = true;

        for(int i = 0; i < d2dParent.childCount; i++)
        {
            d2dParent.GetChild(i).GetComponent<Rigidbody2D>().isKinematic = false;

            if(stats.supportsSwap)
            {
                d2dParent.GetChild(i).GetComponent<D2dSwap>().Swap();
            }
        }
    }

    public void FractureAll()
    {
        destroyed = true;

        for (int i = 0; i < d2dParent.childCount; i++)
        {
            d2dParent.GetChild(i).GetComponent<D2dFracturer>().Fracture();
            d2dParent.GetChild(i).GetComponent<D2dDestroyer>().enabled = true;
        }
    }

    public void DestroyCompleteBuilding()
    {
        if (destroyed || d2dParent.childCount == 0 || completeDestroyed) return;

        wasHit = true;
        completeDestroyed = true;

        if(defChildCount == 1)
        {
            DestroyPart(transform.GetChild(0).GetChild(0));
        } else
        {
            for (int i = 0; i < d2dParent.childCount; i++)
            {
                DestroyChild(i);
            }
        }
    }

    public void DestroyChild(int id)
    {
        D2DDestructionEvent dEvent = new D2DDestructionEvent
        {
            houseID = id,
            frameWait = id
        };

        destructionEvents.Add(dEvent);
    }

    public void DestroyAll()
    {
        if (destroyed) return;
        destroyed = true;

        for (int i = 0; i < d2dParent.childCount; i++)
        {
            d2dParent.GetChild(i).GetComponent<D2dDestroyer>().Life = 0.1f;
            d2dParent.GetChild(i).GetComponent<D2dDestroyer>().enabled = true;
        }

        if (stats.type == DestructionMode.Type.Flak)
        {
            GetComponent<Flakhandler>().enabled = false;
        }
        else if (stats.type == DestructionMode.Type.EnemyPlaneSmall1)
        {
            GetComponent<EnemyPlaneHandler>().enabled = false;
        } 

        for (int i = 0; i < fireEffectCount; i++)
        {
            if(fireEffects[i] != null)
            {
                fireEffects[i].transform.SetParent(ObjectPooler.Instance.transform);

                //fireEffects[i].SetActive(false);

                fireEffects[i].transform.GetChild(0).GetComponent<ParticleSystem>().Stop();

                fireEffects[i].transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
                fireEffects[i].transform.GetChild(1).GetComponent<ParticleSystem>().Clear();
                fireEffects[i] = null;
            }
        }

        if(particleParent != null)
        {
            particleParent.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        }

        if(!endInvoked)
        {
            endInvoked = true;
            Invoke(nameof(End), 2f);
        }
    }

    private void DestroyPart(Transform part)
    {
        part.GetComponent<Rigidbody2D>().isKinematic = false;
        part.GetComponent<D2dDestroyer>().enabled = true;
        part.GetComponent<D2dFracturer>().Fracture();
    }

    private void OnEnable()
    {
        if(rdyEnable)
        {
            rdyEnable = false;
        } else
        {
            this.enabled = false;
        }
    }

    private void Update()
    {
        int len = destructionEvents.Count;
        for(int i = 0; i < len; i++)
        {
            if(destructionEvents[i].frameWait <= 0)
            {
                if(transform.GetChild(0).childCount > destructionEvents[i].houseID)
                {
                    Transform child = transform.GetChild(0).GetChild(destructionEvents[i].houseID);

                    DestroyPart(child);
                } else
                {
                    Debug.LogWarning("Trying to Delete child " +
                        destructionEvents[i].houseID + " of " + transform.gameObject.name);
                }

                delList.Add(destructionEvents[i]);
            } else
            {
                destructionEvents[i].frameWait--;
            }
        }

        len = delList.Count;
        for(int i = 0; i < len; i++)
        {
            destructionEvents.Remove(delList[i]);
        }

        delList.Clear();

        if(stats.doRotate)
        {
            if(scoreAdded && !destroyed && !fractureCalled)
            {
                lifeTime += Time.deltaTime;

                float rotation = ((lifeTime / 1.5f) * 90);

                d2dParent.transform.GetChild(0).rotation = Quaternion.Euler(0, 0, rotation);
            }
        }
    }
}
