using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using System.Diagnostics;
using debug = UnityEngine.Debug;

public enum CHARACTER_TYPE
{
    UNITY,
    AKAZA,
    NONE,
}

enum ENEMY_TYPE
{

}

public class GameManager : MonoSingleton<GameManager>
{
    // 캐릭터 선택 창
    public int m_iCreateCharacterIndex = -1;       // 캐릭터 선택창에서 빈 슬롯을 눌렀을 때의 인덱스
    public int m_iSelectCharacterIndex = -1;       // 내가 선택한 캐릭터 인덱스

    // 던전 선택
    public int m_iClearDungeon = 0;                // 내가 던전 클리어한 상황
    public int m_iStageNumber = 0;                 // 내가 선택한 던전

    // 인게임
    Stopwatch m_stopwatch = new Stopwatch();       // 스톱워치
    public float m_iClearTime = 0.0f;              // 클리어 시간
    public int m_iScore = 0;                       // 점수
    public int m_iCurComboCount = 0;               // 현재 콤보 횟수
    public int m_iMaxComboCount = 0;               // 최대 콤보 횟수
    public string m_strRank = "F";                 // 랭크 등급

    // 던전에서 파괴되지 말아야할 오브젝트 모음
    private GameObject m_IngameUI;
    public GameObject m_player;
    private GameObject m_objectPoolManager;
    private GameObject m_mainLight;
    private GameObject m_mainCamera;

    void Start()
    {
        DontDestroyOnLoad(GetComponent<GameManager>());    // 게임매니저 오브젝트는 파괴되지 않게 한다. 
    }

    public void resultScene()                        // 던전이 끝난 후 결과 씬 호출
    {
        //Destroy(IngameUI);                           // 인게임UI를 파괴한다.
        SceneManager.LoadScene("RankingScene");      // Stage0-LoadingScene
    }

    public void addScore(int damageAmount)
    {
        m_iScore += damageAmount + m_iCurComboCount;

        if (IsInvoking("initCurComboCount") == true) 
            CancelInvoke("initCurComboCount");        // 콤보 카운트 캔슬 취소
        Invoke("initCurComboCount", 3.0f);            // 3초 뒤 현재 콤보 카운트를 초기화시킨다.
    }

    void initCurComboCount()
    {
        m_iCurComboCount = 0;
    }

    public void enterDungeon(int stageNumber)                  // 내가 선택한 스테이지로 입장한다.
      {
        m_iStageNumber = stageNumber;
        SceneManager.LoadScene("Stage" + m_iStageNumber + "-LoadingScene");       // Stage0-LoadingScene
     }

    public void startDungeon(Vector3 position, Quaternion rotation)
    {
        dontDestroyDungeonObject(position, rotation);

        m_iClearTime = 0.0f;
        m_iScore = 0;

        m_stopwatch.Reset();                                    // 스탑워치 초기화
        m_stopwatch.Start();                                    // 스탑워치 시작
    }

    public void dontDestroyDungeonObject(Vector3 position, Quaternion rotation)      // 던전에서 파괴되지 말아야할 오브젝트 연결
    {
        m_player = (GameObject)Instantiate(Resources.Load("Prefabs/AkazaBattle"), position, rotation);   // 플레이어를 생성
        m_player.name = "Player";                                                                        // 플레이어 오브젝트의 이름은 Player
        m_objectPoolManager = GameObject.Find("ObjectPoolManager").gameObject;   // 오브젝트풀 매니저          
        m_IngameUI = GameObject.Find("InGameUI").gameObject;                       // IngameUI 게임오브젝트
        m_mainLight = GameObject.Find("Directional Light").gameObject;           // 광원
        m_mainCamera = GameObject.Find("Main Camera").gameObject;                // 카메라

        DontDestroyOnLoad(m_IngameUI);                            // 하나의 스테이도중에는 파괴되지 않도록 한다.
        DontDestroyOnLoad(m_player);
        DontDestroyOnLoad(m_objectPoolManager);
        DontDestroyOnLoad(m_mainLight);
        DontDestroyOnLoad(m_mainCamera);
    }

    public void destroyDungeonObject()      // 던전 오브젝트를 파괴
    {
        Destroy(m_IngameUI);
        Destroy(m_player);
        Destroy(m_objectPoolManager);
        Destroy(m_mainLight);
        Destroy(m_mainCamera);
    }

    public void nextDungeonLoad()                                 // 다음 던전 로드
    {
        m_player.GetComponent<NavMeshAgent>().enabled = false;    // 네비메쉬를 끈다.(포탈 이동 시의 위치 에러 방지)

        if (SceneManager.GetActiveScene().name == "Stage1-1Scene")      
            SceneManager.LoadScene("Stage1-2Scene");
        else if (SceneManager.GetActiveScene().name == "Stage1-2Scene")
            SceneManager.LoadScene("Stage1-BossScene");
        else if (SceneManager.GetActiveScene().name == "Stage1-BossScene")  // 보스 씬이 끝난 경우
        {
            destroyDungeonObject();                                         // 던전 오브젝트 파괴
            m_stopwatch.Stop();                                             // 스탑워치를 멈추고 
            m_iClearTime = m_stopwatch.ElapsedMilliseconds / 1000.0f;       // 클리어타임에 대입
            SceneManager.LoadScene("RankingScene");                         // 랭킹 씬을 불러온다.
        }
    }

    public void exitGame()                                         // 게임 종료하기
    {
        Application.Quit();
    }
}
