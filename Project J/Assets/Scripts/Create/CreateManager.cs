using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateManager : MonoBehaviour
{
    private UITexture m_selectIllustTexture;      // 선택한 캐릭터 일러스트
    private UILabel m_selectIntroduceLabel;       // 선택한 캐릭터의 소개 레이블
    private UILabel m_selectCharacterNameLabel;   // 선택한 캐릭터의 이름 레이블
    private UITexture[] m_characterTexutre;       // 선택 가능 캐릭터 미니아이콘 텍스쳐    
    private UIButton[] m_characterButton;         // 선택 가능 캐릭터의 미니아이콘 버튼
    private UIButton m_returnButton;              // 돌아가기 버튼

    private Canvas m_inputNameWindow;             // 닉네임 생성 창
    private bool m_binputNamWindowFlag = false;   // 닉네임 생성 창 활성화 여부
    private Text m_inputNameText;
    private CHARACTER_TYPE m_eCharacterType = CHARACTER_TYPE.NONE;

    // Start is called before the first frame update
    void Start()
    {
       m_selectIllustTexture = transform.Find("Panel/SelectIllustTexture").GetComponent<UITexture>();
       m_returnButton = transform.Find("Panel/ReturnButton").GetComponent<UIButton>();
       m_selectIntroduceLabel = transform.Find("Panel/SelectIntroduceLabel").GetComponent<UILabel>();       // 선택한 캐릭터의 소개 레이블 동기화
       m_selectCharacterNameLabel = transform.Find("Panel/SelectCharacterNameLabel").GetComponent<UILabel>();
       m_inputNameWindow = transform.Find("InputNameCanvas").GetComponent<Canvas>();
       m_inputNameWindow.gameObject.SetActive(false);          // 종료 창 초기상태 false
       m_inputNameText = m_inputNameWindow.transform.Find("InputField/Text").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Icon1()
    {
        m_eCharacterType = CHARACTER_TYPE.UNITY;
        m_selectIllustTexture.mainTexture = Resources.Load("Texture/UnityChan0", typeof(Texture2D)) as Texture2D;
        m_selectCharacterNameLabel.text = "UNITY-CHAN";
        m_selectIntroduceLabel.text = "클래스 : 격투가\n착용무기 : 너클\n\n빠른 스피드와 접근전에서 강력한 데미지를 주는 근거리 타격 캐릭터이다.";
    }

    public void icon2()
    {
        m_eCharacterType = CHARACTER_TYPE.AKAZA;
        m_selectIllustTexture.mainTexture = Resources.Load("Texture/Akaza0", typeof(Texture2D)) as Texture2D;
        m_selectCharacterNameLabel.text = "AKAZA";
        m_selectIntroduceLabel.text = "클래스 : 마검사\n착용무기 : 장검\n\n장검과 흑마법을 연계한 이용한 넓은 범위의 공격범위를 가지는 캐릭터이다.";
    }

    public void icon3()
    {
        m_eCharacterType = CHARACTER_TYPE.NONE;
        m_selectIllustTexture.mainTexture = null;
    }

    public void icon4()
    {
        m_eCharacterType = CHARACTER_TYPE.NONE;
        m_selectIllustTexture.mainTexture = null;
    }


    public void returnButton()
    {
        SceneManager.LoadScene("SelectScene");
    }

    public void selectFinishButton()       // 선택완료 버튼(닉네임 입력창 활성화)
    {
        if (m_eCharacterType != CHARACTER_TYPE.NONE)
        {
            m_binputNamWindowFlag = true;                                     //
            m_inputNameWindow.gameObject.SetActive(m_binputNamWindowFlag);    //
        }
    }

    public void InputNameFinish()          // 닉네임 입력 완료
    {
        DataManager.instance.addCreateInfo(m_eCharacterType, m_inputNameText.text);
        SceneManager.LoadScene("SelectScene");
        m_binputNamWindowFlag = false;                                    
        m_inputNameWindow.gameObject.SetActive(m_binputNamWindowFlag);

    }

    public void inputNameCancel()          // 닉네임 입력 취소
    {
        m_binputNamWindowFlag = false;                                    
        m_inputNameWindow.gameObject.SetActive(m_binputNamWindowFlag);    
    }
}

