using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler Instance;

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private Queue<GameObject> destroyedPipeQueue = null;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for(int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            if(pool.tag.Equals("DestroyedPipePart"))
            {
                destroyedPipeQueue = objectPool;
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public void RecreatePool(string tag)
    {
        Debug.Log("Redo");

        Queue<GameObject> queue = poolDictionary[tag];
        while(queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            Destroy(obj);
        }

        Pool pool = null;

        for(int i = 0; i < pools.Count; i++)
        {
            if(pools[i].tag.Equals(tag))
            {
                pool = pools[i];
                break;
            }
        }

        for (int i = 0; i < pool.size; i++)
        {
            GameObject obj = Instantiate(pool.prefab, transform);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }
    }

    public GameObject SpawnFromPoolCheck(string tag)
    {
        GameObject objToSpawn = poolDictionary[tag].Dequeue();

        if (objToSpawn == null)
        {
            Debug.LogError("Spawned Obj is null!");
        }

        poolDictionary[tag].Enqueue(objToSpawn);

        return objToSpawn;
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, bool resetRotation = true, bool resetScale = false)
    {
        GameObject objToSpawn = SpawnFromPoolCheck(tag);

        if(objToSpawn == null)
        {
            Debug.LogError("Spawned Obj is null!");
        }

        return SpawnFromPoolEnd(objToSpawn, position, rotation, resetRotation, resetScale);
    }

    public GameObject SpawnFromPoolEnd(GameObject objToSpawn, Vector3 position, Quaternion rotation, bool resetRotation = true, bool resetScale = false)
    {
        if (resetScale)
        {
            objToSpawn.transform.localScale = new Vector3(1, 1, 1);
        }

        objToSpawn.SetActive(false);
        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;

        if (resetRotation)
        {
            objToSpawn.transform.rotation = rotation;
        }

        return objToSpawn;
    }

    public GameObject SpawnPipePart(Vector3 position, Quaternion rotation, bool resetRotationAndScale = true)
    { //Extra Funktion weil so oft aufgerufen -> bessere performance
        GameObject objToSpawn = destroyedPipeQueue.Dequeue();

        destroyedPipeQueue.Enqueue(objToSpawn);

        if (resetRotationAndScale)
        {
            objToSpawn.transform.localScale = new Vector3(1, 1, 1);
            objToSpawn.transform.rotation = rotation;
        }

        if(objToSpawn.activeSelf)
        {
            objToSpawn.SetActive(false);
        }

        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;

        return objToSpawn;
    }
}
