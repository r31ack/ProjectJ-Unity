using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


public class MinotaurInfomation : EnemyInfomation
{
    void Awake()
    {
        m_fMaxHP = 300;
        m_fCurHP = m_fMaxHP;
        m_iGetExp = 10;

        m_fIdleTimer = idleHoldTime;
        m_fPatrolTimer = patrolHoldTime;
        m_fFollowTargetTimer = followHoldTime;
    }
}
