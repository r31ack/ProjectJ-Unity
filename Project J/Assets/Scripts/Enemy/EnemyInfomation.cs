using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public enum CROWD_CONTROL
{
    NONE,
    STUN,
    STUN_IMMUNE,
    DOWN,
    BACK_ATTACK,
}

public enum SPECIAL_ATTACK
{
    CRITICAL,               // 크리티컬
    BACK_ATTACK,            // 백어택
    CRITICAL_BACK_ATTACK,   // 크리티컬 백어택
}

public enum ENEMY_STATE   // 기본 적의 AI 상태 (아래로 갈수록 높은 우선순위)
{
    IDLE,        // 정지
    PATROL,      // 순찰 (주변을 서서히 탐색)
    DETECTION,   // 감지 (타겟을 대상으로 빠르게 따라간다.)
    RETURN,      // 복귀 (생성 위치에서 너무 멀어진 경우)
    GET_DAMAGE,  // 피격 (공격자를 대상으로 무조건 따라간다.)
    BASE_ATTACK, // 공격 (공격범위 안에 들어온 경우)
    SKILL,       // 스킬 (몬스터 타입별 확장용)
    STUN = 19,   // 스턴
    DIE = 444,   // 사망
}

public class EnemyInfomation : Enemy
{
    private Camera m_uiCamera;              // UI 카메라

    public float idleHoldTime = 5.0f;            // 대기 시간
    public float patrolHoldTime = 5.0f;          // 순찰 시간
    public float followHoldTime = 5.0f;          // 피격시 쫓아오는 시간
    public float attackDistance = 3.0f;

    private float m_fIdleTimer;                  // 대기 타이머
    private float m_fPatrolTimer;                // 순찰 유지 타이머
    private float m_fFollowTargetTimer;          // 피격시 쫓아오는 타이머 

    protected NavMeshAgent m_agent;                          // 네비메시 에이전트 
    private Vector3 m_spawnPosition;                       // 생성된 위치
    protected float m_fSpawnDistance;                        // 생성된 위치와의 거리차
    private Vector3 m_patrolPosition;                      // 순찰하려는 위치
    protected ENEMY_STATE m_eEnemyState = ENEMY_STATE.IDLE;  // 적의 상태

    protected Collider m_punchCollider;       // 공격판정 콜라이더
    private Transform m_playerTransform;
    UnityChanInfomation unityChanScripte;
    private Transform m_sunTransform;

    private float m_fCoroutineTime = 0.1f;    // 코루틴 주기

    void Awake()
    {
        m_fMaxHP = 300;
        m_fCurHP = m_fMaxHP;

        m_fIdleTimer = idleHoldTime;                   
        m_fPatrolTimer = patrolHoldTime;           
        m_fFollowTargetTimer = followHoldTime;
        m_iGetExp = 10;
    }

    void Start()
    {
        m_uiCamera = GameObject.Find("NGUICamera").GetComponent<Camera>();

        m_animator = GetComponent<Animator>();
        m_playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        unityChanScripte = m_playerTransform.GetComponent<UnityChanInfomation>();

        m_sunTransform = GameObject.Find("Directional Light").GetComponent<Transform>();
        m_targetTransform = m_playerTransform;
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.speed = 0;
        m_spawnPosition = GetComponent<Transform>().position;

        m_punchCollider = transform.GetChild(0).GetComponent<Collider>();         // 0번째 자식 오브젝트의 콜라이더를 받아옴

        StartCoroutine("enemyUpdate");
    }

