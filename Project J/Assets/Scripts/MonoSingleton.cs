using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>, new()
{
    private static T _instance = null;
    private static GameObject _gameObject = null;
    private static bool _isInitialize = false;

    protected virtual void OnInitialize() { }

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                // Scene에서 T 타입의 게임오브젝트를 찾는다.
                _instance = GameObject.FindObjectOfType(typeof(T)) as T;

                // Scene내에 없다면 새로 GameObject를 생성(__TypeName)
                if (_instance == null)
                {
                    _gameObject = new GameObject("__" + typeof(T).ToString());
                    _instance = _gameObject.AddComponent<T>();

                    // 씬이 변경되어도 제거되지 않도록 설정
                    DontDestroyOnLoad(_gameObject);

                    // GameObject를 새로 만들었음에도 _instance가 null 이면 Error!!!
                    if (_instance == null)
                    {
                        Debug.LogError("Problem during the creation of " + typeof(T).ToString());
                    }
                }

                // Init.
                if (!_isInitialize)
                {
                    _instance.OnInitialize();
                    _isInitialize = true;
                }
            }

            return _instance;
        }
    }

    // If no other monobehaviour request the instance in an awake function
    // executing before this one, no need to search the object.
    private void Awake()
    {
        //Debug.Log( "#####" + typeof(T).ToString() + " : Awake()" );
        if (_instance == null)
            _instance = this as T;

        // Init.
        if (!_isInitialize)
        {
            OnInitialize();
            _isInitialize = true;
        }
    }

    // if have a derrived class use below example
    // (만일 상속받은 클래스에서 OnDestroy를 해주고 싶다면 아래 예제를 사용).
    protected void OnDestroy()
    {
        if (_gameObject != null)
            Destroy(_gameObject);

        _instance = null;
        _isInitialize = false;
    }

    // Make sure the instance isn't referenced anymore when the user quit, just in case.
    void OnApplicationQuit()
    {
        _instance = null;
    }
}