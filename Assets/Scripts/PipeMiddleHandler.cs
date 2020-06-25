using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeMiddleHandler : MonoBehaviour
{
    private int health = 3, maxHealth = 3, hits = 0;

    public float xOffset = 0, abstand = 0;
    ObjectPooler objPooler;

    private void Awake()
    {
        objPooler = ObjectPooler.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetMiddleObj();
    }

    public void ResetMiddleObj()
    {
        health = maxHealth;
        xOffset = 0;
        hits = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            collision.gameObject.SetActive(false);

            bool ok = false;
            hits++;

            if(health == 3 && hits == 1)
            {
                ok = true;
            } else if(health == 2 && hits == 2)
            {
                ok = true;
            } else if(health == 1 && hits == 1)
            {
                ok = true;
            }

            if(ok)
            {
                health--;
                hits = 0;

                Vector2 size = GetComponent<SpriteRenderer>().size;
                size.x -= 0.3333f;

                GetComponent<SpriteRenderer>().size = size;

                int amount = (int)(size.y * 75) / 25;

                float startY = transform.position.y + (25 * (amount / 2)) - 12.5f;

                Vector3 pos = transform.position;
                pos.z = 0;

                if(health == 2)
                {
                    pos.x -= 25f;
                } else if(health == 1)
                {
                    pos.x -= 12.5f;
                }

                PipeData pData = transform.parent.GetChild(0).GetComponent<PipeData>();

                for (int i = 0; i < amount; i++)
                {
                    pos.y = startY - (25 * i);

                    GameObject dPart = objPooler.SpawnFromPool("DestroyedPipePart", pos, Quaternion.identity, true, true);

                    pData.ResetDestroyedPart(dPart, true);

                    dPart.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
                    dPart.GetComponent<Rigidbody2D>().AddExplosionForce(100000, collision.gameObject.transform.position, 300);

                    pData.AddDestroyedPart(dPart);
                }

                if (health <= 0)
                {
                    gameObject.SetActive(false);
                    transform.parent.GetComponent<PipeHolder>().GetAssignedBlus().transform.
                                    GetChild(0).GetComponent<BoxCollider2D>().enabled = true;
                    xOffset = 0;
                } else
                {
                    xOffset += 12.5f;
                }

                FlatterFogelHandler.Instance.StartCameraShake(0.15f);
                SoundManager.Instance.PlaySound(SoundManager.Sound.PipeExplosion);
            } else
            {
                SoundManager.Instance.PlaySound(SoundManager.Sound.PipeHit);
            }

        }
    }
}
