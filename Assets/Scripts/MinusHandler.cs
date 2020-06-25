using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MinusHandler : MonoBehaviour
{
    private bool isActive = false, aimAssist = false, exploded = false;
    public GameObject lightObj, player;

    [SerializeField]
    private ParticleSystem trailSystem = null;

    private Vector2 dir;
    private float speed = 100f, targetY = -999, defaultEmission = 10;

    private void ResetMinus()
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
        GetComponent<Rigidbody2D>().simulated = true;

        ParticleSystem.EmissionModule em = trailSystem.emission;
        em.rateOverTime = defaultEmission;
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

    public void StartMinusFlak(GameObject player, float speed)
    {
        transform.localScale = new Vector3(40, 40, 40);

        this.player = player;
        ResetMinus();

        exploded = false;

        Vector3 target = PredictedPosition(player.transform.position,
            transform.position, new Vector3(0, 0, 0), speed);

        target.x += Random.Range(-100, 200);
        target.y += Random.Range(-150, 75);

        dir = target - transform.position;
        dir = dir.normalized;

        this.targetY = player.transform.position.y;
        this.speed = speed;
    }

    public void StartMinus(Vector2 dir, float speed, GameObject player)
    {
        transform.localScale = new Vector3(83.936f, 83.936f, 83.936f);

        this.player = player;

        ResetMinus();

        this.dir = dir;
        this.speed = speed;

        Invoke("StartAimAssist", 0.5f);
        Invoke("EndMinus", 7f);
    }

    private void EndMinus()
    {
        gameObject.SetActive(false);
    }

    private void StartAimAssist()
    {
        aimAssist = true;
    }

    private void Explode()
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
        Invoke("DisableMinus", 3f);
    }

    private void DisableMinus()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
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
