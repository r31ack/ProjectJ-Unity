using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ITEN_INDEX
{
    ITEM_INDEX_SWORD
}

public class DefaultCharacterInfo  // 기본 캐릭터 스텟과 상승스텟 관련 정보 (Key : 캐릭터 타입(enum))
{
    public int m_iMaxExp;       // 다음 레벨에 도달하기까지의 경험치
    public int m_iMaxExpUp;     // 레벨업시의 경험치 한도 상승 수치
    public int m_iMaxHp;        // 1레벨의 생명력
    public int m_iMaxHpUp;      // 레벨업시의 최대 생명력 상승량
    public int m_iStr;          // 1레벨의 공격력  
    public int m_iStrUp;        // 레벨업시의 공격력 상승량   
    public int m_iDef;          // 1레벨 방어력
    public int m_iDefUp;        // 레벨업시의 방어력 상승량
}

public class DefaultItemInfo             // 아이템의 고유 정보 (key : 아이템 이미지 이름(string)))
{
    public string m_strName;      // 아이템 이름
    public string m_strExplain;   // 아이템 설명
    public ITEM_TYPE m_eType;      // 아이템 종류
    public int m_iValue;          // 아이템 타입별 수치 (무기:공격력, 방어구:방어력, 물약:회복력)
    public int m_iBuyGold;        // 구매 가격
}

public class ShopItemInfo    // 상점에 있는 아이템 정보, key : 아이템이미지이름(string)
{
    public ITEM_TYPE m_eType; // 아이템 종류
    public int m_iBuyGold;   // 구매 가격
}

public class DefaultDataManager : MonoSingleton<DefaultDataManager> // 다시 내보내기 할 필요가 없는 디폴트 정보를 담은 데이터 Load파일 관리 클래스
{
    private Dictionary<CHARACTER_TYPE, DefaultCharacterInfo> m_dicDefaultCharacterInfo = new Dictionary<CHARACTER_TYPE, DefaultCharacterInfo>();   // 기본 캐릭터 스텟과 상승스텟 관련 정보
    private Dictionary<string, ShopItemInfo> m_dicShopItemInfo = new Dictionary<string, ShopItemInfo>();        // 상점 아이템 정보
    public Dictionary<string, DefaultItemInfo> m_dicDefaultItemInfo = new Dictionary<string, DefaultItemInfo>();                    // 아이템 고유 정보
    private bool m_bloadShopItemInfoState = false;
    private bool m_bloadItemInfoState = false;
    private bool m_bloadDefulatCharacterInfoState = false;

    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Dictionary<CHARACTER_TYPE, DefaultCharacterInfo> loadDefaultCharacterInfo() // SCV파일로된 캐릭터 타입별 디폴트 스텟정보를 로드해 반환
    {
        Debug.Log("캐릭터 디폴트 정보 불러오기");
        if (m_bloadDefulatCharacterInfoState == false)
        {
            Debug.Log("CSV 파일에서 캐릭터 디폴트 정보 불러오기");
            TextAsset text = Resources.Load<TextAsset>("Data/DefaultCharacterInfo"); // 리소스 로드를 통해 테이블을 로드한다.
            string content = text.text;                                    // content안에는 1줄로 데이터가 쭉 나열되어 있다.
            string[] line = content.Split('\n');                           // string을 '\n' 기준으로 분리해서 line배열에 넣는다.
            for (int i = 2; i < line.Length - 1; i++)                      // 0 ~ 1번 라인은 테이블 타입 구분 용도로 사용한다. 2번째 라인부터 라인 갯수만큼 테이블 생성 (마지막NULL 한칸 제외해서 -1라인)
            {
                string[] column = line[i].Split(',');                      // 열의 정보값을 ','로 구분해 column배열에 넣는다. SCV파일은 ,로 구분되어 있으므로
                DefaultCharacterInfo table = new DefaultCharacterInfo();   // SCV순서와 구조체 데이터 형식이 일치하여야 함
                CHARACTER_TYPE key = CHARACTER_TYPE.NONE;                  // key값이 될 캐릭터 종류
                int index = 0;                                             // 0번째 열부터 시작

                key = (CHARACTER_TYPE)int.Parse(column[index++]);          // 첫번째 값을 정수형으로 받은 후 enum으로 전환해 key에 대입
                table.m_iMaxExp = int.Parse(column[index++]);
                table.m_iMaxExpUp = int.Parse(column[index++]);
                table.m_iMaxHp = int.Parse(column[index++]);
                table.m_iMaxHpUp = int.Parse(column[index++]);
                table.m_iStr = int.Parse(column[index++]);
                table.m_iStrUp = int.Parse(column[index++]);
                table.m_iDef = int.Parse(column[index++]);
                table.m_iDefUp = int.Parse(column[index++]);
                m_dicDefaultCharacterInfo.Add(key, table);
            }
            m_bloadDefulatCharacterInfoState = true;
        }
        Debug.Log("캐릭터 디폴트 정보 불러오기 완료");
        return m_dicDefaultCharacterInfo;
    }