    // Update is called once per frame
    IEnumerator enemyUpdate()
    {
        while (true)
        {
            m_animator.SetFloat("moveVelocity", m_agent.speed);                                       // 현재 이동속도를 애니메이터에 전달
            m_eEnemyState = (ENEMY_STATE)m_animator.GetInteger("stateLevel");                         // 상태 레벨을 받아옴

            if (m_eEnemyState == ENEMY_STATE.STUN)
            {
                m_agent.destination = transform.position;                                             // 이동 중지
                m_agent.speed = 0;
            }
            if (m_eEnemyState == ENEMY_STATE.DIE)
            {
                m_agent.destination = transform.position;                                             // 이동 중지
                m_agent.speed = 0;
            }
            else                                                                                      // 사망 상태가 아니라면 업데이트를 진행한다.
            {
                m_aniState = m_animator.GetCurrentAnimatorStateInfo(0);                               // 현재 애니메이션 진행상태
                m_aniTransition = m_animator.GetAnimatorTransitionInfo(0);                            // 현재 애니메이션 전환상태
                m_fTargetDistance = Vector3.Distance(m_targetTransform.position, transform.position); // 타겟과의 거리차를 계산함
                m_fSpawnDistance = Vector3.Distance(m_spawnPosition, transform.position);             // 생성된 위치와의 거리차를 계산함

                timerStateTransition();                         // 타이머에 따른 상태 전이
                distanceStateTransition();                      // 거리에 따른 상태 전이 
                animationTransition();                          // 애니메이션에 따른 상태 전이

                if (m_animator.GetBool("stateTransition") == true)         // 상태 전이가 일어났으면 알림
                {
                    OperateInput();                                       // 상태에 따른 행동패턴 입력
                    m_animator.SetBool("stateTransition", false);         // 상태 전이 알림 해제
                }
                OperateUpdate();
            }
            yield return new WaitForSeconds(m_fCoroutineTime);
        }
    }

private void timerStateTransition()           // 타이머가 지난 후 전이 함수
    {
        if (m_fAttackHoldTime > 0.0f)                                    // 공격판정 유지시간 처리
            m_fAttackHoldTime -= m_fCoroutineTime;
        else                                                             // 공격판정 시간이 끝나면
            m_punchCollider.isTrigger = false;                           // 콜라이더가 충돌하지 않도록 공격판정이 있는 모든 트리거 비활성화

        if (m_eEnemyState == ENEMY_STATE.PATROL && m_fPatrolTimer > 0.0f)                // 순찰 시간이 존재하면
        {
            m_fPatrolTimer -= m_fCoroutineTime;     // 순찰 유지 시간을 감소시킴
            if (m_fPatrolTimer <= 0.0f)           // 순찰 시간이 끝났으면
            {
                m_eEnemyState = (ENEMY_STATE)Random.Range(0, 2);     // IDLE 상태 또는 Patorl 상태 중 하나로 전이
                m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
                m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
            }
        }
        if (m_eEnemyState == ENEMY_STATE.IDLE && m_fIdleTimer > 0.0f)                // idle 유지시간이 존재하면
        {
            m_fIdleTimer -= m_fCoroutineTime;     // idle 유지 시간을 감소시킴
            if (m_fIdleTimer <= 0.0f)           // idle 시간이 끝났으면
            {
                m_eEnemyState = (ENEMY_STATE)Random.Range(0, 2);     // IDLE 상태 또는 Patorl 상태 중 하나로 전이
                m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
                m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
            }
        }
        if (m_eEnemyState == ENEMY_STATE.GET_DAMAGE && m_fFollowTargetTimer > 0.0f)   // 타겟 추적 유지시간이 존재하면
        {
            m_fFollowTargetTimer -= m_fCoroutineTime;     // 타겟 추적 유지 시간을 감소시킴
            if (m_fFollowTargetTimer <= 0.0f)           // 타겟 추적 시간이 끝났으면
            {
                m_eEnemyState = ENEMY_STATE.IDLE;       // IDLE 상태로 전이
                m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
                m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
            }
        }
    }

