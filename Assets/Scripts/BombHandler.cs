using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Destructible2D;
using DG.Tweening;

public class BombHandler : MonoBehaviour
{
    private bool explosionCalled = false;
    private float lifeTime = 0f;

    [SerializeField]
    private AnimationCurve explosionCurve = null;

    [SerializeField]
    private GameObject explosionEfect = null;

    [SerializeField]
    private ParticleSystem trail = null;

    [SerializeField]
    private SpriteRenderer explosionSprite = null;

    public void ResetBomb(float xForce = 250)
    {
        explosionCalled = false;

        GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Rigidbody2D>().simulated = true;

        explosionEfect.SetActive(false);

        explosionSprite.color = Color.white;

        ParticleSystem.EmissionModule main = trail.emission;
        main.rateOverTime = 150;

        lifeTime = 0f;

        GetComponent<Rigidbody2D>().velocity = new Vector2(xForce, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (explosionCalled) return;

        explosionCalled = true;

        Vector3 point = collision.GetContact(0).point;
        point.z = transform.position.z;

        transform.position = point;

        BombExplosion(collision.gameObject);
    }

    private void BombExplosion(GameObject collider = null)
    {
        ParticleSystem.EmissionModule main = trail.emission;
        main.rateOverTime = 0;

        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;

        explosionEfect.SetActive(true);

        if(collider != null)
        {
            float mp = collider.GetComponent<D2D_DamageHolder>().multiplier;
            GetComponent<D2dExplosion>().StartExplosion(mp, true);

            collider.transform.parent.parent.GetComponent<D2D_HouseHandler>().lastHitPos = transform.position;
            collider.transform.parent.parent.GetComponent<D2D_HouseHandler>().DestroyCompleteBuilding();
        }

        Invoke("DisableExplosionSprite", 0.1f);
        Invoke("DisableBomb", 2f);
    }

    private void DisableExplosionSprite()
    {
        explosionSprite.DOFade(0, 0.25f);
    }

    private void DisableBomb()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if(!explosionCalled)
        {
            lifeTime += Time.deltaTime;

            float rotation = 90 - ((lifeTime / 2f) * 90);
            transform.rotation = Quaternion.Euler(0, 0, rotation);
        } else
        {
            if(FlatterFogelHandler.Instance.gameActive)
            {
                transform.Translate(new Vector3(-FlatterFogelHandler.scrollSpeed * Time.deltaTime, 0, 0));
            }
        }

        if(transform.position.y < 253)
        {
            BombExplosion();
        }
    }
}
