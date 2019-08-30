using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SkillUIButton  // 동적으로 바뀌는 스킬 UI 버튼
{
    public UIButton m_button;          // 클릭시 동작하기 위한 버튼
    public UISprite m_forgroundImage;  // 비활성화 시의 스킬 이미지
    public UISprite m_backgroundImage; // 활성화 시의 스킬 이미지 (자식 오브젝트)  
    public UILabel m_coolTimeText;     // 쿨타임 표시
}

public class SkillUIManager : MonoSingleton<SkillUIManager>
{
    private SkillUIButton[] m_skillUIButton = new SkillUIButton[6];

    private int m_iSkillMaxCount;
    GameObject m_subIconUI;                   // 서브 아이콘 UI  
    GameObject[] m_skillPrefab;
    UISprite[] m_image;
    UILabel[] m_name;
    UILabel[] m_coolTime;
    UIGrid m_grid;


    SKILL_TYPE[] m_eCurSkillType =  new SKILL_TYPE[6];           // 현재 버튼에 해당하는 스킬 타입

    private void Awake()
    {
        List<DefaultSkillInfo> skillInfo = CharacterInfoManager.instance.m_arrLstDefaultSkillInfo;
        m_iSkillMaxCount = 6;

        m_grid = GetComponent<UIGrid>();

        m_skillPrefab = new GameObject[m_iSkillMaxCount];     // 스킬 최대 갯수만큼 스킬프리팹 할당
        m_image = new UISprite[m_iSkillMaxCount];
        m_coolTime = new UILabel[m_iSkillMaxCount];
        m_name = new UILabel[m_iSkillMaxCount];

        m_subIconUI = transform.Find("SublIconlUI").gameObject;

        for (int i=0; i< m_iSkillMaxCount; i++)
        {     

            m_skillPrefab[i] = (GameObject)Instantiate(Resources.Load("Prefabs/SkillIcon"));   // 스킬 아이콘 오브젝트를 프리팹으로 생성
            m_skillPrefab[i].transform.parent = m_subIconUI.transform;
            m_skillPrefab[i].transform.localScale = Vector3.one;
            m_image[i] = m_skillPrefab[i].transform.GetComponentInChildren<UISprite>();
            m_image[i].spriteName = skillInfo[i].m_strImageName+"0";
            m_name[i] = m_skillPrefab[i].transform.Find("Name").GetComponent<UILabel>();
            m_name[i].text = skillInfo[i].m_strName;
            m_coolTime[i] = m_skillPrefab[i].transform.Find("CoolTime").GetComponent<UILabel>();
        }

        for (int i = 0; i < 6; i++)
        {
            Transform skillUITransform = transform.Find("ButtonIconUI/Button" + (i + 1));
            m_skillUIButton[i].m_button = skillUITransform.GetComponent<UIButton>();
            m_skillUIButton[i].m_forgroundImage = skillUITransform.GetComponent<UISprite>();
            m_skillUIButton[i].m_backgroundImage = skillUITransform.transform.Find("Disable").GetComponent<UISprite>();
            m_skillUIButton[i].m_coolTimeText = skillUITransform.transform.Find("CoolTime").GetComponent<UILabel>();
        }
    }

    void Start()
    {
        setDefulatSkillType();
        StartCoroutine(coolTimeUpdate());
    }

    private void FixedUpdate()
    {
        inputButton();     // 입력 버튼 UI 처리 (fixed에서 안하면 inputManager와 버튼 우선순위 충돌이 일어남)
    }

    private void Update()
    {
        buttonUpdate();
    }

