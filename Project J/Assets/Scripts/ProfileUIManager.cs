using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileUIManager : MonoBehaviour
{
    private UISprite m_characterMiniIcon;     // 캐릭터 미니 아이콘
    private UILabel m_UserNameText;           // 유저 닉네임 텍스트
    // Start is called before the first frame update

    void Awake()
    {
        m_characterMiniIcon = transform.Find("CharacterMiniIcon").GetComponent<UISprite>();
    }

    void Start()
    {
        m_characterMiniIcon.spriteName = "Akaza1";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
