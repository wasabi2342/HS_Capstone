using UnityEngine;
using UnityEngine.UI;

public enum RewardEffectType
{
    HealHP,
    DamageBuff,
    DrawCard,
    Gold,
    // 필요하면 더 추가
}
[System.Serializable]
public class RewardData
{
    /* ─ UI 노출용 ─ */
    public string rewardName;      // "A 보상", "B 보상" …
    [TextArea] public string rewardDetail;  // 설명
    public Sprite Icon;      // DetailBox·보상 버튼에 표시할 아이콘

    /* ─ 실제 효과 정의용 ─ */
    public RewardEffectType effectType; // Heal, DamageBuff, Card 등 구분
    //public float effectValue;          // 회복량·배율·드랍확률 같은 수치
}
