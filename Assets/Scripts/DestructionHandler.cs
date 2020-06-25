using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Destructible2D;

public class DestructionHandler : MonoBehaviour
{
    private List<GameObject> destructionObjs = new List<GameObject>();
    private List<GameObject> otherDestructionObjs = new List<GameObject>();
    private List<DestructionMode> lastSpawnedDestructionObjs = new List<DestructionMode>();
    private List<GameObject> delList = new List<GameObject>();

    public GameObject player;
    public GameObject[] housePrefabs;
    public GameObject[] otherDestructionPrefabs;

    public Transform visualObstacleParent;

    public static DestructionHandler Instance;

    private ObjectPooler objectPooler;

    private bool blussiPlaneActive = false, flakActive = false;
    private int d2dLayerCounter = 0, enemyPlanesActive = 0;

    private void Awake()
    {
        Instance = this;
        objectPooler = ObjectPooler.Instance;
    }

    public void DisableEnable(bool enable)
    {
        if(enable)
        {
            InvokeRepeating("SpawnD2DObj", 0f, 0.25f);
        } else
        {
            CancelInvoke("SpawnD2DObj");
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < destructionObjs.Count; i++)
        {
            destructionObjs[i].GetComponent<D2D_HouseHandler>().DestroyAll();
        }
        destructionObjs.Clear();

        for (int i = 0; i < otherDestructionObjs.Count; i++)
        {
            otherDestructionObjs[i].GetComponent<D2D_HouseHandler>().DestroyAll();
        }
        otherDestructionObjs.Clear();

        lastSpawnedDestructionObjs.Clear();

        blussiPlaneActive = false;
        flakActive = false;
        enemyPlanesActive = 0;
    }

    public void PlayerShootD2D()
    {
        //PlayerBombD2D();
        //return;

        Vector3 playerPos = player.transform.position;
        playerPos.x += 53;

        GameObject newP = objectPooler.SpawnFromPool("Projectile", playerPos, Quaternion.identity);
        newP.GetComponent<Rigidbody2D>().velocity = new Vector2(750, 0);

        newP.GetComponent<ProjectileHandler>().ResetProjectile(true);
    }

    public void PlayerBombD2D()
    {
        Vector3 playerPos = player.transform.position;
        //playerPos.y -= 53;

        GameObject newBomb = objectPooler.SpawnFromPool("Bomb", playerPos, Quaternion.Euler(0, 0, 90));
        newBomb.GetComponent<BombHandler>().ResetBomb(250);
    }

