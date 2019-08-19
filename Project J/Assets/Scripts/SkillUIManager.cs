using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUIManager : MonoBehaviour
{
    private int m_iSkillMaxCount;

    GameObject[] m_skillPrefab;
    UISprite[] m_image;
    UILabel[] m_name;
    UILabel[] m_coolTime;
    UIGrid m_grid;

    private void Awake()
    {
        List<DefaultSkillInfo> skillInfo = CharacterInfoManager.instance.m_arrLstDefaultSkillInfo;
        m_iSkillMaxCount = skillInfo.Count;

        m_grid = GetComponent<UIGrid>();


        m_skillPrefab = new GameObject[m_iSkillMaxCount];     // 스킬 최대 갯수만큼 스킬프리팹 할당
        m_image = new UISprite[m_iSkillMaxCount];
        m_coolTime = new UILabel[m_iSkillMaxCount];
        m_name = new UILabel[m_iSkillMaxCount];

        for (int i=0; i< m_iSkillMaxCount; i++)
        {     
            m_skillPrefab[i] = (GameObject)Instantiate(Resources.Load("Prefabs/SkillIcon"));   // 스킬 아이콘 오브젝트를 프리팹으로 생성
            m_skillPrefab[i].transform.parent = transform;
            m_skillPrefab[i].transform.localScale = Vector3.one;
            m_image[i] = m_skillPrefab[i].transform.GetComponentInChildren<UISprite>();
            m_image[i].spriteName = skillInfo[i].m_strImageName+"0";
            m_name[i] = m_skillPrefab[i].transform.Find("Name").GetComponent<UILabel>();
            m_name[i].text = skillInfo[i].m_strName;
            m_coolTime[i] = m_skillPrefab[i].transform.Find("CoolTime").GetComponent<UILabel>();
        }
    }

    void Start()
    {
        StartCoroutine(coolTimeUpdate());
    }

    IEnumerator coolTimeUpdate()
    {
        while (true)
        {
            List<DefaultSkillInfo> skillInfo = CharacterInfoManager.instance.m_arrLstDefaultSkillInfo;

            for (int i = 0; i < m_iSkillMaxCount; i++)
            {
                float coolTime = skillInfo[i].m_fCurCoolTime;
                if(coolTime <= 0.0f)
                {
                    m_image[i].spriteName = skillInfo[i].m_strImageName+"1";
                    m_coolTime[i].text = "";
                }
                else
                {
                    m_image[i].spriteName = skillInfo[i].m_strImageName+"0";
                    m_coolTime[i].text = coolTime.ToString();
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
}
