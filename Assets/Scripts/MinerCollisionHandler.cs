using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerCollisionHandler : MonoBehaviour
{
    public int dir;
    public GameObject collisionObj = null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("MineGround"))
        {
            collisionObj = collision.gameObject;
        }
    }

    public GameObject Mine()
    {
        GameObject coll = collisionObj;

        collisionObj = null;

        return coll;
    }
}
