using UnityEngine;
using System.Collections;

public class AkazaOperation : PlayerOperation   // Akaza 조작 스크립트
{
    override protected void OperateInput()
    {
        if (m_iStateLevel < 18)      // 상태레벨이 피격중이 아니면 조작 가능
        {
            if (m_iStateLevel == 0)  // 상태레벨이 0이면 기본행위만
            {
                move();              // 이동
                rush();
                assassination();
            }
            if (m_iStateLevel <= 1)     // 상태레벨이 1이하 기본 콤보만
            {
                baseAttack();        // 기본공격
            }
            if (m_iStateLevel <= 1)
            {
                leafAttack();
            }
            if (m_iStateLevel == 4)
            {
                flash();             
                baseAttack();        // 기본공격
                rampage();           // 유지공격
                moveAttack();

            }
            if (m_iStateLevel >=4 && m_iStateLevel <= 5)
                swordWind();
            if (m_iStateLevel != 9 && m_iStateLevel != (int)PLAYER_STATE.HIDE)
            {
                roll();
            }
            if (m_iStateLevel == 9)
            {
                rush();
                leafAttack();
            }
            drinkPotion();

            if(m_iStateLevel == (int)PLAYER_STATE.HIDE)
                assassination();
        }
    }

    void rush()
    {
        if (CharacterInfoManager.instance.coolTimeCheck((int)SKILL_TYPE.RUSH) == false) // 쿨타임이 존재하지 않을 시
        {
            if ((Input.GetKey(KeyCode.Keypad3)==true) || (InputManager.instance.keyPressCheck(KeyCode.Keypad3)==true))
            {
                if (Input.GetKey(KeyCode.W) || (InputManager.instance.keyPressCheck(KeyCode.W) == true))
                {
                    m_animator.SetBool("rush", true);
                    m_animator.SetInteger("stateLevel", 4);
                    m_animator.SetInteger("direction", 2);
                    SkillUIManager.instance.setSkillType(3, SKILL_TYPE.ROTATE_ATTACK);
                    SkillUIManager.instance.setSkillType(5, SKILL_TYPE.RAMPAGE);
                    SkillUIManager.instance.setSkillType(6, SKILL_TYPE.FLASH);
                }
                else if (Input.GetKey(KeyCode.S) || (InputManager.instance.keyPressCheck(KeyCode.S) == true))
                {
                    m_animator.SetBool("rush", true);
                    m_animator.SetInteger("stateLevel", 4);
                    m_animator.SetInteger("direction", 4);
                    SkillUIManager.instance.setSkillType(6, SKILL_TYPE.SWORD_WIND);
                }
            }
        }
    }

    void flash()
    {
        if (CharacterInfoManager.instance.coolTimeCheck((int)SKILL_TYPE.FLASH) == false) // 쿨타임이 존재하지 않을 시
        {
            if (Input.GetKeyDown(KeyCode.Keypad6) == true || InputManager.instance.keyPressCheck(KeyCode.Keypad6) == true)
            {
                m_animator.SetBool("flash", true);
                m_animator.SetInteger("stateLevel", 3);
            }
        }
    }

    void rampage()
    {
        if (CharacterInfoManager.instance.coolTimeCheck((int)SKILL_TYPE.RAMPAGE) == false) // 쿨타임이 존재하지 않을 시
        {
            if (Input.GetKey(KeyCode.Keypad5) == true || InputManager.instance.keyPressCheck(KeyCode.Keypad5) == true)         // 마우스 왼쪽 키를 눌럿으면
            {
                if (m_animator.GetBool("holdAttack") == false && m_animator.GetInteger("stateLevel") != 2)
                {
                    m_animator.SetFloat("holdTimer", 2.5f);
                    m_animator.SetBool("holdAttack", true);
                    m_animator.SetInteger("stateLevel", 4);   // 상태 레벨 4로 세팅 
                }
            }
            else if (Input.GetKeyUp(KeyCode.Keypad5) || InputManager.instance.keyUpCheck(KeyCode.Keypad5) == true)
            {
                if (m_animator.GetBool("holdAttack") == true)
                {
                    m_animator.SetFloat("holdTimer", -0.1f);
                    m_animator.SetBool("holdAttack", false);
                }
            }
        }
    }

