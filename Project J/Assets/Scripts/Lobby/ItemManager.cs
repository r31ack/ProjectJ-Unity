﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ITEM_TYPE  // 아이템 종류
{
    WEAPON,    // 무기
    ARMOR,      // 방어구
    POTION,    // 포션
}

public class ItemManager : MonoBehaviour
{
    private Dictionary<string, DefaultItemInfo> m_dicDefaultItemInfo = new Dictionary<string, DefaultItemInfo>(); // 아이템 고유 정보를 가진 딕셔너리
    private Dictionary<string, ShopItemInfo> m_dicIShoptemInfo = new Dictionary<string, ShopItemInfo>(); // 상점 아이템의 정보를 가진 딕셔너리
    private LinkedList<UIButton> m_lnkLstInventoryItemButton = new LinkedList<UIButton>();        // 인벤토리 아이템의 버튼 자료형을 모아둔 링크드리스트
    private Dictionary<string, UIButton> m_dicShopItemButton = new Dictionary<string, UIButton>(); // 상점 아이템의 버튼 자료형을 모아둔 딕셔너리
    private UISprite m_explainItem;                                                          // 아이템 설명 오브젝트
    private UILabel m_explainLabel;                                                          // 아이템 설명 레이블
    private ITEM_TYPE m_eShopItemType;                                                        // 상점에서 클릭한 아이템 타입

    // 특정 아이템을 선택했을때 필요한 변수
    private UIButton m_clickShopItem = null;                                              // 클릭한 상점 아이템의 버튼
    private UIButton m_clickInventoryItem = null;                                         // 클릭한 인벤토리 아이템의 버튼
    private GameObject m_clickShopItemMark;                                               // 아이템 클릭 표시용 스프라이트 오브젝트         
    private GameObject m_clickInventoryItemMark;                                          // 아이템 클릭 표시용 스프라이트 오브젝트 

    // 장착 타입별 슬롯
    private GameObject m_infomationUI;        // 인포메이션 UI (인포메이션 창의 활성화여부에 따라 업데이트를 추가 체크)
    private GameObject m_weaponSlot;
    private GameObject m_armorSlot;
    private GameObject m_potionSlot;
    private UILabel m_dynamicButtonLabel;                  // 열은 창에 맞게 착용하기, 버리기, 판매하기 등으로 바뀌는 기능

    private void Awake()
    {
        loadInventoryItem();                                                                       // 인벤토리 데이터를 불러와 슬롯에 넣는다
        loadShopItem();                                                                            // 상점 데이터를 불러와 슬롯에 넣는다.
        m_dicDefaultItemInfo = DefaultDataManager.instance.loadDefaultItemInfo();                      // 아이템 정보를 가지고있는 딕셔너리를 받아옴
        m_explainItem = transform.Find("ItemExplainBone").GetComponent<UISprite>();                    // 아이템 클릭시 아이템 설명 스프라이트이미지
        m_explainLabel = m_explainItem.transform.Find("ItemExplainLabel").GetComponent<UILabel>();     // 아이템 클릭시 아이템 설명 레이블

        m_infomationUI = transform.Find("InfomationUI").gameObject;
        m_armorSlot = m_infomationUI.transform.Find("WearSlot/ArmorSlot").gameObject;
        m_weaponSlot = m_infomationUI.transform.Find("WearSlot/WeaponSlot").gameObject;
        m_potionSlot = m_infomationUI.transform.Find("WearSlot/PotionSlot").gameObject;

        m_clickShopItemMark = transform.Find("ShopUI/ClickItemMark").gameObject;
        m_clickInventoryItemMark = transform.Find("InventoryUI/ClickItemMark").gameObject;
        m_dynamicButtonLabel = transform.Find("InventoryUI/DynamicLabel").GetComponent<UILabel>();
    }

    private void Start()
    {
        showShopItem(ITEM_TYPE.WEAPON);      // 처음에는 무기 타입의 아이템을 상점에서 보여준다.
    }

