using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogUIManager : MonoBehaviour
{
    private UILabel m_nameLabel;                        // 대화거는 캐릭터 이름 레이블 
    private UILabel m_chatLabel;                        // 대화 내용 레이블
    private string[] m_strChatContent = new string[5];  // 대화내용 모음
    private int m_strCurChatCount = 0;                         // 대화내용 인덱스 카운트
    private int m_strMaxChatCount = 4;

    // Start is called before the first frame update

    void Awake()
    {
        m_strChatContent[0] = "거기 너, 튼튼해 보이는데 나좀 도와주지 않을래?\n\n\n";
        m_strChatContent[1] = "여길 지나가야 하는데 혼자서 좀 버거운 참이거든\n\n\n";
        m_strChatContent[2] = "앞에 가서 주의를 좀 끌어 줘\n내가 그래도 활솜씨는 좋아서 말이야\n\n";
        m_strChatContent[3] = "확실히 서포트 해줄 테니깐! 뒤는 맡겨둬\n\n\n";

        m_chatLabel = transform.Find("ChatLabel").GetComponent<UILabel>();
        m_chatLabel.text = m_strChatContent[m_strCurChatCount++];
    }

    public void nextChat()
    {
        if (m_strCurChatCount < m_strMaxChatCount)                   // 현재 채팅카운트가 최대 카운트보다 작으면
        {
            m_chatLabel.text = m_strChatContent[m_strCurChatCount];   // 채팅 출력
            m_strCurChatCount++;
        }
        else
            GameManager.instance.endNPCChat();
    }
}
