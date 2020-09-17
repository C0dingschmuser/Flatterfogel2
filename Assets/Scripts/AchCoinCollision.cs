using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchCoinCollision : MonoBehaviour
{
    public AchievementHandler achHandler;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        achHandler.CollisionEnter(collision.gameObject);
    }
}
