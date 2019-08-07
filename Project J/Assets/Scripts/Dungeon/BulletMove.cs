using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BulletMove : MonoBehaviour
{
    public float lifeTime;
    public string poolItemName = "swordWind";
    Rigidbody rigid;

    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        lifeTime = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(transform.forward *100 * Time.deltaTime, Space.World);

        lifeTime -= Time.deltaTime;
        if (lifeTime < 0)
        {
            ObjectPoolManager.Instance.PushToPool(poolItemName, this.gameObject);
            lifeTime = 1.0f;
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "enemy")                   // 충돌 대상이 적 태그를 가지고 있으면
        {
            EnemyTestInfomation enemyScript = coll.GetComponentInParent<EnemyTestInfomation>();   // 적 스크립트를 받아와서
            enemyScript.attacted(50);         // 50데미지 부여
        }
    }
}
