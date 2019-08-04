using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    public List<ObjectPool> objectPool = new List<ObjectPool>();
    
    void Awake()
    {
        for (int i = 0; i < objectPool.Count; i++)
        {
            objectPool[i].init(transform);
        }
    }

    public bool PushToPool(string itemName, GameObject gameObject, Transform parent = null)
    {
        ObjectPool pool = GetPoolItem(itemName);
        if (pool == null)
            return false;

        pool.push(gameObject, parent == null ? transform : parent);
        return true;
    }

    public GameObject PopFromPool(string itemName, Transform parent = null)
    {
        ObjectPool pool = GetPoolItem(itemName);
        if (pool == null)
            return null;

        return pool.pop(parent);
    }

    ObjectPool GetPoolItem(string itemName)
    {
        for (int i = 0; i < objectPool.Count; i++)
        {
            if (objectPool[i].poolObjectName.Equals(itemName))
                return objectPool[i];
        }
        return null;
    }
}
