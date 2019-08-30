using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllCharacterInfo      // 캐릭터의 모든 데이터정보
{
    public string m_strUserName = "다이제쿠키";     // 유저 닉네임 (값이 없을 경우에 한에 테스트용 임시)
    public CHARACTER_TYPE m_eCharacterType = CHARACTER_TYPE.AKAZA; // 캐릭터 타입 (디폴트 AKAZA)
    public int m_iLevel;                    // 레벨
    public int m_iMaxHp = 100;                    // 최대 체력
    public int m_iMaxExp;                   // 다음 레벨이 되기 위한 경험치
    public int m_iCurExp;                   // 현재 경험치
    public int m_iStr;                      // 공격력
    public int m_iDef;                      // 방어력
    public int m_iJam;                      // 보석
    public int m_iGold;                     // 골드
    public int m_iWeaponStr;                // 무기 추가 공격력
    public int m_iArmorDef;                 // 방어구 추가 방어력
}

public class DefaultSkillInfo
{
    public string m_strImageName;
    public string m_strName;
    public float m_fMaxCoolTime;
    public float m_fCurCoolTime;

    public DefaultSkillInfo()
    {
    }

    public DefaultSkillInfo(string imageName, string name, float coolTime)
    {
        m_strImageName = imageName;
        m_strName = name;
        m_fMaxCoolTime = coolTime;
        m_fCurCoolTime = 0.0f;
    }
}

public enum SKILL_TYPE
{
    RUSH,          // 돌진
    LEAF_ATTACK,   // 강습
    FLASH,         // 일섬
    RAMPAGE,       // 난무
    SWORD_WIND,    // 검풍
    ASSASSINATION, // 암살

    POTION,        // 포션 (표시하진 않음)
    NOT_COOLTIME_SKILL = 7, // 쿨타임이 없는 스킬
    BASE_ATTACK = 7,
    ROTATE_ATTACK,
    ROLL,
}


public class CharacterInfoManager : MonoSingleton<CharacterInfoManager> // 캐릭터의 능력치 및 쿨타임 등 전반적인 관리 클래스
{
    public AllCharacterInfo m_characterInfo = new AllCharacterInfo();  // 캐릭터의 모든 정보를 가지고 있는 클래스 변수
    private Dictionary<CHARACTER_TYPE, DefaultCharacterInfo> m_dicDefaultCharacterInfo;   // 캐릭터의 디폴트 정보를 가지고 있는 변수
    public int m_iCurHp = 100;      // 디폴트 100(테스트용)
    public int m_iCurStr = 30;      // 디폴트 30(테스트용)
    public List<DefaultSkillInfo> m_arrLstDefaultSkillInfo = new List<DefaultSkillInfo>();
    bool m_bCoolTimeDownState = false;
    bool m_bLoadUserFlag = false;           // 한번 로딩 한 경우 재로딩을 방지

    public float getHpPercent()
    {
        return ((float)m_iCurHp / (float)m_characterInfo.m_iMaxHp);
    }

    void Awake()
    {
        m_arrLstDefaultSkillInfo.Add(new DefaultSkillInfo("Rush", "돌진", 4.0f));
        m_arrLstDefaultSkillInfo.Add(new DefaultSkillInfo("LeafAttack","강습",8.0f));
        m_arrLstDefaultSkillInfo.Add(new DefaultSkillInfo("Flash", "일섬", 10.0f));
        m_arrLstDefaultSkillInfo.Add(new DefaultSkillInfo("Rampage","난무",12.0f));    
        m_arrLstDefaultSkillInfo.Add(new DefaultSkillInfo("SwordWind","검풍",15.0f));  
        m_arrLstDefaultSkillInfo.Add(new DefaultSkillInfo("Assassination","암살", 18.0f));

        m_arrLstDefaultSkillInfo.Add(new DefaultSkillInfo("Potion", "포션", 5.0f));
        m_arrLstDefaultSkillInfo.Add(new DefaultSkillInfo("BaseAttack", "기본기", 0.0f));
        m_arrLstDefaultSkillInfo.Add(new DefaultSkillInfo("RotateAttack", "회전공격", 0.0f));
        m_arrLstDefaultSkillInfo.Add(new DefaultSkillInfo("Roll", "구르기", 0.0f));
    }

    public bool coolTimeCheck(int skillType)
    {
        if (m_arrLstDefaultSkillInfo[skillType].m_fCurCoolTime > 0.0f)
        {
            return true;
        }
        return false;
    }

