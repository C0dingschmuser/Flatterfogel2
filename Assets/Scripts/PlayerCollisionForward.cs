using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionForward : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        FF_PlayerData.Instance.CollisionEnter2D(collision);
    }
}
