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
        if (isVoted)
            return;

        if (!isSelected && !isVoted)
        {
            UIRewardPanel.Instance.UnhighlightAllNonVoted(this);
            isSelected = true;
            if (normalHighlight != null)
                normalHighlight.enabled = true;
            UIRewardPanel.Instance.ShowDetailLocal(rewardIndex);
        }
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

            foreach (var btn in UIRewardPanel.Instance.rewardButtons)
            {
                if (btn != this && btn.unityButton != null)
                    btn.unityButton.interactable = false;
            }

            UIRewardPanel.Instance.RequestVote(rewardIndex);
        }
    }

    public void DisableNormalHighlight()
    {
        if (normalHighlight != null)
            normalHighlight.enabled = false;
        isSelected = false;
    }

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
