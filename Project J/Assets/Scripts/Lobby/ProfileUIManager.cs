using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileUIManager : MonoBehaviour
{
    private UISprite m_characterMiniIcon;                // 캐릭터 미니 아이콘
    private UILabel m_UserNameText;                      // 유저 닉네임 텍스트
    private UILabel m_LevelText;                         // 레벨 
    private UILabel m_expPercentText;                    // 경험치 퍼센트
    private UILabel m_jamCountText;                      // 보석량
    private UILabel m_goldCountText;                     // 골드량
    private AllCharacterInfo m_characterInfomation;   // 캐릭터 정보를 받아온다.

    private UILabel m_infomationText;                    // 캐릭터 정보창 활성화 시의 모든 데이터정보 텍스트
    private UILabel m_statusText;                        // 스텟 정보 텍스트

    private string m_strCharacterName;                   // 캐릭터명

    void Awake()
    {
        m_characterMiniIcon = transform.Find("CharacterMiniIcon").GetComponent<UISprite>();         // UI연결
        m_UserNameText = transform.Find("UserNameText").GetComponent<UILabel>();
        m_LevelText = transform.Find("LevelText").GetComponent<UILabel>();
        m_expPercentText= transform.Find("ExpPercenText").GetComponent<UILabel>();
        m_jamCountText= transform.Find("JamCountText").GetComponent<UILabel>();
        m_goldCountText= transform.Find("GoldCountText").GetComponent<UILabel>();

        m_infomationText = GameObject.Find("ItemWindow").transform.Find("InfomationUI/InfomationText").GetComponent<UILabel>();
        m_statusText = GameObject.Find("ItemWindow").transform.Find("InfomationUI/StatusPlusText").GetComponent<UILabel>();
    }

    void Start()
    {
        m_characterInfomation = CharacterInfoManager.instance.m_characterInfo;   // 캐릭터 정보를 받아온다.
        m_UserNameText.text = m_characterInfomation.m_strUserName;                  // 유저 닉네임 표시
        m_LevelText.text = m_characterInfomation.m_iLevel.ToString();
        m_expPercentText.text = (((float)m_characterInfomation.m_iCurExp / (float)m_characterInfomation.m_iMaxExp)*100.0f).ToString("N0");  // 소수점 제외
        m_jamCountText.text = m_characterInfomation.m_iJam.ToString();
        m_goldCountText.text = m_characterInfomation.m_iGold.ToString();
        
        if (m_characterInfomation.m_eCharacterType == CHARACTER_TYPE.UNITY)         // 캐릭터 타입을 받아와서
        {
            m_characterMiniIcon.spriteName = "UnityChan1";                          // 해당 캐릭터 아이콘 표시
            m_strCharacterName = "UnityChan";
        }
        if (m_characterInfomation.m_eCharacterType == CHARACTER_TYPE.AKAZA)         // 캐릭터 타입을 받아와서
        {
            m_characterMiniIcon.spriteName = "Akaza1";                              // 해당 캐릭터 아이콘 표시
            m_strCharacterName = "Akaza";
        }

        m_infomationText.text = m_characterInfomation.m_strUserName + "\n\n" + m_strCharacterName + "\n\n" + m_characterInfomation.m_iLevel.ToString() + "\n\n" +
            m_characterInfomation.m_iCurExp.ToString() + "/" + m_characterInfomation.m_iMaxExp.ToString() + "\n\n" + m_characterInfomation.m_iMaxHp.ToString();
   
        m_statusText.text = m_characterInfomation.m_iStr.ToString()  + " (+" + m_characterInfomation.m_iWeaponStr.ToString() + ")\n\n" +
            m_characterInfomation.m_iDef.ToString() + " (+" + m_characterInfomation.m_iArmorDef.ToString() + ")";

    }

    public void changeGold()
    {
        m_goldCountText.text = m_characterInfomation.m_iGold.ToString();
    }

    public void changeStatus()
    {
        m_statusText.text = m_characterInfomation.m_iStr.ToString() + " (+" + m_characterInfomation.m_iWeaponStr.ToString() + ")\n\n" +
        m_characterInfomation.m_iDef.ToString() + " (+" + m_characterInfomation.m_iArmorDef.ToString() + ")";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
