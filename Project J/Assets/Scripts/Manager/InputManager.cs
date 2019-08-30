using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KeyInfo		// 입력된 키 정보
{
    public bool keyDown;       // 키를 누른 상태
    public bool keyPress;      // 키가 눌러지고 있는 상태
    public bool keyUp;			// 키를 떼는 상태
};

public class InputManager : MonoSingleton<InputManager>
{
    Dictionary<KeyCode, KeyInfo> m_dicRegistKey = new Dictionary<KeyCode, KeyInfo>();

    private void Awake()
    {
        registKey(KeyCode.A);
        registKey(KeyCode.W);
        registKey(KeyCode.D);
        registKey(KeyCode.S);

        registKey(KeyCode.Keypad1);
        registKey(KeyCode.Keypad2);
        registKey(KeyCode.Keypad3);
        registKey(KeyCode.Keypad4);
        registKey(KeyCode.Keypad5);
        registKey(KeyCode.Keypad6);
    }

    // Update is called once per frame
    public void LateUpdate()                // 입력이 끝난 후 입력상태를 갱신하기 위해 LateUpdate사용
    {
        foreach (var value in m_dicRegistKey.Values.ToList())
        {
            if (value.keyDown == true)
            {
                value.keyDown = false;
                value.keyPress = true;     // 키 누르는 중인 상태로 전환
                value.keyUp = false;       
            }
            else if (value.keyUp)          // 키를 뗀 상태이면
            {
                value.keyDown = false;     // 다운 상태값 아님
                value.keyPress = false;    // 누른 상태값 아님
                value.keyUp = false;       // 키업 상태가 아님
            }
            else
            {
                value.keyDown = false;     // 다운 상태값 아님
                value.keyPress = false;    // 누른 상태값 아님
                value.keyUp = true;        // 키업 상태로 전환
            }
        }
    }

    public void registKey(KeyCode keycode)
    {
        m_dicRegistKey.Add(keycode, new KeyInfo());
    }

    public void inputUpKey(KeyCode keycode)
    {
        m_dicRegistKey[keycode].keyDown = false;
        m_dicRegistKey[keycode].keyPress = false;
        m_dicRegistKey[keycode].keyUp = true;
    }

    public void inputPressKey(KeyCode keycode)
    {
        m_dicRegistKey[keycode].keyDown = false;
        m_dicRegistKey[keycode].keyPress = true;
        m_dicRegistKey[keycode].keyUp = false;
    }
    public void inputDownKey(KeyCode keycode)
    {
        m_dicRegistKey[keycode].keyDown = true;
        m_dicRegistKey[keycode].keyPress = false;
        m_dicRegistKey[keycode].keyUp = false;
    }

    public bool keyPressCheck(KeyCode keyCode)
    {
        return m_dicRegistKey[keyCode].keyPress;
    }
    public bool keyUpCheck(KeyCode keyCode)
    {
        return m_dicRegistKey[keyCode].keyUp;
    }
    public bool keyDownCheck(KeyCode keyCode)
    {
        return m_dicRegistKey[keyCode].keyDown;
    }
}
