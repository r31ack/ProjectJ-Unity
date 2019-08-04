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
        transform.Translate(transform.forward *300 * Time.deltaTime, Space.World);

        lifeTime -= Time.deltaTime;
        if (lifeTime < 0)
        {
            ObjectPoolManager.Instance.PushToPool(poolItemName, this.gameObject);
            lifeTime = 1.0f;
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "enemy")
        {
            coll.GetComponent<EnemyTestInfomation>().attacted(100,false);
            Destroy(coll.gameObject);
        }
    }
}
