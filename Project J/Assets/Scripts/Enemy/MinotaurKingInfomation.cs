using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class MinotaurKingInfomation : EnemyInfomation
{
    private float m_fShoutCoolTime = 8.0f;
    private float m_fEarthquakeCoolTime = 0.0f;    // 초기 쿨타임
    private ParticleSystem m_fShoutRange;
    private Transform m_earthquakeRange;            // 지진 일으키기 범위
    private ParticleSystem m_earthquakeEffect;      // 지진 일으키기 이펙트
    private bool m_bPhaseChange = false;

    private void Awake()
    {
        m_fMaxHP = 3000;
        m_fCurHP = m_fMaxHP;
        //m_targetTransform = GameObject.Find("Player").transform;
        m_fShoutRange = transform.Find("ShoutRange").GetComponent<ParticleSystem>();
        m_earthquakeEffect = transform.Find("EarthquakeEffect").GetComponent<ParticleSystem>();
        m_earthquakeRange = transform.Find("EarthquakeRange");
        m_earthquakeRange.GetComponent<MeshRenderer>().material.color = Color.red;
        m_earthquakeRange.transform.localScale = new Vector3(300, 0, 1);        // 초기 공격범위는 (300,0,1)

        m_fShoutRange.Stop();
        m_earthquakeEffect.Stop();
        InvokeRepeating("shoutCoolTime", 0.0f, 0.1f);  // 초기 쿨타임 발동
        InvokeRepeating("earthquakeCoolTime", 0.0f, 0.1f);  // 초기 쿨타임 발동
    }

    protected override void distanceStateTransition()                 // 거리에 따른 상태 전이 함수 
    {
        if (Input.GetKeyDown(KeyCode.O) == true)
            m_fCurHP = 1.0f;


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
            if (m_eEnemyState == ENEMY_STATE.DETECTION)     // 감지상태 특수스킬
            {
                if (IsInvoking("earthquakeCoolTime") == false)        // 쿨타임이 없으면 내려치기발동
                {
                    m_fEarthquakeCoolTime = 10.0f;
                    m_animator.SetTrigger("earthquake");
                    InvokeRepeating("earthquakeCoolTime", 0.0f, 0.1f);
                    m_eEnemyState = ENEMY_STATE.SKILL;
                    m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
                    m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
                }
            }
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

    public override void animationTransition()      // 애니메이션 전이 추가 상속내용
    {
        if (m_aniTransition.IsName("Idle -> Earthquake") == true)
        {
            m_earthquakeRange.gameObject.SetActive(true);
            m_earthquakeRange.transform.localScale = new Vector3(300, 0, 1);        // 초기 공격범위는 (300,0,1)
            m_earthquakeRange.transform.position = transform.position + Vector3.up;
        }
        if (m_aniState.IsName("Earthquake") == true && m_aniState.normalizedTime < 0.3f)    // 지진을 일으키기 위해 충전하는 시간동안
        {
            m_earthquakeRange.transform.localScale += Vector3.forward * 80 * m_animator.speed;                      // 앞쪽으로 범위 증가
            m_earthquakeRange.transform.Translate(Vector3.forward*4, Space.Self);
        }
        if (m_aniTransition.IsName("Earthquake -> Idle") == true)
        {
            m_earthquakeEffect.Stop();
            m_eEnemyState = ENEMY_STATE.IDLE;          // IDLE 상태로 전이
            m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
            m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
            m_earthquakeRange.gameObject.SetActive(false);
        }
        if (m_aniState.IsName("Shout") == true && m_aniState.normalizedTime < 0.3f)
        {
           if(m_fShoutRange.isPlaying == false)
                 m_fShoutRange.Play();
        }
        if (m_aniState.IsName("Shout") == true && m_aniState.normalizedTime > 0.3f && m_aniState.normalizedTime < 0.6f)
        {
            if (Vector3.Distance(m_targetTransform.position, transform.position) < 20)   // 콜라이더를 쓰지 않고 반경 20거리 전범위 스턴 공격
            {
                m_targetTransform.GetComponent<UnityChanInfomation>().attated(1, CROWD_CONTROL.STUN);
            }
        }
        if (m_aniState.IsName("Shout") == true || m_aniState.IsName("BaseAttack1")==true ||
            m_aniState.IsName("BaseAttack2") == true || m_aniState.IsName("BaseAttack3")==true || m_aniState.IsName("Earthquake") == true)
        {
            m_eEnemyState = ENEMY_STATE.SKILL;
            m_agent.SetDestination(transform.position); // 타겟을 자기자신으로 하여 
            m_agent.speed = 0;                          // 이동을 멈춤
        }
        if (m_aniTransition.IsName("Shout -> Idle") == true)
        {
            m_animator.SetBool("shout",false);
            m_fShoutRange.Stop();
            m_eEnemyState = ENEMY_STATE.IDLE;          // IDLE 상태로 전이
            m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
            m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
        }

        if (m_aniTransition.IsName("BaseAttack1 -> Idle") == true ||
            m_aniTransition.IsName("BaseAttack2 -> Idle") == true ||
            m_aniTransition.IsName("BaseAttack3 -> Idle") == true ||
            m_aniTransition.IsName("Stun -> Idle") == true)
        {
            m_eEnemyState = ENEMY_STATE.IDLE;          // IDLE 상태로 전이
            m_animator.SetInteger("stateLevel", (int)m_eEnemyState); // 현재 상태 레벨을 애니메이터에 저장
            m_animator.SetBool("stateTransition", true);         // 상태 전이가 일어났다고 알림
            m_earthquakeRange.gameObject.SetActive(false);
        }
    }

    public override void attacted(float damage, CROWD_CONTROL cc = CROWD_CONTROL.NONE)
    {
        int stateLevel = m_animator.GetInteger("stateLevel");                 // 상태 레벨을 받아옴
        if (stateLevel == (int)ENEMY_STATE.DIE)                                // 사망 상태이면 리턴
            return;

        m_fCurHP -= damage;                                                   // 체력 감소    
        if (cc == CROWD_CONTROL.STUN)
            cc = CROWD_CONTROL.STUN_IMMUNE;
        DamageTextVisible((int)damage, cc);
        InGameUIManager.Instance.showEnemyInfo("미노 킹", percentHP);    // 체력 상태를 UI에 전달
        GameManager.instance.addScore((int)damage);
        ScoreUIManager.Instance.replaceScroeUI();         // 스코어UI를 갱신

        if (m_fCurHP <= 0.0f && stateLevel != (int)ENEMY_STATE.DIE)           // 체력이 0보다 작고 사망상태가 아니면
        {
            m_animator.SetTrigger("die");                // 사망 트리거 활성화
            m_eEnemyState = ENEMY_STATE.DIE;             // 사망상태로 전환
            m_animator.Play("Die");
            Destroy(transform.gameObject, 3.0f);   // 3초뒤삭제
            dropItem();
            dropGold();
            GameManager.instance.winDungeon();
        }
        else if (stateLevel != (int)ENEMY_STATE.DIE)
        {
            if(m_fCurHP <= m_fMaxHP*0.5f && m_bPhaseChange == false)   // 체력이 50퍼 이하이면 다음페이즈
            {
                GameManager.instance.startSubDialog();
                m_animator.speed = 1.5f;
                m_bPhaseChange = true;
            }
            if (IsInvoking("shoutCoolTime") == false && m_bPhaseChange == true)        // 2페이즈부터는 소리치기발동
            {
                m_fShoutCoolTime = 24.0f;
                m_animator.SetBool("shout",true);
                InvokeRepeating("shoutCoolTime", 0.0f, 0.1f);
                m_eEnemyState = ENEMY_STATE.SKILL;
            }
        }
        m_animator.SetInteger("stateLevel", (int)m_eEnemyState); //상태 전이
        m_animator.SetBool("stateTransition", true);
    }

    void shoutCoolTime()
    {
        if (m_fShoutCoolTime <= 0.0f)
            CancelInvoke("shoutCoolTime");
        else
            m_fShoutCoolTime -= 0.1f;
    }
    void earthquakeCoolTime()
    {
        if (m_fEarthquakeCoolTime <= 0.0f)
            CancelInvoke("earthquakeCoolTime");
        else
            m_fEarthquakeCoolTime -= 0.1f;
    }

    override public void hit()
    {
        if (m_aniState.IsName("BaseAttack1") == true || m_aniState.IsName("BaseAttack2") || m_aniState.IsName("BaseAttack3") == true)        // 공격 이면
        {
            m_punchCollider.isTrigger = true;           // 공격판정 콜라이더 트리거 활성화
            m_fCurAttackDamage = 10.0f;                  // 현재 모션에서의 공격 데미지 
            m_fAttackHoldTime = 0.5f;                   // 공격판정 유지시간 0.5초
        }
        if (m_aniState.IsName("Shout") == true)    // 소리치기 이면
        {
           Stage1Manager.Instance.createEnemySkill();
        }
        if (m_aniState.IsName("Earthquake") == true)    // 땅치기 이면
        {
            m_earthquakeEffect.Play();
            CapsuleCastAll();
        }
    }

    void CapsuleCastAll()           // 캡슐 캐스트를 전방50m발사
    {
        Vector3 topVector = transform.position + Vector3.up*2;
        Vector2 bottomVector = transform.position;
        Vector3 direction = transform.forward;
        float radius = 3f;
        RaycastHit[] hits = Physics.SphereCastAll(topVector, radius, direction,60.0f);

        Debug.DrawRay(topVector, direction*10, Color.red, 10.0f,true);

        foreach (var hit in hits)
        {
            Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.tag == "player")
            {
                m_targetTransform.GetComponent<UnityChanInfomation>().attated(30,CROWD_CONTROL.DOWN);
            }
        }
    }
}