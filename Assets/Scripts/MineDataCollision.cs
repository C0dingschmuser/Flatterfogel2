using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineDataCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        transform.parent.GetComponent<MineData>().CollisionEnter2D(gameObject, collision);
    }
}
