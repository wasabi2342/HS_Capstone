// ===================== RewardButton.cs =====================
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardButton : MonoBehaviour
{
    /* ───────── 정적 (현재 선택된 버튼) ───────── */
    private static RewardButton currentlySelected = null;

    /* ───────── Inspector ───────── */
    [Header("General")]
    public int rewardIndex;          // 보상 인덱스
    public Button unityButton;          // 클릭 이벤트용

    [Header("Check Slots (미리 배치한 이미지)")]
    public GameObject[] checkSlots;     // Slot0, Slot1, … (SetActive=false 로 시작)

    /* ───────── 내부 자료구조 ───────── */
    private readonly List<int> voteOrder = new();   // 왼→오른쪽 순서
    private readonly HashSet<int> voters = new(); // 중복 방지

    /* ───────── 상태 ───────── */
    [HideInInspector] public bool isSelected = false; // 1차 선택 여부
    public bool isVoted = false;         // 투표 확정 여부

    /* ───────── 초기화 ───────── */
    public void Init()
    {
        if (unityButton) unityButton.interactable = true;

        voteOrder.Clear();
        voters.Clear();
        foreach (var slot in checkSlots)
            slot.SetActive(false);

        isSelected = false;
        isVoted = false;

        if (currentlySelected == this)
            currentlySelected = null;
    }

    /* ───────── 클릭 ───────── */
    public void OnClickButton()
    {
        if (isVoted) return; // 이미 확정된 버튼이면 무시

        /* ① 첫 번째 클릭 → 선택 & 상세 보기 */
        if (currentlySelected != this)
        {
            if (currentlySelected != null)
                currentlySelected.isSelected = false; // 이전 선택 해제

            currentlySelected = this;
            isSelected = true;
            UIRewardPanel.Instance.ShowDetailLocal(rewardIndex);
            return;
        }

        /* ② 두 번째 클릭 → 투표 확정 */
        ConfirmVote();
    }

    private void ConfirmVote()
    {
        isVoted = true;

        // 다른 버튼 잠금
        foreach (var b in UIRewardPanel.Instance.rewardButtons)
            if (b != this && b.unityButton)
                b.unityButton.interactable = false;

        UIRewardPanel.Instance.RequestVote(rewardIndex);
    }

    /* ───────── 체크 아이콘 추가 (왼쪽부터) ───────── */
    public void AddCheckIcon(int actorNum)
    {
        if (voters.Contains(actorNum)) return;            // 중복 방지
        if (voteOrder.Count >= checkSlots.Length) return; // 슬롯 초과 방지

        int idx = voteOrder.Count;        // 왼쪽부터 채움
        checkSlots[idx].SetActive(true);

        voteOrder.Add(actorNum);
        voters.Add(actorNum);
    }

    /* ───────── 체크 아이콘 제거 (오른쪽부터) ───────── */
    public void RemoveCheckIcon(int actorNum)
    {
        if (!voters.Contains(actorNum)) return;

        int removeIdx = voteOrder.IndexOf(actorNum);
        voters.Remove(actorNum);
        voteOrder.RemoveAt(removeIdx);

        /* 모든 슬롯 재정렬 : 왼쪽부터 voteOrder 수만큼만 활성화 */
        for (int i = 0; i < checkSlots.Length; i++)
            checkSlots[i].SetActive(i < voteOrder.Count);
    }
}
