using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//리소스 로더(Prefab, Texture등 리소스들을 로드할때 사용.)
public static class ResourceLoader
{
    static Dictionary<string, Object> _resources = new Dictionary<string, Object>();

    public static void ReleaseAllResource()
    {
        _resources.Clear();
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// 시스템 오브젝트 형태로 리소스 로드
    /// </summary>
    /// <param name="prefabFullPath"> 파일경로 </param>
    /// <param name="type"> 오브젝트 타입 </param>
    /// <returns>오브젝트 반환</returns>
    public static Object LoadResource(string prefabFullPath, System.Type type)
    {
        string[] splitPath = prefabFullPath.Split(Util.PATH_SEPERATOR);
        string prefabPath = string.Empty;

        for (int i = 0; i < splitPath.Length - 1; ++i)
        {
            prefabPath += splitPath[i];
            prefabPath += "/";
        }

        return LoadResource(prefabPath, splitPath[splitPath.Length - 1], type);
    }

    public static Object LoadResource(string prefabFullPath)
    {
        return LoadResource(prefabFullPath, typeof(UnityEngine.GameObject));
    }

    public static Object LoadResource(string prefabPath, string prefabName)
    {
        return LoadResource(prefabPath, prefabName, typeof(UnityEngine.GameObject));
    }

    public static Object LoadResource(string prefabPath, string prefabName, System.Type type)
    {
        Object resource = null;
        string fullPath = prefabPath + prefabName;

        if (prefabPath != null)
        {
            if (!prefabPath.EndsWith("/") && !prefabPath.EndsWith("\\"))
                prefabPath += '/';
        }

        if (true == _resources.ContainsKey(fullPath))
        {
            resource = _resources[fullPath];
        }
        else
        {
            resource = Resources.Load(prefabPath + prefabName);

            if (resource != null)
                _resources.Add(fullPath, resource);
        }

        return resource;
    }

    public static GameObject CreatePrefab(string prefabFullPath, Vector3 position)
    {
        Object resource = LoadResource(prefabFullPath);

        if (null == resource)
            return null;

        return (GameObject)GameObject.Instantiate((GameObject)resource, position, new Quaternion());
    }

    public static GameObject CreatePrefab(string prefabFullPath)
    {
        Object resource = LoadResource(prefabFullPath);

        return PrefabInstantiate(resource, null);
    }

    public static GameObject CreatePrefab(string prefabFullPath, Transform parent)
    {
        Object resource = LoadResource(prefabFullPath);

        return PrefabInstantiate(resource, parent);
    }

    public static GameObject CreatePrefab(string prefabPath, string prefabName)
    {
        return CreatePrefab(prefabPath, prefabName, null);
    }

    public static GameObject CreatePrefabFromCache(string prefabName, Transform parent)
    {
        if (false == _resources.ContainsKey(prefabName))
        {
            Debug.LogError("Donothave a resource. Load cache or use Create prefab function, please.");
            return null;
        }

        return PrefabInstantiate(_resources[prefabName], parent);
    }

    public static GameObject CreatePrefab(string prefabPath, string prefabName, Transform parent)
    {
        Object resource = LoadResource(prefabPath, prefabName);
        return PrefabInstantiate(resource, parent);
    }

    public static GameObject CreatePrefab(string prefabPath, string prefabName, Vector3 position)
    {
        Object resource = LoadResource(prefabPath, prefabName);

        if (null == resource)
            return null;

        return PrefabInstantiate(resource, position);
    }

    public static GameObject CreatePrefab(string prefabPath, string prefabName, Vector3 position, Quaternion rotation)
    {
        Object resource = LoadResource(prefabPath, prefabName);

        if (null == resource)
            return null;

        return PrefabInstantiate(resource, position, rotation);
    }

    public static GameObject CreatePrefab(Object resource, Transform parent)
    {
        return PrefabInstantiate(resource, parent);
    }

    public static GameObject CreatePrefab(Object resource, Vector3 position)
    {
        return PrefabInstantiate(resource, position);
    }

    private static GameObject PrefabInstantiate(Object resource, Transform parent)
    {
        if (null == resource)
            return null;

        GameObject output = (GameObject)GameObject.Instantiate((GameObject)resource);

        if (null != parent)
        {
            output.transform.parent = parent;
            output.transform.localPosition = Vector3.zero;
            output.transform.localScale = Vector3.one;
            output.transform.localRotation = Quaternion.identity;

            if (output.layer == LayerMask.NameToLayer("Default"))
                output.layer = parent.gameObject.layer;
        }

        return output;
    }

    private static GameObject PrefabInstantiate(Object resource, Vector3 position, Quaternion rotation)
    {
        if (null == resource)
            return null;

        GameObject output = (GameObject)GameObject.Instantiate((GameObject)resource, position, rotation);

        return output;
    }

    private static GameObject PrefabInstantiate(Object resource, Vector3 position)
    {
        if (null == resource)
            return null;

        GameObject output = (GameObject)GameObject.Instantiate((GameObject)resource, position, new Quaternion());

        return output;
    }

    public static void PlayEffectSound(string path, string audioname, float volume = 1f)
    {
        AudioClip clipObj;
        clipObj = (AudioClip)LoadResource(path, audioname, typeof(AudioClip));

        if (clipObj != null)
        {
            //    EffectAudioController.Instance.PlayAudio(clipObj, volume);
        }
        else
        {
            Debug.Log("Path : " + path);
            Debug.Log("AudioName : " + audioname);
            Debug.Log("Volume : " + volume);
            Debug.Log("AudioClip Null or Not Find File.");
        }
    }

    public static void DestroyAllResource()
    {
        _resources.Clear();
    }
}
