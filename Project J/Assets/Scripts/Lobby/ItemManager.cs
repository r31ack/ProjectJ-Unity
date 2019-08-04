using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_TYPE  // 아이템 종류
{
    WEAPON,    // 무기
    ARMOR,      // 방어구
    POTION,    // 포션
}

public class ItemManager : MonoBehaviour
{
    private Dictionary<string, ItemInfo> m_dicItemInfo = new Dictionary<string, ItemInfo>(); // 아이템 고유 정보를 가진 딕셔너리
    private Dictionary<string, ShopItemInfo> m_dicIShoptemInfo = new Dictionary<string, ShopItemInfo>(); // 상점 아이템의 정보를 가진 딕셔너리
    private LinkedList<UIButton> m_lnkLstInventoryItemButton = new LinkedList<UIButton>();        // 인벤토리 아이템의 버튼 자료형을 모아둔 링크드리스트
    private Dictionary<string, UIButton> m_dicShopItemButton = new Dictionary<string, UIButton>(); // 상점 아이템의 버튼 자료형을 모아둔 딕셔너리
    private UISprite m_explainItem;                                                          // 아이템 설명 오브젝트
    private UILabel m_explainLabel;                                                          // 아이템 설명 레이블
    private ITEM_TYPE m_eShopItemType;                                                        // 상점에서 클릭한 아이템 타입

    private void Awake()
    {
        loadInventoryItem();                                                                       // 인벤토리 데이터를 불러와 슬롯에 넣는다
        loadShopItem();                                                                            // 상점 데이터를 불러와 슬롯에 넣는다.
        m_dicItemInfo = DataManager.instance.loadItemInfo();                                       // 아이템 정보를 가지고있는 딕셔너리를 받아옴
        m_explainItem = GameObject.Find("ExplainPanel").transform.Find("ItemExplainBone").GetComponent<UISprite>();  // 아이템 클릭시 아이템 설명 스프라이트이미지
        m_explainLabel = m_explainItem.transform.Find("ItemExplainLabel").GetComponent<UILabel>();                   // 아이템 클릭시 아이템 설명 레이블
    }

    private void Start()
    {
        showShopItem(ITEM_TYPE.WEAPON);      // 처음에는 무기 타입의 아이템을 상점에서 보여준다.
    }

    private void Update()
    {
        explainItem();
    }

