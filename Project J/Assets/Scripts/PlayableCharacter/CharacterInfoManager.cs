using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllCharacterInfo      // 캐릭터의 모든 데이터정보
{
    public string m_strUserName = null;     // 유저 닉네임
    public CHARACTER_TYPE m_eCharacterType; // 캐릭터 타입
    public int m_iLevel;                    // 레벨
    public int m_iMaxHp;                    // 최대 체력
    public int m_iMaxExp;                   // 다음 레벨이 되기 위한 경험치
    public int m_iCurExp;                   // 현재 경험치
    public int m_iStr;                      // 공격력
    public int m_iDef;                      // 방어력
    public int m_iJam;                      // 보석
    public int m_iGold;                     // 골드
    public int m_iWeaponStr;                // 무기 추가 공격력
    public int m_iArmorDef;                 // 방어구 추가 방어력
}

public class CharacterInfoManager : MonoSingleton<CharacterInfoManager>
{
    public AllCharacterInfo m_characterInfo = new AllCharacterInfo();  // 캐릭터의 모든 정보를 가지고 있는 클래스 변수
    private Dictionary<CHARACTER_TYPE, DefaultCharacterInfo> m_dicDefaultCharacterInfo;   // 캐릭터의 디폴트 정보를 가지고 있는 변수
    void Awake()
    {
       
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loadUserInfo(int characterIndex)        // 유저 이름에 해당하는 정보를 불러온다.
    {
        DynamicCharacterInfo characterInfo = DataManager.instance.loadDynamicCharacterInfo(characterIndex);
        m_characterInfo.m_strUserName = characterInfo.m_strUserName;
        m_characterInfo.m_eCharacterType = characterInfo.m_eCharacterType;
        m_characterInfo.m_iLevel = characterInfo.m_iLevel;
        m_characterInfo.m_iCurExp = characterInfo.m_iExp;
        m_characterInfo.m_iJam = characterInfo.m_iJam;
        m_characterInfo.m_iGold = characterInfo.m_iGold;
        calculateStatus();                   // 불러온 정보와 디폴트 정보를 비교해서 스텟을 정한다.
    }

    void calculateStatus()                   // 캐릭터 타입과 현재 레벨을 통해 나머지의 능력치를 계산한다.
    {
        m_dicDefaultCharacterInfo = DefaultDataManager.instance.loadDefaultCharacterInfo();        // 디폴트 캐릭터 정보를 모두 받아옴
        DefaultCharacterInfo info = m_dicDefaultCharacterInfo[m_characterInfo.m_eCharacterType];   // 캐릭터 타입에 따른 정보를 분리함

        int level = m_characterInfo.m_iLevel;                                     // 현재 레벨을 받아와서 계산

        m_characterInfo.m_iMaxHp = info.m_iMaxHp + level* info.m_iMaxHpUp;        // 기본 체력 + 레벨당 상승 체력
        m_characterInfo.m_iMaxExp = info.m_iMaxExp + level * info.m_iMaxExpUp;    // 다음레벨이 되기위한 기본 경험치 + 레벨당 상승 경험치
        m_characterInfo.m_iStr = info.m_iStr + level * info.m_iStrUp;             // 기본 공격력 + 레벨당 상승 공격력
        m_characterInfo.m_iDef= info.m_iDef + level * info.m_iDefUp;              // 기본 방어력 + 레벨당 상승 방어력
    }

    public void saveUserInfo(int characterIndex)
    {

    }
}
