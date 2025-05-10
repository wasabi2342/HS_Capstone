using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    [Header("���� ��ũ�� �� �׷�")]
    public GameObject optionList;
    public GameObject guideList;

    [Header("���� ���� �гε�")]
    public GameObject[] rightPanels; // Credit, GraphicOption, Character ��

    private void Start()
    {
        // ������ �� ��� RightPanel ���� ����
        foreach (var panel in rightPanels)
        {
            panel.SetActive(false);
        }
    }


    /// ��� Guide / Option ��ư Ŭ�� �� ȣ��
    public void ShowLeftList(string listName)
    {
        optionList.SetActive(listName == "Option");
        guideList.SetActive(listName == "Guide");

        // ���� ����Ʈ �ٲ� ������ ���� ���� �г� ���� ����
        foreach (var panel in rightPanels)
        {
            panel.SetActive(false);
        }
    }

    /// ���� ���� �׸� Ŭ�� �� ȣ�� (ex: "Credit")
    public void ShowRightPanel(string panelName)
    {
        foreach (var panel in rightPanels)
        {
            panel.SetActive(panel.name == panelName);
        }
    }
}