    IEnumerator coolTimeUpdate()
    {
        while (true)
        {
            List<DefaultSkillInfo> skillInfo = CharacterInfoManager.instance.m_arrLstDefaultSkillInfo;
            for (int i = 0; i < m_iSkillMaxCount; i++)
            {
                float coolTime = skillInfo[i].m_fCurCoolTime;
                if(coolTime <= 0.0f)
                {
                    m_image[i].spriteName = skillInfo[i].m_strImageName+"1";
                    m_coolTime[i].text = "";
                }
                else
                {
                    m_image[i].spriteName = skillInfo[i].m_strImageName+"0";
                    m_coolTime[i].text = coolTime.ToString("N0");
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    void inputButton() // 버튼 입력에 따른 UI처리 함수 (윈도우 디버그 용도)
    {
        //  키보드 입력시 버튼 비주얼 동기화
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1 + i) == true)                                // 해당 키패드를 눌릴 경우
                m_skillUIButton[i].m_button.SetState(UIButtonColor.State.Pressed, true);    // 버튼이 활성화됨
            else if (Input.GetKeyUp(KeyCode.Keypad1 + i) == true)                           // 뗄 경우
                m_skillUIButton[i].m_button.SetState(UIButtonColor.State.Normal, true);     // 버튼 비활성화

            if (m_skillUIButton[i].m_button.state == (UIButtonColor.State.Pressed))    // 버튼이 눌러저있다면
                InputManager.instance.inputPressKey(KeyCode.Keypad1 + i);
        }
    }

    public void setDefulatSkillType()  // 가장 기본적인 스킬배치 (Idle로 전이 경우)
    {
        setSkillType(1, SKILL_TYPE.POTION);
        setSkillType(2, SKILL_TYPE.ROLL);
        setSkillType(3, SKILL_TYPE.RUSH);
        setSkillType(4, SKILL_TYPE.BASE_ATTACK);
        setSkillType(5, SKILL_TYPE.ASSASSINATION);
        setSkillType(6, SKILL_TYPE.LEAF_ATTACK);
    }

    public void setSkillType(int numberPad, SKILL_TYPE skillType)  // 스킬 타입을 동적으로 세팅한다. (매개변수 1. 스킬 패드번호, 2. 스킬 종류)
    {
        List<DefaultSkillInfo> skillInfo = CharacterInfoManager.instance.m_arrLstDefaultSkillInfo;

        // 스킬 타입을 바꾸고 
        m_eCurSkillType[numberPad - 1] = skillType;
        // 버튼 이미지를 바꾼다. 
        m_skillUIButton[numberPad - 1].m_button.normalSprite = skillInfo[(int)skillType].m_strImageName + "2";         // 2은 원형 흑백 (버튼의 노말스프라이트를 변경해야 전체적인 동기화가 잘 됨)
        m_skillUIButton[numberPad - 1].m_backgroundImage.spriteName = skillInfo[(int)skillType].m_strImageName + "3";  // 3는 원형 컬러
    }

    void buttonUpdate()
    {
        for(int i=0; i<6; i++)
        {
            if (m_eCurSkillType[i] < SKILL_TYPE.NOT_COOLTIME_SKILL)      // 쿨타임이 있는 스킬만 쿨타임 표시 (enum에서 NOT_COOLTIME_SKILL보다 인덱스가 높은것은 제외)
                buttonUpdate(i, m_eCurSkillType[i]);                    // 1번 넘버패드 자리는 포션
            else                                                        // 쿨타임이 없는 스킬
            {
                m_skillUIButton[i].m_forgroundImage.fillAmount = 0;   // 쿨타임 양을 0으로 만듬 
                m_skillUIButton[i].m_coolTimeText.text = "";          // 쿨타임 텍스트를 지움
            }

        }
    }

    void buttonUpdate(int index, SKILL_TYPE skillType)                  // 해당 인덱스에 맞는 스킬 타입별 쿨타임 동적 업데이트
    {
        List<DefaultSkillInfo> skillInfo = CharacterInfoManager.instance.m_arrLstDefaultSkillInfo;

        float coolTime = skillInfo[(int)skillType].m_fCurCoolTime;
        float coolTimePercent = skillInfo[(int)skillType].m_fCurCoolTime / skillInfo[(int)skillType].m_fMaxCoolTime;
        m_skillUIButton[index].m_forgroundImage.fillAmount = coolTimePercent;

        if (coolTime <= 0.0f)
        {
            m_skillUIButton[index].m_coolTimeText.text = "";
        }
        else
        {
            m_skillUIButton[index].m_coolTimeText.text = coolTime.ToString("N0");
        }
    }

    private void OnEnable() // 활성화 되면 다시 쿨타임 코루틴실행
    {
        StartCoroutine(coolTimeUpdate()); 
    }

    public void button1Click()
    {
        InputManager.instance.inputDownKey(KeyCode.Keypad1);
    }
    public void button2Click()
    {
        InputManager.instance.inputDownKey(KeyCode.Keypad2);
    }
    public void button3Click()
    {
        InputManager.instance.inputDownKey(KeyCode.Keypad3);
    }
    public void button4Click()
    {
        InputManager.instance.inputDownKey(KeyCode.Keypad4);
    }
    public void button5Click()
    {
        InputManager.instance.inputDownKey(KeyCode.Keypad5);
    }
    public void button6Click()
    {
        InputManager.instance.inputDownKey(KeyCode.Keypad6);
    }
}
