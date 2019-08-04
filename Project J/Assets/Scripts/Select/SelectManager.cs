using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SelectManager : MonoBehaviour
{
    private UITexture[] m_characterTexutre;       // 캐릭터 이미지 텍스쳐       
    private UILabel[] m_characterLabel;           // 생성된 캐릭터 정보 레이블
    private UIButton[] m_characterButton;         // 생성과 시작 할 수 있는 버튼
    private UIButton m_returnButton;              // 돌아가기 버튼
    private Dictionary<int, CreateInfo> m_dicCreateInfo;              // 생성 캐릭터 정보

    void Awake()
    {
        int maxSlotCount = 3;
        m_characterLabel = new UILabel[maxSlotCount];
        m_characterTexutre = new UITexture[maxSlotCount];
        m_characterButton = new UIButton[maxSlotCount];

        for (int i = 0; i < maxSlotCount; i++)
        {
            m_characterTexutre[i] = transform.Find("Panel/CharacterTexture" + i).GetComponent<UITexture>(); // 텍스쳐 컴포넌트 대입
            m_characterButton[i] = m_characterTexutre[i].GetComponent<UIButton>();                          // 텍스쳐 안에 버튼 대입
            m_characterLabel[i] = transform.Find("Panel/CharacterLabel" + i).GetComponent<UILabel>();
        }
        m_returnButton = transform.Find("Panel/ReturnButton").GetComponent<UIButton>();
        GameManager.instance.m_iCreateCharacterIndex = -1;                                                  // -1 : 아무 선택도 하지 않음
    }

    // Start is called before the first frame update
    void Start()
    {
        m_dicCreateInfo = DataManager.instance.loadCreateInfo();             // 데이터매니저에서 캐릭터 생성 정보를 받음

        foreach (KeyValuePair<int, CreateInfo> iterator in m_dicCreateInfo)  // 반복자를 순회하면서
        {
            int slotIndex = iterator.Key;
            string userName = iterator.Value.m_strUserName;                  // 반복자 내의 정보를 임시변수에 대입
            CHARACTER_TYPE characterType = iterator.Value.m_eCharacterType;  // 캐릭터 타입
            string characterName = null;                                     // 캐릭터 이름

            if (characterType == CHARACTER_TYPE.UNITY)
            {
                characterName = "< UNITY-CHAN >";                            // 캐릭터명
                GameObject character = (GameObject)Instantiate(Resources.Load("Prefabs/UnityChan"), new Vector3(-1.2f + 0.85f * slotIndex, -0.4f, -0.2f), Quaternion.AngleAxis(180.0f, new Vector3(0, 1, 0)));   // 180도 회전 생성
                character.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f); // 스케일 0.6으로 변경
                character.GetComponent<UnityChanOperation>().enabled = false;   // 조작 스크립트 비활성화
            }
            else if (characterType == CHARACTER_TYPE.AKAZA)
            {
                characterName = "< AKAZA >";                                 // 캐릭터명
                GameObject character = (GameObject)Instantiate(Resources.Load("Prefabs/Akaza"), new Vector3(-1.2f + 0.85f * slotIndex, -0.4f, -0.2f), Quaternion.AngleAxis(180.0f, new Vector3(0, 1, 0))); // 180도 회전 생성
                character.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);  // 스케일 0.6으로 변경
                character.GetComponent<UnityChanOperation>().enabled = false;  // 조작 스크립트 비활성화
            }
            m_characterLabel[slotIndex].text = characterName + "\n닉네임 : " + userName;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    ////// 이벤트 콜백 함수 //////

    public void enterButton()
    {
        int selectIndexSlot = GameManager.instance.m_iCreateCharacterIndex;
        if (selectIndexSlot != -1)
        {
            GameManager.instance.m_strSelectUserName = m_dicCreateInfo[selectIndexSlot].m_strUserName;  // 로비창에 넘어가기위한 캐릭터 정보 최종설정
            SceneManager.LoadScene("LobbyScene");                                                       // 로비씬을 불러옴
        }
    }

    public void returnButton()                      // 되돌아가기 버튼
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void deleteButton()                      // 캐릭터 삭제 버튼
    {
        if(DataManager.instance.deleteCreateInfo(GameManager.instance.m_iCreateCharacterIndex) == true) // 만약 삭제가 됬으면
            SceneManager.LoadScene("SelectScene");          // 씬 재로딩
    }

    public void selectButton0()                                                 // 선택창0 버튼
    {
        GameManager.instance.m_iCreateCharacterIndex = 0;                       // 게임매니저가 0번 인덱스를 선택했다고 저장
        foreach (KeyValuePair<int, CreateInfo> iterator in m_dicCreateInfo)  // 반복자를 순회하면서
        {
            if (iterator.Key == 0)                               // 슬롯 인덱스 0이 있는 경우엔 리턴한다.
                return;
        }
        SceneManager.LoadScene("CreateScene");                                 // 슬롯 인덱스 0이 없으면 캐릭터 생성창으로 바꾼다.
    }

    public void selectButton1()                                                 // 선택창0 버튼
    {
        GameManager.instance.m_iCreateCharacterIndex = 1;                       // 게임매니저가 0번 인덱스를 선택했다고 저장
        foreach (KeyValuePair<int, CreateInfo> iterator in m_dicCreateInfo)  // 반복자를 순회하면서
        {
            if (iterator.Key == 1)                               // 슬롯 인덱스 1이 있는 경우엔 리턴한다.
                return;
        }
        SceneManager.LoadScene("CreateScene");                                 // 슬롯 인덱스 1이 없으면 캐릭터 생성창으로 바꾼다.
    }

    public void selectButton2()                                                 // 선택창0 버튼
    {
        GameManager.instance.m_iCreateCharacterIndex = 2;                       // 게임매니저가 0번 인덱스를 선택했다고 저장
        foreach (KeyValuePair<int, CreateInfo> iterator in m_dicCreateInfo)  // 반복자를 순회하면서
        {
            if (iterator.Key == 2)                               // 슬롯 인덱스 2이 있는 경우엔 리턴한다.
                return;
        }
        SceneManager.LoadScene("CreateScene");                                 // 슬롯 인덱스 2이 없으면 캐릭터 생성창으로 바꾼다.
    }
}
