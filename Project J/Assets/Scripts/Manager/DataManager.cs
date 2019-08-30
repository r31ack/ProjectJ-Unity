using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;             // 파일 입출력
using SimpleJSON;            // JSON

public class CreateInfo      // 생성한 캐릭터 정보, key : 슬롯 인덱스(int)
{
    public string         m_strUserName;     // 캐릭터 닉네임
    public CHARACTER_TYPE m_eCharacterType;  // 캐릭터 종류
}

public class DynamicCharacterInfo           // 동적으로 변하는 캐릭터 정보
{
    public string m_strUserName;            // 유저 닉네임
    public CHARACTER_TYPE m_eCharacterType; // 캐릭터 타입
    public int m_iLevel;                    // 레벨
    public int m_iExp;                      // 경험치
    public int m_iJam;                      // 보석
    public int m_iGold;                     // 골드

    public void setDynamicCharacterInfo(string userName, int characterType, int level, int exp, int jam, int gold)  // 매게변수 세팅
    {
        m_strUserName = userName;
        m_eCharacterType = (CHARACTER_TYPE)characterType;
        m_iLevel = level;
        m_iExp = exp;
        m_iJam = jam;
        m_iGold = gold;
    }
}

public class RankingInfo                    // 랭킹 정보 (key : 유저 닉네임)
{
    public string m_strUserName;
    public CHARACTER_TYPE m_eCharacterType; // 캐릭터 타입
    public int m_iScore;
    public float m_fClearTime;

    public RankingInfo()        // 기본 생성자
    {
    }
    public RankingInfo(string userName, int characterType, int score, float clearTime)  // 매게변수 생성자
    {
        m_strUserName = userName;
        m_eCharacterType = (CHARACTER_TYPE)characterType;
        m_iScore = score;
        m_fClearTime = clearTime;
    }
}

public class CreateCharacterTable           // 생성한 캐릭터 정보를 담는 테이블
{
    public int Index { get; set; }          // 테이블의 고유번호
    public int CharacterIndex { get; set; } // 캐릭터 구분번호
    public string UserName { get; set; }    // 유저의 닉네임
}

public class InventoryInfo        // 인벤토리에 있는 아이템 정보
{
    public string m_strImageName; // 아이템 이미지이름 (Key)
    public int    m_iSlotRow;     // 슬롯의 가로 위치
    public int    m_iSlotCol;     // 슬롯의 세로 위치

    public InventoryInfo()        // 기본 생성자
    {
    }        
    public InventoryInfo(string imageName, int slotRow, int slotCol)  // 매게변수 생성자
    {
        m_strImageName = imageName;
        m_iSlotRow = slotRow;
        m_iSlotCol = slotCol;
    }
}

public class DataManager : MonoSingleton<DataManager>
{
    private Dictionary<int, CreateCharacterTable> m_dicCreateCharacterTable = new Dictionary<int, CreateCharacterTable>();  // 생성한 캐릭터 정보의 딕셔너리
    private Dictionary<int, CreateInfo> m_dicCreateInfo = new Dictionary<int, CreateInfo>();                    // 생성 캐릭터 정보
    private DynamicCharacterInfo m_characterInfo = new DynamicCharacterInfo();                                    // 캐릭터 정보
    private Dictionary<string, InventoryInfo> m_dicInventoryInfo = new Dictionary<string, InventoryInfo>();     // 인벤토리 아이템 정보
    private Dictionary<string, RankingInfo> m_dicRankingInfo = new Dictionary<string, RankingInfo>();           // 유저명을 기반으로 랭킹 정보를 가지고 있는 딕셔너리

    private bool m_bCreateInfoLoadState = false;                                                                // 생성 캐릭터 정보를 로드 했었는지 체크
    private bool m_bRankingLoadState = false;                                                                // 랭킹 정보를 로드 했었는지 체크


    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {
    }

