using UnityEngine;
using System.Collections;
using UnityEngine.AI;                                            // NavMesh를 사용해 컨트롤

public enum PLAYER_STATE
{
    IDLE = 0,
    BASE_ATTACK_BEGIN = 1,  // 기본 공격
    BASE_ATTACK_1 = 1,    // 기본공격 1
    BASE_ATTACK_2 = 2,    // 기본공격 2
    BASE_ATTACK_3,        // 기본공격 3
    BASE_ATTACK_END,

    SKILL_BEGIN,      // 쿨타임이 돌아가는 스킬
    DASH_SKILL_1,       // 대쉬스킬 : 이동 발도
    CHARGE_SKILL_1,     // 차지스킬1 : 리프어택
    CHARGE_SKILL_2,     // 차지스킬2 : 검기 날리기
    HOLD_SKILL_1,       // 홀드스킬 : 난무
    SKILL_END,

    STEP = 8,   // 스텝 스킬 (빠르게 이동)
    ROLL = 9,   // 구르기 스킬 (다양한 스킬 캔슬 및 이동회피)
    HIDE = 13,  // 하이딩 스킬
    DIE = 44,

}

public class PlayerOperation : Player   // 유니티짱 조작 스크립트
{
    // 캐릭터의 조작에 따라 UI가 바뀌는 경우 (ex:NPC와의 대화 시작)

    ParticleSystem m_healEffect;
    protected int m_iStateLevel;

    private void Awake()
    {
        m_healEffect = transform.Find("HealEffect").GetComponent<ParticleSystem>();
        m_healEffect.Stop();
    }

    void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    void Update()
    {
        m_aniTransition = m_animator.GetAnimatorTransitionInfo(0);           // 현재 애니메이션 전환상태
        m_aniState = m_animator.GetCurrentAnimatorStateInfo(0);              // 현재 애니메이션 진행상태

        m_iStateLevel = m_animator.GetInteger("stateLevel");                 // 현재 상태 레벨을 받아옴
        OperateInput();
  
        if (Input.GetMouseButtonDown(0) == true)
        {
            rayCastNPC(12);
        }
    }

    protected void drinkPotion()                                             // 포션 먹기
    {
        if (Input.GetKey(KeyCode.Keypad1) || InputManager.instance.keyDownCheck(KeyCode.Keypad1) == true)        // 포션 먹기
        {
            if (CharacterInfoManager.instance.coolTimeCheck((int)SKILL_TYPE.POTION) == false)               // 쿨타임이 없으면
            {
                int maxHP = CharacterInfoManager.instance.m_playerInfo.m_iMaxHp;
                m_healEffect.Play();
                CharacterInfoManager.instance.beginCoolTimeDown((int)SKILL_TYPE.POTION);                // 쿨타임 시동
                if (CharacterInfoManager.instance.m_iCurHp >= maxHP)
                {
                    // 포션을 먹었다고 판정내리지않음
                }
                else
                {

                    CharacterInfoManager.instance.m_iCurHp += 30;
                    if (CharacterInfoManager.instance.m_iCurHp > maxHP)
                        CharacterInfoManager.instance.m_iCurHp = maxHP;
                }
            }
        }
    }

    protected virtual void OperateInput()
    {
        if (m_iStateLevel < 18)      // 상태레벨이 피격중이 아니면 조작 가능
        {
            if (m_iStateLevel == 0)     // 상태레벨이 0이면 기본행위            
                move();              // 이동
            if (m_iStateLevel <= 1)     // 상태레벨이 1이하 기본 콤보만
                baseAttack();        // 기본공격
            if (m_iStateLevel == 4)
                baseAttack();        // 기본공격
            if (m_iStateLevel != 9)
                roll();
            drinkPotion();           // 포션은 언제든지 먹을수잇음
        }
    }

    protected virtual void move()
    {
        float moveVelocity = m_animator.GetFloat("moveVelocity");  // 애니메이터로부터 현재 이동속도를 받아옴

        if (Input.GetKey(KeyCode.W) || InputManager.instance.keyPressCheck(KeyCode.W)==true)        // 앞방향 이동
        {
            if (moveVelocity < 20)         // 속도가 40보다 작다면
                moveVelocity += 30.0f * Time.deltaTime;         // 속도를 1 더함
        }
        else if (Input.GetKey(KeyCode.S) || InputManager.instance.keyPressCheck(KeyCode.S) == true)   // 뒷걸음
        {
            if (moveVelocity > -10)        // 뒤로가는 속도가 20보다 작다면
                moveVelocity -= 30.0f * Time.deltaTime;      // 뒤로가는 속도를 1 더함
        }
        else                                // 이동중이 아닐 때 
        {
            if (moveVelocity > 1)
                moveVelocity -= 30.0f * Time.deltaTime;
            else if (moveVelocity < -1)
                moveVelocity += 30.0f * Time.deltaTime;
            else
                moveVelocity = 0.0f;
        }
        rotate();
        transform.Translate(transform.forward * moveVelocity * Time.deltaTime, Space.World);   // 앞으로 현재 속도만큼 이동
        m_animator.SetFloat("moveVelocity", moveVelocity);                                     // 애니메이터에 현재 속도 세팅
    }

