using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardButton : MonoBehaviour
{
    public int rewardIndex;
    public Button unityButton;

    [Header("Highlight Images")]
    public Image normalHighlight;
    public Image selectHighlight;
    public Image selectRoad;

    [Header("Check Icons")]
    public Transform checkParent;
    public GameObject checkPrefab;
    public float checkSpacing = 20f;

    // 플레이어별 체크 아이콘 리스트: <actorNum, 아이콘 리스트>
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
        // 이미 투표 확정된 버튼이면 무시
        if (isVoted) return;

        // 첫 클릭 → 노랑 하이라이트, 상세정보
        if (!isSelected && !isVoted)
        {
            UIRewardPanel.Instance.UnhighlightAllNonVoted(this);
            isSelected = true;
            if (normalHighlight != null)
                normalHighlight.enabled = true;

            UIRewardPanel.Instance.ShowDetailLocal(rewardIndex);
        }
        // 두 번째 클릭 → 초록(투표 확정), 다른 버튼 비활성, 네트워크에 투표 요청
        else if (isSelected && !isVoted)
        {
            isVoted = true;
            if (normalHighlight != null) normalHighlight.enabled = false;
            if (selectHighlight != null) selectHighlight.enabled = true;
            if (selectRoad != null) selectRoad.enabled = true;

            UIRewardPanel.Instance.ShowDetailLocal(rewardIndex);

            // 로컬에서 다른 버튼 비활성
            foreach (var btn in UIRewardPanel.Instance.rewardButtons)
            {
                if (btn != this && btn.unityButton != null)
                    btn.unityButton.interactable = false;
            }

            // 전체에 투표 요청
            UIRewardPanel.Instance.RequestVote(rewardIndex);
        }
    }

    public void DisableNormalHighlight()
    {
        if (normalHighlight != null)
            normalHighlight.enabled = false;
        isSelected = false;
    }

    // 특정 플레이어(actorNum)에 대한 체크 아이콘 생성
    public void AddCheckIcon(int actorNum)
    {
        if (checkPrefab == null || checkParent == null) return;

        if (!playerCheckIcons.ContainsKey(actorNum))
            playerCheckIcons[actorNum] = new List<GameObject>();

        GameObject newCheck = Instantiate(checkPrefab, checkParent);
        int idx = playerCheckIcons[actorNum].Count;
        newCheck.transform.localPosition = new Vector3(idx * checkSpacing, 0f, 0f);
        playerCheckIcons[actorNum].Add(newCheck);
    }

    // 특정 플레이어(actorNum)가 추가한 체크 아이콘만 제거
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
