using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardButton : MonoBehaviour
{
    public int rewardIndex;          // 0=A, 1=B, 등
    public Button unityButton;

    [Header("Highlight Images")]
    public Image normalHighlight;    // 첫 클릭 시 노란 하이라이트
    public Image selectHighlight;    // 두 번째 클릭 시 초록 하이라이트
    public Image selectRoad;         // 초록 하이라이트 시 추가 표시 (옵션)

    [Header("Check Icons")]
    public Transform checkParent;    // 체크 아이콘이 생성될 부모 오브젝트
    public GameObject checkPrefab;   // 체크 아이콘 프리팹 (Inspector에 할당)
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
        // 이미 투표가 확정되어 있으면 아무 동작도 하지 않음
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
        // 두 번째 클릭: 투표 확정 처리 → 초록 하이라이트 전환, 다른 버튼 비활성화, 투표 요청 (전체)
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

            // 로컬: 다른 보상 버튼들의 인터랙션 비활성화
            foreach (var btn in UIRewardPanel.Instance.rewardButtons)
            {
                if (btn != this && btn.unityButton != null)
                    btn.unityButton.interactable = false;
            }

            // 전체: 투표 요청 RPC 전송 (PhotonNetworkManager를 통해)
            UIRewardPanel.Instance.RequestVote(rewardIndex);
        }
    }

    public void DisableNormalHighlight()
    {
        if (normalHighlight != null)
            normalHighlight.enabled = false;
        isSelected = false;
    }

    // 체크 아이콘 생성 (RPC를 통해 호출 시)
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
