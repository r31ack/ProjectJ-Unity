using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ArrowMove : MonoBehaviour
{
    public float lifeTime;
    Rigidbody rigid;
    TrailRenderer m_trail;

    void Awake()
    {
        m_trail = transform.Find("default").GetComponent<TrailRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        lifeTime = 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(transform.forward * 200 * Time.deltaTime, Space.World);

        if (lifeTime < 0)
        {
            m_trail.Clear();
            lifeTime = 2.0f;
            ObjectPoolManager.Instance.PushToPool("arrow", this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "enemy")                   // 충돌 대상이 적 태그를 가지고 있으면
        {
            EnemyInfomation enemyScript = coll.GetComponentInParent<EnemyInfomation>();   // 적 스크립트를 받아와서
            enemyScript.attacted(30);         // 30데미지 부여
        }
    }
}
