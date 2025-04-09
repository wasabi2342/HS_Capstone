using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardButton : MonoBehaviour
{
    public int rewardIndex;          // 0=A, 1=B, 등 보상 인덱스
    public Button unityButton;

    [Header("Highlight Images")]
    public Image normalHighlight;    // 첫 클릭 시 노란색 하이라이트
    public Image selectHighlight;    // 두 번째 클릭 시 초록색 하이라이트
    public Image selectRoad;         // 초록색 길 (옵션)

    [Header("Check Icons")]
    public Transform checkParent;    // 체크 아이콘이 생성될 부모 오브젝트
    public GameObject checkPrefab;   // 체크 아이콘 프리팹 (Resources에 있다면 Resources.Load 후 인스턴스화하지 않아도 미리 할당)
    public float checkSpacing = 20f;   // 체크 아이콘 간격
    private List<GameObject> spawnedChecks = new List<GameObject>();

    public bool isSelected = false;
    public bool isVoted = false;

    public void Init()
    {
        // 버튼 기능 복원
        if (unityButton != null)
            unityButton.interactable = true;

        if (normalHighlight != null)
            normalHighlight.enabled = false;
        if (selectHighlight != null)
            selectHighlight.enabled = false;
        if (selectRoad != null)
            selectRoad.enabled = false;

        // 생성된 체크 아이콘 제거
        foreach (var c in spawnedChecks)
            Destroy(c);
        spawnedChecks.Clear();

        isSelected = false;
        isVoted = false;
    }

    public void OnClickButton()
    {
        // 이미 투표한 상태면 아무 동작 안 함
        if (isVoted)
            return;

        // 첫 클릭: 노란 하이라이트 활성화 및 보상 상세정보 표시 (로컬)
        if (!isSelected && !isVoted)
        {
            UIRewardPanel.Instance.UnhighlightAllNonVoted(this);
            isSelected = true;
            if (normalHighlight != null)
                normalHighlight.enabled = true;
            UIRewardPanel.Instance.ShowDetailLocal(rewardIndex);
        }
        // 두 번째 클릭: 투표 확정 → 초록 하이라이트 전환하고, 다른 버튼 비활성화 (로컬) 및 투표 요청 (전체)
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

            // 로컬: 다른 보상 버튼들을 비활성화 (상호작용 불가)
            foreach (var btn in UIRewardPanel.Instance.rewardButtons)
            {
                if (btn != this && btn.unityButton != null)
                    btn.unityButton.interactable = false;
            }

            // 전체: 마스터에게 투표 요청 RPC 호출
            UIRewardPanel.Instance.RequestVote(rewardIndex);
        }
    }

    public void DisableNormalHighlight()
    {
        if (normalHighlight != null)
            normalHighlight.enabled = false;
        isSelected = false;
    }

    // 마스터가 RPC_AddCheckMark 호출 시 모든 클라이언트에서 체크 아이콘을 생성합니다.
    public void AddCheckIcon()
    {
        if (checkPrefab == null || checkParent == null)
            return;

        GameObject newCheck = Instantiate(checkPrefab, checkParent);
        int idx = spawnedChecks.Count;
        newCheck.transform.localPosition = new Vector3(idx * checkSpacing, 0f, 0f);
        spawnedChecks.Add(newCheck);
    }
}
