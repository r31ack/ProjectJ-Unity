using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    public int m_iEnemyCount = 0;

    // 캐릭터 전체에서 파괴되지 말아야할 오브젝트
    private GameObject m_itemWindow;

    // 던전에서 파괴되지 말아야할 오브젝트 모음
    private UIPanel m_ingameUI;
    private GameObject m_ingameSubUI;
    public GameObject m_player;                     
    private GameObject m_objectPoolManager;     
    private GameObject m_mainLight;                
    private GameObject m_camera;                   // 인게임에 쓰이는 모든 카메라 모음


    //  인게임 이벤트용 오브젝트
    FollowCam m_followCam;                         // 카메라 조작을 위함
    private GameObject m_dialogWindow;             // npc대화창
    private GameObject m_subDialogWindow;          // npc 서브 대화창
    private UIPanel m_winWindowPanel;                // 승리시 활성화
    private GameObject m_deathWindow;              // 사망시 활성화
    private UIPanel m_explainPanel;               // 설명 관련 패널
    private UILabel m_playerStateExplainLabel;    // 플레이어 상태 표시 레이블
    private UILabel m_dungeonStateExplainLabel;   // 던전 상태 표시 레이블

    void Start()
    {
        DontDestroyOnLoad(GetComponent<GameManager>());    // 게임매니저 오브젝트는 파괴되지 않게 한다. 
    }

    public void Update()        // 디버그용 업데이트
    {
        if (Input.GetKeyDown(KeyCode.P) == true)
        {
            m_player.GetComponent<NavMeshAgent>().enabled = false;    // 네비메쉬를 끈다.(포탈 이동 시의 위치 에러 방지)
            if (SceneManager.GetActiveScene().name == "Stage1-1Scene")
                SceneManager.LoadScene("Stage1-2Scene");
            else if (SceneManager.GetActiveScene().name == "Stage1-2Scene")
                SceneManager.LoadScene("Stage1-BossScene");
        }
    }

    public void addScore(int damageAmount)
    {
        m_iScore += damageAmount + m_iCurComboCount;

        if (IsInvoking("initCurComboCount") == true) 
            CancelInvoke("initCurComboCount");        // 콤보 카운트 캔슬 취소
        Invoke("initCurComboCount", 3.0f);            // 3초 뒤 현재 콤보 카운트를 초기화시킨다.
    }

    public void startNPCChat()                      // NPC와의 채팅 시작
    {
        m_followCam.target = GameObject.Find("NPC").transform.Find("CameraTarget");
        m_followCam.moveDamping = 4.0f;
        m_ingameUI.gameObject.SetActive(false);             // 인게임 UI비활성화
        m_dialogWindow.SetActive(true);          // 대화창을 활성화
        m_player.GetComponent<PlayerOperation>().enabled = false;
    }

    public void endNPCChat()                        // NPC와의 채팅 끝 (바리게이트를 비활성화 )
    {
        GameObject barrigate = GameObject.Find("Map").transform.Find("QuestBarrigate").gameObject;    // 바리게이트를 찾아서
        if(barrigate.activeSelf==true)                                                                // 바리게이트가 있으면 (NPC와의 처음 대화)
        {
            barrigate.SetActive(false);                                                               // 바리게이트 비활성화
            GameObject.Find("NPC").GetComponent<MisakiInfomation>().battleMode(true);                 // NPC전투참전
            stateExplain(2);                                                                          // 전투참전 설명
        }
        m_followCam.target = m_player.transform.Find("CameraTarget");
        m_followCam.moveDamping = 15.0f;
        m_ingameUI.gameObject.SetActive(true);             // 인게임 UI비활성화
        m_dialogWindow.SetActive(false);          // 대화창을 활성화
        m_player.GetComponent<PlayerOperation>().enabled = true;
    }

    public void playerDie()
    {
        m_deathWindow.SetActive(true);
    }
    
    public void revive()
    {
        m_deathWindow.SetActive(false);
        m_player.GetComponent<PlayerState>().revive();
    }

    public void bossCutSceenStart()
    {
        m_followCam.target = GameObject.Find("Boss").transform.Find("CameraTarget");
        m_followCam.moveDamping = 3.0f;
        m_ingameUI.gameObject.SetActive(false);             // 인게임 UI비활성화
        m_player.GetComponent<PlayerOperation>().enabled = false;
        Invoke("bossCutSceenEnd", 3.0f);
    }

    public void bossCutSceenEnd()
    {
        m_followCam.target = m_player.transform.Find("CameraTarget");
        m_followCam.moveDamping = 15.0f;
        m_player.GetComponent<PlayerOperation>().enabled = true;
        m_ingameUI.gameObject.SetActive(true);             // 인게임 UI비활성화
    }

    public void clearDungeon()
    {
        if (SceneManager.GetActiveScene().name == "Stage1-1Scene")
            startSubDialog();
    }

    public void winDungeon()                          // 던전 승리 알림 함수
    {
        m_winWindowPanel.gameObject.SetActive(true);
        m_winWindowPanel.alpha = 0.0f;
        InvokeRepeating("winPanelFadeIn", 3.0f, 0.05f);   // 승리 알림창의 알파를 서서히 증가 Invoke함수
        InvokeRepeating("uiPanelFadeOut", 3.0f, 0.05f);   // 승리 알림창의 알파를 서서히 증가 Invoke함수
    }

    public void winPanelFadeIn()      // 승리 패널의 알파 증가
    {
        m_winWindowPanel.alpha += 0.05f;
        if (m_winWindowPanel.alpha >= 1.0f)
        {
            CancelInvoke("winPanelFadeIn");
            InvokeRepeating("winPanelFadeOut", 3.0f, 0.05f);   // 승리 알림창의 알파를 서서히 증가 Invoke함수
        }
    }

    public void winPanelFadeOut()
    {
        m_winWindowPanel.alpha -= 0.05f;
        if (m_winWindowPanel.alpha <= 0.0f)
        {
            CancelInvoke("winPanelFadeOut");
            nextDungeonLoad();
        }
    }

    public void uiPanelFadeOut()
    {
        m_ingameUI.alpha -= 0.05f;
        if (m_ingameUI.alpha <= 0.0f)
        {
            CancelInvoke("panelFadeOut");
        }
    }

    public void startSubDialog()
    {
        m_subDialogWindow.SetActive(true);

        if (SceneManager.GetActiveScene().name == "Stage1-1Scene")
            m_subDialogWindow.GetComponent<SubDialogUIManager>().setSubDialogValue("char_misaki_bustup", "안녕! 다음에 또 만나자구");
        else if (SceneManager.GetActiveScene().name == "Stage1-BossScene")
            m_subDialogWindow.GetComponent<SubDialogUIManager>().setSubDialogValue("Minotaur1", "제법이구나 이제부터 제대로 상대해 주마!!!");
    }

    public void endSubDialog()
    {
        if (SceneManager.GetActiveScene().name == "Stage1-1Scene")
        {
            GameObject.Find("NPC").SetActive(false);
            GameManager.instance.stateExplain(3);
        }
        else if (SceneManager.GetActiveScene().name == "Stage1-BossScene")
            GameManager.instance.stateExplain(4);
        m_subDialogWindow.SetActive(false);
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

    public void dontDestroyCharacterObject()                    
    {
        m_itemWindow = GameObject.Find("ItemWindow");       // 아이템 관련 창을 찾아서
        DontDestroyOnLoad(m_itemWindow);                    // 캐릭터가 변하기 전까지는 파괴하지 않는다.
    }

    public void dontDestroyDungeonObject(Vector3 position, Quaternion rotation)      // 던전에서 파괴되지 말아야할 오브젝트 연결
    {
        if(CharacterInfoManager.instance.m_playerInfo.m_eCharacterType == CHARACTER_TYPE.AKAZA)
             m_player = (GameObject)Instantiate(Resources.Load("Prefabs/AkazaBattle"), position, rotation);   // 플레이어를 생성
        else if (CharacterInfoManager.instance.m_playerInfo.m_eCharacterType == CHARACTER_TYPE.UNITY)
            m_player = (GameObject)Instantiate(Resources.Load("Prefabs/UnityChanBattle"), position, rotation);   // 플레이어를 생성

        m_player.name = "Player";                                                                        // 플레이어 오브젝트의 이름은 Player
        m_objectPoolManager = GameObject.Find("ObjectPoolManager").gameObject;   // 오브젝트풀 매니저          
        m_ingameUI = GameObject.Find("InGameUI").GetComponent<UIPanel>();                                        // IngameUI 게임오브젝트

        m_explainPanel = GameObject.Find("ExplainPanel").GetComponent<UIPanel>();
        m_playerStateExplainLabel = m_explainPanel.transform.Find("StateExplain").GetComponent<UILabel>();
        m_dungeonStateExplainLabel = m_explainPanel.transform.Find("DungeonStateExplain").GetComponent<UILabel>();
        m_deathWindow = m_ingameUI.transform.Find("DeathWindow").gameObject;
        m_ingameSubUI = GameObject.Find("InGameSubUI");

        m_dialogWindow = m_ingameSubUI.transform.Find("DialogWindow").gameObject;     // NPC 대화창
        m_subDialogWindow = m_ingameSubUI.transform.Find("SubDialogWindow").gameObject;     // NPC 대화창
        m_winWindowPanel = m_ingameSubUI.transform.Find("WinWindow").GetComponent<UIPanel>();     // 승리창
        m_mainLight = GameObject.Find("Directional Light").gameObject;           // 광원
        m_camera = GameObject.Find("Camera").gameObject;                // 카메라
        m_followCam = m_camera.GetComponentInChildren<FollowCam>();

        DontDestroyOnLoad(m_ingameUI.gameObject);                            // 하나의 스테이도중에는 파괴되지 않도록 한다.
        DontDestroyOnLoad(m_player);
        DontDestroyOnLoad(m_objectPoolManager);
        DontDestroyOnLoad(m_mainLight);
        DontDestroyOnLoad(m_camera);
        DontDestroyOnLoad(m_ingameSubUI);
    }

    public void destroyDungeonObject()      // 던전 오브젝트를 파괴
    {
        Destroy(m_dialogWindow);
        Destroy(m_ingameUI.gameObject);
        Destroy(m_player);
        Destroy(m_objectPoolManager);
        Destroy(m_mainLight);
        Destroy(m_camera);
        Destroy(m_ingameSubUI);
    }

    public void destoryCharacterObject()
    {
        Destroy(m_itemWindow);
    }

    public void levelUp()
    {
        m_playerStateExplainLabel.gameObject.SetActive(true);
        m_playerStateExplainLabel.text = "Level Up!";
        m_playerStateExplainLabel.alpha = 1.0f;

        if (IsInvoking("fadeOutStateText") == true)
            CancelInvoke("fadeOutStateText");
        InvokeRepeating("fadeOutStateText", 3.0f, 0.05f);
    }

    public void fadeOutStateText()
    {
        m_playerStateExplainLabel.alpha -= 0.05f;
        if (m_playerStateExplainLabel.alpha <= 0.0f)
        {
            CancelInvoke("fadeOutStateText");
            m_playerStateExplainLabel.gameObject.SetActive(false);
        }
    }

    public void stateExplain(int type)
    {
        m_dungeonStateExplainLabel.gameObject.SetActive(true);
        m_dungeonStateExplainLabel.alpha = 1.0f;
        if (type == 0)
            m_dungeonStateExplainLabel.text = "대화를 해야 진행할 수 있습니다.";
        else if (type == 1)
            m_dungeonStateExplainLabel.text = "몬스터를 모두 잡아야 진행할 수 있습니다.";
        else if(type == 2)
            m_dungeonStateExplainLabel.text = "[00ff00][ 의문의 소녀 ][-] 전투 참전";
        else if (type == 3)
            m_dungeonStateExplainLabel.text = "[00ff00][ 의문의 소녀 ][-] 전투 이탈";
        else if (type == 4)
            m_dungeonStateExplainLabel.text = "[ff0000][ 미노킹 ][-] 의 공격속도가 상승하였습니다.";

        if (IsInvoking("fadeOutText") == true)
            CancelInvoke("fadeOutText");
        InvokeRepeating("fadeOutText", 3.0f, 0.05f);  // 서서히 알파를 감소시키는 Invoke함수
    }

    private void fadeOutText()                        // 서서히 알파를 감소시키는 Invoke함수
    {
        m_dungeonStateExplainLabel.alpha -= 0.05f;
        if (m_dungeonStateExplainLabel.alpha <= 0.0f)
        {
            CancelInvoke("fadeOutText");
            m_dungeonStateExplainLabel.gameObject.SetActive(false);
        }
    }

    public void nextDungeonLoad()                                 // 다음 던전 로드
    {
        m_player.GetComponent<NavMeshAgent>().enabled = false;    // 네비메쉬를 끈다.(포탈 이동 시의 위치 에러 방지)

        if (SceneManager.GetActiveScene().name == "Stage1-1Scene")
        {
            m_ingameUI.transform.Find("MiniMapCanvas/MiniMapTerrainImage").GetComponent<RawImage>().texture = Resources.Load("Texture/MiniMap1-3", typeof(Texture2D)) as Texture2D;
            debug.Log("텍스쳐바뀜");
            SceneManager.LoadScene("Stage1-2Scene");
        }
        else if (SceneManager.GetActiveScene().name == "Stage1-2Scene")
        {
            m_ingameUI.transform.Find("MiniMapCanvas/MiniMapTerrainImage").GetComponent<RawImage>().texture = Resources.Load("Texture/MiniMap1-2", typeof(Texture2D)) as Texture2D;
            debug.Log("텍스쳐바뀜");
            SceneManager.LoadScene("Stage1-BossScene");
        }
        else if (SceneManager.GetActiveScene().name == "Stage1-BossScene")  // 보스 씬이 끝난 경우
        {
            destroyDungeonObject();                                         // 던전 오브젝트 파괴
            m_stopwatch.Stop();                                             // 스탑워치를 멈추고 
            m_iClearTime = m_stopwatch.ElapsedMilliseconds / 1000.0f;       // 클리어타임에 대입
            SceneManager.LoadScene("RankingScene");                         // 랭킹 씬을 불러온다.
        }
    }

    public void returnLobby()
    {
        destroyDungeonObject();                                         // 던전 오브젝트 파괴
        SceneManager.LoadScene("LobbyScene");
    }

    public void exitGame()                                         // 게임 종료하기
    {
        Application.Quit();
    }
}
