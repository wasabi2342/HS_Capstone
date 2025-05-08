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
    /// MasterClient ����: �� HP ���� ����
    /// </summary>
    [PunRPC] public void DamageToMaster(float damage, int attackerActor, Vector3 attackerPos);
    /// <summary>
    /// MasterClient �� HP ��� ��, ��� Ŭ���̾�Ʈ�� HP ���� ����
    /// </summary>
    [PunRPC] public void UpdateHP(float damage);

    public void TakeDamage(float damage, AttackerType attackerType = AttackerType.Default);
}
