using UnityEngine;

public class GuideButtonDirectActivator : MonoBehaviour
{
    public GameObject rightInfoPanel;       // RightInfoPanel
    public GameObject guideToActivate;      // �� ��ư�� ������ ���̵� ������Ʈ

    public void ActivateAssignedGuide()
    {
        // ��� ���� ����
        foreach (Transform child in rightInfoPanel.transform)
        {
            child.gameObject.SetActive(false);
        }

        // �� ��ư�� ������ ���̵常 �ѱ�
        if (guideToActivate != null)
        {
            guideToActivate.SetActive(true);
        }
    }
}