    public Dictionary<string, DefaultItemInfo> loadDefaultItemInfo()
    {
        if (m_bloadItemInfoState == false)
        {
            TextAsset text = Resources.Load<TextAsset>("Data/DefaultItemInfo");        // 리소스 로드를 통해 테이블을 로드한다.
            string content = text.text;                                     // content안에는 1줄로 데이터가 쭉 나열되어 있다.
            string[] line = content.Split('\n');                            // string을 '\n' 기준으로 분리해서 line배열에 넣는다.

            for (int i = 2; i < line.Length - 1; i++)      // 0 ~ 1번 라인은 테이블 타입 구분 용도로 사용한다. 2번째 라인부터 라인 갯수만큼 테이블 생성 (마지막NULL 한칸 제외해서 -1라인)
            {
                string[] column = line[i].Split(',');                     // 열의 정보값을 ','로 구분해 column배열에 넣는다. SCV파일은 ,로 구분되어 있으므로
                DefaultItemInfo table = new DefaultItemInfo();                          // SCV순서와 구조체 데이터 형식이 일치하여야 함
                int index = 0;                                            // 0번째 열부터 시작

                string itemName = column[index++].Replace("\r", ""); // 저장 후 인덱스를 계속 증가시켜 읽는다.
                table.m_strName = column[index++].Replace("\r", ""); // 0
                table.m_strExplain = column[index++].Replace("\r", ""); // 0
                table.m_eType = (ITEM_TYPE)int.Parse(column[index++]);
                table.m_iValue = int.Parse(column[index++]);
                table.m_iBuyGold = int.Parse(column[index++]);
                m_dicDefaultItemInfo.Add(itemName, table);          // 딕셔너리에 테이블 생성정보 삽입
            }
            m_bloadItemInfoState = true;
        }
        return m_dicDefaultItemInfo;
    }

    public Dictionary<string, ShopItemInfo> loadShopItemInfo()
    {
        if (m_bloadShopItemInfoState == false)                                // 상점 정보 로드 이력이 없으면 CSV파일에서 불러온다.
        {
            Debug.Log("CSV 파일 상점 판매 아이템 정보 불러오기");
            TextAsset text = Resources.Load<TextAsset>("Data/ShopItemInfo");  // 리소스 로드를 통해 테이블을 로드한다.
            string content = text.text;                                      // content안에는 1줄로 데이터가 쭉 나열되어 있다.
            string[] line = content.Split('\n');                             // string을 '\n' 기준으로 분리해서 line배열에 넣는다.
            for (int i = 2; i < line.Length - 1; i++)                        // 0 ~ 1번 라인은 테이블 타입 구분 용도로 사용한다. 2번째 라인부터 라인 갯수만큼 테이블 생성 (마지막NULL 한칸 제외해서 -1라인)
            {
                string[] column = line[i].Split(',');                        // 열의 정보값을 ','로 구분해 column배열에 넣는다. SCV파일은 ,로 구분되어 있으므로
                ShopItemInfo table = new ShopItemInfo();                     // SCV순서와 구조체 데이터 형식이 일치하여야 함
                string key = null;                                           // key값이 될 문자열의 닉네임 보관장소
                int index = 0;                                              // 0번째 열부터 시작

                key = column[index++].Replace("\r", "");
                table.m_eType = (ITEM_TYPE)int.Parse(column[index++]);
                table.m_iBuyGold = int.Parse(column[index++]);
                m_dicShopItemInfo.Add(key, table);
            }
            m_bloadShopItemInfoState = true;                                // 상점 정보를 로드한 상태로 변경
        }
        return m_dicShopItemInfo;                                           // 상점 아이템 정보 반환
    }
}
