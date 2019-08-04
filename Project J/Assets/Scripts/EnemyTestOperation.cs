using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTestOperation : Enemy
{
    public enum ENEMY_STATE   // 적의 AI 상태
    {
        PATROL,         // 정지 순찰
        DETECTION,      // 감지
        ATTACK,         // 공격
        RETURN,         // 원위치로 돌아가기
    }

    private Vector3 m_createPosition;                        // 처음 생성된 위치
    private float m_fCreateDistance;                         // 처음 생성된 위치와의 거리차

    private NavMeshAgent m_agent;                            // 네비메시 에이전트 
    Vector3 m_patrolPosition;                                // 순찰하려는 위치
    public float patrolTimer = 3.0f;                         // 순찰 1회 유지 시간
    private ENEMY_STATE m_eEnemyState = ENEMY_STATE.PATROL;  // 적의 상태
    private int m_patrolBehaviour;                           // 순찰이 어떤 행위 중인가? (가만히 있거나 이동하거나)

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_thisTransform = GetComponent<Transform>();
        m_targetTransform = GameObject.Find("Player").GetComponent<Transform>();
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.isStopped = false;
       m_createPosition = GetComponent<Transform>().position;
    }

    // Update is called once per frame
    void Update()
    {
        m_aniState = m_animator.GetCurrentAnimatorStateInfo(0);                                      // 현재 애니메이션 진행상태
        m_aniTransition = m_animator.GetAnimatorTransitionInfo(0);                                   // 현재 애니메이션 전환상태
        m_fTargetDistance = Vector3.Distance(m_targetTransform.position, m_thisTransform.position);  // 상대와의 거리차를 계산함
        m_fCreateDistance = Vector3.Distance(m_createPosition, m_thisTransform.position);
        m_animator.SetFloat("moveVelocity", m_agent.speed);                                          // 현재 이동속도를 애니메이터에 전달


        if (m_fCreateDistance > 50)                         // 처음 생성된 위치로부터 50이상 벗어나면
        {
            m_eEnemyState = ENEMY_STATE.RETURN;             // 되돌아가기
        }

        if(m_eEnemyState == ENEMY_STATE.RETURN)             // 되돌아가는 상태면 아무런 상태도 전환될 수 없음
        {
            if (m_fCreateDistance < 3)                      // 원위치와의 거리차가 3 이하이면
                m_eEnemyState = ENEMY_STATE.PATROL;         // 순찰 상태로 전환
        }
        else if(m_eEnemyState != ENEMY_STATE.RETURN)        // 되돌아가는 상태가 아니라면 새로운 상태 부여
        {
            if (m_fTargetDistance > 50)                     // 50이상의 범위
                m_eEnemyState = ENEMY_STATE.PATROL;
            else if (m_fTargetDistance > 2)                 // 2~50까지의 범위
                m_eEnemyState = ENEMY_STATE.DETECTION;
            else if (m_fTargetDistance <= 2)                // 2이하의 범위
                m_eEnemyState = ENEMY_STATE.ATTACK;
        }
        OperateInput();                                     // 적의 행동패턴 입력
    }

    void OperateInput()
    {
        int stateLevel = m_animator.GetInteger("stateLevel");       // 현재 상태 레벨을 받아옴
        if (stateLevel != 44)                                       // 상태레벨이 사망이 아니면 조작 가능
        {   
            switch (m_eEnemyState)
            {
            case ENEMY_STATE.PATROL:
                patrol();
                break;
            case ENEMY_STATE.DETECTION:
                targetMove(m_targetTransform.position,10);
                break;
            case ENEMY_STATE.ATTACK:
                m_thisTransform.LookAt(m_targetTransform);
                baseAttack();
                skill();
                break;
            case ENEMY_STATE.RETURN:
                targetMove(m_createPosition, 60);
                break;
            }
        }
    }

    void patrol()                       // 순찰
    {
        patrolTimer -= Time.deltaTime;  // 순찰 유지 시간을 감소시킴
        if (patrolTimer < 0.0f)         // 순찰 시간이 끝났으면
        {
            setPatrolBehaviour();       // 순찰 행위를 지정
            patrolTimer = 5.0f;         // 순찰 시간 갱신
        }
        else
        {
            if (m_patrolBehaviour == 0)
            {
                m_agent.speed = 0;
                m_agent.destination = m_thisTransform.position;                                                       // IDLE 상태

            }
            else if (m_patrolBehaviour == 1)
            {
                m_agent.speed = 5;
                targetMove(m_patrolPosition,5);
            }
        }

    }

    void setPatrolBehaviour()         // 순찰 행위 지정
    {
        m_patrolBehaviour = Random.Range(0, 2);                     // 0~1까지의 값중 랜덤   
        if (m_patrolBehaviour == 1)                                 // MOVE 상태
        {
            float randomX = Random.Range(0f, 100f);                 // x랜덤생성 0~100
            float randomZ = Random.Range(0f, 100f);                 // z랜덤생성 0~100
            m_patrolPosition = transform.position + new Vector3(randomX, 0, randomZ);      // 정찰 위치 지정
        }
    }

    void targetMove(Vector3 targetPos, float moveSpeed)         // 타겟으로 지정 속도만큼 이동
    {
        Debug.Log(m_agent.pathStatus);
        m_agent.speed = moveSpeed;
       m_agent.SetDestination(targetPos);
    }

    void baseAttack()
    {
        if (m_fTargetDistance <= 2)                 // 2~50까지의 범위
        {
            m_agent.destination = m_thisTransform.position;
            m_animator.SetTrigger("attack");
            m_agent.speed = 0;
        }
        //else
        //    transform.Translate(transform.forward * 10 * Time.deltaTime, Space.World);
    }

    void skill()
    {
        if (m_fTargetDistance <= 10)
        {
            m_animator.SetTrigger("shout");
            m_animator.SetFloat("shoutCoolTime",10.0f);
        }
    }
}
