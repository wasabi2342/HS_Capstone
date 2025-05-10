using Photon.Pun;
using UnityEngine;

public enum AttackerType
{
    Default,
    WhitePlayer,
    PinkPlayer,
    Enemy
}

public interface IDamageable
{
    /// <summary>
    /// MasterClient 전용: 실 HP 감소 로직
    /// </summary>
    [PunRPC] public void DamageToMaster(float damage, Vector3 attackerPos);
    /// <summary>
    /// MasterClient 가 HP 계산 후, 모든 클라이언트에 HP 비율 전송
    /// </summary>
    [PunRPC] public void UpdateHP(float damage);

    public void TakeDamage(float dmg, Vector3 attackerPos, AttackerType type = AttackerType.Default);
}
