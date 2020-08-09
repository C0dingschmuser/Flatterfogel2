using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using DG.Tweening;

public class MinusHandler : MonoBehaviour
{
    private bool isActive = false, aimAssist = false, exploded = false;
    public GameObject lightObj, player;

    [SerializeField]
    private ParticleSystem trailSystem = null;

    private CoroutineHandle mainUpdate;
    private Vector3 targetPosition;
    private Vector2 dir;
    private float speed = 100f, targetY = -999, defaultEmission = 10;

    private void ResetMinus(bool start = false)
    {
        isActive = true;

        if (OptionHandler.lightEnabled == 1)
        {
            lightObj.SetActive(true);
        }
        else
        {
            lightObj.SetActive(false);
        }

        GetComponent<SpriteRenderer>().enabled = true;

        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<CircleCollider2D>().isTrigger = true;

        GetComponent<Rigidbody2D>().simulated = true;

        ParticleSystem.EmissionModule em = trailSystem.emission;
        em.rateOverTime = defaultEmission;

        if(start)
        {
            mainUpdate = Timing.RunCoroutine(Util._EmulateUpdate(_MainUpdate, this));
        }
    }

    public void StartMinusFlak(GameObject player, Vector3 target, float speed)
    {
        transform.localScale = new Vector3(30, 30, 30);

        targetPosition = target;

        this.player = player;
        ResetMinus(true);

        exploded = false;

        dir = target - transform.position;
        dir = dir.normalized;

        this.targetY = player.transform.position.y;
        this.speed = speed;

        transform.DOScale(65, 0.25f);
    }

    public void StartMinus(Vector2 dir, float speed, GameObject player)
    {
        transform.localScale = new Vector3(83.936f, 83.936f, 83.936f);

        this.player = player;

        ResetMinus(true);

        this.dir = dir;
        this.speed = speed;

        Invoke(nameof(StartAimAssist), 0.5f);
        Invoke(nameof(EndMinus), 7f);
    }

    private void EndMinus()
    {
        gameObject.SetActive(false);
    }

    private void StartAimAssist()
    {
        aimAssist = true;
    }

    public void Explode()
    {
        exploded = true;

        ParticleSystem.EmissionModule em = trailSystem.emission;
        em.rateOverTime = 0;

        lightObj.SetActive(false);
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;

        isActive = false;

        FlatterFogelHandler.Instance.AddFlakEffect(transform.position);
        Invoke(nameof(DisableMinus), 3f);
    }

    private void DisableMinus()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Timing.KillCoroutines(mainUpdate);
    }

    // Update is called once per frame
    void _MainUpdate()
    {
        //Debug.Log(isActive);

        if(isActive)
        {
            Vector3 pos = transform.position;

            if(aimAssist)
            {
                Vector2 newDir = player.transform.position - pos;
                newDir = newDir.normalized;

                //lineare richtungsänderung
                DOTween.To(() => dir, x => dir = x, newDir, 0.5f).SetEase(Ease.Linear);

                //speed = 400;

                aimAssist = false;
            }

            if(!exploded)
            {
                pos.x += dir.x * speed * Time.deltaTime;
                pos.y += dir.y * speed * Time.deltaTime;

                if (targetY > -999)
                {
                    if(targetY - pos.y < 10)
                    {
                        Explode();
                    }
                }

                if (pos.x <= -800 || pos.y >= 2300)
                { //out of bounds
                    isActive = false;
                    gameObject.SetActive(false);
                }

                transform.position = pos;
            }

        }
    }
}
