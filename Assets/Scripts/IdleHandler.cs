using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class IdleHandler : MonoBehaviour
{
    public Camera mainCamera;

    public Rigidbody2D rb;
    public BoxCollider2D coll, bottomColl;

    public SpriteRenderer playerRenderer, wingRenderer;

    private CoroutineHandle idleHandler;

    private float startPointX, startPointY;
    private bool isBeingHeld = false, isActive = false;

    public void StartIdle()
    {
        rb.velocity = Vector2.zero;
        rb.simulated = true;

        isActive = true;

        idleHandler = Timing.RunCoroutine(_mainHandler());

        coll.enabled = true;
        coll.isTrigger = false;

        bottomColl.enabled = true;
    }

    private bool InBounds(Vector3 mPos)
    {
        if(mPos.x >= transform.position.x - 75 &&
            mPos.x <= transform.position.x + 75 &&
            mPos.y >= transform.position.y - 75 &&
            mPos.y <= transform.position.y + 75)
        {
            return true;
        }

        return false;
    }

    private bool InBorder(Vector3 mPos)
    {
        if(mPos.x >= -688 && mPos.x <= -283 &&
            mPos.y >= 257)
        {
            return true;
        }

        return false;
    }

    private void Update()
    {
        if (!isActive) return;

        Vector3 pos = Input.mousePosition;

        pos.z = 500f;

        pos = mainCamera.ScreenToWorldPoint(pos);

        if (Input.GetMouseButton(0))
        {
            if(!isBeingHeld)
            {
                if(InBounds(pos))
                {
                    startPointX = pos.x - transform.localPosition.x;
                    startPointY = pos.y - transform.localPosition.y;

                    rb.simulated = false;

                    isBeingHeld = true;
                }
            } else
            {                
                if (pos.x < -690)
                {
                    pos.x = -690;
                } else if(pos.x > -283)
                {
                    pos.x = -283;
                }

                if(pos.y < 257)
                {
                    pos.y = 257;
                }

                transform.localPosition =
                    new Vector3(pos.x - startPointX, pos.y - startPointY, transform.localPosition.z);
            }
        } else
        {
            if(isBeingHeld)
            {
                isBeingHeld = false;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0;

                rb.simulated = true;
            }
        }
    }

    private IEnumerator<float> _mainHandler()
    {
        yield return Timing.WaitForSeconds(4);

        while(true)
        {
            if(isBeingHeld)
            {
                yield return Timing.WaitForSeconds(0.1f);
                continue;
            }

            bool dir = false;

            if(Random.Range(0, 2) == 0)
            {
                dir = true;
            }

            if(transform.position.x <= -600)
            { //nur nach rechts
                dir = false;
            } else if(transform.position.x >= -251)
            { //nur nach links
                dir = true;
            }

            float x = 150;

            playerRenderer.flipX = false;
            wingRenderer.flipX = false;

            if(dir)
            {
                x = -x;
                playerRenderer.flipX = true;
                wingRenderer.flipX = true;
            }

            rb.velocity = new Vector2(x, 600);

            yield return Timing.WaitForSeconds(Random.Range(1.25f, 10f));
        }
    }

    public void StopIdle()
    {
        Timing.KillCoroutines(idleHandler);

        isActive = false;

        playerRenderer.flipX = false;
        wingRenderer.flipX = false;

        rb.velocity = Vector2.zero;
        rb.simulated = false;

        coll.isTrigger = true;
    }
}
