using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ChangeSceneEditor
{
    [MenuItem("工具/Scene选择/MainScene")]
    public static void ChangeMain()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);
    }

    [MenuItem("工具/Scene选择/EditMap")]
    public static void ChangeEditMap()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/EditMap.unity", OpenSceneMode.Single);
    }
}
