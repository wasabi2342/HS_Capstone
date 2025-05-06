using UnityEngine;

[CreateAssetMenu(menuName = "EnemyStatus Data")]
public class EnemyStatusSO : ScriptableObject
{
    [Header("Identification")]
    public string enemyName = "Default Enemy"; // �� �̸�


    public float maxHealth = 100f; // �ִ� ü��
    public float moveSpeed = 2f; // �⺻ �̵� �ӵ�
    public float chaseSpeedMultiplier = 1.1f; // �߰� �� �̵� �ӵ� ���� [1]

    [Header("Damage / Stun")]
    public float headOffset = 1.8f;
    public float hitStunTime = 0.3f;
    public float hitKnockbackStrength = 3f;
    public float maxShield = 30f;

    public float detectRange = 1f; // �÷��̾� ���� ����
    public float attackRange = .5f; // ���� ����
    public LayerMask playerLayerMask; // �÷��̾� ������ ���� ���̾� ����ũ [2, 3]
    public Bounds spawnAreaBounds = new Bounds(Vector3.zero, Vector3.one * 10f); // ���� ���� Bounds (Wander ����, �⺻�� ����)


    public float waitCoolTime = 0.5f; // ���� �غ� �ð� (wait cool)
    public float attackDuration = 1.0f; // ���� �ִϸ��̼�/���� �ð� (����)
    public float attackCoolTime = 2.0f; // ���� �� ��ٿ� (attack cool)

    [Header("Attack Properties")]
    public float attackDamage = 10f; // ���ݷ�
    // public GameObject attackEffectPrefab; // ���� ����Ʈ ������ (���� ����)
}