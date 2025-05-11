using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    [Header("왼쪽 스크롤 뷰 그룹")]
    public GameObject optionList;
    public GameObject guideList;

    [Header("우측 설명 패널들")]
    public GameObject[] rightPanels; // Credit, GraphicOption, Character 등

    private void Start()
    {
        // 시작할 때 모든 RightPanel 내용 끄기
        foreach (var panel in rightPanels)
        {
            panel.SetActive(false);
        }
    }


    /// 상단 Guide / Option 버튼 클릭 시 호출
    public void ShowLeftList(string listName)
    {
        optionList.SetActive(listName == "Option");
        guideList.SetActive(listName == "Guide");

        // 하위 리스트 바꿀 때마다 우측 설명 패널 전부 끄기
        foreach (var panel in rightPanels)
        {
            panel.SetActive(false);
        }
    }

    /// 좌측 세부 항목 클릭 시 호출 (ex: "Credit")
    public void ShowRightPanel(string panelName)
    {
        foreach (var panel in rightPanels)
        {
            panel.SetActive(panel.name == panelName);
        }
    }
}
