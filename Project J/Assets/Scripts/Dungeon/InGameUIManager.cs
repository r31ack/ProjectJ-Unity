using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
    public UIButton m_button1; // 조작 버튼 1
    public UIButton m_button2; // 조작 버튼 2
    public UIButton m_button3; // 조작 버튼 3
    public UIButton m_buttonA; // 조작 버튼 A
    public UIButton m_buttonB; // 조작 버튼 B

    public UISlider hpBar;     // HP Bar
    public UIScrollBar holdTimerBar;    // 스킬 홀드 타임표시
    public UISlider chargeTimerBar; // 원형 충전 타이머바
    public UISprite sprKnob;   // 컨트롤러 방향 손잡이 이미지
    private UILabel m_chargeLevelLabel;

    private UnityChanInfomation m_unityChanInfo;                           // 유니티짱의 데이터 정보를 가지고 있는 스크립트
    private Vector2 m_vec2KnobCenterPos = new Vector2(-500, -250);   // 조이스틱 손잡이 중심 좌표
    private Vector2 m_vec2KnobNormalPos;                             // 조이스틱 손잡이 노말 벡터 (손잡이가 꺾인 방향을 알기 위해)
    private float m_fKnobAngle;                                      // 조이스틱 중심 위치로부터 손잡이까지의 각도
    private float m_fKnobRadius = 50.0f;                             // 조이스틱 중심 위치로부터 반지름 

    GameObject m_characterInfomationMenu;         // 캐릭터 정보 창 최상위 부모 오브젝트
    GameObject m_inventoryMenu;                   // 인벤토리 창 최상위 부모 오브젝트

    void Awake()
    {
        m_button1 = transform.Find("ControllerPanel/Button1").GetComponent<UIButton>();
        m_button2 = transform.Find("ControllerPanel/Button2").GetComponent<UIButton>();
        m_button3 = transform.Find("ControllerPanel/Button3").GetComponent<UIButton>();
        m_buttonA = transform.Find("ControllerPanel/ButtonA").GetComponent<UIButton>();
        m_buttonB = transform.Find("ControllerPanel/ButtonB").GetComponent<UIButton>();

        m_characterInfomationMenu = transform.Find("ItemWindow/InfomationUI").gameObject;
        m_inventoryMenu = transform.Find("ItemWindow/InventoryUI").gameObject;
    }



    // Start is called before the first frame update
    void Start()
    {
        m_unityChanInfo = GameObject.Find("Player").GetComponent<UnityChanInfomation>();  // 유니티짱의 데이터 정보를 가지고 있는 스크립트와 연결
        holdTimerBar = transform.Find("InfoPanel/HoldTimer").GetComponent<UIScrollBar>();
        chargeTimerBar = transform.Find("InfoPanel/ChargeTimer").GetComponent<UISlider>();
        m_chargeLevelLabel = transform.Find("InfoPanel/ChargeTimer/ChargeLevelLabel").GetComponent<UILabel>();
    }
    // Update is called once per frame
    void Update()
    {
        joysticControll();                                                              // 조이스틱 UI 처리
        inputButton();                                                                  // 입력 버튼 UI 처리
        hpBar.value = m_unityChanInfo.curHP / m_unityChanInfo.maxHP;  // 체력을 퍼센트 수치화
        if (m_unityChanInfo.m_fHlodTimer > 0.0f)
        {
            holdTimerBar.gameObject.SetActive(true);
            holdTimerBar.barSize = m_unityChanInfo.m_fHlodTimer / 2.5f;
        }
        else
            holdTimerBar.gameObject.SetActive(false);

        if (m_unityChanInfo.m_fChargeTimer > 0.0f)
        {
            chargeTimerBar.gameObject.SetActive(true);
            chargeTimerBar.value = 1 - (m_unityChanInfo.m_fChargeTimer / 3.0f);

            if (m_unityChanInfo.m_fChargeTimer > 2.0f)
                m_chargeLevelLabel.text = "[ff0000]3[-]";
            else if (m_unityChanInfo.m_fChargeTimer > 1.0f)
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

        m_fKnobAngle = Quaternion.FromToRotation(Vector3.up, m_vec2KnobNormalPos - Vector2.zero).eulerAngles.z; // 두 벡터 사이의 각도 (0~360도)

        if (m_vec2KnobNormalPos == Vector2.zero)                                                                // 조이스틱을 조작하지 않았으면
            sprKnob.SetRect(m_vec2KnobCenterPos.x, m_vec2KnobCenterPos.y, 100, 100);                            // 조이스틱 위치는 가운데
        else                                                                                                    // 조이스틱을 조작 했으면
            sprKnob.SetRect(m_vec2KnobCenterPos.x + m_fKnobRadius * Mathf.Sin(m_fKnobAngle * Mathf.Deg2Rad),    // 조이스틱 위치는 중심 좌표로부터 반지름까지의 해당 각도 거리에 위치
                              m_vec2KnobCenterPos.y - m_fKnobRadius * Mathf.Cos(m_fKnobAngle * Mathf.Deg2Rad), 100, 100);
    }

    void inputButton() // 버튼 입력에 따른 UI처리 함수 (윈도우 디버그 용도)
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S))
                m_buttonB.SetState(UIButtonColor.State.Pressed, true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            m_buttonB.SetState(UIButtonColor.State.Normal, true);
        if (Input.GetKeyDown(KeyCode.I))
            m_button1.SetState(UIButtonColor.State.Pressed, true);
        else if (Input.GetKeyUp(KeyCode.I))
            m_button1.SetState(UIButtonColor.State.Normal, true);
        if (Input.GetKeyDown(KeyCode.O))
            m_button2.SetState(UIButtonColor.State.Pressed, true);
        else if (Input.GetKeyUp(KeyCode.O))
            m_button2.SetState(UIButtonColor.State.Normal, true);
        if (Input.GetKeyDown(KeyCode.P))
            m_button3.SetState(UIButtonColor.State.Pressed, true);
        else if (Input.GetKeyUp(KeyCode.P))
            m_button3.SetState(UIButtonColor.State.Normal, true);
    }

    public void openCharacterInfomation()
    {
        m_characterInfomationMenu.SetActive(true);
    }

    public void openInventory()
    {
        m_inventoryMenu.SetActive(true);
    }

    public void openShop()
    {
        m_characterInfomationMenu.SetActive(false);
        m_inventoryMenu.SetActive(true);
    }

    public void closeWindow()
    {
        m_characterInfomationMenu.SetActive(false);
        m_inventoryMenu.SetActive(false);
    }

}


