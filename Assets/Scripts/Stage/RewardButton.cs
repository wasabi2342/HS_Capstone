using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardButton : MonoBehaviour
{
    public int rewardIndex;          // 0=A, 1=B …
    public Button unityButton;

    [Header("Highlight Images")]
    public Image normalHighlight;    // 첫 클릭 노랑
    public Image selectHighlight;    // 두 번째 클릭 초록
    public Image selectRoad;         // 초록 길(옵션)

    [Header("Check Icons")]
    public Transform checkParent;
    public GameObject checkPrefab;
    public float checkSpacing = 20f;
    private List<GameObject> spawnedChecks = new List<GameObject>();

    public bool isSelected = false;
    public bool isVoted = false;

    public void Init()
    {
        if (unityButton != null)
            unityButton.interactable = true;

        if (normalHighlight != null) normalHighlight.enabled = false;
        if (selectHighlight != null) selectHighlight.enabled = false;
        if (selectRoad != null) selectRoad.enabled = false;

        foreach (var c in spawnedChecks)
            Destroy(c);
        spawnedChecks.Clear();

        isSelected = false;
        isVoted = false;
    }

    public void OnClickButton()
    {
        // 이미 투표 확정(초록)이면 무시
        if (isVoted) return;

        // 첫 클릭 → 노랑
        if (!isSelected && !isVoted)
        {
            // 다른 노랑 해제
            RewardManager.Instance.UnhighlightAllNonVoted(this);

            isSelected = true;
            if (normalHighlight != null)
                normalHighlight.enabled = true;

            // 보상 상세 표시 (로컬 전용)
            RewardManager.Instance.ShowDetailLocal(rewardIndex);
        }
        // 두 번째 클릭 → 초록 (투표 요청)
        else if (isSelected && !isVoted)
        {
            isVoted = true;

            if (normalHighlight != null) normalHighlight.enabled = false;
            if (selectHighlight != null) selectHighlight.enabled = true;
            if (selectRoad != null) selectRoad.enabled = true;

            RewardManager.Instance.ShowDetailLocal(rewardIndex);

            // 로컬 → 마스터에게 “이걸로 투표”
            RewardManager.Instance.RequestVote(rewardIndex);
        }
    }

    public void DisableNormalHighlight()
    {
        if (normalHighlight != null)
            normalHighlight.enabled = false;
        isSelected = false;
    }

    // 마스터가 RPC_AddCheckMark → 모든 클라이언트에서 2개씩 아이콘 생성
    public void AddCheckIcon()
    {
        if (checkPrefab == null || checkParent == null) return;

        GameObject newCheck = Instantiate(checkPrefab, checkParent);
        int idx = spawnedChecks.Count;
        newCheck.transform.localPosition = new Vector3(idx * checkSpacing, 0f, 0f);

        spawnedChecks.Add(newCheck);
    }
}
