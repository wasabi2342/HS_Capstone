using Photon.Pun;

public interface IDamageable
{
    
    [PunRPC] public void DamageToMaster(float damage);

    [PunRPC] public void UpdateHP(float damage);

    public void TakeDamage(float damage);
}