    public DynamicCharacterInfo loadDynamicCharacterInfo(int characterIndex)       // Json데이터에서 유저 캐릭터 정보를 불러온 후 클래스 변수에 담아 반환
    {
        Debug.Log("JSON 파일 캐릭터 정보 불러오기");
        TextAsset userInfoText = Resources.Load<TextAsset>("Data/CharacterIndex" + characterIndex);     // 텍스트 에셋으로 Json파일을 불러 옴

        if (userInfoText != null)                                                // 텍스트가 존재한다면
        {
            JSONNode nodeData = JSON.Parse(userInfoText.text) as JSONNode;       // 테이블 형태로 파싱
            if (nodeData != null)                                                // 노드 데이터가 존재한다면
            {
                JSONObject jsonObject = nodeData["CharacterInfo"] as JSONObject;      // 유저 정보를 받음 (유저 정보는 한 라인 밖에 없음)
                m_characterInfo.setDynamicCharacterInfo(jsonObject["userName"], jsonObject["characterType"], jsonObject["level"], jsonObject["exp"], jsonObject["jam"], jsonObject["gold"]);
            }
        }
        Debug.Log("JSON 파일 캐릭터 정보 불러오기 완료");
        return m_characterInfo;
    }

    public Dictionary<string, RankingInfo> loadRankingInfo()       // Json데이터에서 랭킹 정보를 불러온다.
    {
        if (m_bRankingLoadState == false)
        {
            Debug.Log("JSON 파일 랭킹 정보 불러오기");
            TextAsset userInfoText = Resources.Load<TextAsset>("Data/RankingInfo");   // 텍스트 에셋으로 Json파일을 불러 옴

            if (userInfoText != null)                                                // 텍스트가 존재한다면
            {
                JSONNode nodeData = JSON.Parse(userInfoText.text) as JSONNode;       // 테이블 형태로 파싱
                if (nodeData != null)                                                // 노드 데이터가 존재한다면
                {
                    JSONNode dicNode = nodeData["RankingInfo"] as JSONObject;        // 랭킹 정보를 노드로 받음
                    foreach (KeyValuePair<string, JSONNode> iterator in dicNode)     // 테이블을 반복자를 통해 라인별로 순회하면서
                    {
                        JSONObject dicObject = dicNode[iterator.Key] as JSONObject;     // key에 해당하는 오브젝트 정보를 받아옴
                        m_dicRankingInfo.Add(iterator.Key, new RankingInfo(iterator.Key, dicObject["characterType"].AsInt, dicObject["score"].AsInt, dicObject["clearTime"].AsFloat)); // 키,행렬의 정보를 딕셔너리에 삽입
                    }
                }
            }
            m_bRankingLoadState = true;
        }
        Debug.Log("JSON 파일 랭킹 정보 불러오기 완료");
        return m_dicRankingInfo;
    }

    public Dictionary<string, InventoryInfo> loadInventoryInfo(int characterIndex)    // 외부데이터에서 인벤토리 정보를 불러온 후 딕셔너리에 담아 반환
    {
       m_dicInventoryInfo.Clear();                                                    // 딕셔너리 정보를 비우고 시작
       Debug.Log("JSON 파일 인벤토리 정보 불러오기 시작");
       TextAsset inventoryInfoText = Resources.Load<TextAsset>("Data/CharacterIndex" + characterIndex);     // 텍스트로 Json파일을 불러 옴

       if(inventoryInfoText != null)                                                // 텍스트가 존재한다면
       {
            JSONNode nodeData = JSON.Parse(inventoryInfoText.text) as JSONNode;     // 테이블 형태로 파싱
            if (nodeData != null)                                                   // 노드 데이터가 존재한다면
            {
                JSONNode dicNode = nodeData["InventoryInfo"] as JSONNode;           // 인벤토리 정보가 들어잇는 테이블
                foreach (KeyValuePair<string, JSONNode> iterator in dicNode)        // 테이블을 반복자를 통해 라인별로 순회하면서
                {
                    JSONObject dicObject = dicNode[iterator.Key] as JSONObject;     // key에 해당하는 오브젝트 정보를 받아옴
                    m_dicInventoryInfo.Add(iterator.Key, new InventoryInfo(iterator.Key, dicObject["row"], dicObject["col"])); // 키,행렬의 정보를 딕셔너리에 삽입
                }
            }
       }
        Debug.Log("JSON 파일 인벤토리 정보 불러오기 완료");
        return m_dicInventoryInfo;
    }

