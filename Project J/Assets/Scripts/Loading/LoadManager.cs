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
    private UIScrollBar percentBar;
    private AsyncOperation loadState;
    

    private void Start()
    {
        loadState = SceneManager.LoadSceneAsync("Stage1-1Scene");
        loadState.allowSceneActivation = false;
        percentBar = sprProgressBar.GetComponent<UIScrollBar>();
    }

    private void Update()
    {
        percentBar.barSize += loadState.progress;

        if (loadState.progress >= 0.9f)
        {
            txtLoadingInfo.text = "Load Success! Touch Please";

            if(Input.GetMouseButtonDown(0))
              loadState.allowSceneActivation = true;
        }
        else
            txtLoadingInfo.text = "Loading...";
    }
}