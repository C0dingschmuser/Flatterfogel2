using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PipeMiddleHandler : MonoBehaviour
{
    public float xOffset = 0, abstand = 0;
    public bool rotateActive = false;

    public void StartRotation()
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
            transform.Rotate(new Vector3(0, 0, 25 * Time.deltaTime));
        }
    }
}
