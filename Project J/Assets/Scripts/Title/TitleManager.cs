using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    private Text m_explainText;
    private Button m_startButton;
    bool m_bTouchFlag = false;

    public void startButtonClick()
    {
        SceneManager.LoadScene("SelectCharacterScene");
    }

    // Start is called before the first frame update
    void Start()
    {
        m_explainText = GameObject.Find("ExplainText").GetComponent<Text>();
        m_startButton = GameObject.Find("StartButton").GetComponent<Button>();
        m_explainText.gameObject.SetActive(true);
        m_startButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_bTouchFlag == false)                          // 터치 상태가 false일 경우
        {
            if (Input.GetMouseButtonDown(0) == true)        // 아무 곳이나 터치하면 버튼 활성화
            {
                m_bTouchFlag = true;                        // 터치 상태 true
                m_explainText.gameObject.SetActive(!m_bTouchFlag);  // 터치하세요 설명 비활성화
                m_startButton.gameObject.SetActive(m_bTouchFlag);   // 버튼 활성화
            }
        }
    }
}
