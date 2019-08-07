using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum CHARACTER_TYPE
{
    UNITY,
    AKAZA,
    NONE,
}

enum ENEMY_TYPE
{

}

public class GameManager : MonoSingleton<GameManager>
{
    private Canvas m_exitWindow;                   // 게임 종료 확인 창
    private bool m_bExitWindowFlag = false;        // 게임 종료 확인 창 활성화 여부
    public int m_iCreateCharacterIndex = -1;       // 캐릭터 선택창에 내가 누른 버튼
    public int m_iSelectCharacterIndex = -1;       // 로비에 입장시에 내가 선택한 캐릭터 인덱스

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(GetComponent<GameManager>());    // 게임매니저 오브젝트는 파괴되지 않게 한다. 
        m_exitWindow = transform.Find("ExitWindow").GetComponent<Canvas>();
        m_exitWindow.gameObject.SetActive(false);          // 종료 창 초기상태 false
    }

    // Update is called once per frame
    void Update()
    {
        exitWindow();
    }

    void exitWindow()                                        // 게임 종료 창
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true)        // ESC키를 누르면
        {
            m_bExitWindowFlag = !m_bExitWindowFlag;          // 상태 전환
            m_exitWindow.gameObject.SetActive(m_bExitWindowFlag);          // 종료 창 초기상태 false
            Time.timeScale = 0.0f;
        }
    }

    public void continueGame()                                     // 게임 계속 하기
    {
        m_bExitWindowFlag = false;
        m_exitWindow.gameObject.SetActive(m_bExitWindowFlag);
        Time.timeScale = 1.0f;
    }

    public void exitGame()                                         // 게임 종료하기
    {
        Application.Quit();
    }
}
