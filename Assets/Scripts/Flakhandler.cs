using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flakhandler : MonoBehaviour
{
    public GameObject topFlak, player;

    private ObjectPooler objectPooler;
    private bool canShoot = false;

    private int currentBarrel = 0;
    private float reload = 1f, maxReload = 1f;

    private void Awake()
    {
        objectPooler = ObjectPooler.Instance;
    }

    private void Start()
    {
        player = FF_PlayerData.Instance.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = player.transform.position;

        pos.z = topFlak.transform.position.z;

        topFlak.transform.up = pos - topFlak.transform.position;

        float z = topFlak.transform.rotation.eulerAngles.z;

        bool tmpShootOk = true;

        if(z > 46 && z < 180)
        {
            tmpShootOk = false;
            topFlak.transform.rotation = Quaternion.Euler(0, 0, 46);
        } else if(z > 180 && z < 317)
        {
            tmpShootOk = false;
            topFlak.transform.rotation = Quaternion.Euler(0, 0, 317);
        }

        canShoot = tmpShootOk;

        reload -= Time.deltaTime;

        if(reload <= 0 && canShoot)
        {
            reload = maxReload;

            GameObject newMinus = objectPooler.SpawnFromPool("Minus", 
                topFlak.transform.GetChild(1 + currentBarrel).position, Quaternion.identity);

            newMinus.GetComponent<MinusHandler>().StartMinusFlak(player, 350);

            if (currentBarrel == 0)
            {
                currentBarrel = 1;
            } else
            {
                currentBarrel = 0;
            }
        }

        if(transform.position.x < -639)
        {
            this.enabled = false;
        }
    }
}
