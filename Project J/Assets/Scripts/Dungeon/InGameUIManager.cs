using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InGameUIManager : Singleton<InGameUIManager>
{
    public UISlider hpBar;     // HP Bar
    public UIScrollBar holdTimerBar;    // 스킬 홀드 타임표시
    public UISlider chargeTimerBar; // 원형 충전 타이머바
    public UISprite sprKnob;   // 컨트롤러 방향 손잡이 이미지
    private UILabel m_chargeLevelLabel;

    private AkazaState m_unityChanInfo;                              // 유니티짱의 데이터 정보를 가지고 있는 스크립트
    private Vector2 m_vec2KnobCenterPos = new Vector2(-530, -250);   // 조이스틱 손잡이 중심 좌표
    private Vector2 m_vec2KnobNormalPos;                             // 조이스틱 손잡이 노말 벡터 (손잡이가 꺾인 방향을 알기 위해)
    private float m_fKnobAngle;                                      // 조이스틱 중심 위치로부터 손잡이까지의 각도
    private float m_fKnobRadius = 50.0f;                             // 조이스틱 중심 위치로부터 반지름 

    // 인게임 메뉴 창 관련 오브젝트
    GameObject m_characterInfomationMenu;         // 캐릭터 정보 창 최상위 부모 오브젝트
    GameObject m_inventoryMenu;                   // 인벤토리 창 최상위 부모 오브젝트
    GameObject m_optionMenu;                      // 옵션 창 최상의 부모 오브젝트

    GameObject m_enemyInfoUI;   // 적의 정보를 표시하는 UI

    private UISlider m_enemyHpPercentSlider;
    private UILabel m_enemyNameLable;

    private UILabel m_comboCount;
    private bool m_bComboChainState;

    private UILabel m_dynamicButtonLabel;                  // 열은 창에 맞게 착용하기, 버리기, 판매하기 등으로 바뀌는 기능
    private Canvas m_miniMapCanvas;


    void Awake()
    {
        // 인게임 메뉴 오브젝트 
        m_characterInfomationMenu = GameObject.Find("ItemWindow").transform.Find("InfomationUI").gameObject;
        m_optionMenu = GameObject.Find("ItemWindow").transform.Find("OptionUI").gameObject;
        m_inventoryMenu = GameObject.Find("ItemWindow").transform.Find("InventoryUI").gameObject;
        m_dynamicButtonLabel = GameObject.Find("ItemWindow").transform.Find("InventoryUI/DynamicLabel").GetComponent<UILabel>();

        m_enemyInfoUI = transform.Find("InfoPanel/EnemyInfoUI").gameObject;
        m_enemyHpPercentSlider = m_enemyInfoUI.transform.Find("EnemyHPSlider").GetComponent<UISlider>();
        m_enemyNameLable = m_enemyInfoUI.transform.Find("EnemyName").GetComponent<UILabel>();

        holdTimerBar = transform.Find("InfoPanel/HoldTimer").GetComponent<UIScrollBar>();
        chargeTimerBar = transform.Find("InfoPanel/ChargeTimer").GetComponent<UISlider>();
        m_chargeLevelLabel = transform.Find("InfoPanel/ChargeTimer/ChargeLevelLabel").GetComponent<UILabel>();

        m_miniMapCanvas = transform.Find("MiniMapCanvas").GetComponent<Canvas>();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_unityChanInfo = GameObject.Find("Player").GetComponent<AkazaState>();  // 상태 데이터 정보를 가지고 있는 스크립트와 연결
    }

    // Update is called once per frame
    void Update()
    {
        joysticControll();                                                              // 조이스틱 UI 처리
        hpBar.value = CharacterInfoManager.instance.getHpPercent();                     // 체력을 퍼센트 수치화
        if (m_unityChanInfo.m_fHlodTimer > 0.0f)
        {
            holdTimerBar.gameObject.SetActive(true);
            holdTimerBar.barSize = m_unityChanInfo.m_fHlodTimer / 2.5f;
        }
        else
            holdTimerBar.gameObject.SetActive(false);

        if (m_unityChanInfo.m_fSwordWindTimer > 0.0f && m_unityChanInfo.m_fSwordWindChargeFlag == true)
        {
            chargeTimerBar.gameObject.SetActive(true);
            chargeTimerBar.value = 1 - (m_unityChanInfo.m_fSwordWindTimer / 2.4f);

            if (m_unityChanInfo.m_fSwordWindTimer > 1.6f)
                m_chargeLevelLabel.text = "[ff0000]3[-]";
            else if (m_unityChanInfo.m_fSwordWindTimer > 0.8f)
                m_chargeLevelLabel.text = "[ffff00]2[-]";
            else
                m_chargeLevelLabel.text = "[ffffff]1[-]";
        }
        else
            chargeTimerBar.gameObject.SetActive(false);
    }

    void joysticControll() // 조이스틱 조작에 따른 UI 처리 함수
    {
        if (Input.GetKey(KeyCode.W))        // 앞  
            m_vec2KnobNormalPos.y = -1.0f;
        else if (Input.GetKey(KeyCode.S))   // 뒤
            m_vec2KnobNormalPos.y = 1.0f;
        else
            m_vec2KnobNormalPos.y = 0.0f;
        if (Input.GetKey(KeyCode.A))        // 좌
            m_vec2KnobNormalPos.x = 1.0f;
        else if (Input.GetKey(KeyCode.D))   // 우
            m_vec2KnobNormalPos.x = -1.0f;
        else
            m_vec2KnobNormalPos.x = 0.0f;

        if (Input.GetMouseButton(0) == true)
        {
            Vector3 mousePos = Input.mousePosition;                                         // 마우스 포지션을 받음
            if (mousePos.x < 280 && mousePos.x >= 0 && mousePos.y < 280 && mousePos.y >= 0) // 마우스 위치가 조이스틱안에 들어왓으면
            {
                Vector3 dirVector = new Vector3(120, 120, 0) - mousePos;
                dirVector.Normalize();
                m_vec2KnobNormalPos.x = dirVector.x;
                m_vec2KnobNormalPos.y = dirVector.y;
                m_fKnobAngle = Quaternion.FromToRotation(Vector3.up, m_vec2KnobNormalPos - Vector2.zero).eulerAngles.z; // 두 벡터 사이의 각도 (0~360도)

                if (mousePos.x < 280 && mousePos.x >= 0 && mousePos.y < 280 && mousePos.y >= 0) // 마우스 위치가 조이스틱안에 들어왓으면)
                {
                    if (m_fKnobAngle >= 270 || m_fKnobAngle < 90)      // -45~45도는 뒤로
                    {
                        InputManager.instance.inputPressKey(KeyCode.S);
                    }
                    else if (m_fKnobAngle >= 90 && m_fKnobAngle < 270)      // 45~135도는 위
                    {
                        InputManager.instance.inputPressKey(KeyCode.W);
                    }
                    if (m_fKnobAngle >= 45 && m_fKnobAngle < 135)      // 45~135도는 오른쪽
                    {
                        InputManager.instance.inputPressKey(KeyCode.D);
                    }
                    else if (m_fKnobAngle >= 225 && m_fKnobAngle < 315)      // 45~135도는 왼쪽
                    {
                        InputManager.instance.inputPressKey(KeyCode.A);
                    }
                }
            }
        }
        else // 마우스 입력이 없는 경우
            m_fKnobAngle = Quaternion.FromToRotation(Vector3.up, m_vec2KnobNormalPos - Vector2.zero).eulerAngles.z; // 두 벡터 사이의 각도 (0~360도)

        if (m_vec2KnobNormalPos == Vector2.zero)                                                                // 조이스틱을 조작하지 않았으면
            sprKnob.SetRect(m_vec2KnobCenterPos.x, m_vec2KnobCenterPos.y, 100, 100);                            // 조이스틱 위치는 가운데
        else                                                                                                    // 조이스틱을 조작 했으면
            sprKnob.SetRect(m_vec2KnobCenterPos.x + m_fKnobRadius * Mathf.Sin(m_fKnobAngle * Mathf.Deg2Rad),    // 조이스틱 위치는 중심 좌표로부터 반지름까지의 해당 각도 거리에 위치
                                m_vec2KnobCenterPos.y - m_fKnobRadius * Mathf.Cos(m_fKnobAngle * Mathf.Deg2Rad), 100, 100);
    }

    public void openCharacterInfomation()
    {
        m_miniMapCanvas.gameObject.SetActive(false);
        m_dynamicButtonLabel.text = "착용하기";
        m_optionMenu.SetActive(false);                  // 옵션 창 비활성화
        m_characterInfomationMenu.SetActive(true);
        m_inventoryMenu.SetActive(true);
    }

    public void openInventory()
    {
        m_miniMapCanvas.gameObject.SetActive(false);
        m_dynamicButtonLabel.text = "버리기";
        m_optionMenu.SetActive(false);                  // 옵션 창 비활성화
        m_inventoryMenu.SetActive(true);
        m_characterInfomationMenu.SetActive(false);
    }

    public void openShop()
    {
        m_miniMapCanvas.gameObject.SetActive(false);
        m_optionMenu.SetActive(false);                  // 옵션 창 비활성화
        m_characterInfomationMenu.SetActive(false);
        m_inventoryMenu.SetActive(true);
    }

    public void openOption()
    {
        m_miniMapCanvas.gameObject.SetActive(false);
        m_optionMenu.SetActive(true);                  // 옵션 창 비활성화
        m_characterInfomationMenu.SetActive(false);    // 캐릭터 정보창 비활성화
        m_inventoryMenu.SetActive(false);              // 인벤토리창 비활성화
    }

    public void closeWindow()
    {
        m_miniMapCanvas.gameObject.SetActive(true);
        m_optionMenu.SetActive(false);                  // 옵션 창 비활성화
        m_characterInfomationMenu.SetActive(false);     // 캐릭터 정보창 비활성화
        m_inventoryMenu.SetActive(false);               // 인벤토리창 비활성화
    }

    public void showEnemyInfo(string enemyName, float hpPercent) // 적을 공격했을 경우 이름과 체력바가 표시된다.
    {
        m_enemyInfoUI.SetActive(true);                // UI를 활성화시킴
        if (IsInvoking("hideEnemyInfo") == true)      // 인보크가 실행중이면 취소해야 한다.
            CancelInvoke("hideEnemyInfo");

        m_enemyNameLable.text = enemyName;
        m_enemyHpPercentSlider.value = hpPercent;

        if (m_enemyHpPercentSlider.value <= 0.0f)       // 표시된 적의 체력이 0.0이하이면
            Invoke("hideEnemyInfo", 3.0f);              // 3초뒤에 적의 정보를 표시하는 UI를 숨긴다.
    }

    public void hideEnemyInfo()     // 적의 정보를 숨긴다.
    {
        m_enemyInfoUI.SetActive(false);
    }

    public void revive()            // 부활
    {
        GameManager.instance.revive();
    }

    public void returnLobby()       // 로비로 돌아가기
    {
        GameManager.instance.returnLobby();
    }
}



