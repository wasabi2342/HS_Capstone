using UnityEngine;

public class GuideButtonDirectActivator : MonoBehaviour
{
    public GameObject rightInfoPanel;       // RightInfoPanel
    public GameObject guideToActivate;      // 이 버튼이 열어줄 가이드 오브젝트

    public void ActivateAssignedGuide()
    {
        // 모든 설명 끄기
        foreach (Transform child in rightInfoPanel.transform)
        {
            child.gameObject.SetActive(false);
        }

        // 이 버튼이 지정한 가이드만 켜기
        if (guideToActivate != null)
        {
            guideToActivate.SetActive(true);
        }
    }
}


