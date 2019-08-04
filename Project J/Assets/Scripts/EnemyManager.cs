using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManager : Singleton<EnemyManager>
{
    public GameObject enemyPrefab;
    public float createDelay = 3.0f;
    float createTimer = 0.0f;

    public UILabel rayCastTarget;
    public UISlider enemyHpUI;
    float m_fEnemyHpUI;
    public int score;
    public int hp;
    public Text defeat;
    public bool defeatFlag;
    public UIButton resetButton;
    public Util util;
    public GameObject rayCastTargetObject;

    private LinkedList<GameObject> m_lstEnemy = new LinkedList<GameObject>();

    public void ResetGame()
    {
        resetButton.gameObject.SetActive(false);
        defeat.gameObject.SetActive(false);
        defeatFlag = false;
        hp = 10;
        score = 0;

        foreach (var item in m_lstEnemy)
        {
            Destroy(item);
        }
        m_lstEnemy.Clear();
    }

    void craeteEnemy()
    {
        if (m_lstEnemy.Count <= 10)
        {
            if (createTimer > createDelay)
            {
                float randomX = Random.Range(0f, 100f); // x랜덤생성 0~100
                float randomZ = Random.Range(0f, 100f); // z랜덤생성 0~100

                m_lstEnemy.AddLast(Instantiate(enemyPrefab, new Vector3(randomX, 0, randomZ), Quaternion.identity));
                createTimer = 0.0f;
            }
            createTimer += Time.deltaTime;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        resetButton.gameObject.SetActive(false);
        defeatFlag = false;
        hp = 10;
    }

    // Update is called once per frame
    void Update()
    {
        if (defeatFlag == false)
        {
            craeteEnemy();
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
                resetButton.gameObject.SetActive(true);
                defeat.gameObject.SetActive(true);
                defeatFlag = true;
            }
        }
    }
}
