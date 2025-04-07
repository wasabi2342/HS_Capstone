
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;


public class OdinEditor: OdinEditorWindow
{
    public int test;

    [Button("Test Button", ButtonSizes.Large)]
    public void TestButton()
    {
        Debug.Log("Test Button Pressed!"+ test);

    }

    [MenuItem("오딘 테스트 에디터/멀로로 기획 QA에디터")]
    private static void OpenWindow()
    {
        GetWindow<OdinEditor>().Show();
    }
}
