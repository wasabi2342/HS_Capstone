// =========================================================== DifficultyManager.cs
using UnityEngine;
using Photon.Pun;

/// <summary>
///   스테이지(씬)-단위 난이도 조정용 매니저
///     플레이어 수에 따른 "스폰 마리 수" / "몬스터 스탯" 배율을 계산  
///     StageManager, MonsterSpawner, EnemyFSM 등에서 읽어 간다  
///     씬마다 인스펙터 값만 바꾸면 레벨 디자이너가 손쉽게 난이도 튜닝 가능
/// </summary>
[DisallowMultipleComponent]
public class DifficultyManager : MonoBehaviourPunCallbacks
{
    /* ───── 싱글턴 (씬마다 1개) ───── */
    public static DifficultyManager Instance { get; private set; }
    void Awake() => Instance = this;

    /* ───── 배율 계산 방식 ───── */
    public enum Mode { LinearIncrement, CustomCurve }
    [Header("▼ Scaling Mode ▼")]
    public Mode scalingMode = Mode.LinearIncrement;

    /* 선형(1인 기준 + A×(N-1)) ------------------ */
    [Header("Linear Increment (기본)")]
    [Tooltip("스폰 마리 수 : 1P=1.0, 이후 + A × (playerCount-1)")]
    [Range(0f, 2f)] public float deltaCountMul = 0.5f;   // 예) 1→1.0, 2→1.5, 3→2.0, 4→2.5
    [Tooltip("HP·공격력 : 1P=1.0, 이후 + A × (playerCount-1)")]
    [Range(0f, 2f)] public float deltaStatMul = 0.4f;   // 예) 1→1.0, 2→1.4, 3→1.8, 4→2.2

    /* 커스텀 AnimationCurve 플레이어 수 -> 배율 관계 곡선으로 조절 ---------------------- */
    [Header("Custom Curve   (playerCount : 1→4)")]
    public AnimationCurve countCurve = AnimationCurve.Linear(1, 1f, 4, 2.5f);
    public AnimationCurve statCurve = AnimationCurve.Linear(1, 1f, 4, 2.2f);

    public float GetCountMultiplier(int playerCount)
    {
        playerCount = Mathf.Clamp(playerCount, 1, 4);

        return scalingMode switch
        {
            Mode.LinearIncrement => 1f + deltaCountMul * (playerCount - 1),
            Mode.CustomCurve => countCurve.Evaluate(playerCount),
            _ => 1f
        };
    }

    public float GetStatMultiplier(int playerCount)
    {
        playerCount = Mathf.Clamp(playerCount, 1, 4);

        return scalingMode switch
        {
            Mode.LinearIncrement => 1f + deltaStatMul * (playerCount - 1),
            Mode.CustomCurve => statCurve.Evaluate(playerCount),
            _ => 1f
        };
    }
}