    public void saveDefaultUserInfo(int characterIndex, string userName, CHARACTER_TYPE characterType) // 캐릭터를 처음 생성한 경우 디폴트 정보를 저장한다.
    {
        m_characterInfo.m_strUserName = userName;              
        m_characterInfo.m_eCharacterType = characterType;
        m_characterInfo.m_iLevel = 1;
        m_characterInfo.m_iExp = 0;
        m_characterInfo.m_iJam = 0;
        m_characterInfo.m_iGold = 1000;                        // 디폴트 세팅 후에
        m_dicInventoryInfo.Add("W_Sword001", new InventoryInfo("W_Sword001",0, 0));   // 기본 제공 무기
        m_dicInventoryInfo.Add("A_Armour01", new InventoryInfo("W_Sword001", 0, 1));  // 기본 제공 방어구
        m_dicInventoryInfo.Add("P_Red01", new InventoryInfo("W_Sword001", 0, 2));  // 기본 제공 포션
        saveUserInfo(characterIndex);                          // Json으로 내보낸다.
    }

    public void saveUserInfo(int characterIndex)  // 유저 개인의 정보를 모두 저장한다.
    {
        Debug.Log("JSON 파일 캐릭터 정보 저장하기 시작");
        JSONNode userInfo = new JSONObject();        // 유저 정보 최상단 노드를 하나 생성한다.                                                계층 1단계 (userInfo)

        JSONNode characterInfo = new JSONObject();          // key 노드를 하나 생성함 (내용 : UserInfo)                                       계층 2단계 (characterInfo)
         
        characterInfo.Add("userName", m_characterInfo.m_strUserName);              // 유저 닉네임 삽입
        characterInfo.Add("characterType", (int)m_characterInfo.m_eCharacterType); // 캐릭터 타입 삽입 enum->int 형변환
        characterInfo.Add("level", m_characterInfo.m_iLevel);                      // 레벨 삽입
        characterInfo.Add("exp", m_characterInfo.m_iExp);                          // 경험치 삽입
        characterInfo.Add("jam", m_characterInfo.m_iJam);                          // 보석 갯수 삽입
        characterInfo.Add("gold", m_characterInfo.m_iGold);                        // 골드 수 삽입

        JSONNode inventoryInfo = new JSONObject();             // key 노드를 하나 생성함 (내용 : InventoryInfo)                               계층 2단계 (inventoryInfo)                                      

        foreach (KeyValuePair<string, InventoryInfo> iterator in m_dicInventoryInfo)  // 테이블을 반복자를 통해 인벤토리 정보를 순회하면서
        {
            JSONNode itemName = new JSONObject();                              // 아이템 이름을 key로 가지는 value Node생성                   계층 3단계 (itemName)
            itemName.Add("row", m_dicInventoryInfo[iterator.Key].m_iSlotRow);  // row 정보 대입
            itemName.Add("col", m_dicInventoryInfo[iterator.Key].m_iSlotCol);  // row 정보 대입
            inventoryInfo.Add(iterator.Key, itemName);                    // 아이템 명, 아이템 슬롯 위치정보 대입
        }
        userInfo.Add("CharacterInfo",characterInfo);    // 최상위 부모에게 조립1
        userInfo.Add("InventoryInfo",inventoryInfo);    // 최상위 부모에게 조립2

        File.WriteAllText(Application.dataPath + "/Resources/Data/CharacterIndex"+characterIndex+".json", userInfo.ToString());    // 데이터 저장
        Debug.Log("JSON 파일 Index : " + characterIndex + " 캐릭터 정보 저장하기 완료");
    }

