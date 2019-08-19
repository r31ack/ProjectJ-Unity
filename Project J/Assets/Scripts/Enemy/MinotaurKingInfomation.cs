using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class MinotaurKingInfomation : EnemyInfomation
{
    private void Awake()
    {
        m_fMaxHP = 800;
        m_fCurHP = m_fMaxHP;
    }

    public override void attacted(float damage, CROWD_CONTROL cc = CROWD_CONTROL.NONE)
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
            m_animator.SetTrigger("die");                // 사망 트리거 활성화
            m_eEnemyState = ENEMY_STATE.DIE;             // 사망상태로 전환
            m_animator.Play("Die");
            Destroy(transform.gameObject, 3.0f);   // 3초뒤삭제
            dropItem();
            dropGold();
        }
        else if (stateLevel != (int)ENEMY_STATE.DIE)
        {
            if (cc == CROWD_CONTROL.STUN)
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

    override public void hit()
    {
        if (m_aniState.IsName("BaseAttack1") == true || m_aniState.IsName("BaseAttack2") || m_aniState.IsName("BaseAttack3") == true)        // 공격 이면
        {
            m_punchCollider.isTrigger = true;           // 공격판정 콜라이더 트리거 활성화
            m_fCurAttackDamage = 2.0f;                  // 현재 모션에서의 공격 데미지 2
            m_fAttackHoldTime = 1.0f;                   // 공격판정 유지시간 0.1초
        }
        else if (m_aniState.IsName("Shout") == true)    // 소리치기 이면
        {
            if (Vector3.Distance(m_targetTransform.position, transform.position) < 5)   // 콜라이더를 쓰지 않고 반경 10거리 전범위 스턴 공격
            {
                m_targetTransform.GetComponent<UnityChanInfomation>().attated(10, true);
            }
        }
    }
}