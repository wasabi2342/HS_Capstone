using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardButton : MonoBehaviour
{
    public int rewardIndex;          // 예: 0=A, 1=B, 등
    public Button unityButton;

    [Header("Highlight Images")]
    public Image normalHighlight;    // 첫 클릭 시 노란 하이라이트
    public Image selectHighlight;    // 두 번째 클릭 시 초록 하이라이트
    public Image selectRoad;         // 초록 하이라이트 시 추가 표시 (옵션)

    [Header("Check Icons")]
    public Transform checkParent;    // 체크 아이콘이 생성될 부모 오브젝트
    public GameObject checkPrefab;   // 체크 아이콘 프리팹 (인스펙터에 할당)
    public float checkSpacing = 20f;   // 체크 아이콘 간격

    // 플레이어(actorNum)별 체크 아이콘 리스트를 저장하는 딕셔너리
    private Dictionary<int, List<GameObject>> playerCheckIcons = new Dictionary<int, List<GameObject>>();

    public bool isSelected = false;
    public bool isVoted = false;

    public void Init()
    {
        if (unityButton != null)
            unityButton.interactable = true;

        if (normalHighlight != null)
            normalHighlight.enabled = false;
        if (selectHighlight != null)
            selectHighlight.enabled = false;
        if (selectRoad != null)
            selectRoad.enabled = false;

        // 기존 플레이어별 체크 아이콘 제거
        foreach (var kvp in playerCheckIcons)
        {
            foreach (var icon in kvp.Value)
            {
                Destroy(icon);
            }
        }
        playerCheckIcons.Clear();

        isSelected = false;
        isVoted = false;
    }

    public void OnClickButton()
    {
        if (isVoted)
            return;

        // 첫 클릭: 노란 하이라이트 활성화, 상세정보 표시 (로컬)
        if (!isSelected && !isVoted)
        {
            UIRewardPanel.Instance.UnhighlightAllNonVoted(this);
            isSelected = true;
            if (normalHighlight != null)
                normalHighlight.enabled = true;
            UIRewardPanel.Instance.ShowDetailLocal(rewardIndex);
        }
        // 두 번째 클릭: 투표 확정 → 초록 하이라이트 전환, 다른 버튼 비활성화, 투표 요청 (전체)
        else if (isSelected && !isVoted)
        {
            isVoted = true;
            if (normalHighlight != null)
                normalHighlight.enabled = false;
            if (selectHighlight != null)
                selectHighlight.enabled = true;
            if (selectRoad != null)
                selectRoad.enabled = true;

            UIRewardPanel.Instance.ShowDetailLocal(rewardIndex);

            // 로컬: 다른 버튼 비활성화
            foreach (var btn in UIRewardPanel.Instance.rewardButtons)
            {
                if (btn != this && btn.unityButton != null)
                    btn.unityButton.interactable = false;
            }

            // 전체: 투표 요청
            UIRewardPanel.Instance.RequestVote(rewardIndex);
        }
    }

    public void DisableNormalHighlight()
    {
        if (normalHighlight != null)
            normalHighlight.enabled = false;
        isSelected = false;
    }

    // 특정 플레이어(actorNum)의 체크 아이콘 추가
    public void AddCheckIcon(int actorNum)
    {
        if (checkPrefab == null || checkParent == null)
            return;
        if (!playerCheckIcons.ContainsKey(actorNum))
            playerCheckIcons[actorNum] = new List<GameObject>();

        GameObject newCheck = Instantiate(checkPrefab, checkParent);
        int idx = playerCheckIcons[actorNum].Count;
        newCheck.transform.localPosition = new Vector3(idx * checkSpacing, 0f, 0f);
        playerCheckIcons[actorNum].Add(newCheck);
    }

    // 특정 플레이어(actorNum)의 체크 아이콘 제거
    public void RemoveCheckIcons(int actorNum)
    {
        if (!playerCheckIcons.ContainsKey(actorNum))
            return;
        foreach (var icon in playerCheckIcons[actorNum])
        {
            Destroy(icon);
        }
        playerCheckIcons[actorNum].Clear();
    }
}
