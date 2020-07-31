using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeHolder : MonoBehaviour
{
    public bool isMoving = false, isEmpty = false, tunnel = false, lastInTunnel = false;
    private int mode = 0;
    private float maxY = 0, minY = 0, speed = 0, startY;
    private GameObject assignedBlus, topPipe, bottomPipe;

    public GameObject movingEffect;

    // Start is called before the first frame update
    void Start()
    {
        topPipe = transform.GetChild(0).gameObject;
        bottomPipe = transform.GetChild(1).gameObject;
    }

    public void ResetPH()
    {
        isMoving = false;
        tunnel = false;
        lastInTunnel = false;
    }

    public void SetEmpty(float startY, bool empty)
    {
        isEmpty = empty;
        this.startY = startY;
    }

    public float GetStartY()
    {
        return startY;
    }

    public void SetAssignedBlus(GameObject blus)
    {
        assignedBlus = blus;
    }

    public void StartMove(float minY, float maxY, float speed)
    {
        isMoving = true;

        this.minY = minY;
        this.maxY = maxY;
        this.speed = speed;

        movingEffect.transform.GetChild(0).position = new Vector3(-195, 228.47f, -500); //bottom
        movingEffect.transform.GetChild(1).position = new Vector3(-195, 1427, -500); //top

        if (OptionHandler.particleEffects == 1)
        {
            movingEffect.SetActive(true);
        }

        mode = Random.Range(0, 2);
    }

    public GameObject GetAssignedBlus()
    {
        return assignedBlus;
    }

    public void StopMove()
    {
        isMoving = false;
        movingEffect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving)
        {
            if(!transform.GetChild(0).gameObject.activeSelf &&
                !transform.GetChild(1).gameObject.activeSelf)
            {
                StopMove();
                return;
            }

            Vector3 pos = transform.position;
            Vector3 blusPos = Vector3.zero;

            if (!isEmpty)
            {
                blusPos = assignedBlus.transform.position;
            }

            float diff = 0;

            if(mode == 0)
            { //runter
                diff = -speed * Time.deltaTime;

                pos.y -= speed * Time.deltaTime;
                if(pos.y < minY)
                {
                    diff = -(pos.y + minY);

                    pos.y = minY;
                    mode = 1;
                }

            } else
            { //hoch
                diff = speed * Time.deltaTime;

                pos.y += speed * Time.deltaTime;

                if(pos.y > maxY)
                {
                    diff = pos.y - maxY;

                    pos.y = maxY;
                    mode = 0;
                }
            }

            transform.position = pos;

            blusPos.y = pos.y;

            if(!isEmpty)
            {
                assignedBlus.transform.position = blusPos;
            }

            pos.y = 228.47f;

            if(!isEmpty)
            {
                pos.x = blusPos.x;
            } else
            {
                pos.x = topPipe.transform.position.x + 37.5f;
            }

            movingEffect.transform.GetChild(0).position = pos;

            pos.y = 1427;

            movingEffect.transform.GetChild(1).position = pos;
        }
    }
}
