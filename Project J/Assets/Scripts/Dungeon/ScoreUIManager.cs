using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreUIManager : Singleton<ScoreUIManager>
{
    UIPanel m_scoreUI;          // 스코어 UI 패널
    UILabel m_curComboLabel;    // 현재 콤보 횟수
    UILabel m_rankLabel;       // 등급 
    UILabel m_scoreLabel;       // 점수
    UILabel m_maxComboLabel;    // 최대 콤보 횟수

    bool m_bRankChangeFlag = false;         // 랭크 등급이 변하였을 경우에의 플래그

    void Awake()
    {
        // 멤버변수와 컴포넌트를 연결
        m_scoreUI = GetComponent<UIPanel>();
        m_curComboLabel = transform.Find("CurComboCount").GetComponent<UILabel>();
        m_curComboLabel.text = GameManager.instance.m_iCurComboCount.ToString();
        m_rankLabel = transform.Find("RankCount").GetComponent<UILabel>();
        m_rankLabel.text = GameManager.instance.m_strRank;
        m_scoreLabel = transform.Find("ScoreCount").GetComponent<UILabel>();
        m_scoreLabel.text = GameManager.instance.m_iScore.ToString();
        m_maxComboLabel = transform.Find("MaxComboCount").GetComponent<UILabel>();
        m_maxComboLabel.text = GameManager.instance.m_iMaxComboCount.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void replaceData()   // 콤보 카운트가 올라갈때마다 텍스트 내용 갱신
    {
        int score = GameManager.instance.m_iScore;                  // 스코어를 받아옴
        int curComboCount = ++GameManager.instance.m_iCurComboCount;   // 콤보카운트를 올림
        int maxComboCount = GameManager.instance.m_iMaxComboCount;

        m_curComboLabel.text = curComboCount.ToString();     //  텍스트갱신
        setRank(score);                                      // 스코어에 따른 등급 계산
        m_scoreLabel.text = score.ToString();                // 스코어 표시                           
        if (maxComboCount < curComboCount)                // 현재 콤보가 최대콤보보다 크면
        {
            m_maxComboLabel.text = m_curComboLabel.text;     // 최대 콤보 텍스트갱신
            GameManager.instance.m_iMaxComboCount = curComboCount;
        }
    }

    public void setRank(int score) // 스코어를 통해 랭크 등급을 계산한다.
    {
        string rank;
        if (score > 8000)
            rank = "SSS";
        else if (score > 7000)
            rank = "SS";
        else if (score > 6000)
            rank = "S";
        else if (score > 5000)
            rank = "A";
        else if (score > 4000)
            rank = "B";
        else if (score > 3000)
            rank = "C";
        else if (score > 2000)
            rank = "D";
        else if (score > 1000)
            rank = "E";
        else
            rank = "F";
        if (m_rankLabel.text != rank) // 만약에 텍스트 내용이 현재 등급과 다르다면
        {
            m_bRankChangeFlag = true;  // 플래그를 true로 만듬
            m_rankLabel.text = rank;  // 랭크 등급 갱신
            GameManager.instance.m_strRank = rank;  // 게임매니저 갱신
        }
    }

    public void replaceScroeUI()     // 공격이 적중했을 경우 스코어 내용 갱신
    {
        replaceData();               // 스코어 데이터 갱신
        fontAnimation();             // 폰트 애니메이션 실행
    }

    private void fontAnimation()     // 폰트 애니메이션 함수
    {
        m_scoreUI.alpha = 1.0f;                                  // 알파값 맥스로 변경
        m_curComboLabel.fontSize = 180;                          // 폰트 사이즈 키움(기존 60)

        if (IsInvoking("comboFontSizeAnimation") == true)        // 만약 폰트감소 인보크가 작동중이라면
            CancelInvoke("comboFontSizeAnimation");              // 해당 인보크를 취소시킴
        InvokeRepeating("comboFontSizeAnimation", 0.0f, 0.033f); // 폰트 사이즈 애니메이션 실행

        if (m_bRankChangeFlag == true)                               // 랭크 등급이 바뀌었으면
        {
            m_rankLabel.fontSize = 128;                              // 폰트 사이즈 키움(기존 32)
            if (IsInvoking("rankFontSizeAnimation") == true)        // 만약 폰트감소 인보크가 작동중이라면
                CancelInvoke("rankFontSizeAnimation");              // 해당 인보크를 취소시킴
            InvokeRepeating("rankFontSizeAnimation", 0.0f, 0.033f); // 폰트 사이즈 애니메이션 실행
            m_bRankChangeFlag = false;                              // 플래그 초기화
        }

        if (IsInvoking("fadeScoreUI") == true)    // 만약 알파감소 인보크가 작동중이라면
            CancelInvoke("fadeScoreUI");          // 해당 인보크를 취소시킴
        Invoke("fadeScoreUI", 3.0f);              // 3초 이후부터는 알파값이 낮아지는 인보크
    }

    public void fadeScoreUI()         // 스코어의 알파값을 서서히 낮춘다.
    {
        m_scoreUI.alpha = 0.3f;
    }

    public void comboFontSizeAnimation()         // 콤보시마다 스케일을 조절하는 애니메이션
    {
        m_curComboLabel.fontSize -= 20;        // 폰트 사이즈 감소
        if (m_curComboLabel.fontSize <= 60)    // 폰트 사이즈가 원상복귀되었으면
            CancelInvoke("comboFontSizeAnimation");    // 인보크 종료
    }

    public void rankFontSizeAnimation()         // 콤보시마다 스케일을 조절하는 애니메이션
    {
        m_rankLabel.fontSize -= 3;        // 폰트 사이즈 감소
        if (m_rankLabel.fontSize <= 32)    // 폰트 사이즈가 원상복귀되었으면
            CancelInvoke("rankFontSizeAnimation");    // 인보크 종료
    }
}
