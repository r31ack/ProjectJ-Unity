using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnityChanInfomation : Player    // 캐릭터 정보를 담고 있는 스크립트
{
    // 스킬 이펙트 중 광원을 컨트롤하기 위함
    GameObject m_mainRight;
    GameObject m_playerRight;

    // 착용중인 무기 콜라이더
    private Collider m_weaponCollider;    
    
    // 카메라 스크립트
    private FollowCam m_cameraScript;         

    // 스킬 유지시간 관련
    public float m_fHlodTimer = 0.0f;
    public float m_fLeafAttackTimer = 0.0f;
    public float m_fSwordWindTimer = 0.0f;
    private float m_fCameraRotate = 0.0f;      // 회전시간
    private float m_fAvoidInertiaTime = 0.0f;  // 긴급회피 관성시간

    public bool m_fSwordWindChargeFlag = false;
    private DIRECTION m_eDirection = DIRECTION.NONE;

    private ItemManager m_itemManagerScript;           // 아이템 매니저 스크립트
    private ProfileUIManager m_profileUIManagerScript; // 프로필 UI 매니저 스크립트

    public delegate void playerState(int state);                  // 플레이어 사망 델리게이트
    public static event playerState s_eventPlayerState;

    // 이펙트 관련 함수
    private ParticleSystem m_swordWindChargeEffect;
    private ParticleSystem m_leafAttackRangeEffect;
    private ParticleSystem m_leafAttackEffect;
    private ParticleSystem m_rushEffect;                // 돌진 이펙트
    private ParticleSystem m_flashEffect;               // 
    private ParticleSystem m_levelUpEffect;             // 레벨업 이펙트
    private MeleeWeaponTrail m_baseSlashTrail;

    private float m_fTrailEffectTime = 0.0f;
    GameObject m_leafAttackRange;

    private void Awake()
    {
        m_mainRight = GameObject.Find("Directional Light");
        m_playerRight = transform.Find("EffectLight").gameObject;

        m_itemManagerScript = GameObject.Find("ItemWindow").GetComponent<ItemManager>();
        m_profileUIManagerScript = GameObject.Find("ProfileUI").GetComponent<ProfileUIManager>();

        // 캐릭터 종속 이펙트
        m_swordWindChargeEffect = transform.Find("SwordWindChargeEffect").GetComponent<ParticleSystem>();   // 칼바람 충전 파티클
        m_levelUpEffect = transform.Find("LevelUpEffect").GetComponent<ParticleSystem>();   // 칼바람 충전 파티클
        m_rushEffect = transform.Find("RushEffect").GetComponent<ParticleSystem>();
        m_flashEffect = transform.Find("FlashEffect").GetComponent<ParticleSystem>();

        // 기타 이펙트
        m_leafAttackRange = GameObject.Find("AttackRange").gameObject;
        m_leafAttackRangeEffect = GameObject.Find("LeapAttackRangeEffect").GetComponent<ParticleSystem>();
        m_leafAttackEffect = GameObject.Find("SkillEffectPool").transform.Find("LeafAttackEffect").GetComponent<ParticleSystem>();

    }

    void Start()
    {
        m_animator = GetComponent<Animator>();                                                      // 컴포넌트에서 애니메이터를 동기화
        m_weaponCollider = GameObject.Find("Dragonblade").GetComponent<Collider>(); // 대검 오브젝트의 이름을 찾은 후 콜라이더 동기화
        m_cameraScript = GameObject.Find("Main Camera").GetComponent<FollowCam>();
        m_baseSlashTrail = m_weaponCollider.transform.Find("TrailEffect").GetComponent<MeleeWeaponTrail>();
        m_baseSlashTrail._lifeTime = 0.0f;

        // 모든 이펙트는 초기에 꺼둔다.
        m_rushEffect.Stop();
        m_leafAttackRangeEffect.Stop();
        m_swordWindChargeEffect.Stop();
        m_levelUpEffect.Stop();
        m_flashEffect.Stop();
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
        {
            m_weaponCollider.isTrigger = false;                     // 콜라이더가 충돌하지 않도록 공격판정이 있는 모든 트리거 비활성화
        }

        if (m_fTrailEffectTime > 0.0f)
        {
            m_baseSlashTrail._lifeTime = 0.5f;
            m_fTrailEffectTime -= Time.deltaTime;
            if (m_fTrailEffectTime <= 0.0f)
                m_baseSlashTrail._lifeTime = 0.0f;
        }
        if (m_fAvoidInertiaTime > 0.0f)
        {
            transform.Translate(-transform.forward * 15 * Time.deltaTime, Space.World);     // 뒷쪽으로 이동
            m_fAvoidInertiaTime -= Time.deltaTime;
        }


        if (m_fCameraRotate > 0.0f)                                             // 회전 시간
        {
            m_fCameraRotate -= Time.deltaTime;
            m_cameraScript.moveDamping = 10.0f;
            transform.Rotate(Vector3.up, 400 * Time.deltaTime, Space.World);    // 회전
            if(m_fCameraRotate <= 0.0f)
               m_cameraScript.moveDamping = 15.0f;
        }


        if (m_aniState.IsName("HoldAttack1") == true || m_aniState.IsName("HoldAttack2") == true || m_aniState.IsName("Hide") == true)
        {
            m_fHlodTimer = m_animator.GetFloat("holdTimer");   // 콤보 유지 시간 처리
            if (m_fHlodTimer > 0.0f)
            {
                m_fHlodTimer -= Time.deltaTime;
                m_animator.SetFloat("holdTimer", m_fHlodTimer);
            }
        }
    }
    void animationTransition() // 애니메이션 전이시 정보 처리 관련 함수
    {
        if (m_aniTransition.IsName("BaseAttack1 -> Idle") == true ||  // 기본으로 상태 전이될 경우
            m_aniTransition.IsName("BaseAttack2 -> Idle") == true ||
            m_aniTransition.IsName("BaseAttack3 -> Idle") == true ||
            m_aniTransition.IsName("Jump -> Idle") == true ||
            m_aniTransition.IsName("Roll -> Idle") == true ||
            m_aniTransition.IsName("Attated -> Idle") == true ||
            m_aniTransition.IsName("Roll -> Move") == true ||
            m_aniTransition.IsName("Stun -> Idle") == true ||
            m_aniTransition.IsName("HoldAttackEnd -> Idle") == true ||
            m_aniTransition.IsName("ChargeAttack1 -> Idle") == true ||
            m_aniTransition.IsName("ChargeAttack2 -> Idle") == true ||
            m_aniTransition.IsName("ChargeAttack3 -> Idle") == true ||
            m_aniTransition.IsName("Recovery -> Idle") == true ||
            m_aniTransition.IsName("MoveAttack -> Idle") == true ||
            m_aniTransition.IsName("Cut -> Idle") == true ||
            m_aniTransition.IsName("LeafAttack -> Idle") == true)
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

    void rush()
    {
        if (m_aniTransition.IsName("Roll -> Rush") == true || m_aniTransition.IsName("Move -> Rush") == true || 
            m_aniTransition.IsName("Idle -> Rush") == true)     // 돌진 전환 상태이면
        {
            m_cameraScript.rushKnockBack = true;                // 넉백 효과 true로 만듬
            m_cameraScript.setKnockBackTimer(0.5f);
            m_rushEffect.gameObject.SetActive(true);            // 이펙트 오브젝트 활성화
            if (m_rushEffect.isPlaying == false)                // 이펙트 체크 후 play
                m_rushEffect.Play();
            m_animator.SetFloat("moveVelocity", 0.0f);  // 이동 속도 초기화
            m_animator.SetBool("rush",false);
            CharacterInfoManager.instance.beginCoolTimeDown((int)SKILL_TYPE.RUSH);     // 쿨타임 시작
        }
        if (m_aniState.IsName("Rush") == true && m_aniState.normalizedTime > 0.2f && m_aniState.normalizedTime < 0.45f)
        {
            m_rushEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
   
          transform.Translate(transform.forward * 70 * Time.deltaTime, Space.World);
        }
        else if (m_aniState.IsName("Rush") == true && m_aniState.normalizedTime >= 0.6)
        {
            initAnimatorInfo();
        }
        if (m_aniTransition.IsName("Rush -> Idle") == true)
        {
            m_cameraScript.rushKnockBack = false;
            m_animator.SetBool("rush", false);
            initAnimatorInfo();
        }
        if (m_aniTransition.IsName("Move -> Recovery") == true || m_aniTransition.IsName("Roll -> Recovery") == true)
        {
            CharacterInfoManager.instance.beginCoolTimeDown((int)SKILL_TYPE.RUSH);     // 쿨타임 시작
            m_fAvoidInertiaTime = 0.6f;
            m_animator.SetBool("rush", false);
        }
    }

    void initAnimatorInfo() // 애니메이터 정보를 초기화 (Idle로 전이 시 초기화 할 목적)
    {
        m_animator.SetInteger("baseComboCount", 0); // 기본공격 콤보 횟수를 0으로 만듬
        m_animator.SetInteger("stateLevel", 0);     // 상태 레벨을 0으로 만듬
        m_animator.SetBool("jump", false);          // 점프 상태 초기화
        m_animator.SetFloat("moveVelocity", 0.0f);  // 이동 속도 초기화
        m_animator.SetBool("holdAttack", false);
        m_animator.SetInteger("direction", 0);
        m_animator.SetFloat("swordWindTimer", 0);
        m_animator.ResetTrigger("rush");
        m_animator.ResetTrigger("rollTrigger");
        m_animator.SetBool("flash", false);
        m_animator.SetBool("leafAttack", false);
        m_fLeafAttackTimer = 0.0f;
        m_fSwordWindTimer = 0.0f;
        m_fSwordWindChargeFlag = false;
        SkillUIManager.instance.setDefulatSkillType();      // idle로 전이시 스킬표시를 디폴트로 복귀시킴
    }

    void animationState() // 애니메이션 상태 도중 정보 처리 관련 함수
    {
        roll();       // 구르는 상태 처리
        flash(); // 대쉬공격 상태 처리 
        assasination();
        leafAttack();
        swordWind();
        rampage();
        rush();
    }

    void roll() // 구르는 상태 처리
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

    void flash() // 대쉬공격 상태 처리
    {
        if (m_aniTransition.IsName("BaseAttack2 -> DashAttack") == true || m_aniTransition.IsName("BaseAttack1 -> DashAttack") || 
            m_aniTransition.IsName("BaseAttack3 -> DashAttack") || m_aniTransition.IsName("MoveAttack -> DashAttack"))
        {
            m_fTrailEffectTime = 2.0f;
            m_cameraScript.moveDamping = 0.0f;
            SkillUIManager.instance.setSkillType(6, SKILL_TYPE.SWORD_WIND);
        }

        if ((m_aniState.IsName("DashAttack") == true && m_aniState.normalizedTime > 0.39f && m_aniState.normalizedTime < 0.55f))
        {
            if (m_flashEffect.isPlaying == false)
                m_flashEffect.Play();
            transform.Translate(transform.forward * 80 * Time.deltaTime, Space.World);
        }
        if (m_aniTransition.IsName("DashAttack -> Recovery") == true)
        {
            m_fAvoidInertiaTime = 0.4f;
            CharacterInfoManager.instance.beginCoolTimeDown((int)SKILL_TYPE.FLASH);     // 쿨타임 시작
        }
        if (m_aniTransition.IsName("Recovery -> Idle") == true)
        {
            initAnimatorInfo();
        }
    }

    public void assasination()
    {
        if (m_aniTransition.IsName("Idle -> Hide") == true || m_aniTransition.IsName("Move -> Hide") == true)
        {
            m_playerRight.SetActive(true);
            m_mainRight.SetActive(false);
            if (s_eventPlayerState != null)      // 델리게이트 체크
                s_eventPlayerState(444);
            m_leafAttackRangeEffect.Play();
        }
        else if (m_aniTransition.IsName("Hide -> Cut") == true)
        {
            m_animator.SetFloat("moveVelocity", 0.0f);
            CharacterInfoManager.instance.beginCoolTimeDown((int)SKILL_TYPE.ASSASSINATION);
            m_leafAttackRangeEffect.Stop();
            Invoke("hideEnd", 0.8f);
        }
        else if (m_aniTransition.IsName("Cut -> Idle") == true)
        {
            m_animator.SetFloat("moveVelocity", 0.0f);
            initAnimatorInfo();
        }
        if (m_aniState.IsName("Hide") == true)
        {
            m_fTrailEffectTime = 0.3f;
            m_leafAttackRange.transform.rotation = transform.rotation;
            m_leafAttackRange.transform.position = transform.position;
            m_leafAttackRange.transform.Translate(Vector3.forward * 9, Space.Self);
        }
        if (m_aniState.IsName("Cut") == true && m_aniState.normalizedTime > 0.7f && m_aniState.normalizedTime < 0.72f)
        {
            transform.Translate(Vector3.forward * 70 * Time.deltaTime, Space.Self);
        }
        else if(m_aniState.IsName("Cut") == true && m_aniState.normalizedTime >= 0.72f)
            m_animator.SetFloat("moveVelocity", 0.0f);
    }

    public void hideEnd()
    {
        m_animator.SetBool("hide", false);
        m_playerRight.SetActive(false);
        m_mainRight.SetActive(true);
        if (s_eventPlayerState != null)      // 델리게이트 체크
            s_eventPlayerState(1004);
    }

    public void leafAttack()
    {
        if (m_aniTransition.IsName("Move -> LeafAttackChargeStart") == true || m_aniTransition.IsName("Roll -> LeafAttackChargeStart") == true || 
            m_aniTransition.IsName("Idle -> LeafAttackChargeStart") == true)   
        {
            m_leafAttackRange.transform.position = transform.position;                  // 범위 이펙트 활성화
            m_leafAttackRange.transform.Translate(Vector3.forward*7, Space.Self);
            m_leafAttackRange.transform.rotation = transform.rotation; 
            m_leafAttackRangeEffect.Play();
            m_fLeafAttackTimer = 0.0f;
        }
        if (m_aniState.IsName("LeafAttackChargeStart") == true)
        {
            if (m_fLeafAttackTimer > 1.2f) 
            {
                m_animator.SetBool("leafAttackCharge", false);
            }
            else if (m_animator.GetBool("leafAttackCharge")==true)
            {
                m_leafAttackRange.transform.Translate(Vector3.forward * 10 * (Time.deltaTime), Space.Self);
                m_fLeafAttackTimer += Time.deltaTime;
            }
        }
        else if (m_aniState.IsName("LeafAttack") == true && m_aniState.normalizedTime > 0.2f && m_aniState.normalizedTime < 0.35f)
        {
            transform.Translate(transform.forward * m_fLeafAttackTimer * 1.0f, Space.World);
        }
        if (m_aniTransition.IsName("LeafAttackChargeStart -> LeafAttack") == true)
        {
            CharacterInfoManager.instance.beginCoolTimeDown((int)SKILL_TYPE.LEAF_ATTACK);     // 쿨타임 시작
            m_leafAttackRangeEffect.Stop();
        }
        else if (m_aniTransition.IsName("LeafAttack -> Idle") == true)
        {
            m_fLeafAttackTimer = 0.0f;
            m_animator.SetBool("leafAttack", false);
        }
    }

    public void leafAttackEffect()
    {
        m_leafAttackEffect.gameObject.SetActive(true);
        m_leafAttackEffect.transform.position = transform.position;
        m_leafAttackEffect.transform.rotation = transform.rotation;
        if (IsInvoking("leafAttackEffectTime") == true)
            CancelInvoke("leafAttackEffectTime");
        Invoke("leafAttackEffectTime", 2.0f);
    }
    public void leafAttackEffectTime()
    {
        m_leafAttackEffect.gameObject.SetActive(false);
    }

    public void swordWind()
    {
        if (m_aniTransition.IsName("ChargeAttack1 -> Idle") == true ||
            m_aniTransition.IsName("ChargeAttack2 -> Idle") == true ||
            m_aniTransition.IsName("ChargeAttack3 -> Idle") == true)
        {
            m_swordWindChargeEffect.Stop();
            CharacterInfoManager.instance.beginCoolTimeDown((int)SKILL_TYPE.SWORD_WIND);     // 쿨타임 시작
        }
        if (m_animator.GetBool("swordWind") == true && (m_aniState.IsName("ChargeStart") == true || m_aniState.IsName("Charging") == true))
        {
            if (m_swordWindChargeEffect.isPlaying == false)
                m_swordWindChargeEffect.Play();
            if (m_fSwordWindTimer > 2.4f)
            {
                m_animator.SetBool("swordWind",false);
                m_fSwordWindTimer = 0.0f;
            }
            else
            {
                m_fSwordWindChargeFlag = true;
                m_fSwordWindTimer = m_animator.GetFloat("swordWindTimer");   // 콤보 유지 시간 처리
                m_fSwordWindTimer += Time.deltaTime;
                m_animator.SetFloat("swordWindTimer", m_fSwordWindTimer);
            }
        }
    }

    public void rampage()
    {
        if (m_aniState.IsName("HoldAttack1") == true)
        {
            m_fTrailEffectTime = 1.0f;
            if (m_aniState.normalizedTime > 0.6f)
                m_animator.Play("HoldAttack2", 0, 0.2f);
        }
        else if (m_aniState.IsName("HoldAttack2") == true)
        {
            if (m_aniState.normalizedTime > 0.5f)
                m_animator.Play("HoldAttack1", 0, 0.3f);
        }
        if (m_aniTransition.IsName("HoldAttack1 -> HoldAttackEnd") == true || m_aniTransition.IsName("HoldAttack2 -> HoldAttackEnd") == true)
        {
            CharacterInfoManager.instance.beginCoolTimeDown((int)SKILL_TYPE.RAMPAGE);     // 쿨타임 시작
        }
    }

    void getCrowdContorl()      // 상태이상 처리
    {
        if(m_aniTransition.IsName("Revive -> Idle") == true || m_aniTransition.IsName("Stun -> Idle") == true)
        {
            initAnimatorInfo();
            m_rushEffect.Stop();
            m_leafAttackRangeEffect.Stop();
            m_swordWindChargeEffect.Stop();
            m_levelUpEffect.Stop();
            m_flashEffect.Stop();
            m_cameraScript.moveDamping = 15.0f;
            m_cameraScript.rushKnockBack = false;
        }
    }

     ////////////////////////////////////////////////// 이벤트 처리 함수 ////////////////////////////////////////////////////////////

    public void hit(int type)
    {
        m_weaponCollider.isTrigger = true;               // 무기 콜라이더 트리거 활성화
        if (m_aniState.IsName("MoveAttack") == true)
        {
            m_fTrailEffectTime = 1.0f;
            m_fCurAttackDamage = 1.0f;
            m_fAttackHoldTime = 0.1f;
        }
       else if (m_aniState.IsName("BaseAttack1") == true)
        {
            m_fTrailEffectTime = 0.3f;
            m_fCurAttackDamage = 1.0f;
            m_fAttackHoldTime = 0.1f;
        }
        else if (m_aniState.IsName("BaseAttack2") == true)
        {
            m_fTrailEffectTime = 0.5f;
            m_fCurAttackDamage = 1.5f;
            m_fAttackHoldTime = 0.1f;
        }
        else if (m_aniState.IsName("BaseAttack3") == true)
        {
            m_fCurAttackDamage = 2.0f;
            m_fAttackHoldTime = 0.1f;
        }
        else if (m_aniState.IsName("DashAttack") == true)
        {
            m_fCurAttackDamage = 5.0f;
            m_fAttackHoldTime = 1.0f;
        }
        else if (m_aniState.IsName("HoldAttack1") == true || m_aniState.IsName("HoldAttack2") == true)
        {
            m_fCurAttackDamage = 0.5f;
            m_fAttackHoldTime = 0.033f;
        }
        else if (m_aniState.IsName("ChargeAttack1") == true || m_aniState.IsName("ChargeAttack2") == true || m_aniState.IsName("ChargeAttack3") == true || m_aniState.IsName("HoldAttackEnd") == true)
        {
            m_fTrailEffectTime = 1.0f;
            GameObject swordWind = ObjectPoolManager.Instance.PopFromPool("swordWind");
            swordWind.transform.position = transform.position;   // 위치동기화
            swordWind.transform.rotation = transform.rotation;   // 방향동기화

            if (type == 0)
            {
                swordWind.transform.Translate(new Vector3(5, 2, 3), Space.Self);
                swordWind.transform.Rotate(Vector3.forward, -2, Space.Self);
            }
            else if (type == 1)
            {
                swordWind.transform.Translate(new Vector3(-5, 2, 3), Space.Self);
                swordWind.transform.Rotate(Vector3.forward, 2, Space.Self);
            }
            else if (type == 2)
            {
                swordWind.transform.Translate(new Vector3(0, 2, 3), Space.Self);
                swordWind.transform.Rotate(Vector3.forward, 80, Space.Self);
            }
            swordWind.SetActive(true);
            m_fCurAttackDamage = 1.0f;
            m_fAttackHoldTime = 0.1f;
        }
        else if (m_aniState.IsName("LeafAttack") == true)
        {
            m_fCurAttackDamage = 2.0f;
            m_fAttackHoldTime = 0.1f;
            leafAttackEffect();
            if (s_eventPlayerState != null)      // 델리게이트 체크
                s_eventPlayerState(20);     // 20안에잇는 적 전체 타격
        }
        if(m_aniState.IsName("Cut") == true)
        {
            m_fCurAttackDamage = -5.0f;
            m_fAttackHoldTime = 0.2f;
            m_fTrailEffectTime = 1.0f;
        }
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
            if (s_eventPlayerState != null)             // 델리게이트 체크
                s_eventPlayerState(444);                // 플레이어가 사망했다고 적에게 알림
        }
    }

    public void delay()                // 후딜레이 시 카메라 조작
    {
        m_fCameraRotate = 0.4f;
    }

    private void OnTriggerEnter(Collider coll)                // 공격 충돌 처리
    {
        if (coll.gameObject.tag == "enemy")                   // 충돌 대상이 적 태그를 가지고 있으면
        {
            CROWD_CONTROL cc = CROWD_CONTROL.NONE;            // 특수 공격상태

            int str = CharacterInfoManager.instance.m_iCurStr;
            EnemyInfomation enemyScript = coll.GetComponentInParent<EnemyInfomation>();   // 적 스크립트를 받아와서

            if (Vector3.Dot(coll.transform.forward, (transform.position - coll.transform.position).normalized) < Mathf.Cos(90 * Mathf.Deg2Rad))   // 내적을 구해서 적의 정면에서 90도내에 플레이어가 없으면 (좌우 합쳐 180도)
                cc = CROWD_CONTROL.BACK_ATTACK;                                                                                                    // 백어택 발동
            if (m_fCurAttackDamage * str < 0.0f)
                enemyScript.attacted(-(m_fCurAttackDamage * str), cc);         // 그 적은 내 현재 공격모션의 데미지를 부여함
            else
                enemyScript.attacted(m_fCurAttackDamage * str, cc);         // 그 적은 내 현재 공격모션의 데미지를 부여함
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
            m_itemManagerScript.putInventroyItem(dropItemScripte.getItenName()); // 인벤토리에 아이템을 넣음
            ObjectPoolManager.Instance.PushToPool("DropItem", coll.gameObject);
        }
        if (coll.gameObject.tag == "DropGold")
        {
            DropGold dropGoldScripte = coll.GetComponent<DropGold>();
            CharacterInfoManager.instance.m_characterInfo.m_iGold += coll.GetComponent<DropGold>().getGoldAmount();
            m_profileUIManagerScript.changeGold();
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
        CharacterInfoManager.instance.m_iCurHp = CharacterInfoManager.instance.m_characterInfo.m_iMaxHp;        // 체력 최대치
        if (s_eventPlayerState != null)             // 델리게이트 체크
            s_eventPlayerState(1004);              // 플레이어가 부활했다고 적에게 알림
    }

}