    virtual protected void distanceStateTransition()                 // 거리에 따른 상태 전이 함수 
    {
        if (m_fSpawnDistance > 100)                        // 처음 생성된 위치로부터 100이상 벗어나면
        {
            if (m_eEnemyState < ENEMY_STATE.RETURN)        // 상태 레벨이 위치 복귀보다 낮으면
            {
                m_eEnemyState = ENEMY_STATE.RETURN;        // 위치복귀 상태로 전이
                m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
                m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
            }
        }
        if (m_eEnemyState == ENEMY_STATE.RETURN)           // 되돌아가는 상태면 
        {
            if (m_fSpawnDistance < 5)                      // 원위치와의 거리차가 5 이하이면
            {
                m_eEnemyState = ENEMY_STATE.IDLE;          // IDLE 상태로 전이
                m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
                m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
            }
        }
        if (m_fTargetDistance >= 50)                       // 타겟과의 거리차가 50이상의 범위
        {
            if (m_eEnemyState == ENEMY_STATE.DETECTION)     // 감지 상태 였으면
            {
                m_eEnemyState = ENEMY_STATE.IDLE;              // IDLE 상태로 전이
                m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
                m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
            }
        }
        else if (m_fTargetDistance >= attackDistance)      // 타겟과의 거리차가 공격범위 이상 감지범위 미만
        {
            if (m_eEnemyState < ENEMY_STATE.DETECTION)     // 상태 레벨이 위치 감지보다 낮으면
            {
                m_eEnemyState = ENEMY_STATE.DETECTION;     // 감지 상태로 전이
                m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
                m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
            }
        }
        else if (m_fTargetDistance < attackDistance)        // 타겟과의 거리차가 공격범위 미만
        {
            if (m_eEnemyState < ENEMY_STATE.BASE_ATTACK)   // 상태 레벨이 공격보다 낮으면
            {
                m_eEnemyState = ENEMY_STATE.BASE_ATTACK;   // 공격 상태로 전이
                m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
                m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
            }
        }
    }

    public virtual void animationTransition()                       // 애니메이션의 전이에 따른 상태 전이
    {
        if (m_aniTransition.IsName("BaseAttack1 -> Idle") == true || 
            m_aniTransition.IsName("BaseAttack2 -> Idle") == true ||
            m_aniTransition.IsName("BaseAttack3 -> Idle") == true ||
            m_aniTransition.IsName("Stun -> Idle") == true)
        { 
            m_eEnemyState = ENEMY_STATE.IDLE;          // IDLE 상태로 전이
            m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
            m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
        }
    }

    void OperateInput()
    {
        switch (m_eEnemyState)
        {
        case ENEMY_STATE.IDLE:       // 대기
            m_agent.destination = transform.position;  // 이동 정지
            m_agent.speed = 0;
            m_fIdleTimer = idleHoldTime;
            break;
        case ENEMY_STATE.PATROL:     // 순찰
            float randomX = Random.Range(0f, 30f);                 // x랜덤생성 0~100
            float randomZ = Random.Range(0f, 30f);                 // z랜덤생성 0~100
            m_patrolPosition = m_spawnPosition + new Vector3(randomX, 0, randomZ);      // 정찰 위치 지정 (생성 위치를 기준으로 100,0,100거리)
            m_agent.SetDestination(m_patrolPosition);           // 공격 목표 타겟으로 
            m_agent.speed = 3;                                  // 3의 속도로 이동
            m_fPatrolTimer = patrolHoldTime;
            break;
        case ENEMY_STATE.DETECTION:   // 감지
            m_agent.SetDestination(m_targetTransform.position);  // 공격 목표 타겟으로 
            m_agent.speed = 10;                                  // 10의 속도로 이동
            break;
        case ENEMY_STATE.RETURN:      // 위치 복귀
            m_agent.SetDestination(m_spawnPosition);        // 공격 목표 타겟으로 
            m_agent.speed = 50;                             // 50의 속도로 복귀
            break;
        case ENEMY_STATE.GET_DAMAGE:  // 피격 
            m_agent.SetDestination(m_targetTransform.position);  // 공격 목표 타겟으로 
            m_agent.speed = 10;                                  // 10의 속도로 이동
            m_fFollowTargetTimer = followHoldTime;
            break;
        case ENEMY_STATE.BASE_ATTACK: // 기본공격
            transform.LookAt(m_targetTransform);        // 타겟을 정확히 바라보고
            m_agent.SetDestination(transform.position); // 타겟을 자기자신으로 하여 
            m_agent.speed = 0;                                // 이동을 멈추고
            m_animator.SetTrigger("baseAttackTrigger");                  // attack 트리거 발동
            m_animator.SetInteger("baseAttackType", Random.Range(1, 4)); //1~3의 공격중 하나 발동
            break;
        case ENEMY_STATE.SKILL: // 스킬
            m_agent.SetDestination(transform.position); // 타겟을 자기자신으로 하여 
            m_agent.speed = 0;                          // 이동을 멈춤
            break;
        }
    }