    private void Update()
    {
        if (m_infomationUI.activeSelf == true)  // 인포메이션 UI가 활성화되어 있는 경우
            ItemWear();                         // 아이템을 입엇는지 확인
        explainItem();      // 설명창 활성화 확인
        ItemClick();

        if(Input.GetKeyDown(KeyCode.F12))   // 임시 저장
        {
            DataManager.instance.saveUserInfo(GameManager.instance.m_iSelectCharacterIndex);
        }
    }

    private void ItemWear()
    {
        if (Input.GetMouseButtonUp(0) == true)  // 마우스가 내려간게 체크되면
        {
            if (m_armorSlot.transform.childCount != 0)  // 자식 갯수가 0이 아니면
            {
                CharacterInfoManager.instance.m_characterInfo.m_iArmorDef = getValue(m_armorSlot.transform.GetChild(0).GetComponent<UIButton>().normalSprite);
                CharacterInfoManager.instance.replaceStr();
                ProfileUIManager.Instance.changeStatus();       // 스텟 변화 적용
            }
            else
            {
                CharacterInfoManager.instance.m_characterInfo.m_iArmorDef = 0;
                CharacterInfoManager.instance.replaceStr();
                ProfileUIManager.Instance.changeStatus();       // 스텟 변화 적용
            }
            if (m_weaponSlot.transform.childCount != 0)  // 자식 갯수가 0이 아니면
            {
                CharacterInfoManager.instance.m_characterInfo.m_iWeaponStr = getValue(m_weaponSlot.transform.GetChild(0).GetComponent<UIButton>().normalSprite);
                CharacterInfoManager.instance.replaceStr();
                ProfileUIManager.Instance.changeStatus();       // 스텟 변화 적용
            }
            else
            {
                CharacterInfoManager.instance.m_characterInfo.m_iWeaponStr = 0;
                CharacterInfoManager.instance.replaceStr();
                ProfileUIManager.Instance.changeStatus();       // 스텟 변화 적용
            }
        }
    }

