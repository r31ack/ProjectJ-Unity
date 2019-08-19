﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    GameObject m_characterInfomationMenu;         // 캐릭터 정보 창 최상위 부모 오브젝트
    GameObject m_shopMenu;                        // 상점 창 최상위 부모 오브젝트
    GameObject m_inventoryMenu;                   // 인벤토리 창 최상위 부모 오브젝트
    private AllCharacterInfo m_characterInfomation;   // 캐릭터 정보를 받아온다.

    void Awake()
    {
        if (GameManager.instance.m_iSelectCharacterIndex == -1)
            GameManager.instance.m_iSelectCharacterIndex = 2;
        CharacterInfoManager.instance.loadUserInfo(GameManager.instance.m_iSelectCharacterIndex); // 선택한 유저네임을 기반으로한 유저 데이터를 로딩한다.
    }
    void Start()
    {
        m_characterInfomation = CharacterInfoManager.instance.m_characterInfo;   // 캐릭터 정보를 받아온다.
        m_characterInfomationMenu = transform.Find("ItemWindow/InfomationUI").gameObject;
        m_shopMenu = transform.Find("ItemWindow/ShopUI").gameObject;
        m_inventoryMenu = transform.Find("ItemWindow/InventoryUI").gameObject;

        if (m_characterInfomation.m_eCharacterType == CHARACTER_TYPE.UNITY)         // 캐릭터 타입을 받아와서
        {
            createLobbyCharacter(CHARACTER_TYPE.UNITY);                             // 캐릭터 프리팹 생성
        }
        if (m_characterInfomation.m_eCharacterType == CHARACTER_TYPE.AKAZA)         // 캐릭터 타입을 받아와서
        {
            createLobbyCharacter(CHARACTER_TYPE.AKAZA);                             // 캐릭터 프리팹 생성
        }
    }

    public void createLobbyCharacter(CHARACTER_TYPE characterType)
    {
        if (characterType == CHARACTER_TYPE.UNITY)
        {
            GameObject character = (GameObject)Instantiate(Resources.Load("Prefabs/UnityChanNormal"), new Vector3(-2f, -0.5f, -6.0f), Quaternion.AngleAxis(120.0f, new Vector3(0, 1, 0)));   // 120도 회전 생성
            //character.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f); // 스케일 0.6으로 변경

        }
        else if (characterType == CHARACTER_TYPE.AKAZA)
        {
            GameObject character = (GameObject)Instantiate(Resources.Load("Prefabs/AkazaNormal"), new Vector3(-2f, -0.5f, -6.0f), Quaternion.AngleAxis(120.0f, new Vector3(0, 1, 0))); // 120도 회전 생성
            //character.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);  // 스케일 0.6으로 변경

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void openCharacterInfomation()
    {
        m_characterInfomationMenu.SetActive(true);
        m_shopMenu.SetActive(false);
    }

    public void openInventory()
    {
        m_inventoryMenu.SetActive(true);
        m_shopMenu.SetActive(false);
    }

    public void openShop()
    {
        m_characterInfomationMenu.SetActive(false);
        m_inventoryMenu.SetActive(true);
        m_shopMenu.SetActive(true);
    }

    public void closeWindow()
    {
        m_characterInfomationMenu.SetActive(false);
        m_shopMenu.SetActive(false);
        m_inventoryMenu.SetActive(false);
    }

    public void openSkillInfomation()
    {

    }

    public void enterDungeon()  // 던전 입장 함수
    {
        SceneManager.LoadScene("SelectDungeonScene");
    }
}
