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
                // Scene���� T Ÿ���� ���ӿ�����Ʈ�� ã�´�.
                _instance = GameObject.FindObjectOfType(typeof(T)) as T;

                // Scene���� ���ٸ� ���� GameObject�� ����(__TypeName)
                if (_instance == null)
                {
                    _gameObject = new GameObject("__" + typeof(T).ToString());
                    _instance = _gameObject.AddComponent<T>();

                    // ���� ����Ǿ ���ŵ��� �ʵ��� ����
                    DontDestroyOnLoad(_gameObject);

                    // GameObject�� ���� ����������� _instance�� null �̸� Error!!!
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
    // (���� ��ӹ��� Ŭ�������� OnDestroy�� ���ְ� �ʹٸ� �Ʒ� ������ ���).
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