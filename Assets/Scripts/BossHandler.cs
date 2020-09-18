using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BossHandler : MonoBehaviour
{
    public static BossHandler Instance;
    public GameObject mieserBoss, currentBoss, fireEffect, player,
        laserObj;

    public Sprite[] laserSprites;

    private bool isActive = false, bossIdle = false, laserActive = false,
        fullReload = true, infoShowed = false, direction = false;
    private float reloadTime, maxReloadTime = 5f, idleSpeed = 50f;

    private int shootCount = 0, laserAnCount = 0, attackMode = 0, diff = 0, maxShots = 3;

    private const float defaultIdleSpeed = 50f;
    private ObjectPooler pooler;
    private MieserHandler mieserHandler = null;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        pooler = ObjectPooler.Instance;
    }

    public bool GetActive()
    {
        return isActive;
    }

    public void StartBoss(ulong score)
    {
        isActive = true;

        if (score < 50)
        {
            diff = 0;

            idleSpeed = 50f;
            maxReloadTime = 5f;
            maxShots = 3;
        }
        else if (score < 100)
        {
            diff = 1;

            idleSpeed = 60f;
            maxReloadTime = 4f;
            maxShots = 3;
        }
        else if (score < 150)
        {
            diff = 2;

            idleSpeed = 65f;
            maxReloadTime = 4f;
            maxShots = 4;
        }
        else
        {
            diff = 3;

            idleSpeed = 70f;
            maxReloadTime = 3.5f;
            maxShots = 4;
        }

        currentBoss = mieserBoss;

        currentBoss.SetActive(true);
        mieserHandler = currentBoss.GetComponent<MieserHandler>();
        mieserHandler.StartMieser(diff);

        float startX = 223;
        float startY = Random.Range(362, 602);

        fireEffect = currentBoss.transform.GetChild(3).GetChild(2).gameObject;

        currentBoss.transform.position = new Vector3(startX, startY);
        currentBoss.transform.DOMoveX(-114, 2f);

        reloadTime = maxReloadTime;

        attackMode = 0;//Random.Range(0, 2);
        fullReload = false;

        FlatterFogelHandler.Instance.StartZoomOnBoss(new Vector3(-192, startY), 0.5f, 2f);

        //InvokeRepeating("PlayerShoot", 1f, 0.5f);
        InvokeRepeating(nameof(LaserState), 0f, 0.5f);
        Invoke(nameof(StartIdle), 2.1f);
    }

    public void StopBoss(float moveTime = 2f)
    {
        if (!isActive) return;

        currentBoss.transform.DOMoveX(223, moveTime);

        //CancelInvoke("PlayerShoot");
        CancelInvoke(nameof(ResetLaser));
        CancelInvoke(nameof(LaserState));
        Invoke(nameof(EndBoss), moveTime + 0.01f);

        ResetLaser();

        laserObj.SetActive(false);
        SoundManager.Instance.StopLaserSound();

        isActive = false;
        bossIdle = false;
    }

    private void EndBoss()
    {
        currentBoss.SetActive(false);
    }

    private void LaserState()
    {
        if (attackMode != 1 || !laserActive) return;

        if(laserAnCount == 0)
        {
            laserAnCount = 1;
            laserObj.SetActive(false);
            SoundManager.Instance.StopLaserSound();
        } else
        {
            laserAnCount = 0;
            laserObj.SetActive(true);
            SoundManager.Instance.PlaySound(Sound.Laser);
        }

        laserObj.GetComponent<SpriteRenderer>().sprite = laserSprites[laserAnCount];
    }

    private void StartIdle()
    {
        bossIdle = true;
        //topY = currentBoss.transform.position.y + 10;
        //bottomY = currentBoss.transform.position.y - 10;
    }

    void ResetLaser()
    {
        laserObj.SetActive(false);
        laserActive = false;
        reloadTime = maxReloadTime;
        fullReload = true;
        idleSpeed = defaultIdleSpeed;

        infoShowed = false;

        SoundManager.Instance.StopLaserSound();
    }

    public void PlayerShoot()
    {
        if (!isActive || !bossIdle) return;

        Vector3 playerPos = player.transform.position;
        playerPos.x += 53;

        GameObject newP = pooler.SpawnFromPool("Projectile", playerPos, Quaternion.identity);
        newP.GetComponent<CircleCollider2D>().isTrigger = true;
        newP.GetComponent<Rigidbody2D>().velocity = new Vector2(750, 0);
    }

    public void BossDie(float timeTilDeath)
    {
        bossIdle = false;

        laserObj.SetActive(false);

        StopBoss(2f);
        //Invoke("DeathTimeout", 5f);
    }

    private void DeathTimeout()
    {
        StopBoss(2f);
    }

    // Update is called once per frame
    void Update()
    {
        if(isActive)
        {
            if(bossIdle)
            {
                if(direction)
                { //runter
                    currentBoss.transform.Translate(0, -idleSpeed * Time.deltaTime, 0);

                    if(currentBoss.transform.position.y < 342)
                    {
                        direction = false;
                    }
                } else
                { //hoch
                    currentBoss.transform.Translate(0, idleSpeed * Time.deltaTime, 0);

                    if (currentBoss.transform.position.y > 805)
                    {
                        direction = true;
                    }
                }


                Vector3 currentTarget = currentBoss.transform.position;
                currentTarget.x -= 596.25f;
                currentTarget.y += 572.25f;

                //Vector3 playerPos = player.transform.position;

                if(attackMode == 1)
                {
                    currentTarget = laserObj.transform.position;
                }

                /*if(playerPos.y > currentTarget.y)
                {
                    bool tooLow = false;

                    if (currentBoss.transform.position.y < 806)
                    { //nach oben korrigieren
                        tooLow = true;
                    }

                    if(tooLow || attackMode == 1)
                    {
                        currentBoss.transform.Translate(0, idleSpeed * Time.deltaTime, 0);
                    }
                } else if(playerPos.y < currentTarget.y)
                {
                    bool tooHigh = false;

                    if (currentBoss.transform.position.y > 341)
                    { //nach unten korrigieren
                        tooHigh = true;
                    }

                    if(tooHigh //|| attackMode == 1)
                    {
                        currentBoss.transform.Translate(0, -idleSpeed * Time.deltaTime, 0);
                    }
                }*/

                if(!mieserHandler.IsDead())
                {
                    reloadTime -= Time.deltaTime;

                    if (reloadTime <= 1 && !laserActive)
                    {
                        if (!infoShowed)
                        {
                            infoShowed = true;

                            Vector3 infoPos = currentBoss.transform.position;
                            infoPos.x += 14.25f;
                            infoPos.y += 38.25f;

                            GameObject infoText =
                                pooler.SpawnFromPool("InfoText", infoPos, Quaternion.identity);
                            infoText.GetComponent<InfoText>().StartFlashing("*FAUCH*");
                        }
                    }

                    if (reloadTime <= 0 && !laserActive)
                    { //projektil feuern // laser starten
                        if (fullReload)
                        {
                            fullReload = false;
                            /*if (attackMode == 0)
                            {
                                attackMode = 1;
                            }
                            else
                            {
                                attackMode = 0;
                            }*/
                            //attackMode = 1;//Random.Range(0, 2);
                        }

                        if (attackMode == 0)
                        {
                            fireEffect.SetActive(false);
                            fireEffect.SetActive(true);

                            //59 hoch 80 links
                            Vector3 bossPos = currentBoss.transform.position;
                            bossPos.x -= 80;
                            bossPos.y += 59;

                            Vector2 currentDir = currentTarget - currentBoss.transform.position;
                            currentDir = currentDir.normalized;

                            //option check nicht nötig da in funktion eingebaut
                            FlatterFogelHandler.Instance.StartCameraShake(0.15f);

                            GameObject newMinus = pooler.SpawnFromPool("Minus", bossPos, Quaternion.identity);
                            newMinus.GetComponent<MinusHandler>().StartMinus(currentDir, 500, player);

                            SoundManager.Instance.PlaySound(Sound.MinusShoot);

                            shootCount++;
                            if (shootCount == maxShots)
                            {
                                fullReload = true;
                                reloadTime = maxReloadTime;
                                infoShowed = false;
                                shootCount = 0;
                            }
                            else
                            {
                                reloadTime = 0.3f;
                            }
                        }
                        else if (attackMode == 1)
                        { //laser
                            idleSpeed = 125;

                            SoundManager.Instance.PlaySound(Sound.Laser);

                            laserActive = true;
                            laserObj.gameObject.SetActive(true);
                            Invoke(nameof(ResetLaser), Random.Range(2.5f, 5.5f));
                        }
                    }
                }
            }
        }
    }
}
