using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using DG.Tweening;

public class MieserHandler : MonoBehaviour
{
    public GameObject whiteParent, deathEffect, mieser;
    public GameObject[] whiteParts;
    public AchievementHandler achHandler;

    [SerializeField]
    private BossHandler bossHandler = null;

    private float currentAlpha = 1, aliveTime = 0;
    private bool fadeRunning = false, isDead = false;

    private int health = 15;
    private const int maxHealth = 15;

    public static MieserHandler Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartMieser();
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void StartMieser()
    {
        fadeRunning = false;
        isDead = false;

        aliveTime = 0;

        deathEffect.SetActive(false);
        mieser.GetComponent<SpriteRenderer>().color = Color.white;
        health = maxHealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Projectile"))
        {
            collision.gameObject.SetActive(false);

            whiteParent.SetActive(true);
            fadeRunning = true;
            currentAlpha = 1;
            SetWhiteAlpha(currentAlpha);

            health--;
            if(health <= 0 && !isDead)
            { //tod
                Die();
            }
        }
    }

    private void Die()
    {
        float timeTillDeath = 3f;

        int coins = 3;

        if(aliveTime < 11)
        {
            coins = 7;
        } else if(aliveTime < 16)
        {
            coins = 5;
        }

        float stepTime = 1.5f / coins;

        Timing.RunCoroutine(SpawnEndCoins(stepTime, coins));

        isDead = true;

        deathEffect.SetActive(true);
        mieser.GetComponent<SpriteRenderer>().DOColor(Color.red, timeTillDeath);
        Invoke("EndDeath", timeTillDeath);
    }

    private IEnumerator<float> SpawnEndCoins(float time, int coins)
    {
        while(coins > 0)
        {
            Vector3 newPos = transform.position;
            newPos.x += Random.Range(-100, 100);
            newPos.y += Random.Range(-100, 100);

            FlatterFogelHandler.Instance.SpawnCoin(newPos);

            coins--;
            yield return Timing.WaitForSeconds(time);
        }
    }

    private void EndDeath()
    {
        achHandler.QueueStep("mieserBoss", 1);

        FlatterFogelHandler.Instance.ChangeMode();
        bossHandler.BossDie(2f);
        //bossHandler.StopBoss(2f); wird bei deathtimeout aufgerufen
    }

    private void SetWhiteAlpha(float a)
    {
        for(int i = 0; i < whiteParts.Length; i++)
        {
            Color c = whiteParts[i].GetComponent<SpriteRenderer>().color;
            c.a = a;
            whiteParts[i].GetComponent<SpriteRenderer>().color = c;
        }
    }

    // Update is called once per frame
    void Update()
    {
        aliveTime += Time.deltaTime;

        if(fadeRunning)
        {
            currentAlpha -= 4 * Time.deltaTime;
            SetWhiteAlpha(currentAlpha);
            if(currentAlpha <= 0)
            {
                fadeRunning = false;
                whiteParent.SetActive(false);
            }
        }
    }
}
