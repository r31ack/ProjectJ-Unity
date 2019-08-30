using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubDialogUIManager : MonoBehaviour
{
    private UIPanel m_subDialogPanel;
    private UISprite m_characterImage;
    private UILabel m_chatLabel;                        // 대화 내용 레이블
    private string m_strChatContent;  // 대화내용 모음

    // Start is called before the first frame update

    void Awake()
    {
       m_subDialogPanel = GetComponent<UIPanel>();

        m_chatLabel = transform.Find("SubChatLabel").GetComponent<UILabel>();
        m_characterImage = transform.Find("CharacterImage").GetComponent<UISprite>();
    }

    public void setSubDialogValue(string imageName, string value)
    {
        m_characterImage.spriteName = imageName;
        m_chatLabel.text = value;
    }

    public void OnEnable()      // 활성화 시
    {
        m_subDialogPanel.alpha = 0.0f;
        InvokeRepeating("fadeIn",0.0f,0.05f);
    }

    private void fadeIn()
    {
        if (m_subDialogPanel.alpha >= 0.75f)
        {
            CancelInvoke("fadeIn");
            InvokeRepeating("fadeOut", 3.0f, 0.05f);
        }
        else
            m_subDialogPanel.alpha += 0.1f;
    }

    private void fadeOut()
    {
        if (m_subDialogPanel.alpha <= 0.0f)
        {
            CancelInvoke("fadeOut");
            GameManager.instance.endSubDialog();
        }
        else
            m_subDialogPanel.alpha -= 0.1f;
    }
}
