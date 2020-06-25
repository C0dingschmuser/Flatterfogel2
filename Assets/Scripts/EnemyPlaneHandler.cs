using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlaneHandler : MonoBehaviour
{
    public float minY, maxY, speed;

    private ObjectPooler objectPooler;
    private float reload = 1f, maxReload = 1f;
    private int dir = 0;

    private void Awake()
    {
        objectPooler = ObjectPooler.Instance;

        reload = maxReload;
    }

    private void Start()
    {
        dir = Random.Range(0, 2);
    }

    // Update is called once per frame
    void Update()
    {
        reload -= Time.deltaTime;

        if(reload <= 0)
        {
            reload = maxReload;
        }

        if(dir == 0)
        { //hoch
            transform.Translate(0, speed * Time.deltaTime, 0);

            if(transform.position.y > maxY)
            {
                dir = 1;
            }
        } else if(dir == 1)
        { //runter
            transform.Translate(0, -speed * Time.deltaTime, 0);

            if(transform.position.y < minY)
            {
                dir = 0;
            }
        }
    }
}
