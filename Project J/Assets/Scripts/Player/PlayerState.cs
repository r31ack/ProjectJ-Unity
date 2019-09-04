using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerState : Player    // 캐릭터 정보를 담고 있는 스크립트
{
    // 스킬 이펙트 중 광원을 컨트롤하기 위함
    GameObject m_mainRight;
    GameObject m_playerRight;

    // 착용중인 무기 콜라이더
    private Collider m_weaponCollider;    
    
    // 카메라 스크립트
    private FollowCam m_cameraScript;

    private DIRECTION m_eDirection = DIRECTION.NONE;
    public delegate void playerState(int state);                  // 플레이어 사망 델리게이트
    public static event playerState s_eventPlayerState;

    // 이펙트 관련 함수
    protected ParticleSystem m_levelUpEffect;             // 레벨업 이펙트

    GameObject m_leafAttackRange;

    private void Awake()
    {
        m_mainRight = GameObject.Find("Directional Light");
        m_playerRight = transform.Find("EffectLight").gameObject;
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

    protected virtual void timerState()            // 타이머가 걸린 동작을 처리하는 함수
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
        {
            m_weaponCollider.isTrigger = false;                     // 콜라이더가 충돌하지 않도록 공격판정이 있는 모든 트리거 비활성화
        }
    }

    protected virtual void animationTransition() // 애니메이션 전이시 정보 처리 관련 함수
    {
        if (m_aniTransition.IsName("BaseAttack1 -> Idle") == true ||  // 기본으로 상태 전이될 경우
            m_aniTransition.IsName("BaseAttack2 -> Idle") == true ||
            m_aniTransition.IsName("BaseAttack3 -> Idle") == true ||
            m_aniTransition.IsName("Roll -> Idle") == true ||
            m_aniTransition.IsName("Attated -> Idle") == true ||
            m_aniTransition.IsName("Roll -> Move") == true ||
            m_aniTransition.IsName("Stun -> Idle") == true ||
            m_aniTransition.IsName("MoveAttack -> Idle") == true)
        {
            initAnimatorInfo();
        }

        if (m_aniTransition.IsName("AnyState -> Attated") == true)
            m_animator.SetInteger("stateLevel", 18);
        if (m_aniTransition.IsName("AnyState -> Stun") == true)
            m_animator.SetInteger("stateLevel", 20);

        if (m_aniState.IsName("MoveAttack") == true && m_aniState.normalizedTime > 0.2f && m_aniState.normalizedTime < 0.6f)
        {
            switch ((DIRECTION)m_animator.GetInteger("direction"))
            {
                case DIRECTION.LEFT:
                    transform.Translate(transform.right * -20 * Time.deltaTime, Space.World);
                    transform.Rotate(Vector3.up, +240 * Time.deltaTime, Space.World);
                    break;
                case DIRECTION.RIGHT:
                    transform.Translate(transform.right * 20 * Time.deltaTime, Space.World);
                    transform.Rotate(Vector3.up, -240 * Time.deltaTime, Space.World);
                    break;
                case DIRECTION.BOTTOM:
                    transform.Translate(transform.right * 20 * Time.deltaTime, Space.World);
                    transform.Rotate(Vector3.up, -420 * Time.deltaTime, Space.World);
                    break;
            }
        }
        getCrowdContorl();
    }


    protected virtual void initAnimatorInfo() // 애니메이터 정보를 초기화 (Idle로 전이 시 초기화 할 목적)
    {
        m_animator.SetInteger("baseComboCount", 0); // 기본공격 콤보 횟수를 0으로 만듬
        m_animator.SetInteger("stateLevel", 0);     // 상태 레벨을 0으로 만듬
        m_animator.SetFloat("moveVelocity", 0.0f);  // 이동 속도 초기화
        m_animator.SetInteger("direction", 0);
    }

    protected virtual void animationState() // 애니메이션 상태 도중 정보 처리 관련 함수
    {
        roll();       // 구르는 상태 처리
    }

    protected virtual void roll() // 구르는 상태 처리
    {
        if(m_aniTransition.IsName("AnyState -> Roll") == true)
        {

        }
        if (m_aniState.IsName("Roll") == true && m_aniState.normalizedTime > 0.1f && m_aniState.normalizedTime < 0.5f)
        {
            m_eDirection = (DIRECTION)(int)m_animator.GetFloat("roll");
            m_animator.SetFloat("moveVelocity", 0.0f);
            if (m_eDirection == DIRECTION.LEFT)
                transform.Translate(transform.right * -20 * Time.deltaTime, Space.World);
            else if (m_eDirection == DIRECTION.TOP)
                transform.Translate(transform.forward * 20 * Time.deltaTime, Space.World);
            else if (m_eDirection == DIRECTION.RIGHT)
                transform.Translate(transform.right * 20 * Time.deltaTime, Space.World);
            else if (m_eDirection == DIRECTION.BOTTOM)
                transform.Translate(transform.forward * -20 * Time.deltaTime, Space.World);
        }
        else if (m_aniState.IsName("Roll") == true && m_aniState.normalizedTime > 0.9f)
        {
            m_animator.SetInteger("stateLevel", 0);     // 상태 레벨을 0으로 만듬
            m_animator.SetFloat("roll", 0);             // 
        }
    }

    protected virtual void getCrowdContorl()      // 상태이상 처리
    {
        if(m_aniTransition.IsName("Revive -> Idle") == true || m_aniTransition.IsName("Stun -> Idle") == true)
            initAnimatorInfo();
    }

    ////////////////////////////////////////////////// 이벤트 처리 함수 ////////////////////////////////////////////////////////////

    protected virtual void hit(int type)
    {
        m_weaponCollider.isTrigger = true;               // 무기 콜라이더 트리거 활성화
    }

    public void attated(float damage, CROWD_CONTROL cc) // 피격 처리
    {
        float stateLevel = m_animator.GetInteger("stateLevel");
        CharacterInfoManager.instance.m_iCurHp -= (int)damage;  // 받은 데미지만큼 현재 체력을 감소시킨다.                  

        if (cc == CROWD_CONTROL.STUN)
        {
            if(stateLevel != 20)
                 m_animator.SetInteger("stateLevel", 19);    // 스턴 상태로 레벨 전환
        }
        else if (cc == CROWD_CONTROL.DOWN)
        {
            if (stateLevel != 18)
                m_animator.SetInteger("stateLevel", 17);    // 피격 상태로 레벨 전환
        }

        if (CharacterInfoManager.instance.m_iCurHp <= 0.0f)   // 체력이 없으면
        {
            GameManager.instance.playerDie();           // 플레이어가 사망했다고 게임매니저에 알림 (게임매니저에서 추가 UI 처리)
            m_animator.SetTrigger("dieTrigger");        // 사망 트리거 활성화
            m_animator.SetInteger("stateLevel", 44);    // 사망 상태로 레벨 전환
            delegateState(444);                         // 사망 델리게이트 전달
        }
    }

    private void OnTriggerEnter(Collider coll)                // 공격 충돌 처리
    {
        if (coll.gameObject.tag == "enemy")                   // 충돌 대상이 적 태그를 가지고 있으면
        {
            CROWD_CONTROL cc = CROWD_CONTROL.NONE;            // 특수 공격상태
            SPECIAL_DAMAGE sd = SPECIAL_DAMAGE.NONE;          // 특수 추가 데미지
            int criticalPercent = 20;                         // 기본 치명타 확률

            int str = CharacterInfoManager.instance.m_iCurStr;
            EnemyInfomation enemyScript = coll.GetComponentInParent<EnemyInfomation>();   // 적 스크립트를 받아와서

            if (Vector3.Dot(coll.transform.forward, (transform.position - coll.transform.position).normalized) < Mathf.Cos(90 * Mathf.Deg2Rad))   // 내적을 구해서 적의 정면에서 90도내에 플레이어가 없으면 (좌우 합쳐 180도)
            {
                cc = CROWD_CONTROL.BACK_ATTACK;                                                                                                   // 백어택 발동
                criticalPercent *= 2;                                                                                                             // 크리티컬 확률 2배
            }

            if (Random.Range(0, 100) < criticalPercent)                                                                                           // 크리티컬 확률 안에 들어오면
            {
                if (cc == CROWD_CONTROL.BACK_ATTACK)
                    sd = SPECIAL_DAMAGE.CRITICAL_BACK_ATTACK;
                else
                    sd = SPECIAL_DAMAGE.CRITICAL;
            }
                enemyScript.attacted(m_fCurAttackDamage * str, sd, cc);         // 그 적은 내 현재 공격모션의 데미지를 부여함
        }
        if (coll.gameObject.tag == "Potal")                     // 충돌 대상이 스테이지를 넘어가는 포탈이면
        {
            if (s_eventPlayerState == null)
                GameManager.instance.nextDungeonLoad();
            else
                GameManager.instance.stateExplain(1);
        }
        if (coll.gameObject.tag == "DropItem")                   // 충돌 대상이 아이템이면
        {
            DropItem dropItemScripte = coll.GetComponent<DropItem>();
            ItemManager.Instance.putInventroyItem(dropItemScripte.getItenName()); // 인벤토리에 아이템을 넣음
            ObjectPoolManager.Instance.PushToPool("DropItem", coll.gameObject);
        }
        if (coll.gameObject.tag == "DropGold")
        {
            DropGold dropGoldScripte = coll.GetComponent<DropGold>();
            CharacterInfoManager.instance.m_playerInfo.m_iGold += coll.GetComponent<DropGold>().getGoldAmount();
            ProfileUIManager.Instance.changeGold();
            ObjectPoolManager.Instance.PushToPool("DropGold", coll.gameObject);
        }
        if(coll.gameObject.tag == "Barrigate")
        {
           GameManager.instance.stateExplain(0);
        }
    }

    public void levelUp()
    {
        m_levelUpEffect.Play();
        GameManager.instance.levelUp();
    }

    public void revive()
    {
        m_levelUpEffect.Play();
        m_animator.SetTrigger("revive");
        CharacterInfoManager.instance.m_iCurHp = CharacterInfoManager.instance.m_playerInfo.m_iMaxHp;        // 체력 최대치
        delegateState(1004);
    }

    public void delegateState(int state)
    {
        if (s_eventPlayerState != null)             // 델리게이트 체크
            s_eventPlayerState(state);              // 플레이어가 부활했다고 적에게 알림
    }
}