    private void SpawnD2DObj()
    {
        if (Random.Range(0, 15) >= 0) //== 0
        {

            int type = Random.Range(0, 3);

            type = 1;

            if (type == 0)
            {
                blussiPlaneActive = true;
                SpawnBlussiPlane();
            }
            else if (type == 1)
            {
                flakActive = true;
                SpawnFlak();
            }
            else if (type == 2)
            {
                if (enemyPlanesActive > 0)
                {
                    SpawnD2DObj();
                    return;
                }
                else
                {
                    enemyPlanesActive = 3;
                    SpawnEnemyPlane();
                }
            }
        }

        if (destructionObjs.Count > 15) return;

        Vector3 pos = new Vector3(647, 790, -200);
        bool skyscraper, ok = true;

        int len = lastSpawnedDestructionObjs.Count;
        for (int i = 0; i < len && i < 4; i++)
        {
            if (lastSpawnedDestructionObjs[i].type ==
                DestructionMode.Type.Skyscraper)
            { //Wenn in letzten 4 gespawnten skysc dann keinen neuen
                ok = false;
                break;
            }
        }

        if(flakActive)
        {
            ok = false;
        }

        skyscraper = ok;

        GameObject newHousePrefab = null;
        DestructionMode newStats;

        ok = false;
        while (!ok)
        {
            ok = true;
            newHousePrefab = housePrefabs[Random.Range(0, housePrefabs.Length)];

            if (!skyscraper)
            { //keinen neuen skyscraper
                if (newHousePrefab.GetComponent<D2D_HouseHandler>().stats.type ==
                    DestructionMode.Type.Skyscraper)
                {
                    ok = false;
                }
            } /*else
            {
                if (newHousePrefab.GetComponent<D2D_HouseHandler>().stats.type !=
                    DestructionMode.Type.Skyscraper)
                {
                    ok = false;
                }
            }*/
        }

        GameObject prefab = newHousePrefab;
        pos.y = prefab.GetComponent<D2D_HouseHandler>().yStartPos;

        GameObject newObj = Instantiate(prefab, pos, Quaternion.identity, visualObstacleParent);

        newObj.GetComponent<D2D_HouseHandler>().rdyEnable = true;
        newObj.GetComponent<D2D_HouseHandler>().enabled = true;

        newStats = newObj.GetComponent<D2D_HouseHandler>().stats;

        if (lastSpawnedDestructionObjs.Count > 0)
        { //wenn größer 0 neue pos berechnen
            DestructionMode last = lastSpawnedDestructionObjs[0]; //0 ist vorheriges obj
            pos.x = last.spawnPos.x;
            pos.z = last.spawnPos.z;

            if (last.obj != null)
            {
                pos.x = last.obj.transform.position.x;
                pos.z = last.obj.transform.position.z;
            }

            pos.x += last.minRightDist +
                newStats.minRightDist;
        }

        newObj.transform.position = pos;

        newStats.spawnPos = pos;
        //Warum instantiate und kein pool? 
        //  -> Durch zerstörung gehts schneller zu instantiaten als obj zu rebuilden

        newStats.obj = newObj;

        /*if(lastSpawnedDestructionObjs.Count > 0)
        { //blus spawnen
            Vector3 oldPos = lastSpawnedDestructionObjs[0].obj.transform.position;

            float middleX = oldPos.x + (pos.x - oldPos.x) / 2;
            float middleY = oldPos.y + Mathf.Abs(pos.y - oldPos.y) / 2;

            SpawnBlus(new Vector3(middleX, middleY));
        }*/

        lastSpawnedDestructionObjs.Insert(0, newStats);
        if (lastSpawnedDestructionObjs.Count > 4)
        { //wir wollen nicht mehr als die letzten 5 gespawnten objs
            lastSpawnedDestructionObjs.RemoveAt(lastSpawnedDestructionObjs.Count - 1);
        }

        SetLayerRecursively(newObj, 15 + d2dLayerCounter);

        Transform d2dObjs = newObj.GetComponent<D2D_HouseHandler>().GetD2DObjParent();

        for (int i = 0; i < d2dObjs.childCount - 1; i++)
        {
            d2dObjs.GetChild(i).GetComponent<D2dImpactDamage>().Mask |=
                (1 << LayerMask.NameToLayer("D2DDestructible" + d2dLayerCounter.ToString()));
        }

        d2dLayerCounter++;
        if (d2dLayerCounter >= 6)
        {
            d2dLayerCounter = 0;
        }

        destructionObjs.Add(newObj);
    }

    private void SpawnBlussiPlane()
    {
        bool ok = true;
        for (int i = 0; i < otherDestructionObjs.Count; i++)
        { //checken ob blussiplane schon gespawnt
            if (otherDestructionObjs[i].GetComponent<D2D_HouseHandler>().stats.type ==
                DestructionMode.Type.BlussiPlane)
            {
                ok = false;
                break;
            }
        }

        if (!ok) return;

        Vector3 spawnPos = otherDestructionPrefabs[0].GetComponent<D2D_HouseHandler>().stats.spawnPos;

        GameObject plane =
            Instantiate(otherDestructionPrefabs[0], spawnPos, Quaternion.identity, visualObstacleParent);

        plane.GetComponent<D2D_HouseHandler>().rdyEnable = true;
        plane.GetComponent<D2D_HouseHandler>().enabled = true;

        otherDestructionObjs.Add(plane);
    }

