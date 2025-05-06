using UnityEngine;

[CreateAssetMenu(menuName = "EnemyStatus Data")]
public class EnemyStatusSO : ScriptableObject
{
    [Header("Identification")]
    public string enemyName = "Default Enemy"; // 적 이름


    public float maxHealth = 100f; // 최대 체력
    public float moveSpeed = 2f; // 기본 이동 속도
    public float chaseSpeedMultiplier = 1.1f; // 추격 시 이동 속도 배율 [1]

    [Header("Damage / Stun")]
    public float headOffset = 1.8f;
    public float hitStunTime = 0.3f;
    public float hitKnockbackStrength = 3f;
    public float maxShield = 30f;

    public float detectRange = 1f; // 플레이어 감지 범위
    public float attackRange = .5f; // 공격 범위
    public LayerMask playerLayerMask; // 플레이어 감지를 위한 레이어 마스크 [2, 3]
    public Bounds spawnAreaBounds = new Bounds(Vector3.zero, Vector3.one * 10f); // 스폰 영역 Bounds (Wander 범위, 기본값 설정)


    public float waitCoolTime = 0.5f; // 공격 준비 시간 (wait cool)
    public float attackDuration = 1.0f; // 공격 애니메이션/판정 시간 (추정)
    public float attackCoolTime = 2.0f; // 공격 후 쿨다운 (attack cool)

    [Header("Attack Properties")]
    public float attackDamage = 10f; // 공격력
    // public GameObject attackEffectPrefab; // 공격 이펙트 프리팹 (선택 사항)
}