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
    public Image selectRoad;         // 초록색 길(옵션, 필요 시)

    [Header("Check Icons")]
    public Transform checkParent;    // 체크 아이콘이 생성될 부모 오브젝트
    public GameObject checkPrefab;   // 체크 아이콘 프리팹
    public float checkSpacing = 20f;   // 체크 아이콘 간격
    private List<GameObject> spawnedChecks = new List<GameObject>();

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

        foreach (var c in spawnedChecks)
            Destroy(c);
        spawnedChecks.Clear();

        isSelected = false;
        isVoted = false;
    }

    public void OnClickButton()
    {
        // 이미 투표 확정된 경우(초록 하이라이트)에는 아무 동작도 안 함
        if (isVoted)
            return;

        // 첫 클릭: 노란색 하이라이트 활성화 및 보상 상세정보 표시
        if (!isSelected && !isVoted)
        {
            UIRewardPanel.Instance.UnhighlightAllNonVoted(this);
            isSelected = true;
            if (normalHighlight != null)
                normalHighlight.enabled = true;
            UIRewardPanel.Instance.ShowDetailLocal(rewardIndex);
        }
        // 두 번째 클릭: 초록색 하이라이트로 변경 및 투표 요청
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
            UIRewardPanel.Instance.RequestVote(rewardIndex);
        }
    }

    public void DisableNormalHighlight()
    {
        if (normalHighlight != null)
            normalHighlight.enabled = false;
        isSelected = false;
    }

    // 마스터가 RPC_AddCheckMark을 호출할 때 모든 클라이언트에서 2개씩 체크 아이콘 생성
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