    private void SpawnEnemyPlane()
    {
        Vector3 spawnPos = otherDestructionPrefabs[0].GetComponent<D2D_HouseHandler>().stats.spawnPos;

        for(int i = 0; i < 3; i++)
        {
            spawnPos.x += 278 * i;

            GameObject ePlane =
                Instantiate(otherDestructionPrefabs[2], spawnPos, Quaternion.identity, visualObstacleParent);

            ePlane.GetComponent<D2D_HouseHandler>().rdyEnable = true;
            ePlane.GetComponent<D2D_HouseHandler>().enabled = true;

            otherDestructionObjs.Add(ePlane);
        }

    }

    private void SpawnFlak()
    {
        bool ok = true;
        for (int i = 0; i < otherDestructionObjs.Count; i++)
        { //checken ob flak schon gespawnt
            if (otherDestructionObjs[i].GetComponent<D2D_HouseHandler>().stats.type ==
                DestructionMode.Type.Flak)
            {
                ok = false;
                break;
            }
        }

        if (!ok) return;

        Vector3 pos = new Vector3(647, 790, -200);
        pos.y = otherDestructionPrefabs[1].GetComponent<D2D_HouseHandler>().yStartPos;

        GameObject newObj = Instantiate(otherDestructionPrefabs[1], pos, Quaternion.identity, visualObstacleParent);

        newObj.GetComponent<D2D_HouseHandler>().rdyEnable = true;
        newObj.GetComponent<D2D_HouseHandler>().enabled = true;

        DestructionMode newStats = newObj.GetComponent<D2D_HouseHandler>().stats;

        if (lastSpawnedDestructionObjs.Count > 0)
        { //wenn größer 0 neue pos berechnen
            DestructionMode last = lastSpawnedDestructionObjs[0]; //0 ist vorheriges obj
            pos.x = last.spawnPos.x;
            pos.z = last.spawnPos.z;

            if (last.obj != null)
            {
                pos.x = last.obj.transform.position.x;
                pos.z = last.obj.transform.position.z;
            }

            pos.x += last.minRightDist +
                newStats.minRightDist;
        }

        newObj.transform.position = pos;

        newStats.spawnPos = pos;

        newStats.obj = newObj;

        lastSpawnedDestructionObjs.Insert(0, newStats);
        if (lastSpawnedDestructionObjs.Count > 4)
        { //wir wollen nicht mehr als die letzten 5 gespawnten objs
            lastSpawnedDestructionObjs.RemoveAt(lastSpawnedDestructionObjs.Count - 1);
        }

        otherDestructionObjs.Add(newObj);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void HandleScrolling(float scrollSpeed)
    {
        int len = destructionObjs.Count;
        for (int i = 0; i < len; i++)
        {
            bool ok = destructionObjs[i].GetComponent<D2D_HouseHandler>().UpdateHouseHandler(scrollSpeed);

            if (!ok)
            {
                delList.Add(destructionObjs[i]);
            }
        }

        for (int i = 0; i < delList.Count; i++)
        {
            destructionObjs.Remove(delList[i]);
            Destroy(delList[i]);
        }
        delList.Clear();

        len = otherDestructionObjs.Count;
        for (int i = 0; i < len; i++)
        {
            bool ok = otherDestructionObjs[i].GetComponent<D2D_HouseHandler>().UpdateHouseHandler(scrollSpeed);

            if (!ok)
            {
                if(otherDestructionObjs[i].GetComponent<D2D_HouseHandler>().stats.type ==
                    DestructionMode.Type.BlussiPlane)
                {
                    blussiPlaneActive = false;
                } else if(otherDestructionObjs[i].GetComponent<D2D_HouseHandler>().stats.type ==
                    DestructionMode.Type.BlussiPlane)
                {
                    flakActive = false;
                } else if(otherDestructionObjs[i].GetComponent<D2D_HouseHandler>().stats.type ==
                    DestructionMode.Type.EnemyPlaneSmall1)
                {
                    enemyPlanesActive--;
                }

                delList.Add(otherDestructionObjs[i]);
            }
        }

        for (int i = 0; i < delList.Count; i++)
        {
            otherDestructionObjs.Remove(delList[i]);
            Destroy(delList[i]);
        }
        delList.Clear();
    }
}
