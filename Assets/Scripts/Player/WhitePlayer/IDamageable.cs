using Photon.Pun;

public enum AttackerType
{
    Default,
    WhitePlayer,
    PinkPlayer,
    Enemy
}

public interface IDamageable
{
    
    [PunRPC] public void DamageToMaster(float damage);

    [PunRPC] public void UpdateHP(float damage);

    public void TakeDamage(float damage, AttackerType attackerType = AttackerType.Default);
}
