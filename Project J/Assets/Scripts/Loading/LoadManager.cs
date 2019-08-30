using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public static string nextScene;             // 다음 씬

    public UISprite sprProgressBar;
    public UILabel txtLoadingInfo;
    private string[] m_loadExplainText = new string[5];
    private UIScrollBar percentBar;
    private AsyncOperation loadState;
    
    private void Start()
    {
        m_loadExplainText[0] = "특정 조건에서 발동되는 다양한 연계 스킬을 활용해 보세요";
        m_loadExplainText[1] = "클리어 타임은 최종스코어에 영향을 미칩니다.";
        m_loadExplainText[2] = "회피기는 대부분의 모션에서도 사용 가능합니다";
        m_loadExplainText[3] = "이동입력 키는 방향키에 따라 스킬이 달라집니다.";

        if (SceneManager.GetActiveScene().name == "Lobby-LoadingScene")
        {
            GameManager.instance.dontDestroyCharacterObject();
            loadState = SceneManager.LoadSceneAsync("LobbyScene");
        }
        else if (SceneManager.GetActiveScene().name == "Stage1-LoadingScene")
            loadState = SceneManager.LoadSceneAsync("Stage1-1Scene");
        loadState.allowSceneActivation = false;
        percentBar = sprProgressBar.GetComponent<UIScrollBar>();
        txtLoadingInfo.text = m_loadExplainText[Random.Range(0, 4)];
    }

    private void Update()
    {
        percentBar.barSize += 0.05f;

        if (loadState.progress >= 0.9f && percentBar.barSize >= 1.0f)
        {
            if (Input.GetMouseButtonDown(0))
              loadState.allowSceneActivation = true;
        }
    }
}