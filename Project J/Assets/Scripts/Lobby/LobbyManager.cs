using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    GameObject m_shopMenu;                        // 상점 창 최상위 컴포넌트
    GameObject m_InventoryMenu;                   // 인벤토리 창 최상위 컴포넌트

    // Start is called before the first frame update
    void Start()
    {
        m_shopMenu = transform.Find("ItemWindow/ShopUI").gameObject;
        m_InventoryMenu = transform.Find("ItemWindow/InventoryUI").gameObject;
        createLobbyCharacter(CHARACTER_TYPE.AKAZA);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void openInventory()
    {
        m_InventoryMenu.SetActive(true);
        m_shopMenu.SetActive(false);
    }

    public void openCharacterInfomation()
    {

    }


    public void createLobbyCharacter(CHARACTER_TYPE characterType)
    {
        if (characterType == CHARACTER_TYPE.UNITY)
        {
            GameObject character = (GameObject)Instantiate(Resources.Load("Prefabs/UnityChan"), new Vector3(-2f, -0.5f, -6.0f), Quaternion.AngleAxis(120.0f, new Vector3(0, 1, 0)));   // 120도 회전 생성
            //character.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f); // 스케일 0.6으로 변경
            character.GetComponent<UnityChanOperation>().enabled = false;   // 조작 스크립트 비활성화
        }
        else if (characterType == CHARACTER_TYPE.AKAZA)
        {
            GameObject character = (GameObject)Instantiate(Resources.Load("Prefabs/Akaza"), new Vector3(-2f, -0.5f, -6.0f), Quaternion.AngleAxis(120.0f, new Vector3(0, 1, 0))); // 120도 회전 생성
            //character.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);  // 스케일 0.6으로 변경
            character.GetComponent<UnityChanOperation>().enabled = false;  // 조작 스크립트 비활성화
        }
    }
    public void openShop()
    {
        m_InventoryMenu.SetActive(true);
        m_shopMenu.SetActive(true);
    }

    public void closeWindow()
    {
        m_shopMenu.SetActive(false);
        m_InventoryMenu.SetActive(false);
    }

    public void openSkillInfomation()
    {

    }

    public void enterDungeon()  // 던전 입장 함수
    {
        SceneManager.LoadScene("LoadingScene");
    }
}
