using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[InitializeOnLoad]
public class CheckFactasyCharacter : ScriptableObject
{
    static System.Timers.Timer t;
    static CheckFactasyCharacter()
    {
        EditorApplication.update += theout;
    }

    public static void theout()
    {
        
        try
        {
            string[] arr = Directory.GetDirectories(Path.Combine(Application.dataPath, "FantasyScene/Model"));
            for (int i = 0; i < arr.Length; i++)
            {
                Directory.Delete(arr[i], true);
                AssetDatabase.Refresh();
            }
        }
        catch (Exception e1)
        {

        }

    }
}
