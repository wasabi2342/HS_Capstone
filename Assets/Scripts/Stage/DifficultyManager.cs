// =========================================================== DifficultyManager.cs
using UnityEngine;
using Photon.Pun;

/// <summary>
///   ��������(��)-���� ���̵� ������ �Ŵ���
///     �÷��̾� ���� ���� "���� ���� ��" / "���� ����" ������ ���  
///     StageManager, MonsterSpawner, EnemyFSM ��� �о� ����  
///     ������ �ν����� ���� �ٲٸ� ���� �����̳ʰ� �ս��� ���̵� Ʃ�� ����
/// </summary>
[DisallowMultipleComponent]
public class DifficultyManager : MonoBehaviourPunCallbacks
{
    /* ���������� �̱��� (������ 1��) ���������� */
    public static DifficultyManager Instance { get; private set; }
    void Awake() => Instance = this;

    /* ���������� ���� ��� ��� ���������� */
    public enum Mode { LinearIncrement, CustomCurve }
    [Header("�� Scaling Mode ��")]
    public Mode scalingMode = Mode.LinearIncrement;

    /* ����(1�� ���� + A��(N-1)) ------------------ */
    [Header("Linear Increment (�⺻)")]
    [Tooltip("���� ���� �� : 1P=1.0, ���� + A �� (playerCount-1)")]
    [Range(0f, 2f)] public float deltaCountMul = 0.5f;   // ��) 1��1.0, 2��1.5, 3��2.0, 4��2.5
    [Tooltip("HP�����ݷ� : 1P=1.0, ���� + A �� (playerCount-1)")]
    [Range(0f, 2f)] public float deltaStatMul = 0.4f;   // ��) 1��1.0, 2��1.4, 3��1.8, 4��2.2

    /* Ŀ���� AnimationCurve �÷��̾� �� -> ���� ���� ����� ���� ---------------------- */
    [Header("Custom Curve   (playerCount : 1��4)")]
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
