using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinHandler : MonoBehaviour
{ //Klasse obsolet
    public Transform deathEffect, image;
    public bool destroyed = false;

    private float timer = 4f, maxTimer = 4f;

    public void ResetCoin()
    {
        deathEffect.gameObject.SetActive(false);
        image.gameObject.SetActive(true);

        destroyed = false;
        timer = maxTimer;
    }

    public void DestroyCoin()
    {
        destroyed = true;

        //ScoreHandler.Instance.AddCoin();

        image.gameObject.SetActive(false);
        deathEffect.gameObject.SetActive(true);
    }

    public bool UpdateCoin(float scrollSpeed)
    {
        bool ok = true;

        transform.Translate(-scrollSpeed * Time.deltaTime, 0, 0);

        if((transform.position.x < -790 && destroyed) || timer < 0)
        {
            ok = false;
        }

        if(destroyed)
        {
            if (timer >= 0)
            {
                timer -= Time.deltaTime;
            }
        }

        return ok;
    }
}
