using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePooler : MonoBehaviour
{
    [System.Serializable]
    public class ObstaclePool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObstaclePooler Instance;

    public List<ObstaclePool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (ObstaclePool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.transform.SetParent(this.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector2 anchoredPos, Transform parent)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject obj = poolDictionary[tag].Dequeue();
        obj.SetActive(true);
        obj.transform.SetParent(parent, false);
        obj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;

        return obj;
    }
    public void ReturnToPool(GameObject obj)
    {
        string tag = obj.name.Replace("(Clone)", "").Trim();
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"No pool found for tag: {tag}");
            return;
        }

        poolDictionary[tag].Enqueue(obj);
    }

}
