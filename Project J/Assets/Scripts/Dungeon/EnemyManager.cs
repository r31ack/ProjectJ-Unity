using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManager : Singleton<EnemyManager>
{
    Transform[] m_spawnPosition;              // 적이 생성되는 장소

    public GameObject enemyPrefab;
    public float createDelay = 3.0f;

    public UILabel rayCastTarget;
    public UISlider enemyHpUI;
    float m_fEnemyHpUI;
    public int score;
    public int hp;
    public Text defeat;
    public bool defeatFlag;
    public Util util;
    public GameObject rayCastTargetObject;

    private LinkedList<GameObject> m_lstEnemy = new LinkedList<GameObject>();

    void craeteEnemy()
    {
        for(int i=0; i<m_spawnPosition.Length; i++)
            m_lstEnemy.AddLast(Instantiate(enemyPrefab, m_spawnPosition[i].position, m_spawnPosition[i].rotation));
    }

    // Start is called before the first frame update
    void Start()
    {
        m_spawnPosition = GameObject.Find("SpawnPosition").GetComponentsInChildren<Transform>();
        craeteEnemy();
        defeatFlag = false;
        hp = 10;
    }

    // Update is called once per frame
    void Update()
    {
        if (defeatFlag == false)
        {
            rayCastTargetObject = Util.RayCastTagObject("enemy", 70);
            rayCastTargetObject = Util.RayCastTagObject("enemyAttack", 70);
            if (rayCastTargetObject != null)
            {
                enemyHpUI.gameObject.SetActive(true);
                rayCastTarget.text = "Enemy!!!";
                enemyHpUI.GetComponent<UISlider>().value = rayCastTargetObject.GetComponentInParent<EnemyTestInfomation>().percentHP;
            }
            else
            {
                enemyHpUI.gameObject.SetActive(false);
                rayCastTarget.text = "";
            }
            if (hp < 0)
            {
                defeat.gameObject.SetActive(true);
                defeatFlag = true;
            }
        }
    }
}
