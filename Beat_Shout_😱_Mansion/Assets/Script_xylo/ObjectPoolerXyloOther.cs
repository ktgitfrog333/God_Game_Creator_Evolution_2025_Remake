using UnityEngine;
using System.Collections.Generic;



public class ObjectPoolerXyloOther : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPoolerXyloOther Instance;
    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} does not exist.");
            return null;
        }

        GameObject objectToSpawn;
        if (poolDictionary[tag].Count == 0)
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool == null)
            {
                Debug.LogError("No pool configuration found for tag " + tag);
                return null;
            }
            objectToSpawn = Instantiate(pool.prefab); // Use original prefab
            ResetObjectState(objectToSpawn);
        }
        else
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
            ResetObjectState(objectToSpawn);
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return;
        }

        // ここから
        Pool pool = pools.Find(p => p.tag == tag);
        if (pool != null)
        {
            string prefabName = pool.prefab.name;
            string objectName = objectToReturn.name.Replace("(Clone)", "").Trim();



            if (tag != prefabName)
            {
                Debug.LogWarning("返却するタグ名とプレハブ名が一致しません。");
                Debug.LogWarning("タグ: " + tag + ", プレハブ名: " + prefabName + ", オブジェクト名: " + objectName);
            }
        }
        else
        {

        }
        // ここまで

        // オブジェクトがアクティブな場合のみ SetParent(null) を実行
        if (objectToReturn.activeInHierarchy)
        {
            objectToReturn.transform.SetParent(null);
        }

        // オブジェクトの状態をリセット
        ResetObjectState(objectToReturn);

        // オブジェクトを非アクティブにし、プールに返却
        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }

    private void ResetObjectState(GameObject objectToReset)
    {
        objectToReset.transform.position = Vector3.zero;
        objectToReset.transform.rotation = Quaternion.identity;

        Rigidbody rb = objectToReset.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;  
            rb.angularVelocity = Vector3.zero;
        }
    }
}