
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

    [Button("가호획득", ButtonSizes.Large)]
    public void OpenBlessingPanel()
    {
        UIManager.Instance.OpenPopupPanelInCameraCanvas<UISelectBlessingPanel>();
        Debug.Log("가호획득 버튼 클릭됨");
    }

    [MenuItem("오딘 테스트 에디터/멀로로 기획 QA에디터")]
    private static void OpenWindow()
    {
        GetWindow<OdinEditor>().Show();
    }
}