    public Dictionary<int, CreateInfo> loadCreateInfo() 
    {
        if (m_bCreateInfoLoadState == false)                               // 생성정보를 로드한 이력이 없으면 불러온다.
        {
            Debug.Log("CSV 파일 캐릭터 생성 정보 불러오기");
            TextAsset text = Resources.Load<TextAsset>("Data/CreateInfo"); // 리소스 로드를 통해 테이블을 로드한다.
            string content = text.text;                                    // content안에는 1줄로 데이터가 쭉 나열되어 있다.
            string[] line = content.Split('\n');                           // string을 '\n' 기준으로 분리해서 line배열에 넣는다.
            for (int i = 2; i < line.Length - 1; i++)                      // 0 ~ 1번 라인은 테이블 타입 구분 용도로 사용한다. 2번째 라인부터 라인 갯수만큼 테이블 생성 (마지막NULL 한칸 제외해서 -1라인)
            {
                string[] column = line[i].Split(',');                      // 열의 정보값을 ','로 구분해 column배열에 넣는다. SCV파일은 ,로 구분되어 있으므로
                CreateInfo table = new CreateInfo();                       // SCV순서와 구조체 데이터 형식이 일치하여야 함
                int key = 0;                                               // key값이 될 슬롯 번호
                int index = 0;                                             // 0번째 열부터 시작

                key = int.Parse(column[index++]);
                table.m_strUserName = column[index++].Replace("\r", "");
                table.m_eCharacterType = (CHARACTER_TYPE)(int.Parse(column[index++]));
                m_dicCreateInfo.Add(key, table);
            }
            m_bCreateInfoLoadState = true;                                 // 로드 이력을 true로 변환
        }                                                                  
        return m_dicCreateInfo;                                            // 딕셔너리 반환
    }

    public void addCreateInfo(CHARACTER_TYPE characterType, string userName)      // 생성 정보에 새로 생성한 캐릭터정보를 더한다.
    {
        CreateInfo createinfo = new CreateInfo();
        createinfo.m_eCharacterType = characterType;
        createinfo.m_strUserName = userName;
        m_dicCreateInfo.Add(GameManager.instance.m_iCreateCharacterIndex, createinfo);
        saveCreateInfo();
    }

    public bool equalNameCheck(string userName)    // 동일한 닉네임이 존재하는지 체크 (임시적인 JSON으로 다른 유저명을 불러와 체크함)
    {
        loadRankingInfo();
        foreach (var iterator in m_dicRankingInfo)
        {
            if (userName == iterator.Key)
                return true;
        }
        return false;
    }

    public bool deleteCreateInfo(int deleteKey)         // 생성 정보에서 선택한 캐릭터를 삭제한다.
    {
        if (m_dicCreateInfo.Remove(deleteKey) == true) // 선택한 슬롯 인덱스 삭제가 완료됫으면
        {
            DataManager.instance.saveCreateInfo();     // 생성 파일 정보를 저장
            return true;                               // true반환      
        }
        return false;                                  // false반환
    }

    public void saveCreateInfo()    // 캐릭터 생성 정보를 저장한다.
    {
        Debug.Log("CSV 파일 캐릭터 생성 정보 저장");
        StreamWriter sw = System.IO.File.CreateText(Application.dataPath + "/Resources/Data/CreateInfo.csv");
        sw.WriteLine("int,string,int");
        sw.WriteLine("slotIndex,userName,characterType");

        int line = m_dicCreateCharacterTable.Count;
        foreach (KeyValuePair<int, CreateInfo> iterator in m_dicCreateInfo)
            sw.WriteLine(iterator.Key + "," + iterator.Value.m_strUserName + "," + ((int)(iterator.Value.m_eCharacterType)).ToString());
        sw.Close();
    }
}