    protected virtual void rotate()   // 좌우 회전
    {
        if (Input.GetKey(KeyCode.A) || InputManager.instance.keyPressCheck(KeyCode.A) == true)        // 좌우 회전
            transform.Rotate(Vector3.up, -100 * Time.deltaTime, Space.World);
        else if (Input.GetKey(KeyCode.D) || InputManager.instance.keyPressCheck(KeyCode.D) == true)
            transform.Rotate(Vector3.up, +100 * Time.deltaTime, Space.World);
    }


    protected virtual void baseAttack()
    {
        if (Input.GetKeyDown(KeyCode.Keypad4) == true || InputManager.instance.keyDownCheck(KeyCode.Keypad4) == true)
        {
            int baseComboCount = m_animator.GetInteger("baseComboCount");       // 콤보 횟수를 받아옴

            if (baseComboCount == 0)   // 콤보횟수가 없으면
            {
                SkillUIManager.instance.setSkillType(6, SKILL_TYPE.FLASH);
                SkillUIManager.instance.setSkillType(5, SKILL_TYPE.RAMPAGE);
                SkillUIManager.instance.setSkillType(3, SKILL_TYPE.ROTATE_ATTACK);

                m_animator.SetFloat("baseComboTimer", 1.0f);   // 다음 콤보 연결 유지가능 시간 1초 세팅
                m_animator.SetInteger("stateLevel", 4);        // 상태 레벨 4로 세팅 
                m_animator.SetInteger("baseComboCount", 1);    // 콤보 횟수 1로 세팅
            }
            else if (baseComboCount == 1)   // 콤보횟수가 1이면
            {
                if (m_aniState.IsName("BaseAttack1") == true && m_aniState.normalizedTime > 0.5f) // baseAttack1의 애니메이션이 50퍼 이상이면
                {
                    m_animator.SetFloat("baseComboTimer", 1.0f);   // 다음 콤보 연결 유지가능 시간 1초 세팅
                    m_animator.SetInteger("baseComboCount", 2);
                    m_animator.SetInteger("stateLevel", 4);        // 상태 레벨 4로 세팅 
                }
            }
            else if (baseComboCount == 2)   // 콤보횟수가 2이면
            {
                if (m_aniState.IsName("BaseAttack2") == true && m_aniState.normalizedTime > 0.4f) // baseAttack1의 애니메이션이 40퍼 이상이면
                {
                    m_animator.SetFloat("baseComboTimer", 1.0f);   // 다음 콤보 연결 유지가능 시간 1초 세팅
                    m_animator.SetInteger("baseComboCount", 3);
                    m_animator.SetInteger("stateLevel", 4);        // 상태 레벨 4로 세팅    
                }
            }
        }
        if (Input.GetKey(KeyCode.Keypad4) == true || InputManager.instance.keyPressCheck(KeyCode.Keypad4) == true)
            m_animator.SetBool("baseCombo", true);
        else
            m_animator.SetBool("baseCombo", false);
    }


    protected void moveAttack()   // 이동과 동시에 가능한 공격타입
    {
        if (Input.GetKey(KeyCode.Keypad3) || InputManager.instance.keyPressCheck(KeyCode.Keypad3) == true)        // 좌우 회전
        {
            if (Input.GetKey(KeyCode.A))
            {
                m_animator.SetInteger("direction", 1);
                m_animator.SetInteger("stateLevel", 4);        // 상태 레벨 4로 세팅 

                SkillUIManager.instance.setSkillType(6, SKILL_TYPE.FLASH);
                SkillUIManager.instance.setSkillType(5, SKILL_TYPE.RAMPAGE);
            }

            else if (Input.GetKey(KeyCode.D))
            {
                m_animator.SetInteger("direction", 3);
                m_animator.SetInteger("stateLevel", 4);        // 상태 레벨 4로 세팅 

                SkillUIManager.instance.setSkillType(6, SKILL_TYPE.FLASH);
                SkillUIManager.instance.setSkillType(5, SKILL_TYPE.RAMPAGE);
            }

            if (Input.GetKey(KeyCode.W))
            {
                m_animator.SetInteger("direction", 2);
                m_animator.SetInteger("stateLevel", 4);        // 상태 레벨 4로 세팅 
            }

            else if (Input.GetKey(KeyCode.S))
            {
                m_animator.SetInteger("direction", 4);
                m_animator.SetInteger("stateLevel", 4);        // 상태 레벨 4로 세팅 
            }
        }
    }

    protected virtual void roll()
    {
        if (Input.GetKey(KeyCode.LeftShift) || InputManager.instance.keyPressCheck(KeyCode.Keypad2) == true)
        {
            if (Input.GetKey(KeyCode.A))
            {
                m_animator.SetFloat("roll", 1);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                m_animator.SetFloat("roll", 2);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                m_animator.SetFloat("roll", 3);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                m_animator.SetFloat("roll", 4);
            }
            else
                m_animator.SetFloat("roll", 4);

            m_animator.SetTrigger("rollTrigger");
            m_animator.SetInteger("stateLevel", 9);
            SkillUIManager.instance.setSkillType(6, SKILL_TYPE.LEAF_ATTACK);
            SkillUIManager.instance.setSkillType(3, SKILL_TYPE.RUSH);
        }
    }

    void rayCastNPC(float rayLength)   // npc와 대화하기 위한 레이케스트
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits;

        hits = Physics.RaycastAll(ray, rayLength);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider.tag == "NPC")
            {
                GameManager.instance.startNPCChat();     // 채팅을 시작
                m_animator.SetFloat("moveVelocity", 0);  // 속도 초기화
            }
        }
    }
}
