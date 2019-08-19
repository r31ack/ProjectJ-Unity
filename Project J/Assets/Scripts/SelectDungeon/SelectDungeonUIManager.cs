using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectDungeonUIManager : MonoBehaviour
{
    UISprite m_stageSelectSprite;       // 스테이지 선택시 따라다니는 표시 스프라이트
    UIButton[] m_stageButton = new UIButton[5];           // 스테이지별 버튼 갯수
    UILabel m_dungeonInfoLabel;                         // 던전 설명 레이블
    UILabel m_dungeonMonsterInfoLabel;                  // 던전 몬스터 설명 레이블
    UITexture m_bossImageTexture;                       // 해당 던전의 보스이미지 텍스처

    int m_iClearDungeonStage;
    int m_iSelectDungeonStage;


    private void Awake()
    {
        m_stageSelectSprite = GameObject.Find("CurStage").GetComponent<UISprite>();  // UISprite 컴포넌트 동기화

        for (int i=0; i<5; i++)
        {
            m_stageButton[i] = GameObject.Find("Stage"+i).GetComponent<UIButton>();
        }
        m_iClearDungeonStage = GameManager.instance.m_iClearDungeon;
        m_iSelectDungeonStage = m_iClearDungeonStage;  // 초기 위치 표시는 클리어한 던전기준

        m_stageSelectSprite.transform.position = m_stageButton[m_iSelectDungeonStage].transform.position + new Vector3(0, 0.2f, 0);  // 현재 스테이지 위치 좌표 동기화

        m_dungeonInfoLabel = GameObject.Find("DungeonLable").GetComponent<UILabel>();
        m_bossImageTexture = GameObject.Find("SelectDungeonInfo").transform.Find("BossImage").GetComponent<UITexture>();
        m_dungeonMonsterInfoLabel = GameObject.Find("MonstarInfo").GetComponent<UILabel>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) == true)
        {
            for(int i=0; i<5; i++)
            {
                if (m_stageButton[i].state == UIButtonColor.State.Pressed)
                {
                    if(i <= m_iClearDungeonStage + 1)           // 내가 선택한 던전이 클리어 던전의 다음 던전보다 낮으면 
                    {
                        m_iSelectDungeonStage = i;                              // 해당 인덱스 선택
                        m_stageSelectSprite.transform.position = m_stageButton[i].transform.position + new Vector3(0, 0.2f, 0); // 선택 표시좌표 동기화
                        m_bossImageTexture.gameObject.SetActive(true);
                        ShowStageInfo(i, true);
                    }
                    else
                    {
                        m_bossImageTexture.gameObject.SetActive(false);
                        ShowStageInfo(i, false);
                    }
                    break;                                                      // 더이상 순회할 필요가 없으므로 for문 중지
                }
            }
        }
    }

    void ShowStageInfo(int stage, bool enterPossibleCheck)   // 스테이지 정보와 입장가능, 불가능에 따른 표시를 나눔
    {
        switch(stage)
        {
            case 0:
                if (enterPossibleCheck == true)
                {
                    m_bossImageTexture.gameObject.SetActive(false);
                    m_dungeonInfoLabel.text = "< 해안가 >\n\n폭풍우에 휩쓸려 어떠한 해안가 근처에 떠내려 오게 되었다.";
                    m_dungeonMonsterInfoLabel.text = "";
                }
                break;
            case 1:
                if (enterPossibleCheck == true)
                {
                    m_bossImageTexture.mainTexture.name = "Minotaur";
                    m_dungeonInfoLabel.text = "< 마을 외각의 숲 >\n\n최근 숲 주변에 늘어난 미노타우르스의 횡포로 마을 사람들이 곤경에 빠져 있다.";
                    m_dungeonMonsterInfoLabel.text = "미노타우르스 (Lv.3)\n미노 킹 (Lv.10)";
                }
                break;
            case 2:
                if (enterPossibleCheck == true)
                {

                }
                else
                {
                    m_dungeonInfoLabel.text = "< 무덤 >\n\nStage1 [마을 외각의 숲]\n클리어 시 개방";
                    m_dungeonMonsterInfoLabel.text = "";
                }
                break;
            case 3:
                if (enterPossibleCheck == true)
                {

                }
                else
                {
                    m_dungeonInfoLabel.text = "< 낡은 성당 >\n\nStage2 [ 무덤 ]\n클리어 시 개방";
                    m_dungeonMonsterInfoLabel.text = "";
                }
                break;
            case 4:
                if (enterPossibleCheck == true)
                {

                }
                else
                {
                    m_dungeonInfoLabel.text = "< 성벽 >\n\nStage3 [ 낡은 성당 ]\n클리어 시 개방";
                    m_dungeonMonsterInfoLabel.text = "";
                }
                break;
        }
    }

    public void enterButton()
    {
        if(m_iSelectDungeonStage != 0)  // 스테이지 선택을 하지 않은것(또는 스타트지점 선택)이 아니면
            GameManager.instance.enterDungeon(m_iSelectDungeonStage);
    }

    public void returnButton()                      // 되돌아가기 버튼
    {
        SceneManager.LoadScene("LobbyScene");
    }
}
