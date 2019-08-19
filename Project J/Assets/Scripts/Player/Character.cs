using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum DIRECTION
{
    NONE = 0,
    LEFT = 1,
    TOP = 2,
    RIGHT = 3,
    BOTTOM = 4,
}

public class Character : MonoBehaviour                 // 캐릭터 상속용 스크립트
{
    protected Animator m_animator;                     // 애니메이터
    protected AnimatorStateInfo m_aniState;            // 애니메이터 진행 상태
    protected AnimatorTransitionInfo m_aniTransition;  // 애니메이터 전환 상태
    protected float m_fMaxHP;                          // 최대 체력
    protected float m_fCurHP;                          // 현재 체력
    protected float m_fCurAttackDamage;                // 현재 공격타입에 따른 추가 데미지
    protected float m_fAttackHoldTime;                 // 콜라이더 트리거체크 공격일 경우 공격판정 유지 시간

    public float maxHP                                 // 최대 체력 반환
    {
        get
        {
            return m_fMaxHP;
        }
    }
    public float curHP                                 // 현재 체력 반환
    {
        get
        {
            return m_fCurHP;
        }
    }
    public float percentHP                             //  0~1 크기 체력 비율 반환 (체력 바에 사용할 용도)
    {
        get
        {
            return m_fCurHP / maxHP;
        }
    }
}

public class Player : Character
{
}

public class Enemy : Character
{
    protected int m_iGetExp;                   // 얻는 경험치

    protected Transform m_targetTransform;       // 타겟의 트랜스폼 
    protected float m_fTargetDistance = 0.0f;    // 자신과 타겟과의 거리차
}