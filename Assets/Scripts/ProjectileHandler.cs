using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Destructible2D;

public class ProjectileHandler : MonoBehaviour
{
    public int damage = 0;
    public UnityEngine.Experimental.Rendering.Universal.Light2D lightObj;
    private bool d2dProjectile = false, explosionCalled = false;

    public void ResetProjectile(bool d2d = false)
    {
        d2dProjectile = d2d;
        explosionCalled = false;
        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Rigidbody2D>().simulated = true;

        if(d2d)
        {
            GetComponent<CircleCollider2D>().isTrigger = false;
        } else
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
        }
        lightObj.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!d2dProjectile || explosionCalled) return;

        explosionCalled = true;

        lightObj.enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<CircleCollider2D>().enabled = false;

        Vector3 point = collision.GetContact(0).point;
        point.z = transform.position.z;

        //point.x += 10;

        transform.position = point;

        float mp = collision.gameObject.GetComponent<D2D_DamageHolder>().multiplier;
        collision.transform.parent.parent.GetComponent<D2D_HouseHandler>().
            WeaponHit(transform.position, damage, collision.gameObject);

        FlatterFogelHandler.Instance.D2D_Hit(point, collision.gameObject);

        GetComponent<D2dExplosion>().StartExplosion(mp);
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.x > -9.1f)
        {
            gameObject.SetActive(false);
        }

        //transform.Translate(750 * Time.deltaTime, 0, 0);
    }
}
