using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    public UIButton btn1;      // 조작 버튼1
    public UIButton btn2;      // 조작 버튼2
    public UIButton btnA;      // 조작 버튼A
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
        if(m_unityChanInfo.m_fHlodTimer > 0.0f)
        {
            holdTimerBar.gameObject.SetActive(true);
            holdTimerBar.barSize = m_unityChanInfo.m_fHlodTimer / 2.5f;
        }
        else
            holdTimerBar.gameObject.SetActive(false);

        if(m_unityChanInfo.m_fChargeTimer > 0.0f)
        {
            chargeTimerBar.gameObject.SetActive(true);
            chargeTimerBar.value = 1- (m_unityChanInfo.m_fChargeTimer / 3.0f);

            if (m_unityChanInfo.m_fChargeTimer > 2.0f)
                m_chargeLevelLabel.text = "[ff0000]3[-]";
            else if(m_unityChanInfo.m_fChargeTimer > 1.0f)
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

    void inputButton() // 버튼 입력에 따른 UI처리 함수
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S))
                btnA.SetState(UIButtonColor.State.Pressed, true);
        }
        else
            btnA.SetState(UIButtonColor.State.Normal, true);

        if (Input.GetMouseButton(0))
            btn1.SetState(UIButtonColor.State.Pressed, true);
        else
            btn1.SetState(UIButtonColor.State.Normal, true);

        if (Input.GetMouseButton(1))
            btn2.SetState(UIButtonColor.State.Pressed, true);
        else
            btn2.SetState(UIButtonColor.State.Normal, true);
    }
}