    private void explainItem()                         // 아이템 설명창
    {
        if (Input.GetMouseButtonDown(0) == true)                // 마우스를 클릭하면
        {
            foreach (UIButton iterator in m_lnkLstInventoryItemButton) // 버튼 반복자로 링크드리스트를 순회
            {
                if (iterator.state == UIButtonColor.State.Pressed)     // 눌러저 있는 버튼을 찾은 경우
                {
                    DefaultItemInfo iteminfo = m_dicDefaultItemInfo[iterator.normalSprite]; // 눌러진 아이템 이미지에 맞는 정보를 찾음
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
                    DefaultItemInfo iteminfo = m_dicDefaultItemInfo[iterator.Value.normalSprite]; // 눌러진 아이템 이미지에 맞는 정보를 찾음
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

    private void ItemClick()      // 아이템을 클릭
    {
        if (Input.GetMouseButtonDown(0) == true)                // 마우스를 클릭하면
        {
            foreach (KeyValuePair<string, UIButton> iterator in m_dicShopItemButton)  // 반복자로 상점 아이템의 딕셔너리를 순회하면서
            {
                if (iterator.Value.state == UIButtonColor.State.Pressed)     // 눌러저 있는 버튼을 찾은 경우
                {
                    m_clickShopItemMark.SetActive(true);
                    m_clickShopItem = iterator.Value;                        // 클릭 상점 아이템에 변수에 대입
                    m_clickShopItemMark.transform.position = iterator.Value.transform.position; // 위치 동기화  
                }
            }
            foreach (UIButton iterator in m_lnkLstInventoryItemButton) // 반복자로 인벤토리 아이템의 딕셔너리를 순회
            {
                if (iterator.state == UIButtonColor.State.Pressed)     // 눌러저 있는 버튼을 찾은 경우
                {
                    m_clickInventoryItemMark.SetActive(true);
                    m_clickInventoryItem = iterator;                        // 클릭 상점 아이템에 변수에 대입
                    m_clickInventoryItemMark.transform.position = iterator.transform.position; // 위치 동기화  
                }
            }
        }

        if (m_clickInventoryItem == null)                                                          // 클릭한 아이템이 없으면
            m_clickInventoryItemMark.SetActive(false);                                             // 마크 비활성화
        else                                                                                       // 클릭한 아이템이 있으면
        {
            m_clickInventoryItemMark.transform.position = m_clickInventoryItem.transform.position; // 마크의 위치는 해당 아이템의 위치
        }
    }
    private void loadInventoryItem() // 인벤토리 데이터를 불러온 후 해당 데이터를 기반으로 아이템을 생성하고 슬롯에 넣는다.
    {
        Dictionary<string, InventoryInfo> dicInventoryInfo = DataManager.instance.loadInventoryInfo(GameManager.instance.m_iSelectCharacterIndex); // 외부데이터에서 인벤토리 정보를 불러온 후 딕셔너리에 담아 반환
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
        m_dicIShoptemInfo = DefaultDataManager.instance.loadShopItemInfo(); // 외부데이터에서 상점 아이템 정보를 불러온 후 딕셔너리에 담아 반환
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
        return m_dicDefaultItemInfo[itemName].m_iBuyGold;
    }

    public int getValue(string itemName)    // 해당 아이템의 능력 수치를 반환함
    {
        return m_dicDefaultItemInfo[itemName].m_iValue;
    }

    public ITEM_TYPE getItemType(string itemName) // 해당 아이템의 타입을 반한홤
    {
        return m_dicDefaultItemInfo[itemName].m_eType;
    }

    public void buyButtonClick()
    {
        if (m_clickShopItem != null)     // 클릭한 상점 아이템이 있다면
        {
            int buyGold = m_dicDefaultItemInfo[m_clickShopItem.normalSprite].m_iBuyGold;           // 상점 아이템의 구매 가격

            if (CharacterInfoManager.instance.m_characterInfo.m_iGold > buyGold)                   // 내가 가진 돈이 구매가격보다 크면
            {
                CharacterInfoManager.instance.m_characterInfo.m_iGold -= buyGold;                  // 내 골드 정보에서 구매 가격을 뺀다.
                putInventroyItem(m_clickShopItem.normalSprite);                                    // 클릭한 상점 아이템을 인벤토리에 넣는다.
                ProfileUIManager.Instance.changeGold();                                                    // 구매할때의 골드 변화 적용
            }
            else                                                                                   // 돈이 없으면 종료
                return;
        }
    }

    public void putInventroyItem(string itemImageName) // 아이템을 인벤토리에 넣는다.
    {
        GameObject itemPrefab = (GameObject)Instantiate(Resources.Load("Prefabs/Item"));   // 아이템 오브젝트를 프리팹으로 생성
        UIButton itemButton = itemPrefab.GetComponent<UIButton>();                         // 프리팹으로부터 UI버튼 정보를 받음

        for (int row = 0; row < 4; row++)  // 인벤토리의 가로 순회
        {
            for (int col = 0; col < 4; col++)       // 인벤토리의 세로 순회
            {
                Transform slot = transform.Find("InventoryUI/InventorySlot/InventorySlot" + row + col);
                if (slot.childCount == 0)  // 인벤토리 슬롯의 자식 갯수가 0이면 (=슬롯에 아이템이 비어잇다)
                {
                    itemButton.transform.parent = slot.transform; // 해당 슬롯을 부모로 지정
                    itemButton.normalSprite = itemImageName;                                          // 상점 아이템 정보 동기화
                    itemButton.transform.position = slot.position;                                    // 포지션 동기화
                    itemButton.transform.localScale = Vector3.one;                                    // 스케일 1로 변경

                    ITEM_TYPE putItemType = getItemType(itemImageName);                               // 넣을 아이템 타입을 찾아서                   
                    m_lnkLstInventoryItemButton.AddLast(itemButton);                                  // 링크드 리스트에 버튼정보 삽입
                    return;
                }
            }
        }
    }

    public void weaponTypeButtonClick()
    {
        m_clickShopItem = null;      // 클릭한 상점 아이템 초기화
        m_clickShopItemMark.SetActive(false);
        showShopItem(ITEM_TYPE.WEAPON);
    }

    public void armorTypeButtonClick()
    {
        m_clickShopItem = null;      // 클릭한 상점 아이템 초기화
        m_clickShopItemMark.SetActive(false);
        showShopItem(ITEM_TYPE.ARMOR);
    }

    public void potionTypeButtonClick()
    {
        m_clickShopItem = null;      // 클릭한 상점 아이템 초기화
        m_clickShopItemMark.SetActive(false);
        showShopItem(ITEM_TYPE.POTION);
    }

    public void dynamicButtonClick()
    {
        if (m_dynamicButtonLabel.text == "착용하기")
        {
            ITEM_TYPE selectItemType = getItemType(m_clickInventoryItem.normalSprite);
            if (selectItemType == ITEM_TYPE.WEAPON)
            {
                if (m_weaponSlot.transform.childCount == 0)  // 자식 갯수가 0이면
                {
                    m_clickInventoryItem.transform.parent = m_weaponSlot.transform;             // 입음
                    m_clickInventoryItem.transform.position = m_weaponSlot.transform.position;  // 위치 동기화
                    CharacterInfoManager.instance.m_characterInfo.m_iWeaponStr = getValue(m_clickInventoryItem.normalSprite);
                    CharacterInfoManager.instance.replaceStr();
                    ProfileUIManager.Instance.changeStatus();       // 스텟 변화 적용
                    m_clickInventoryItem = null;
                }
            }
            else if (selectItemType == ITEM_TYPE.ARMOR)
            {
                if (m_armorSlot.transform.childCount == 0)  // 자식 갯수가 0이면
                {
                    m_clickInventoryItem.transform.parent = m_armorSlot.transform;             // 입음
                    m_clickInventoryItem.transform.position = m_armorSlot.transform.position;  // 위치 동기화
                    CharacterInfoManager.instance.m_characterInfo.m_iWeaponStr = getValue(m_clickInventoryItem.normalSprite);
                    CharacterInfoManager.instance.replaceStr();
                    ProfileUIManager.Instance.changeStatus();       // 스텟 변화 적용
                    m_clickInventoryItem = null;
                }
            }
            else if (selectItemType == ITEM_TYPE.POTION)
            {
                if (m_potionSlot.transform.childCount == 0)  // 자식 갯수가 0이면
                {
                    m_clickInventoryItem.transform.parent = m_potionSlot.transform;             // 입음
                    m_clickInventoryItem.transform.position = m_potionSlot.transform.position;  // 위치 동기화
                    m_clickInventoryItem = null;
                }
            }
        }
        else if (m_dynamicButtonLabel.text == "판매하기")
        {
            if (m_clickInventoryItem != null)
            {
                int saleGoldCount = getBuyGold(m_clickInventoryItem.normalSprite);               // 클릭한 아이템 구매 가격을 받아와 플레이어 골드 상승
                CharacterInfoManager.instance.m_characterInfo.m_iGold += saleGoldCount;          // 캐릭터 정보에 골드량을 증가시킨다.
                GameObject.Find("ProfileUI").GetComponent<ProfileUIManager>().changeGold();      // UI에 골드량을 바꾼다.
                m_lnkLstInventoryItemButton.Remove(m_clickInventoryItem);
                Destroy(m_clickInventoryItem.gameObject);                                        // 아이템 삭제
            }
        }
        else if (m_dynamicButtonLabel.text == "버리기")
        {
            m_lnkLstInventoryItemButton.Remove(m_clickInventoryItem);
            Destroy(m_clickInventoryItem.gameObject);                                        // 해당 아이템 파괴
        }
    }

    public void sortInventory()     // 빈 슬롯을 당김
    {
        int row = 0;
        int col = 0;

        foreach (UIButton iterator in m_lnkLstInventoryItemButton) // 반복자로 인벤토리내에 있는 아이템을 순회
        {
            Transform slot = transform.Find("InventoryUI/InventorySlot/InventorySlot" + row + col);  // 슬롯을 찾아서
            iterator.transform.parent = slot;                                                        // 부모 슬롯 변경
            iterator.transform.position = slot.position;                                             // 부모 슬롯 위치 재조립
            col++;
            if (col >= 4)
            {
                col = 0;
                row++;
            }
        }
    }
}
