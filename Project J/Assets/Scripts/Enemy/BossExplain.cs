using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossExplain : MonoBehaviour
{
    UIPanel m_bossExplain;

    private void Awake()
    {
        m_bossExplain = transform.Find("UIRoot").GetComponent<UIPanel>();
        m_bossExplain.alpha = 0.0f;
    }
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("fadeAnimation",1.0f,0.1f);
    }

    void fadeAnimation()
    {
        m_bossExplain.alpha += 0.1f;
        if (m_bossExplain.alpha >= 1.0f)
        {
            Invoke("destroyObject", 1.0f);
            CancelInvoke("fadeAnimation");
        }
    }

    private void destroyObject()
    {
        Destroy(this.gameObject);
    }
}
