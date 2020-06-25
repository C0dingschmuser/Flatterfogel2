using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformData : MonoBehaviour
{

    public int height, type = 0;
    public BoxCollider2D destroyCollider;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("FF_Player"))
        {
            DeathCause type = DeathCause.OutOfWorld;

            if (FlatterFogelHandler.Instance.miningMode) return;

            if(CompareTag("Spike"))
            {
                type = DeathCause.Minus;
            }

            if (!SROptions.Current.IgnoreMinus)
            {
                FF_PlayerData.Instance.Die(type);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("FF_Player"))
        {
            FF_PlayerData.Instance.SetGroundHeight(height);
        }
    }
}
