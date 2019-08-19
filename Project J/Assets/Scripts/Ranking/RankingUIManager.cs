using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class RankingUIManager : Singleton<RankingUIManager>
{
    Dictionary<string, RankingInfo> m_dicRankingInfo;  // 데이터에서 받아온 랭킹 정보
    List<RankingInfo> m_arrLstRankingInfo = new List<RankingInfo>();          // 딕셔너리를 랭킹순서를 기반index로 하는 arrayList로 저장
    ScrollRect m_scrollRect;
    public int m_iMinShowRank = 1;     // 최소 보여주고 있는 랭킹
    public int m_iMaxShowRank = 0;     // 최대 보여주고 있는 랭킹
    UIWrapContent m_uIWrapContent;

    // 플레이어 표시용
    private UILabel m_rankNumberText;    // 랭킹
    private UILabel m_userNameText;      // 유저명
    private UILabel m_scoreText;         // 스코어
    private UILabel m_clearTimeText;     // 클리어시간

    void Awake()
    {
        m_rankNumberText = transform.Find("PlayerWindow/PlayerRankNumber").GetComponent<UILabel>();
        m_userNameText = transform.Find("PlayerWindow/PlayerName").GetComponent<UILabel>();
        m_scoreText = transform.Find("PlayerWindow/PlayerScore").GetComponent<UILabel>();
        m_clearTimeText = transform.Find("PlayerWindow/PlayerClearTime").GetComponent<UILabel>();

        m_iMaxShowRank = 0;     // 최대 보여주고 있는 랭킹
        addSortPlayerRanking();
    }

    void Start()
    {
        m_uIWrapContent = GameObject.Find("UIWrap Content").GetComponent<UIWrapContent>();
        m_uIWrapContent.minIndex = -(m_dicRankingInfo.Count-1);     // 딕셔너리 컨테이너 갯수가 최대 랭킹 인덱스 갯수
        m_uIWrapContent.maxIndex = 0;                       
    }

    void setPlayerRankInfo(RankingInfo playerInfo)
    {
        for (int i = 0; i < m_arrLstRankingInfo.Count; i++)
        {
            if (m_arrLstRankingInfo[i].m_strUserName == playerInfo.m_strUserName)
            {
                m_rankNumberText.text = (i + 1).ToString() + "위";
                break;
            }
        }
        m_userNameText.text = playerInfo.m_strUserName;
        m_scoreText.text = playerInfo.m_iScore.ToString() + "점";
        m_clearTimeText.text = playerInfo.m_fClearTime.ToString("N2") + "초";
    }
    void addSortPlayerRanking()
    {
        m_dicRankingInfo = DataManager.instance.loadRankingInfo();      // 랭킹 정보를 로드함


        // 플레이어의 랭크 정보를 로드함 
        string userName = CharacterInfoManager.instance.m_characterInfo.m_strUserName;
        RankingInfo playerInfo = new RankingInfo(userName, (int)CharacterInfoManager.instance.m_characterInfo.m_eCharacterType, GameManager.instance.m_iScore, GameManager.instance.m_iClearTime);

        if (m_dicRankingInfo.ContainsKey(userName) == true)   // 이미 유저네임이 있으면
        {
            if (m_dicRankingInfo[userName].m_iScore <= playerInfo.m_iScore)  // 스코어가 지금것이 더 높으면 추가하고 그렇지 않으면 추가하지않음
            {
                m_dicRankingInfo.Remove(userName);             // 해당 유저 삭제 후
                m_dicRankingInfo.Add(userName, playerInfo);    // 딕셔너리에 추가
            }
        }
        else                                                  // 중복된 유저네임이 없으면
            m_dicRankingInfo.Add(userName, playerInfo);       // 딕셔너리에 추가


        m_dicRankingInfo = m_dicRankingInfo.OrderByDescending(node => node.Value.m_iScore).ToDictionary(pair => pair.Key, pair => pair.Value);  // 스코어 기준으로 정렬

        m_arrLstRankingInfo.Capacity = m_dicRankingInfo.Count;  // 배열리스트 딕셔너리 저장소만큼 용량확보

        foreach (KeyValuePair<string, RankingInfo> iterator in m_dicRankingInfo) // 정렬된 딕셔너리를 순회하면서
            m_arrLstRankingInfo.Add(iterator.Value);                             // 배열리스트에 순서대로 삽입

        setPlayerRankInfo(playerInfo);                  // UI에 플레이어 정보 표시
    }

    public List<RankingInfo> getRankingList()           // 배열리스트 기반의 랭킹리스트 반환
    {
        return m_arrLstRankingInfo;
    }

    public void lobbyReturnButton()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}
