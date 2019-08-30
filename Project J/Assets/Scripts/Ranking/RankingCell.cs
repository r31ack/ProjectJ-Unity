using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankingCell : MonoBehaviour   // 랭킹 하나의 정보를 가지고있는 셀
{
    public UISprite m_cellBone;            // 셀의 스프라이트 틀
    public UISprite m_profileImage;        // 캐릭터 프로필 이미지
    public UILabel m_rankNumberText;       // 랭킹
    public UILabel m_userNameText;         // 유저명
    public UILabel m_scoreText;            // 스코어
    public UILabel m_clearTimeText;        // 클리어시간 

    Dictionary<string, RankingInfo> m_dicRankingInfo;
    List<RankingInfo> m_arrLstRankingInfo;

    private int m_iThisRankNumber;           // 현재 내가 가지고있는 랭킹 번호

    private Vector3 prePos;

    void Awake()
    {
        m_cellBone = GetComponent<UISprite>();
       m_dicRankingInfo = DataManager.instance.loadRankingInfo();
        m_arrLstRankingInfo = RankingUIManager.Instance.getRankingList();

        m_rankNumberText = transform.Find("RankNumber").GetComponent<UILabel>();
        m_profileImage = transform.Find("ProfileImage").GetComponent<UISprite>();
        m_userNameText = transform.Find("UserName").GetComponent<UILabel>();
        m_scoreText = transform.Find("Score").GetComponent<UILabel>();
        m_clearTimeText = transform.Find("ClearTime").GetComponent<UILabel>();

        m_iThisRankNumber = ++RankingUIManager.Instance.m_iMaxShowRank;
        setRankingInfo(m_iThisRankNumber);
    }

    private void Start()
    {
        this.transform.localScale = Vector3.one;
        prePos = transform.position;
    }

    public void setRankingInfo(int rankNumber)
    {
        m_rankNumberText.text = rankNumber.ToString() + "위";

        switch (m_arrLstRankingInfo[rankNumber-1].m_eCharacterType)
        {
            case CHARACTER_TYPE.UNITY:
                m_profileImage.spriteName = "UnityChan1";
                break;
            case CHARACTER_TYPE.AKAZA:
                m_profileImage.spriteName = "Akaza1";
                break;
        }
        m_userNameText.text = m_arrLstRankingInfo[rankNumber-1].m_strUserName;
        m_scoreText.text = m_arrLstRankingInfo[rankNumber-1].m_iScore.ToString() + "점";
        m_clearTimeText.text = m_arrLstRankingInfo[rankNumber-1].m_fClearTime.ToString("N2");

        if (m_userNameText.text == CharacterInfoManager.instance.m_characterInfo.m_strUserName)
            m_cellBone.spriteName = "Glow - Inner";
        else
            m_cellBone.spriteName = "Highlight - Shadowed";
    }

    public void fromTopToBottom()       // 위쪽 랭킹에서 아래로 갱신
    {
        m_iThisRankNumber = RankingUIManager.Instance.m_iMaxShowRank + 1;
        if (m_iThisRankNumber > m_arrLstRankingInfo.Count)
            return;
        setRankingInfo(m_iThisRankNumber);
        RankingUIManager.Instance.m_iMinShowRank++;
        RankingUIManager.Instance.m_iMaxShowRank++;
    }

    public void fromBottomToTop()       // 아래쪽 랭킹에서 위로 갱신
    {
        m_iThisRankNumber = RankingUIManager.Instance.m_iMinShowRank - 1;
        if (m_iThisRankNumber < 1)
            return;
        setRankingInfo(m_iThisRankNumber);
        RankingUIManager.Instance.m_iMaxShowRank--;
        RankingUIManager.Instance.m_iMinShowRank--;
    }
}