    public void assassination()                                      // 암살 스킬
    {
        if (m_iStateLevel != (int)PLAYER_STATE.HIDE)                 // 하이드 상태가 아니면
        {
            if (Input.GetKey(KeyCode.Keypad5) == true || InputManager.instance.keyPressCheck(KeyCode.Keypad5) == true)           // 5번 키를 눌렸다면
            {
                if (CharacterInfoManager.instance.coolTimeCheck((int)SKILL_TYPE.ASSASSINATION) == false) // 쿨타임이 존재하지 않을 시
                {
                    m_animator.SetFloat("holdTimer", 2.5f);
                    m_animator.SetBool("hide", true);                                                       // 하이드 트리거 발동
                    m_animator.SetInteger("stateLevel", (int)PLAYER_STATE.HIDE);                         // 하이드 상태로 전환
                }
            }
        }
        else if (m_iStateLevel == (int)PLAYER_STATE.HIDE)
        {
            hideWalk();                                                                                     // 걸어다님(암살 태세)
            if (Input.GetKeyUp(KeyCode.Keypad5) == true || InputManager.instance.keyUpCheck(KeyCode.Keypad5) == true)           // 5번 키를 눌렸다면
            {
                m_animator.SetFloat("holdTimer", -0.1f);
                m_animator.SetInteger("stateLevel", 0);
            }
        }
    }

    void hideWalk()     // 은신 이동
    {
        float moveVelocity = m_animator.GetFloat("moveVelocity");  // 애니메이터로부터 현재 이동속도를 받아옴

        if (Input.GetKey(KeyCode.W) || InputManager.instance.keyPressCheck(KeyCode.W))        // 앞방향 이동
        {
            if (moveVelocity < 10)         // 속도가 10보다 작다면
                moveVelocity += 30.0f * Time.deltaTime;         // 속도를 1 더함
            else if (moveVelocity > 11)    // 속도가 11보다 크면 내림
                moveVelocity -= 30.0f * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S) || InputManager.instance.keyPressCheck(KeyCode.S))   // 뒷걸음
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

    void swordWind()
    {
        if (CharacterInfoManager.instance.coolTimeCheck((int)SKILL_TYPE.SWORD_WIND) == false) // 쿨타임이 존재하지 않을 시
        {
            if (Input.GetKey(KeyCode.Keypad6) == true || InputManager.instance.keyPressCheck(KeyCode.Keypad6) == true && m_animator.GetInteger("stateLevel") != 5)         // 마우스 왼쪽 키를 눌럿으면
            {
                m_animator.SetBool("swordWind", true);
                m_animator.SetInteger("stateLevel", 5);   // 상태 레벨 5로 세팅 
            }
            else if (Input.GetKeyUp(KeyCode.Keypad6) == true || InputManager.instance.keyUpCheck(KeyCode.Keypad6) == true)
            {
                if (m_animator.GetBool("swordWind") == true)
                {
                    m_animator.SetBool("swordWind", false);
                }
            }
        }
    }

    void leafAttack()
    {
        if (CharacterInfoManager.instance.coolTimeCheck((int)SKILL_TYPE.LEAF_ATTACK) == false) // 쿨타임이 존재하지 않을 시
        {
            if (Input.GetKey(KeyCode.Keypad6) == true || InputManager.instance.keyPressCheck(KeyCode.Keypad6) == true && m_animator.GetInteger("stateLevel") != 1)         // 마우스 왼쪽 키를 눌럿으면
            {
                m_animator.SetBool("leafAttack", true);
                m_animator.SetInteger("stateLevel", 1);   // 상태 레벨 1로 세팅
                m_animator.SetBool("leafAttackCharge", true);
            }
            else if (Input.GetKeyUp(KeyCode.Keypad6) == true || InputManager.instance.keyUpCheck(KeyCode.Keypad6) == true)
            {
                if (m_animator.GetBool("leafAttackCharge") == true)
                {
                    m_animator.SetBool("leafAttackCharge", false);
                }
            }
        }
    }
}
