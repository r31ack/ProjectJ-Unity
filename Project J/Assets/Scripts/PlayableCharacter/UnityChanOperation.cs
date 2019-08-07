using UnityEngine;
using System.Collections;
using UnityEngine.AI;                                            // NavMesh를 사용해 컨트롤

public class UnityChanOperation : Player   // 유니티짱 조작 스크립트
{
    private NavMeshAgent m_agent;
    //private InGameUIManager m_ingameUIScript;         // UIManager 스크립트

    void Awake()
    {
        //m_ingameUIScript = GameObject.Find("InGameUI").GetComponent<InGameUIManager>();
    }

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        m_aniTransition = m_animator.GetAnimatorTransitionInfo(0);           // 현재 애니메이션 전환상태
        m_aniState = m_animator.GetCurrentAnimatorStateInfo(0);              // 현재 애니메이션 진행상태
                                                                             //r = Input.GetAxis("Mouse X");
        OperateInput();
    }

    void OperateInput()
    {
        int stateLevel = m_animator.GetInteger("stateLevel");   // 현재 상태 레벨을 받아옴

        if (stateLevel < 18)      // 상태레벨이 피격중이 아니면 조작 가능
        {
            if (stateLevel == 0)     // 상태레벨이 0이면 기본행위만
            {
                move();              // 이동
                jump();              // 점프
            }
            if (stateLevel <= 1)     // 상태레벨이 1이하 기본 콤보만
            {
                baseAttack();        // 기본공격
                rotate();            // 좌우 회전
            }
            if (stateLevel <= 2)
            {
                chargeAttack();
                holdAttack();
            }
            if (stateLevel == 4)
            {
                dashAttack();
                baseAttack();        // 기본공격
            }
            roll();
        }
    }

    void move()
    {
        float moveVelocity = m_animator.GetFloat("moveVelocity");  // 애니메이터로부터 현재 이동속도를 받아옴

        if (Input.GetKey(KeyCode.W))        // 앞방향 이동
        {
            if (moveVelocity < 20)         // 속도가 40보다 작다면
                moveVelocity += 30.0f *Time.deltaTime;         // 속도를 1 더함
        }
        else if (Input.GetKey(KeyCode.S))   // 뒷걸음
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
        transform.Translate(transform.forward * moveVelocity * Time.deltaTime, Space.World);   // 앞으로 현재 속도만큼 이동
        m_animator.SetFloat("moveVelocity", moveVelocity);                                     // 애니메이터에 현재 속도 세팅
    }

    void rotate()   // 좌우 회전
    {
        if (Input.GetKey(KeyCode.A))        // 좌우 회전
            transform.Rotate(Vector3.up, -70 * Time.deltaTime, Space.World);
        else if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.up, +70 * Time.deltaTime, Space.World);
    }

    void jump()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            m_animator.SetBool("jump", true);
        }
    }

    void roll()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.A))
                m_animator.SetFloat("roll", 1);
            if (Input.GetKey(KeyCode.W))
                m_animator.SetFloat("roll", 2);
            if (Input.GetKey(KeyCode.D))
                m_animator.SetFloat("roll", 3);
            if (Input.GetKey(KeyCode.S))
                m_animator.SetFloat("roll", 4);
            m_animator.SetInteger("stateLevel", 9);
        }
    }

    void baseAttack()
    {
        if(Input.GetKeyDown(KeyCode.I) == true)         // 마우스 왼쪽 키를 눌럿으면
        {
            int baseComboCount = m_animator.GetInteger("baseComboCount");       // 콤보 횟수를 받아옴

            if (baseComboCount == 0)   // 콤보횟수가 없으면
            {
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
                    m_animator.SetTrigger("baseAttack2");   // 트리거 활성화
                }
            }
            else if (baseComboCount == 2)   // 콤보횟수가 2이면
            {
                if (m_aniState.IsName("BaseAttack2") == true && m_aniState.normalizedTime > 0.4f) // baseAttack1의 애니메이션이 40퍼 이상이면
                {
                    m_animator.SetFloat("baseComboTimer", 1.0f);   // 다음 콤보 연결 유지가능 시간 1초 세팅
                    m_animator.SetInteger("baseComboCount", 3);
                    m_animator.SetInteger("stateLevel", 4);        // 상태 레벨 4로 세팅 
                    m_animator.SetTrigger("baseAttack3");   // 트리거 활성화
                }
            }
        }
    }

    void dashAttack()
    {
        if (Input.GetKeyDown(KeyCode.P) == true)       
        {
            m_animator.SetTrigger("dashAttack");
            m_animator.SetInteger("stateLevel", 2);
        }
    }

    void holdAttack()
    {
        if (Input.GetKeyDown(KeyCode.O) == true)         // 마우스 왼쪽 키를 눌럿으면
        {
            m_animator.SetFloat("holdTimer", 2.5f);
            m_animator.SetBool("holdAttack", true);   
            m_animator.SetInteger("stateLevel", 2);   // 상태 레벨 2로 세팅 
        }
        else if (Input.GetKeyUp(KeyCode.O))
        {
            if (m_animator.GetBool("holdAttack") == true)
            {
                m_animator.SetFloat("holdTimer", -0.1f);
                m_animator.SetBool("holdAttack", false);
            }
        }
    }

    void chargeAttack()
    {
        if (Input.GetKeyDown(KeyCode.P) == true)         // 마우스 왼쪽 키를 눌럿으면
        {
            m_animator.SetBool("chargeAttack", true);   
            m_animator.SetInteger("stateLevel", 2);   // 상태 레벨 2로 세팅 
        }
        else if (Input.GetKeyUp(KeyCode.P))
        {
            if (m_animator.GetBool("chargeAttack") == true)
            {
                m_animator.SetBool("chargeAttack", false);
            }
        }
    }
}