    public void beginCoolTimeDown(int skillType)
    {
        m_arrLstDefaultSkillInfo[skillType].m_fCurCoolTime = m_arrLstDefaultSkillInfo[skillType].m_fMaxCoolTime;  // 해당 스킬의 쿨타임을 맥스치로만듬
        if (m_bCoolTimeDownState == false)                           // 스킬 쿨타임을 내리는 코루틴이동작하지 않고 있다면
           StartCoroutine(coolTimeDown());  // 코루틴 시작
    }

    public bool levelUpCheck()
    {
        if (m_characterInfo.m_iCurExp >= m_characterInfo.m_iMaxExp)
        {
            m_characterInfo.m_iCurExp -= m_characterInfo.m_iMaxExp; // 경험치 하강
            m_characterInfo.m_iLevel++;                             // 레벨 상승
            calculateStatus();                                      // 레벨업에 따른 능력치 재계산
            m_iCurHp = m_characterInfo.m_iMaxHp;                    // HP 회복
            return true;
        }
        return false;
    }

    private IEnumerator coolTimeDown()       // 디폴트 1초마다 쿨타임 계산
    {
        m_bCoolTimeDownState = true;
        while (true)              // 쿨타임 다운상태가 true이면
        {
            bool coolDownState = false;     // 쿨타임 다운 상태는 디폴트 false
            for (int i = 0; i < m_arrLstDefaultSkillInfo.Count; i++)
            {
                if (m_arrLstDefaultSkillInfo[i].m_fCurCoolTime > 0.0f) // 스킬목록중 하나라도 쿨타임이 존재하면
                {
                    m_arrLstDefaultSkillInfo[i].m_fCurCoolTime -= Time.deltaTime;
                    coolDownState = true;
                }
            }
            yield return null;
            if (coolDownState == false)         // 스킬목록 모두 쿨타임이 존재하지 않으면 코루틴종료
                break;
        }
        m_bCoolTimeDownState = false;
    }

    public void loadUserInfo(int characterIndex)        // 유저 이름에 해당하는 정보를 불러온다.
    {
        if (m_bLoadUserFlag == false)
        {
            DynamicCharacterInfo characterInfo = DataManager.instance.loadDynamicCharacterInfo(characterIndex);
            m_characterInfo.m_strUserName = characterInfo.m_strUserName;
            m_characterInfo.m_eCharacterType = characterInfo.m_eCharacterType;
            m_characterInfo.m_iLevel = characterInfo.m_iLevel;
            m_characterInfo.m_iCurExp = characterInfo.m_iExp;
            m_characterInfo.m_iJam = characterInfo.m_iJam;
            m_characterInfo.m_iGold = characterInfo.m_iGold;
            m_dicDefaultCharacterInfo = DefaultDataManager.instance.loadDefaultCharacterInfo();        // 디폴트 캐릭터 정보를 모두 받아옴
            calculateStatus();                   // 불러온 정보와 디폴트 정보를 비교해서 스텟을 정한다.
            m_bLoadUserFlag = true;
        }
    }

    void calculateStatus()                   // 캐릭터 타입과 현재 레벨을 통해 나머지의 능력치를 계산한다.
    {
        DefaultCharacterInfo info = m_dicDefaultCharacterInfo[m_characterInfo.m_eCharacterType];   // 캐릭터 타입에 따른 정보를 분리함

        int level = m_characterInfo.m_iLevel;                                     // 현재 레벨을 받아와서 계산

        m_characterInfo.m_iMaxHp = info.m_iMaxHp + level* info.m_iMaxHpUp;        // 기본 체력 + 레벨당 상승 체력
        m_characterInfo.m_iMaxExp = info.m_iMaxExp + level * info.m_iMaxExpUp;    // 다음레벨이 되기위한 기본 경험치 + 레벨당 상승 경험치
        m_characterInfo.m_iStr = info.m_iStr + level * info.m_iStrUp;             // 기본 공격력 + 레벨당 상승 공격력
        m_characterInfo.m_iDef= info.m_iDef + level * info.m_iDefUp;              // 기본 방어력 + 레벨당 상승 방어력
        m_iCurHp = m_characterInfo.m_iMaxHp;
        m_iCurStr = m_characterInfo.m_iStr + m_characterInfo.m_iWeaponStr;
    }

    public void replaceStr()
    {
        m_iCurStr = m_characterInfo.m_iStr + m_characterInfo.m_iWeaponStr;
    }

    public void saveUserInfo(int characterIndex)
    {

    }
}
