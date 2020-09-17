using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PipeMiddleHandler : MonoBehaviour
{
    public float xOffset = 0, abstand = 0, speed = 25;
    public bool rotateActive = false;

    public void StartRotation(int diff)
    {
        rotateActive = true;

        Color c = ShopHandler.Instance.pipeColor;

        transform.GetChild(0).position =
            new Vector3(transform.position.x, transform.position.y + 295);
        transform.GetChild(0).GetComponent<SpriteRenderer>().color =
            c;

        transform.GetChild(1).position =
            new Vector3(transform.position.x, transform.position.y - 295);
        transform.GetChild(1).GetComponent<SpriteRenderer>().color =
            c;

        switch(diff)
        {
            default:
            case 0:
                speed = 10;
                break;
            case 1:
                speed = 15;
                break;
            case 2:
                speed = 20;
                break;
            case 3:
                speed = 30;
                break;
        }

        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-200, -140));
    }

    public void ResetMiddleObj()
    {
        xOffset = 0;
        rotateActive = false;
    }

    private void Update()
    {
        if(rotateActive)
        {
            transform.Translate(-FlatterFogelHandler.scrollSpeed * Time.deltaTime, 0, 0, Space.World);

            //transform.RotateAround(transform.position, Vector3.forward, 10 * Time.deltaTime);
            transform.Rotate(new Vector3(0, 0, speed * Time.deltaTime));
        }
    }
}
