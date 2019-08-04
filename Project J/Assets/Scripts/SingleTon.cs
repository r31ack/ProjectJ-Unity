using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_this;

    public static T Instance
    {
        get
        {
            if (m_this == null)
                m_this = FindObjectOfType(typeof(T)) as T;
            return m_this;
        }
    }
}
