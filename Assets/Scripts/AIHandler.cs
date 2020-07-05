using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AIHandler : MonoBehaviour
{
    private FlatterFogelHandler ffHandler;
    private ShopHandler shopHandler;

    private GameObject currentBlus;

    public Vector3 startPos = new Vector3(-381, 790);

    [SerializeField]
    private float startY = -2000, startTime = 1f;
    private int endPos = 10, score = 0, maxRotation = 700;

    private Tween angleTween = null;

    [SerializeField]
    private bool dead = false, startYReached = false, startTimeReached = false,
        endColl = false;

    public bool groundHit = false, playerDead = false;

    private void Awake()
    {
        ffHandler = FlatterFogelHandler.Instance;
        shopHandler = ShopHandler.Instance;
    }

    public void StartAI(float startY = 790, float startTime = 1f, int endpos = 10)
    {
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Rigidbody2D>().gravityScale = 175f;

        maxRotation = 700;
        playerDead = false;

        if(angleTween != null)
        {
            angleTween.Kill();
            angleTween = null;
        }

        transform.rotation = Quaternion.identity;

        currentBlus = null;
        score = 0;

        dead = false;
        endColl = false;
        groundHit = false;

        this.startY = startY;
        startYReached = false;

        this.startTime = startTime;
        startTimeReached = false;

        this.endPos = endpos;

        Skin newSkin = shopHandler.GetRandomSkin();

        GetComponent<SpriteRenderer>().sprite = newSkin.sprite;

        Color32 newColor = Color.white;

        newColor.a = 75;

        GetComponent<SpriteRenderer>().color = newColor;

        /*transform.position = new Vector3(Random.Range(-698, -381), 
                                            Random.Range(540, 1040));*/
    }

    public int GetEndPos()
    {
        return endPos;
    }

    public void StopAI()
    {
        GetComponent<Rigidbody2D>().simulated = false;
    }

    private void Fly()
    {
        Vector2 velocity = new Vector3(0, 700f);

        if(playerDead)
        {
            velocity.y *= 2.5f;
            maxRotation = 1750;
        }

        GetComponent<Rigidbody2D>().velocity = velocity;

        HandleRotation();
    }

    public void HandleRotation()
    {
        Rigidbody2D pRB = GetComponent<Rigidbody2D>();

        Vector2 vel = pRB.velocity;
        float rotation = (vel.y / maxRotation) * 45;
        if (rotation < -45)
        {
            rotation = -45;
        }
        angleTween = pRB.DORotate(rotation, 0.05f);
    }

    public void SetCurrentBlus(GameObject newBlus)
    {
        if(newBlus != currentBlus && newBlus != null)
        {
            if(score == endPos)
            {
                GetComponent<CircleCollider2D>().enabled = true;
                endColl = true;
            }
            score++;
        }

        currentBlus = newBlus;
    }

    public bool IsDead()
    {
        return dead;
    }

    public bool IsGroundHit()
    {
        return groundHit;
    }

    public bool HandleAI(float scrollSpeed)
    {
        if (dead)
        {
            if(transform.position.y < 272 && !groundHit)
            {
                groundHit = true;
                GetComponent<CircleCollider2D>().enabled = false;
                GetComponent<Rigidbody2D>().simulated = false;
            }

            if (transform.position.x < -827)
            {
                return false;
            }
            else return true;
        } else
        {
            if(transform.position.x > 33)
            {
                return false;
            }
        }

        if(!startTimeReached)
        {
            startTime -= 0.05f;
            if(startTime <= 0)
            {
                startTimeReached = true;
                GetComponent<Rigidbody2D>().simulated = true;
            }

            return true;
        }

        HandleRotation();

        float currentY = transform.position.y;

        if (currentY > startY && !startYReached)
        {
            return true;
        } else
        {
            startYReached = true;
        }

        //Y Das die KI erreichen / halten will
        float targetY = ffHandler.LatestPipePos(gameObject, transform.position.x).y + 75f;

        if(endColl)
        {
            targetY -= 75f;
        }

        // 132.13 Hoch je Sprung nach 0.5s

        float maxDiff = 0.1f;

        float jumpY = currentY + 132.13f;

        if (jumpY >= targetY && jumpY < targetY + maxDiff ||
            jumpY < targetY)
        {
            Fly();
        }

        return true;
    }

    private void Die()
    {
        GetComponent<CircleCollider2D>().isTrigger = false;

        dead = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (dead) return;

        switch (collision.gameObject.tag)
        {
            case "FF_World":
            case "FF_WorldGround":
            case "FF_Pipe":
                Die();
                break;
        }
    }
}