    private void explainItem()                         // 아이템 설명창
    {
        if (Input.GetMouseButtonDown(0) == true)                // 마우스를 클릭하면
        {
            foreach (UIButton iterator in m_lnkLstInventoryItemButton) // 버튼 반복자로 링크드리스트를 순회
            {
                if (iterator.state == UIButtonColor.State.Pressed)     // 눌러저 있는 버튼을 찾은 경우
                {
                    ItemInfo iteminfo = m_dicItemInfo[iterator.normalSprite]; // 눌러진 아이템 이미지에 맞는 정보를 찾음
                    if (iteminfo != null)                                     // 정보가 있다면
                    {
                        m_explainItem.gameObject.SetActive(true);             // 아이템 설명창 상태 활성화
                        string typeExplain = null;
                        m_explainItem.transform.position = iterator.transform.position + new Vector3(0.3f, -0.3f, 0); // 아이템 설명창 위치 설정

                        if (iteminfo.m_eType == ITEM_TYPE.WEAPON)                                                      // 아이템 타입에 따라 설명을 다르게 함
                            typeExplain = "\n타입 : 무기\n공격력 : " + iteminfo.m_iValue;
                        else if (iteminfo.m_eType == ITEM_TYPE.ARMOR)
                            typeExplain = "\n타입 : 방어구\n방어력 : " + iteminfo.m_iValue;
                        else if (iteminfo.m_eType == ITEM_TYPE.POTION)
                            typeExplain = "\n타입 : 물약\n회복력 : " + iteminfo.m_iValue;
                        m_explainLabel.text = "이름 : " + iteminfo.m_strName + typeExplain + "\n설명 : " + iteminfo.m_strExplain +
                            "\n구매가격 : " + iteminfo.m_iBuyGold;
                        break;                          // 아이템 설명창은 1개면 충분하므로 리스트 순회를 중단
                    }
                }
            }
            foreach (KeyValuePair<string, UIButton> iterator in m_dicShopItemButton)  // 반복자로 딕셔너리를 순회하면서
            {
                if (iterator.Value.state == UIButtonColor.State.Pressed)     // 눌러저 있는 버튼을 찾은 경우
                {
                    ItemInfo iteminfo = m_dicItemInfo[iterator.Value.normalSprite]; // 눌러진 아이템 이미지에 맞는 정보를 찾음
                    if (iteminfo != null)                                     // 정보가 있다면
                    {
                        m_explainItem.gameObject.SetActive(true);             // 아이템 설명창 상태 활성화
                        string typeExplain = null;
                        m_explainItem.transform.position = iterator.Value.transform.position + new Vector3(0.3f, -0.3f, 0); // 아이템 설명창 위치 설정

                        if (iteminfo.m_eType == ITEM_TYPE.WEAPON)                                                      // 아이템 타입에 따라 설명을 다르게 함
                            typeExplain = "\n타입 : 무기\n공격력 : " + iteminfo.m_iValue;
                        else if (iteminfo.m_eType == ITEM_TYPE.ARMOR)
                            typeExplain = "\n타입 : 방어구\n방어력 : " + iteminfo.m_iValue;
                        else if (iteminfo.m_eType == ITEM_TYPE.POTION)
                            typeExplain = "\n타입 : 물약\n회복력 : " + iteminfo.m_iValue;
                        m_explainLabel.text = "이름 : " + iteminfo.m_strName + typeExplain + "\n설명 : " + iteminfo.m_strExplain +
                            "\n구매가격 : " + iteminfo.m_iBuyGold;
                        break;                          // 아이템 설명창은 1개면 충분하므로 리스트 순회를 중단
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) == true)     // 마우스를 뗀 경우
            m_explainItem.gameObject.SetActive(false);  // 설명창 비활성화
    }

    private void loadInventoryItem() // 인벤토리 데이터를 불러온 후 해당 데이터를 기반으로 아이템을 생성하고 슬롯에 넣는다.
    {
        Dictionary<string, InventoryInfo> dicInventoryInfo = DataManager.instance.loadInventoryInfo(); // 외부데이터에서 인벤토리 정보를 불러온 후 딕셔너리에 담아 반환
        foreach (KeyValuePair<string, InventoryInfo> iterator in dicInventoryInfo)             // 반복자로 딕셔너리를 순회하면서
        {
            GameObject itemPrefab = (GameObject)Instantiate(Resources.Load("Prefabs/Item"));   // 아이템 오브젝트를 프리팹으로 생성
            UIButton itemButton = itemPrefab.GetComponent<UIButton>();                         // 프리팹으로부터 UI버튼 정보를 받음

            itemButton.transform.parent = transform.Find("InventoryUI/InventorySlot/InventorySlot" + iterator.Value.m_iSlotRow + iterator.Value.m_iSlotCol).transform; // 아이템 부모(슬롯)을 지정함
            itemButton.normalSprite = iterator.Key;                                                                                           // 프리팹 버튼 이미지 갱신
            itemButton.transform.localScale = Vector3.one;                                                                                    // 스케일 1로 변경
            m_lnkLstInventoryItemButton.AddLast(itemButton);                                                                                  // 링크드 리스트에 버튼정보 삽입
        }
    }

    private void loadShopItem() // 상점 아이템 데이터를 불러온 후 해당 데이터를 기반으로 아이템을 생성하고 슬롯에 넣는다.
    {
        m_dicIShoptemInfo = DataManager.instance.loadShopItemInfo(); // 외부데이터에서 상점 아이템 정보를 불러온 후 딕셔너리에 담아 반환
        foreach (KeyValuePair<string, ShopItemInfo> iterator in m_dicIShoptemInfo)              // 반복자로 딕셔너리를 순회하면서
        {
            GameObject itemPrefab = (GameObject)Instantiate(Resources.Load("Prefabs/ShopItem"));   // 아이템 오브젝트를 프리팹으로 생성
            UIButton itemButton = itemPrefab.GetComponent<UIButton>();                         // 프리팹으로부터 UI버튼 정보를 받음

            itemButton.transform.parent = transform.Find("ShopUI/ShopItemSlot").transform;   // 아이템 부모(슬롯)을 지정함
            itemButton.normalSprite = iterator.Key;                                          // 프리팹 버튼 이미지 갱신
            itemButton.transform.localScale = Vector3.one;                                   // 스케일 1로 변경
            m_dicShopItemButton.Add(iterator.Key, itemButton);                                // 딕셔너리에 버튼 정보 삽입
        }
    }

    private void showShopItem(ITEM_TYPE itemTpye)    // 아이템 타입을 지정해 상점에 배치한다.
    {
        int slotIndex = 0;                          // 슬롯 인덱스는 0부터 시작

        foreach (KeyValuePair<string, ShopItemInfo> iterator in m_dicIShoptemInfo)  // 반복자로 딕셔너리를 순회하면서
        {
            if (itemTpye == iterator.Value.m_eType)                             // 타입이 일치하면
            {
                m_dicShopItemButton[iterator.Key].gameObject.SetActive(true);   // 활성화
                m_dicShopItemButton[iterator.Key].transform.parent = transform.Find("ShopUI/ShopItemSlot/ShopItemSlot" + slotIndex++).transform;  // 부모 슬롯 위치를 순차적으로 배치
                m_dicShopItemButton[iterator.Key].transform.position = m_dicShopItemButton[iterator.Key].transform.parent.position;               // 부모 슬롯 위치와 포지션 동기화
            }
            else                                                                // 타입이 일치하지 않으면
                m_dicShopItemButton[iterator.Key].gameObject.SetActive(false);  // 비활성화
        }
    }

    public int getBuyGold(string itemName)   // 해당 아이템 구매 가격을 반환함
    {
        return m_dicItemInfo[itemName].m_iBuyGold;
    }

    public void weaponTypeButtonClick()
    {
        showShopItem(ITEM_TYPE.WEAPON);
    }

    public void armorTypeButtonClick()
    {
        showShopItem(ITEM_TYPE.ARMOR);
    }

    public void potionTypeButtonClick()
    {
        showShopItem(ITEM_TYPE.POTION);
    }       
}
