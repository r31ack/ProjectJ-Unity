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

public class ShopItemInfo    // 상점에 있는 아이템 정보, key : 아이템이미지이름(string)
{
    public ITEM_TYPE m_eType; // 아이템 종류
    public int m_iBuyGold;   // 구매 가격
}

public class CreateCharacterTable           // 생성한 캐릭터 정보를 담는 테이블
{
    public int Index { get; set; }          // 테이블의 고유번호
    public int CharacterIndex { get; set; } // 캐릭터 구분번호
    public string UserName { get; set; }    // 유저의 닉네임
}

public class ItemInfo             // 아이템의 고유 정보
{
    public string m_strImageName; // 아이템 이미지이름 (Key)
    public string m_strName;      // 아이템 이름
    public string m_strExplain;   // 아이템 설명
    public ITEM_TYPE m_eType;      // 아이템 종류
    public int m_iValue;          // 아이템 타입별 수치 (무기:공격력, 방어구:방어력, 물약:회복력)
    public int m_iBuyGold;        // 구매 가격
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
    private Dictionary<int, CreateCharacterTable> m_dicCreateCharacterTable = new Dictionary<int, CreateCharacterTable>();  // 생성한 캐릭터 정보의 테이블 딕셔너리
    private Dictionary<int, CreateInfo> m_dicCreateInfo = new Dictionary<int, CreateInfo>();                    // 생성 캐릭터 정보
    private Dictionary<string, ItemInfo> m_dicItemInfo = new Dictionary<string, ItemInfo>();                    // 아이템 고유 정보
    private Dictionary<string, InventoryInfo> m_dicInventoryInfo = new Dictionary<string, InventoryInfo>();     // 인벤토리 아이템 정보
    private Dictionary<string, ShopItemInfo> m_dicShopItemInfo = new Dictionary<string, ShopItemInfo>();        // 상점 아이템 정보

    private bool m_bCreateInfoLoadState = false;                                                                // 생성 캐릭터 정보를 로드 했었는지 체크


    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {
    }

    public Dictionary<string, InventoryInfo> loadInventoryInfo()                    // 외부데이터에서 인벤토리 정보를 불러온 후 딕셔너리에 담아 반환
    {
       Debug.Log("JSON 파일 인벤토리 정보 불러오기");
       TextAsset inventoryInfoText = Resources.Load<TextAsset>("JsonDataInfo");     // 텍스트로 Json파일을 불러 옴

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
        return m_dicInventoryInfo;
    }

    public Dictionary<string, ItemInfo> loadItemInfo()
    {
        TextAsset text = Resources.Load<TextAsset>("ItemTable");        // 리소스 로드를 통해 테이블을 로드한다.
        string content = text.text;                                     // content안에는 1줄로 데이터가 쭉 나열되어 있다.
        string[] line = content.Split('\n');                            // string을 '\n' 기준으로 분리해서 line배열에 넣는다.

        for (int i = 2; i < line.Length - 1; i++)      // 0 ~ 1번 라인은 테이블 타입 구분 용도로 사용한다. 2번째 라인부터 라인 갯수만큼 테이블 생성 (마지막NULL 한칸 제외해서 -1라인)
        {
            string[] column = line[i].Split(',');                     // 열의 정보값을 ','로 구분해 column배열에 넣는다. SCV파일은 ,로 구분되어 있으므로
            ItemInfo table = new ItemInfo();                          // SCV순서와 구조체 데이터 형식이 일치하여야 함
            int index = 0;                                            // 0번째 열부터 시작
            table.m_strImageName = column[index++].Replace("\r", ""); // 저장 후 인덱스를 계속 증가시켜 읽는다.
            table.m_strName = column[index++].Replace("\r", ""); // 0
            table.m_strExplain = column[index++].Replace("\r", ""); // 0
            table.m_eType = (ITEM_TYPE)int.Parse(column[index++]);
            table.m_iValue = int.Parse(column[index++]);
            table.m_iBuyGold = int.Parse(column[index++]);     
            m_dicItemInfo.Add(table.m_strImageName, table);          // 딕셔너리에 테이블 생성정보 삽입
        }
        return m_dicItemInfo;
    }

    public void saveInventoryTable()
    {
        Debug.Log("JSON 파일형태로 저장 시작");

        //JsonData Inventory = JsonMapper.ToJson(m_arrLstInventoryInfo);
        //File.WriteAllText(Application.dataPath + "/Resources/Table/InventoryTable.json", m_arrLstInventoryInfo.ToString());
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

    public void addCreateInfo(CHARACTER_TYPE characterType, string userName)
    {
        CreateInfo createinfo = new CreateInfo();
        createinfo.m_eCharacterType = characterType;
        createinfo.m_strUserName = userName;
        m_dicCreateInfo.Add(GameManager.instance.m_iCreateCharacterIndex, createinfo);
        saveCreateInfo();
    }

    public bool deleteCreateInfo(int deleteKey)
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

    public Dictionary<string, ShopItemInfo> loadShopItemInfo()
    {
        Debug.Log("CSV 파일 상점 판매 아이템 정보 불러오기");
        TextAsset text = Resources.Load<TextAsset>("Data/ShopItemInfo");  // 리소스 로드를 통해 테이블을 로드한다.
        string content = text.text;                                      // content안에는 1줄로 데이터가 쭉 나열되어 있다.
        string[] line = content.Split('\n');                             // string을 '\n' 기준으로 분리해서 line배열에 넣는다.
        for (int i = 2; i < line.Length - 1; i++)                        // 0 ~ 1번 라인은 테이블 타입 구분 용도로 사용한다. 2번째 라인부터 라인 갯수만큼 테이블 생성 (마지막NULL 한칸 제외해서 -1라인)
        {
            string[] column = line[i].Split(',');                    // 열의 정보값을 ','로 구분해 column배열에 넣는다. SCV파일은 ,로 구분되어 있으므로
            ShopItemInfo table = new ShopItemInfo();                     // SCV순서와 구조체 데이터 형식이 일치하여야 함
            string key = null;                                       // key값이 될 문자열의 닉네임 보관장소
            int index = 0;                                           // 0번째 열부터 시작

            key = column[index++].Replace("\r", "");
            table.m_eType = (ITEM_TYPE)int.Parse(column[index++]);
            table.m_iBuyGold = int.Parse(column[index++]);
            m_dicShopItemInfo.Add(key, table);
        }
        return m_dicShopItemInfo;
    }
}


