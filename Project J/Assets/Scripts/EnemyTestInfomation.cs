using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyTestInfomation : Enemy
{
    private Collider m_punchCollider;       // 공격판정 콜라이더
    private float m_fShoutCoolTime = 0.0f;
    private NavMeshAgent m_agent;                            // 네비메시 에이전트 

    void Awake()
    {
        m_fMaxHP = 100;
        m_fCurHP = m_fMaxHP;
    }

    void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_animator = GetComponent<Animator>();
        m_animator.SetFloat("shoutCoolTime", 10.0f);      // 스킬 초기 쿨타임 10초
        m_thisTransform = GetComponent<Transform>();
        m_targetTransform = GameObject.Find("Player").GetComponent<Transform>();
        m_punchCollider = m_thisTransform.GetChild(0).GetComponent<Collider>();         // 0번째 자식 오브젝트의 콜라이더를 받아옴
    }

    void Update()
    {
        m_aniState = m_animator.GetCurrentAnimatorStateInfo(0);       // 현재 애니메이션 진행상태
        m_aniTransition = m_animator.GetAnimatorTransitionInfo(0);    // 현재 애니메이션 전환상태
        int stateLevel = m_animator.GetInteger("stateLevel");

        if (stateLevel == 44)
        {
            return;
        }
        if (m_aniState.IsName("Attack") == true || m_aniState.IsName("Shout") == true)        // 공격 이면
        {
            m_agent.speed = 0;
        }
        timerState();
        m_fShoutCoolTime -= Time.deltaTime;
        m_fTargetDistance = Vector3.Distance(m_targetTransform.position, m_thisTransform.position);  // 상대와의 거리차를 계산함
    }

    void timerState()          // 타이머가 걸린 동작을 처리하는 함수
    {
        if (m_fAttackHoldTime > 0.0f)                                    // 공격판정 유지시간 처리
            m_fAttackHoldTime -= Time.deltaTime;
        else                                                             // 공격판정 시간이 끝나면
            m_punchCollider.isTrigger = false;                     // 콜라이더가 충돌하지 않도록 공격판정이 있는 모든 트리거 비활성화
    }

    void attack()
    {
        if (m_aniState.IsName("Attack") == true)        // 공격 이면
        {
            m_punchCollider.isTrigger = true;           // 공격판정 콜라이더 트리거 활성화
            m_fCurAttackDamage = 2.0f;                  // 현재 모션에서의 공격 데미지 2
            m_fAttackHoldTime = 1.0f;                   // 공격판정 유지시간 0.1초
        }
        else if (m_aniState.IsName("Shout") == true)    // 소리치기 이면
        {
            if (Vector3.Distance(m_targetTransform.position, m_thisTransform.position) < 5)   // 콜라이더를 쓰지 않고 반경 10거리 전범위 스턴 공격
            {
                m_targetTransform.GetComponent<UnityChanInfomation>().attated(10, true);
            }
        }
    }

    public void attacted(float damage, bool stun = false)
    {
        int stateLevel = m_animator.GetInteger("stateLevel");
        m_fCurHP -= damage;            // 체력 감소

        if (m_fCurHP <= 0.0f && stateLevel != 44)        // 체력이 0보다 작고 사망상태가 아니면
        {
            m_animator.SetTrigger("die");                // 사망 트리거 활성화
            m_animator.SetInteger("stateLevel", 44);     // 사망상태로 전환
            Destroy(m_thisTransform.gameObject, 3.0f);   // 3초뒤삭제
        }
        else if (stateLevel != 44)
        {
          m_animator.SetTrigger("attated");
          //m_animator.Play("Attacted");
        }
    }

    private void OnTriggerEnter(Collider coll)                // 공격 충돌 처리
    {
        if (coll.gameObject.tag == "player")                   // 충돌 대상이 적 태그를 가지고 있으면
        {
            UnityChanInfomation unityChanScripte = coll.GetComponent<UnityChanInfomation>();
            unityChanScripte.attated(m_fCurAttackDamage);   // 해당 스크립트를 받아와서 나의 현재 공격모션에 따른 데미지를 부여함
        }
    }
}