    void OperateUpdate()
    {
        if (m_eEnemyState == ENEMY_STATE.DETECTION)
        {
            m_agent.SetDestination(m_targetTransform.position);  // 공격 목표 타겟으로 
            m_agent.speed = 10;                                  // 10의 속도로 이동
        }
        if (m_eEnemyState == ENEMY_STATE.GET_DAMAGE)
        {
            m_agent.SetDestination(m_targetTransform.position);  // 공격 목표 타겟으로 
            m_agent.speed = 10;                                  // 10의 속도로 이동
        }
    }

    public virtual void hit()
    {
        if (m_aniState.IsName("BaseAttack1") == true || m_aniState.IsName("BaseAttack2") || m_aniState.IsName("BaseAttack3") == true)        // 공격 이면
        {
            m_punchCollider.isTrigger = true;           // 공격판정 콜라이더 트리거 활성화
            m_fCurAttackDamage = 2.0f;                  // 현재 모션에서의 공격 데미지 2
            m_fAttackHoldTime = 1.0f;                   // 공격판정 유지시간 1
        }
    }

    public virtual void attacted(float damage, CROWD_CONTROL cc = CROWD_CONTROL.NONE)
    {
        int stateLevel = m_animator.GetInteger("stateLevel");                 // 상태 레벨을 받아옴
        if (stateLevel == (int)ENEMY_STATE.DIE)                                // 사망 상태이면 리턴
            return;
        m_fCurHP -= damage;                                                   // 체력 감소    
        DamageTextVisible((int)damage, cc);
        InGameUIManager.Instance.showEnemyInfo("미노타우루스", percentHP);    // 체력 상태를 UI에 전달
        GameManager.instance.addScore((int)damage);
        ScoreUIManager.Instance.replaceScroeUI();         // 스코어UI를 갱신

        if (m_fCurHP <= 0.0f && stateLevel != (int)ENEMY_STATE.DIE)           // 체력이 0보다 작고 사망상태가 아니면
        {
            CharacterInfoManager.instance.m_characterInfo.m_iCurExp += m_iGetExp;
            if (CharacterInfoManager.instance.levelUpCheck() == true)
                unityChanScripte.levelUp();
            ProfileUIManager.Instance.changeStatus();

            m_animator.SetTrigger("die");                // 사망 트리거 활성화
            m_eEnemyState = ENEMY_STATE.DIE;             // 사망상태로 전환
            m_animator.Play("Die");
            Destroy(transform.gameObject, 3.0f);   // 3초뒤삭제
            dropItem();
            dropGold();
        }
        else if (stateLevel != (int)ENEMY_STATE.DIE)
        {
            if(cc == CROWD_CONTROL.STUN)
            {
                m_animator.SetTrigger("stun");
                m_eEnemyState = ENEMY_STATE.STUN;
            }
            else
            {
                if (stateLevel != (int)ENEMY_STATE.STUN)
                {
                    m_animator.SetTrigger("attated");
                    m_eEnemyState = ENEMY_STATE.GET_DAMAGE;
                }
            }
        }
        m_animator.SetInteger("stateLevel", (int)m_eEnemyState); //상태 전이
        m_animator.SetBool("stateTransition", true);
    }

