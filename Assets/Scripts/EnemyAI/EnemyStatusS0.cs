using UnityEngine;

[CreateAssetMenu(menuName = "EnemyStatus Data")]
public class EnemyStatusSO : ScriptableObject
{
    [Header("Identification")]
    public string enemyName = "Default Enemy"; // �� �̸�
    public Bounds spawnAreaBounds = new Bounds(Vector3.zero, Vector3.one * 10f); // ���� ���� Bounds (Wander ����, �⺻�� ����)

    [Header("����")]
    public float maxHealth = 100f; // �ִ� ü��
    public float moveSpeed = 2f; // �⺻ �̵� �ӵ�
    public float chaseSpeedMultiplier = 1.1f; // �߰� �� �̵� �ӵ� ���� [1]
    public float navMeshSampleDistance = 3f; // NavMesh ���ø� �Ÿ� (�̵� ��)

    [Header("���� ����")]
    public float maxChaseDistance = 5f;     // ������Ÿ�� �ִ� �Ÿ�
    public float maxChaseTime = 5f;


    [Header("�ǰ�")]
    public float headOffset = 1.8f;
    public float hitStunTime = 0.3f;
    public float hitKnockbackStrength = 3f;
    public float maxShield = 30f;

    [Header("���� �� ����")]
    public float detectRange = 1f; // �÷��̾� ���� ����
    public float attackRange = .5f; // ���� ����
    public LayerMask playerLayerMask; // �÷��̾� ������ ���� ���̾� ����ũ [2, 3]

    public float attackDuration = 1.0f; // ���� �ִϸ��̼�/���� �ð� (����)
    public float attackCoolTime = 2.0f; // ���� �� ��ٿ� (attack cool)

    [Header("���ݷ�")]
    public float attackDamage = 10f; // ���ݷ�
    [Header("���� ����")]
    public AttackWeight[] attackWeights;

    [System.Serializable]
    public class AttackWeight
    {
        public string scriptName;
        [Range(0f, 1f)]
        public float weight = 0.5f;
    }
}