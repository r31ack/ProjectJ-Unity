using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanInfomation : Player    // 캐릭터 정보를 담고 있는 스크립트
{
    private Collider m_weaponCollider;       // 착용중인 무기 콜라이더
    private FollowCam m_cameraScript;          // 카메라 스크립트
    private float m_fCameraRotate = 0.0f;      // 카메라 회전시간
    public float m_fHlodTimer = 0.0f;
    public float m_fChargeTimer = 0.0f;

    private void Awake()
    {
        m_fMaxHP = 100;
        m_fCurHP = m_fMaxHP;
    }

    void Start()
    {
        m_animator = GetComponent<Animator>();                                                      // 컴포넌트에서 애니메이터를 동기화
        m_weaponCollider = GameObject.Find("Dragonblade").GetComponent<Collider>(); // 대검 오브젝트의 이름을 찾은 후 콜라이더 동기화
        m_cameraScript = GameObject.Find("Main Camera").GetComponent<FollowCam>();
    }

    void Update()
    {
        m_aniState = m_animator.GetCurrentAnimatorStateInfo(0);       // 현재 애니메이션 진행상태
        m_aniTransition = m_animator.GetAnimatorTransitionInfo(0);    // 현재 애니메이션 전환상태

        timerState();          // 타이머가 걸린 동작을 처리하는 함수
        animationTransition(); // 애니메이션 전이시 정보 처리 관련 함수
        animationState();      // 애니메이션 상태 도중 정보 처리 관련 함수
    }

    void timerState()          // 타이머가 걸린 동작을 처리하는 함수
    {
        float baseComboTimer = m_animator.GetFloat("baseComboTimer");   // 콤보 유지 시간 처리
        if (baseComboTimer > 0.0f)
        {
            baseComboTimer -= Time.deltaTime;
            m_animator.SetFloat("baseComboTimer", baseComboTimer);
        }

        if (m_fAttackHoldTime > 0.0f)                                    // 공격판정 유지시간 처리
            m_fAttackHoldTime -= Time.deltaTime;     
        else                                                             // 공격판정 시간이 끝나면
            m_weaponCollider.isTrigger = false;                     // 콜라이더가 충돌하지 않도록 공격판정이 있는 모든 트리거 비활성화

        if (m_fCameraRotate > 0.0f)                                             // 카메라 회전 시간
        {
            m_fCameraRotate -= Time.deltaTime;
            transform.Rotate(Vector3.up, 800 * Time.deltaTime, Space.World);    // 카메라 회전
        }

        m_fHlodTimer = m_animator.GetFloat("holdTimer");   // 콤보 유지 시간 처리
        if(m_fHlodTimer > 0.0f)
        {
            m_fHlodTimer -= Time.deltaTime;
            m_animator.SetFloat("holdTimer", m_fHlodTimer);
        }

        if (m_animator.GetBool("chargeAttack") == true)
        {
            if (m_fChargeTimer > 3.0f)
            {
                m_animator.SetBool("chargeAttack", false);
                m_fChargeTimer = 0.0f;
            }
            else
            {
                m_fChargeTimer = m_animator.GetFloat("chargeTimer");   // 콤보 유지 시간 처리
                m_fChargeTimer += Time.deltaTime;
                m_animator.SetFloat("chargeTimer", m_fChargeTimer);
            }
        }
        else
            m_fChargeTimer = 0.0f;
    }

    void animationTransition() // 애니메이션 전이시 정보 처리 관련 함수
    {
        if (m_aniTransition.IsName("BaseAttack1 -> Idle") == true ||  // 기본으로 상태 전이될 경우
            m_aniTransition.IsName("BaseAttack2 -> Idle") == true ||
            m_aniTransition.IsName("BaseAttack3 -> Idle") == true ||
            m_aniTransition.IsName("DashAttack -> Idle") == true ||
            m_aniTransition.IsName("Jump -> Idle") == true ||
            m_aniTransition.IsName("Roll -> Idle") == true ||
            m_aniTransition.IsName("Attated -> Idle") == true ||
            m_aniTransition.IsName("Roll -> Move") == true ||
            m_aniTransition.IsName("Stun -> Idle") == true ||
            m_aniTransition.IsName("HoldAttackEnd -> Idle") == true ||
            m_aniTransition.IsName("ChargeAttack1 -> Idle") == true ||
            m_aniTransition.IsName("ChargeAttack2 -> Idle") == true ||
            m_aniTransition.IsName("ChargeAttack3 -> Idle") == true)
        {
            initAnimatorInfo();
        }

        if (m_aniTransition.IsName("AnyState -> Attated") == true)
            m_animator.SetInteger("stateLevel", 18);
        if (m_aniTransition.IsName("AnyState -> Stun") == true)
            m_animator.SetInteger("stateLevel", 20);

        if (m_aniState.IsName("HoldAttack1") == true)
        {
            if (m_aniState.normalizedTime > 0.6f)
                m_animator.Play("HoldAttack2", 0, 0.2f);
        }
        else if (m_aniState.IsName("HoldAttack2") == true) 
        {
            if (m_aniState.normalizedTime > 0.5f)
                m_animator.Play("HoldAttack1", 0, 0.3f);
        }
    }

    void initAnimatorInfo() // 애니메이터 정보를 초기화 (Idle로 전이 시 초기화 할 목적)
    {
        m_animator.SetInteger("baseComboCount", 0); // 기본공격 콤보 횟수를 0으로 만듬
        m_animator.SetInteger("stateLevel", 0);     // 상태 레벨을 0으로 만듬
        m_animator.ResetTrigger("dashAttack");      // 대쉬공격 트리거 초기화
        m_animator.SetFloat("roll", 0);             // 기본콤보 상태를 false로 만듬
        m_animator.SetBool("jump", false);          // 점프 상태 초기화
        m_animator.SetFloat("moveVelocity", 0.0f);  // 이동 속도 초기화
        m_animator.SetFloat("chargeTimer", 0.0f);
    }

    void animationState() // 애니메이션 상태 도중 정보 처리 관련 함수
    {
        roll();       // 구르는 상태 처리
        dashAttack(); // 대쉬공격 상태 처리 

        if (m_aniState.IsName("Charge") == true && m_aniState.normalizedTime > 0.3f) // baseAttack1의 애니메이션이 40퍼 이상이면
        {

        }
        if (m_aniState.IsName("Charge") == true && m_aniState.normalizedTime > 0.4f) // baseAttack1의 애니메이션이 40퍼 이상이면
        {

        }
    }

    void roll() // 구르는 상태 처리
    {
        if (m_aniState.IsName("Roll") == true && m_aniState.normalizedTime > 0.1f && m_aniState.normalizedTime < 0.7f)
        {
            m_animator.SetFloat("moveVelocity", 0.0f);
            float direction = m_animator.GetFloat("roll");
            if (direction == 1)
                transform.Translate(transform.right * -20 * Time.deltaTime, Space.World);
            else if (direction == 2)
                transform.Translate(transform.forward * 20 * Time.deltaTime, Space.World);
            else if (direction == 3)
                transform.Translate(transform.right * 20 * Time.deltaTime, Space.World);
            else if (direction == 4)
                transform.Translate(transform.forward * -20 * Time.deltaTime, Space.World);
        }
    }

    void dashAttack() // 대쉬공격 상태 처리
    {
        if (m_aniState.IsName("DashAttack") == true && m_aniState.normalizedTime > 0.4f && m_aniState.normalizedTime < 0.5f)
        {
            transform.Translate(transform.forward * 200 * Time.deltaTime, Space.World);
        }
    }


    ////////////////////////////////////////////////// 이벤트 처리 함수 ////////////////////////////////////////////////////////////

    public void attack(int type)    // 공격 처리 
    {
        m_weaponCollider.isTrigger = true;               // 무기 콜라이더 트리거 활성화

        if (m_aniState.IsName("BaseAttack1") == true)
        {
            m_fCurAttackDamage = 20.0f;
            m_fAttackHoldTime = 0.1f;
        }
        else if (m_aniState.IsName("BaseAttack2") == true)
        {
            m_fCurAttackDamage = 30.0f;
            m_fAttackHoldTime = 0.1f;
        }
        else if (m_aniState.IsName("BaseAttack3") == true)
        {
            m_fCurAttackDamage = 40.0f;
            m_fAttackHoldTime = 0.1f;
        }
        else if (m_aniState.IsName("DashAttack") == true)
        {
            m_fCurAttackDamage = 100.0f;
            m_fAttackHoldTime = 1.0f;
        }
        else if (m_aniState.IsName("HoldAttack1") == true || m_aniState.IsName("HoldAttack2") == true)
        {
            m_fCurAttackDamage = 10.0f;
            m_fAttackHoldTime = 1.0f;
        }
        else if (m_aniState.IsName("ChargeAttack1") == true || m_aniState.IsName("ChargeAttack2") == true || m_aniState.IsName("ChargeAttack3") == true)
        {
            GameObject swordWind = ObjectPoolManager.Instance.PopFromPool("swordWind");
            swordWind.transform.position = transform.position;   // 위치동기화
            swordWind.transform.rotation = transform.rotation;   // 방향동기화

            if (type == 0)
            {
                swordWind.transform.Translate(new Vector3(5,2,0), Space.Self);
                swordWind.transform.Rotate(Vector3.forward, -2, Space.Self);
            }
            else if (type == 1)
            {
                swordWind.transform.Translate(new Vector3(-5, 2, 0), Space.Self);
                swordWind.transform.Rotate(Vector3.forward, 2, Space.Self);
            }
            else if (type == 2)
            {
                swordWind.transform.Rotate(Vector3.forward, 80, Space.Self);
            }
            swordWind.SetActive(true);
            m_fCurAttackDamage = 10.0f;
            m_fAttackHoldTime = 1.0f;
        }
    }

    public void attated(float damage, bool stun=false) // 피격 처리
    {
        m_fCurHP -= damage;                      // 받은 데미지만큼 현재 체력을 감소시킨다.
        float stateLevel = m_animator.GetInteger("stateLevel");

        if (stun == true)
        {
            if(stateLevel != 20)
                 m_animator.SetInteger("stateLevel", 19);    // 스턴 상태로 레벨 전환
        }
        else
        {
            //if (stateLevel != 18)
                //m_animator.SetInteger("stateLevel", 17);    // 피격 상태로 레벨 전환
        }

        if (m_fCurHP<=0.0f)                      // 체력이 없으면
        {
            m_animator.SetInteger("stateLevel", 44);    // 사망 상태로 레벨 전환
        }
    }

    public void cameraControll()                // 카메라 조작
    {
        m_fCameraRotate = 0.2f;
    }

    private void OnTriggerEnter(Collider coll)                // 공격 충돌 처리
    {
        if (coll.gameObject.tag == "enemy")                   // 충돌 대상이 적 태그를 가지고 있으면
        {
            EnemyTestInfomation enemyScript = coll.GetComponentInParent<EnemyTestInfomation>();   // 적 스크립트를 받아와서
            enemyScript.attacted(m_fCurAttackDamage);         // 그 적은 내 현재 공격모션의 데미지를 부여함
        }
    }
}

