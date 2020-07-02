using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZigZagHandler : MonoBehaviour
{
    public static float scrollSpeed = 200;

    [SerializeField]
    private FlatterFogelHandler ffHandler;

    [SerializeField]
    private Sprite[] lineSprites = null;

    public GameObject playerArm, player, armPosition;
    public Transform endPart;

    public float rotation = 0, speed = 150;

    public bool zigZagActive = false;

    private int animationStep = 0;
    private float endY = 0, oldScrollSpeed = 0;
    private bool dir = false, isMoving = false, positionSet = false;
    private Tween playerMoveTween = null, playerScaleTween = null;

    private Vector3 startPos, endPos;

    private Coroutine animationRoutine = null;

    public void StartZigZag()
    {
        gameObject.SetActive(true);
        rotation = 0;

        armPosition.transform.position = player.transform.position;

        armPosition.SetActive(false);

        if(Random.Range(0, 2) == 0)
        {
            dir = false;
        } else
        {
            dir = true;
        }

        animationStep = 0;
        playerArm.transform.rotation = Quaternion.Euler(0, 0, rotation);
        zigZagActive = true;

        //animationRoutine = StartCoroutine(HandleAnimation());
    }

    private IEnumerator HandleAnimation()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.01f);

            playerArm.GetComponent<SpriteRenderer>().sprite = lineSprites[animationStep];

            animationStep++;
            if (animationStep >= lineSprites.Length)
            {
                animationStep = 0;
            }
        }
    }

    public void Clicked()
    {
        if (isMoving) return;

        isMoving = true;
        positionSet = false;

        armPosition.transform.position = player.transform.position;

        oldScrollSpeed = FlatterFogelHandler.scrollSpeed;
        FlatterFogelHandler.scrollSpeed *= 1.5f;
        BackgroundHandler.Instance.SpeedUp(true);

        Vector3 diff = endPos - startPos;
        diff.Normalize();

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        player.transform.DORotate(new Vector3(0, 0, rot_z), 0.1f);

        playerArm.transform.rotation = Quaternion.Euler(0, 0, rot_z);

        float targetY = playerArm.transform.GetChild(0).position.y;
        endY = targetY;

        float xDiff = endPart.transform.position.x - armPosition.transform.position.x;

        armPosition.transform.DOMoveX(armPosition.transform.position.x - xDiff, 0.5f);

        playerMoveTween = player.transform.DOMoveY(targetY, 0.5f, true);
        playerScaleTween = player.transform.DOScale(43.875f, 0.1f);

        armPosition.SetActive(true);

        Invoke("EndScale", 0.35f);
        Invoke("EndMove", 0.51f);
    }

    public void StartTouch(Vector3 startPos)
    {
        if (isMoving) return;

        this.startPos = startPos;
    }

    public void EndTouch(Vector3 endPos)
    {
        if (isMoving) return;

        this.endPos = endPos;
        Clicked();
    }

    private void EndScale()
    {
        playerScaleTween = player.transform.DOScale(58.5f, 0.1f);
        player.transform.DORotate(new Vector3(0, 0, 0), 0.1f);
    }

    private void EndMove()
    {
        isMoving = false;

        if (!positionSet)
        {
            FlatterFogelHandler.scrollSpeed = oldScrollSpeed;
            BackgroundHandler.Instance.SpeedUp(false);
            armPosition.transform.position = player.transform.position;

            armPosition.SetActive(false);
        }
    }

    public void StopZigZag()
    {
        zigZagActive = false;
        gameObject.SetActive(false);

        if(animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }
    }

    // Update is called once per frame
    void Update()
    {
        return;

        if(zigZagActive)
        {
            if(!isMoving)
            {
                /*if (!dir)
                { //up
                    rotation += speed * Time.deltaTime;

                    if (rotation >= 60)
                    {
                        dir = true;
                    }
                }
                else
                {
                    rotation -= speed * Time.deltaTime;

                    if (rotation <= -60)
                    {
                        dir = false;
                    }
                }

                playerArm.transform.rotation = Quaternion.Euler(0, 0, rotation);*/
            } else
            {
                if(!positionSet)
                { //-2f * FlatterFogelHandler.scrollSpeed * Time.deltaTime
                    float speed = -3f;

                    if(rotation > 45 && rotation < 55 ||
                        rotation < -45 && rotation > -55)
                    {
                        speed = -2f;
                    } else if(rotation > 55 || rotation < -55)
                    {
                        speed = -1.5f;
                    }

                    armPosition.transform.Translate(speed * (FlatterFogelHandler.scrollSpeed / 2) * Time.deltaTime, 0, 0);

                    if(endPart.position.x <= player.transform.position.x - 40)
                    {
                        positionSet = true;
                        armPosition.transform.position =
                            new Vector3(player.transform.position.x, endY, 292.4f);
                        FlatterFogelHandler.scrollSpeed = oldScrollSpeed;
                        BackgroundHandler.Instance.SpeedUp(false);
                    }
                }
            }
        }
    }
}