    protected virtual void dropItem()
    {
        if (SceneManager.GetActiveScene().name == "Stage1-BossScene")
            return;
        GameObject dropItem = ObjectPoolManager.Instance.PopFromPool("DropItem");
        dropItem.transform.position = transform.position + Vector3.right * 5;        // 위치동기화
        dropItem.transform.rotation = transform.rotation;                            // 방향동기화
        dropItem.SetActive(true);
    }

    protected virtual void dropGold()
    {
        if (SceneManager.GetActiveScene().name == "Stage1-BossScene")
            return;
        GameObject dropGold = ObjectPoolManager.Instance.PopFromPool("DropGold");
        dropGold.transform.position = transform.position + Vector3.left * 5;   // 위치동기화
        dropGold.transform.rotation = transform.rotation;   // 방향동기화
        dropGold.SetActive(true);
    }

    private void OnTriggerEnter(Collider coll)                // 공격 충돌 처리
    {
        if (coll.gameObject.tag == "player")                   // 충돌 대상이 적 태그를 가지고 있으면
        {
            unityChanScripte.attated(m_fCurAttackDamage,CROWD_CONTROL.NONE);   // 해당 스크립트를 받아와서 나의 현재 공격모션에 따른 데미지를 부여함
        }
    }

    void OnEnable()     // 스크립트 활성화 이벤트 
    {
        GameManager.instance.m_iEnemyCount++;                       // 적의 카운트를 늘리고
        UnityChanInfomation.s_eventPlayerState += this.playerState; // 플레이어와 델리게이트 연결
    }

    void OnDisable()    // 스크립트 비활성화 이벤트 (적은 게임 도중에 사망이 아니면 스크립트가 비활성화될 일이 없음)
    {
        GameManager.instance.m_iEnemyCount--;                       // 적의 카운트를 1 줄임
        if (GameManager.instance.m_iEnemyCount == 0)                // 적의 카운트가 없으면
            GameManager.instance.clearDungeon();                    // 해당 던전은 적이 없다고 게임매니저에게 알림
        UnityChanInfomation.s_eventPlayerState -= this.playerState; // 플레이어와 델리게이트 연결해제
    }

    void playerState(int state)
    {
        if (state == 444)    // 사망
            m_targetTransform = m_sunTransform;
        if (state == 1004)    // 부활, 감지 가능
        {
            m_targetTransform = m_playerTransform;
        }
        if(state > 10 && state < 50)                // state값이 10보다 크고 50보다 작으면 범위로 사용할 예정
        {
            if (m_fTargetDistance < state)          // 타겟과의 거리가 넘어온 인자보다 낮으면
                attacted(CharacterInfoManager.instance.m_iCurStr * 0.8f, CROWD_CONTROL.STUN);   // 0.8배율의 데미지를 주고 스턴상태로 만듬
        }
    }

    protected void DamageTextVisible(int damageAmount, CROWD_CONTROL cc=CROWD_CONTROL.NONE)
    {
        GameObject m_damageText = ObjectPoolManager.Instance.PopFromPool("DamageText");
        DamageText damageTextScripte = m_damageText.GetComponent<DamageText>();
        m_damageText.SetActive(true);

        if (cc == CROWD_CONTROL.STUN)
            damageTextScripte.damageAmount = damageAmount.ToString() + "\n[ff0000]스턴[-]";
        else if (cc == CROWD_CONTROL.STUN_IMMUNE)
            damageTextScripte.damageAmount = damageAmount.ToString() + "\n[ff0000]스턴 면역[-]";
        else if (cc == CROWD_CONTROL.BACK_ATTACK)
            damageTextScripte.damageAmount = damageAmount.ToString() + "\n[ff0000]백어택[-]";
        else
            damageTextScripte.damageAmount = damageAmount.ToString();
        damageTextScripte.targetTransform = transform.position;
    }
}
