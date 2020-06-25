using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FlyCloudHandler : MonoBehaviour
{
    [SerializeField] private float aliveTime;

    public void StartCloud(Vector2 vel, Sprite sprite, int zRot, float aliveTime = 1f)
    {
        transform.localScale = new Vector3(75, 75, 75);
        transform.rotation = Quaternion.identity;
        transform.Rotate(new Vector3(0, 0, zRot));

        this.aliveTime = aliveTime;

        GetComponent<SpriteRenderer>().sprite = sprite;
        GetComponent<SpriteRenderer>().color = Color.white;

        if(Random.Range(0, 2) == 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }

        GetComponent<Rigidbody2D>().AddForce(vel);
    }

#pragma warning disable IDE0051 // Nicht verwendete private Member entfernen
    void DisableCloud()
#pragma warning restore IDE0051 // Nicht verwendete private Member entfernen
    {
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(aliveTime > 0)
        {
            aliveTime -= Time.deltaTime;

            if(aliveTime <= 0)
            {
                GetComponent<SpriteRenderer>().DOFade(0, 1f);
                Invoke("DisableCloud", 1.1f);
            }
        }
    }
}
