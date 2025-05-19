// =========================================================== DifficultyManager.cs
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 플레이어 수(1~4) 별로 개별 배율을 슬라이더로 지정해서 난이도를 조정.
///   Count  : 스폰 마리 수
///   Hp / Atk / Shield : 각각의 스탯 배율
/// </summary>
[DisallowMultipleComponent]
public class DifficultyManager : MonoBehaviourPunCallbacks
{
    public static DifficultyManager Instance { get; private set; }
    void Awake() => Instance = this;

    // ────────────────── Inspector ──────────────────
    [System.Serializable]
    public struct MultiplierRow
    {
        [Range(0.1f, 5f)] public float countMul;   // 스폰 마리 수
        [Range(0.1f, 5f)] public float hpMul;      // 체력
        [Range(0.1f, 5f)] public float atkMul;     // 공격력
        [Range(0.1f, 5f)] public float shieldMul;  // 쉴드
    }

    [Header("Player-Count별 배율 (Index 0 = 1P, 3 = 4P)")]
    [Tooltip("Size를 4 로 유지하세요")]
    public MultiplierRow[] table = new MultiplierRow[4] {
        new MultiplierRow{ countMul=1, hpMul=1, atkMul=1, shieldMul=1 },   // 1P
        new MultiplierRow{ countMul=1.5f, hpMul=1.4f, atkMul=1.3f, shieldMul=1.6f }, // 2P
        new MultiplierRow{ countMul=2.0f, hpMul=1.8f, atkMul=1.6f, shieldMul=2.0f }, // 3P
        new MultiplierRow{ countMul=2.5f, hpMul=2.2f, atkMul=2.0f, shieldMul=2.5f }  // 4P
    };

    // ────────────────── API ──────────────────
    int ClampPC(int pc) => Mathf.Clamp(pc, 1, 4) - 1;   // 1→0, …, 4->3

    public float CountMul(int pc) => table[ClampPC(pc)].countMul;
    public float HpMul(int pc) => table[ClampPC(pc)].hpMul;
    public float AtkMul(int pc) => table[ClampPC(pc)].atkMul;
    public float ShieldMul(int pc) => table[ClampPC(pc)].shieldMul;

#if UNITY_EDITOR
    // Size가 4가 아니면 자동 보정
    void OnValidate()
    {
        if (table == null || table.Length != 4)
        {
            var tmp = new MultiplierRow[4];
            for (int i = 0; i < Mathf.Min(table?.Length ?? 0, 4); i++) tmp[i] = table[i];
            table = tmp;
        }
    }
#endif
}
