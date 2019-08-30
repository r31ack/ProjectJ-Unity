using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class Stage1Manager : Singleton<Stage1Manager>
{
    private LinkedList<GameObject> m_lstEnemy = new LinkedList<GameObject>(); // 적 오브젝트 모음
    private Transform m_playerPosition;                                       // 플레이어가 스폰되는 위치 (포탈 위치)
    Transform[] m_spawnPosition;                                              // 적이 스폰되는 위치

    void Awake()
    {
        m_playerPosition = GameObject.Find("PlayerPosition").GetComponent<Transform>();     // 플레이어 생성 위치를 찾음
        craeteEnemy();                                                                      // 해당 스테이지에 맞는 적 생성

        //GameObject player = (GameObject)Instantiate(Resources.Load("Prefabs/AkazaBattle"), m_playerPosition.position, m_playerPosition.rotation);
        //player.name = "Player";

        if (SceneManager.GetActiveScene().name == "Stage1-1Scene")      // 스테이지1 씬이면        
        {
            GameManager.instance.startDungeon(m_playerPosition.position, m_playerPosition.rotation);        // 던전을 시작함 (DontDestroyLoad 오브젝트 처리)
        }
        else
        {
            GameManager.instance.m_player.transform.position = m_playerPosition.position;                   // 포탈 포지션으로 플레이어를 날린후
            GameManager.instance.m_player.transform.rotation = m_playerPosition.rotation;                   // 포탈 포지션으로 플레이어를 날린후
            GameManager.instance.m_player.GetComponent<NavMeshAgent>().enabled = true;                      // 플레이어 활성화
        }
        if (SceneManager.GetActiveScene().name == "Stage1-BossScene")
        {
            GameManager.instance.bossCutSceenStart();
        }
    }

    void craeteEnemy()  // 적 생성 함수
    {
        if (SceneManager.GetActiveScene().name == "Stage1-BossScene")   // 보스 씬이면 적을 생성하지 않음
            return;
        m_spawnPosition = GameObject.Find("SpawnPosition").GetComponentsInChildren<Transform>();
        for (int i = 1; i < m_spawnPosition.Length; i++)                // 0번은 부모 트랜스폼이므로 제외해야 한다..
            m_lstEnemy.AddLast((GameObject)Instantiate(Resources.Load("Prefabs/Enemy/Minotaur"), m_spawnPosition[i].position, m_spawnPosition[i].rotation));
    }

    public void createEnemySkill()
    {
        m_spawnPosition = GameObject.Find("SpawnPosition").GetComponentsInChildren<Transform>();
        for (int i = 1; i < m_spawnPosition.Length; i++)                // 0번은 부모 트랜스폼이므로 제외해야 한다..
            m_lstEnemy.AddLast((GameObject)Instantiate(Resources.Load("Prefabs/Enemy/Minotaur"), m_spawnPosition[i].position, m_spawnPosition[i].rotation));
    }
}

