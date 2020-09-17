using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;

public class Flakhandler : MonoBehaviour
{
    public GameObject topFlak, player;

    public bool onPipe = false;

    [SerializeField]
    private Transform[] barrelObjs = null;

    private ObjectPooler objectPooler;
    private CoroutineHandle mainHandle;
    
    private bool canShoot = false;

    private int currentBarrel = 0, diff = 0;
    private float reload = 1f, maxReload = 1f, bulletSpeed = 350;

    private void Awake()
    {
        objectPooler = ObjectPooler.Instance;
    }

    private void Start()
    {
        player = FF_PlayerData.Instance.gameObject;

        if(onPipe)
        {
            bulletSpeed = 350;
            maxReload = 2.5f;
            //maxReload = 0.1f;
            //bulletSpeed = 400;
        } else
        {
            reload = 2f;
            maxReload = 2f;
            bulletSpeed = 300;
        }
    }

    private Vector3 PredictedPosition(Vector3 targetPosition, Vector3 shooterPosition, Vector3 targetVelocity, float projectileSpeed)
    {
        Vector3 displacement = targetPosition - shooterPosition;
        float targetMoveAngle = Vector3.Angle(-displacement, targetVelocity) * Mathf.Deg2Rad;
        //if the target is stopping or if it is impossible for the projectile to catch up with the target (Sine Formula)
        if (targetVelocity.magnitude == 0 || targetVelocity.magnitude > projectileSpeed && Mathf.Sin(targetMoveAngle) / projectileSpeed > Mathf.Cos(targetMoveAngle) / targetVelocity.magnitude)
        {
            //Debug.Log("Position prediction is not feasible.");
            return targetPosition;
        }
        //also Sine Formula
        float shootAngle = Mathf.Asin(Mathf.Sin(targetMoveAngle) * targetVelocity.magnitude / projectileSpeed);
        return targetPosition + targetVelocity * displacement.magnitude / Mathf.Sin(Mathf.PI - targetMoveAngle - shootAngle) * Mathf.Sin(shootAngle) / targetVelocity.magnitude;
    }

    public static float Clamp0360(float eulerAngles)
    {
        float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;
        if (result < 0)
        {
            result += 360f;
        }
        return result;
    }

    private void OnEnable()
    {
        Timing.KillCoroutines(mainHandle);
        mainHandle = Timing.RunCoroutine(Util._EmulateUpdate(_MainUpdate, this));

        if(onPipe)
        {
            ulong score = FlatterFogelHandler.Instance.GetScore();

            score = 25;

            if(score < 50)
            {
                diff = 0;

                bulletSpeed = 290;
                maxReload = 3.25f;
            } else if(score < 100)
            {
                diff = 1;

                bulletSpeed = 300;
                maxReload = 3f;
            } else if(score < 150)
            {
                diff = 2;

                bulletSpeed = 325;
                maxReload = 2.5f;
            } else
            {
                diff = 3;

                bulletSpeed = 350;
                maxReload = 2.5f;
            }
        }
    }

    private void OnDisable()
    {
        Timing.KillCoroutines(mainHandle);
    }

    // Update is called once per frame
    void _MainUpdate()
    {
        Vector3 pos = PredictedPosition(player.transform.position,
            topFlak.transform.position, new Vector3(FlatterFogelHandler.scrollSpeed / 2, 0, 0), bulletSpeed);

        //Debug.Log(pos);

        pos.z = topFlak.transform.position.z;

        Vector3 targetVector = pos - topFlak.transform.position;
        targetVector.Normalize();

        //topFlak.transform.up = targetVector;//pos - topFlak.transform.position;

        //float z = topFlak.transform.rotation.eulerAngles.z;

        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, targetVector);
        topFlak.transform.rotation = rotation;

        bool tmpShootOk = true;

        float z = rotation.eulerAngles.z;

        if(z > 63 && z < 180)
        {
            tmpShootOk = false;
            topFlak.transform.rotation = Quaternion.Euler(0, 0, 63);
        } else if(z > 180 && z < 317)
        {
            tmpShootOk = false;
            topFlak.transform.rotation = Quaternion.Euler(0, 0, 317);
        }

        if(onPipe && !ShootingPipeHandler.Instance.shootingOK)
        { //erst schießen wenn erster zoom fertig
            return;
        }

        canShoot = tmpShootOk;

        reload -= Time.deltaTime;

        if(reload <= 0 && canShoot)
        {
            float dist = Vector3.Distance(pos, topFlak.transform.position);

            if(dist < 1200)
            {
                reload = maxReload;

                GameObject newMinus = objectPooler.SpawnFromPool("Minus",
                    barrelObjs[currentBarrel].position, Quaternion.identity);

                //pos.x += Random.Range(-100, 200);
                //pos.y += Random.Range(-150, 75);

                //Debug.Log(pos);

                pos = PredictedPosition(player.transform.position,
                    topFlak.transform.position, new Vector3(0, 0, 0), bulletSpeed);

                if(onPipe)
                {
                    switch (diff)
                    {
                        default:
                            pos.x += Random.Range(60, 80);
                            pos.y += Random.Range(60, 80);
                            break;
                        case 1:
                            pos.x += Random.Range(30, 60);
                            pos.y += Random.Range(30, 60);
                            break;
                        case 2:
                            pos.x += Random.Range(15, 30);
                            pos.y += Random.Range(15, 30);
                            break;
                        case 3:
                            pos.x += Random.Range(0, 7);
                            pos.y += Random.Range(0, 7);
                            break;
                    }
                }

                pos.z = topFlak.transform.position.z;

                newMinus.GetComponent<MinusHandler>().StartMinusFlak(player, pos, bulletSpeed);

                if (currentBarrel == 0)
                {
                    currentBarrel = 1;
                }
                else
                {
                    currentBarrel = 0;
                }
            }
        }

        if(transform.position.x < -639)
        {
            this.enabled = false;
        }
    }
}
