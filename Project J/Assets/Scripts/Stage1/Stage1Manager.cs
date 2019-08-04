using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CurSceneManager : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject playerUI;

    public GameObject unityChan;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(mainCamera);
        DontDestroyOnLoad(playerUI);
        DontDestroyOnLoad(unityChan);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true)
            SceneManager.LoadScene("Stage2Scene");
    }
}
