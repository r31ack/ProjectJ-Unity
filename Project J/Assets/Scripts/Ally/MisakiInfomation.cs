using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ALLY_STATE   // 기본 아군의 AI 상태 (아래로 갈수록 높은 우선순위)
{
    IDLE,        // 비전투 상태
    PATROL,      // 적 위치를 탐색함
    FOLLOW,      // 플레이어를 따라감
    BASE_ATTACK, // 공격
}

public class MisakiInfomation : Player
{
    public float patrolHoldTime = 3.0f;

    private NavMeshAgent m_agent;                         // 네비메시 에이전트 
    protected ALLY_STATE m_eAllyState = ALLY_STATE.IDLE;  // 비전투 상태
    private Transform m_playerTransform;                  // 플레이어 트랜스폼   
    private Transform m_enemyTransform;                   // 타겟 적 트랜스폼
    private float m_fCoroutineTime = 0.033f;                // 코루틴 주기
    private float m_fTargetDistance = 0.0f;
    private Vector3 m_patrolPosition;
    private float m_fPatrolTimer = 0.0f;

    private bool m_bVisibleCamera;

    private float m_fAttackTimer = 0.0f;                // 공격 유지 시간


    private GameObject m_bArrow;                        // 화살촉!

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.speed = 0;
        m_bArrow = GameObject.Find("Elven Long Bow Arrow");
        m_bArrow.SetActive(false);
    }

    public void battleMode(bool modeState)
    {
        if (modeState == true)
        {
            transform.Find("Imoticon").gameObject.SetActive(false);
            m_eAllyState = ALLY_STATE.PATROL;
            m_animator.SetTrigger("battleMode");
            StartCoroutine("allyUpdate");
        }
        else
        {
            m_eAllyState = ALLY_STATE.IDLE;
            StopCoroutine("allyUpdate");
        }
    }

    IEnumerator allyUpdate()
    {
        while (true)
        {
            m_aniState = m_animator.GetCurrentAnimatorStateInfo(0);                               // 현재 애니메이션 진행상태
            m_aniTransition = m_animator.GetAnimatorTransitionInfo(0);                            // 현재 애니메이션 전환상태
            m_animator.SetFloat("moveVelocity", m_agent.speed);                                   // 현재 이동속도를 애니메이터에 전달
            m_eAllyState = (ALLY_STATE)m_animator.GetInteger("stateLevel");                        // 상태 레벨을 받아옴
            m_fTargetDistance = Vector3.Distance(m_playerTransform.position, transform.position); // 플레이어와의 거리차를 계산함

            if (m_fTargetDistance > 30)
            {
                if (m_eAllyState < ALLY_STATE.FOLLOW)          
                {
                    m_eAllyState = ALLY_STATE.FOLLOW;
                    m_animator.SetInteger("stateLevel", (int)m_eAllyState); // 현재 상태 레벨을 애니메이터에 저장
                    m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
                }
            }
            else if(m_fTargetDistance < 10)
            {
                if (m_eAllyState != ALLY_STATE.BASE_ATTACK)           // 상태 레벨이 공격보다 낮으면
                {
                    m_eAllyState = ALLY_STATE.PATROL;
                    m_animator.SetInteger("stateLevel", (int)m_eAllyState); // 현재 상태 레벨을 애니메이터에 저장
                    m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
                }
            }

            animationTransition();

            if (m_animator.GetBool("stateTransition") == true)         // 상태 전이가 일어났으면 알림
            {
                OperateInput();                                       // 상태에 따른 행동패턴 입력
                m_animator.SetBool("stateTransition", false);         // 상태 전이 알림 해제
            }
            OperateUpdate();

            //Debug.Log(m_eAllyState);

            if (Input.GetKey(KeyCode.Keypad0))
                m_animator.SetTrigger("attack");
            yield return new WaitForSeconds(m_fCoroutineTime);
        }
    }

    void OperateInput()               // 상태가 전이되었을 당시 호출되는 입력 함수
    {
        switch (m_eAllyState)         // 상태를 확인하고
        { 
        case ALLY_STATE.IDLE:         // 기본
            m_agent.SetDestination(transform.position);
            m_agent.speed = 0.0f;
             break;
        case ALLY_STATE.PATROL:       // 탐색
            patrol();
            break;
        case ALLY_STATE.FOLLOW:       // 플레이어를 쫓음
            followPlayer();
            break;
        }
    }

    void OperateUpdate()              // 상태 유지 동안 지속적으로 해야 하는 업데이트
    {
        switch (m_eAllyState)
        {
            case ALLY_STATE.PATROL:         // 탐색
                patrol();
                break;
            case ALLY_STATE.FOLLOW:       // 플레이어를 쫓음
                followPlayer();
                //rayCastForwardLine();
                break;
            case ALLY_STATE.BASE_ATTACK:
                if (m_enemyTransform != null && m_fAttackTimer > 0.0f)
                {
                    transform.LookAt(m_enemyTransform);     // 타겟팅 적을 바라봄
                    m_animator.SetTrigger("attack");        // 공격트리거
                        m_fAttackTimer -= 0.1f;
                    if (m_agent.speed > 0.0f)
                        m_agent.speed -= 1.0f;
                }
                else
                {
                    m_eAllyState = ALLY_STATE.PATROL;
                    m_animator.SetInteger("stateLevel", (int)m_eAllyState); // 현재 상태 레벨을 애니메이터에 저장
                    m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
                }
                break;
        }
    }

    void animationTransition()                       // 애니메이션의 전이에 따른 처리
    {
        if (m_aniTransition.IsName("Idle -> Draw") == true || m_aniTransition.IsName("Shot -> Draw") == true)   // 조준상태가 전이되면
            m_bArrow.SetActive(true);                           // 화살을꺼냄
        else if (m_aniTransition.IsName("Aim -> Shot") == true)  // 발사를 하면
        {
            if (m_bArrow.activeSelf == true)
            {
                m_bArrow.SetActive(false);                          // 화살을 비활성화 시키고 프리팹으로 생성
                GameObject arrow = ObjectPoolManager.Instance.PopFromPool("arrow");
                arrow.transform.position = transform.position + Vector3.up * 1.4f;   // 위치동기화
                arrow.transform.rotation = transform.rotation;
                arrow.SetActive(true);
            }
        }
    }

    void patrol()
    {
        m_animator.SetBool("aimMode", true);
        m_agent.SetDestination(transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, m_playerTransform.rotation, Time.deltaTime * 5);      // 서서히 회전시킴   
        m_agent.speed = 0.0f;  
        rayCastSphere();
    }

    bool rayCastForwardLine()   
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.forward, 50.0f);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider.tag == "enemy")
            {
                m_animator.SetBool("aimMode", true);
                m_eAllyState = ALLY_STATE.BASE_ATTACK;
                m_animator.SetInteger("stateLevel", (int)m_eAllyState); // 현재 상태 레벨을 애니메이터에 저장
                m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
                m_enemyTransform = hit.transform;
                //Debug.Log("찾음");
                m_fAttackTimer = 3.0f;
                return true;
            }
        }
        return false;
    }

    bool rayCastSphere()   // 조준 타겟을 찾기 위한 레이케스트
    {
        var hitColliders = Physics.OverlapSphere(transform.position, 50.0f);

        for (var i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].tag == "enemy")
            {
                m_animator.SetBool("aimMode", true);
                m_eAllyState = ALLY_STATE.BASE_ATTACK;
                m_animator.SetInteger("stateLevel", (int)m_eAllyState); // 현재 상태 레벨을 애니메이터에 저장
                m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
                m_enemyTransform = hitColliders[i].transform;
                //Debug.Log("찾음");
                m_fAttackTimer = 3.0f;
                return true;
            }
        }
        return false;
    }

    void followPlayer()
    {
        if (m_fTargetDistance >= 20)
        {
            m_animator.SetBool("aimMode", false);
            m_agent.SetDestination(m_playerTransform.position);
            m_agent.speed = 10.0f;
        }
        else
        {
            m_eAllyState = ALLY_STATE.PATROL;
            m_animator.SetInteger("stateLevel", (int)m_eAllyState); // 현재 상태 레벨을 애니메이터에 저장
            m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
        }
    }
}