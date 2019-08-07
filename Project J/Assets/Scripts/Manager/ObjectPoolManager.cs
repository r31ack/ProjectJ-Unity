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

    public bool PushToPool(string itemName, GameObject gameObject)
    {
        ObjectPool pool = GetPoolItem(itemName);
        if (pool == null)
            return false;

        pool.push(gameObject);
        return true;
    }

    public GameObject PopFromPool(string itemName)
    {
        ObjectPool pool = GetPoolItem(itemName);
        if (pool == null)
            return null;

        return pool.pop();
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
