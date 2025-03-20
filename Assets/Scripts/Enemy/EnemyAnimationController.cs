using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class MonsterAnimationController : MonoBehaviourPunCallbacks, IDamageable
{
    private Animator animator;
    private NavMeshAgent agent;
    private float lastDirection = 1f;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isHit = false;
    private float velocityMagnitude;
    private WeaponBase currentWeapon;
    private int health;

    [SerializeField] private EnemyStatus enemyStatus;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        currentWeapon = GetComponent<WeaponBase>();

        if (enemyStatus == null)
        {
            Debug.LogError("EnemyStatus가 할당되지 않았습니다!");
            return;
        }

        health = enemyStatus.hp;
    }

    void Update()
    {
        if (isDead) return;

        velocityMagnitude = agent.velocity.magnitude;

        if (velocityMagnitude > 0.1f)
        {
            lastDirection = Mathf.Sign(agent.velocity.x);
        }

        animator.SetFloat("velocityX", velocityMagnitude);
        animator.SetFloat("lastDirection", lastDirection);
        animator.SetBool("isMoving", velocityMagnitude > 0.1f);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isHit", isHit);
        animator.SetBool("isDead", isDead);
    }

    public void StartAttack()
    {
        if (isDead) return;

        isAttacking = true;
        animator.SetBool("isAttacking", true);

        Invoke(nameof(ResetAttack), 1.0f);
    }

    private void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    [PunRPC]
    public void DamageToMaster(float damage)
    {
        health -= (int)damage;
        Debug.Log($"{enemyStatus.name} 체력 감소: {health} HP 남음");

        if (health <= 0)
        {
            Die();
        }
    }

    [PunRPC]
    public void UpdateHP(float damage)
    {
        health -= (int)damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(DamageToMaster), RpcTarget.All, damage);
        }
    }

    private void HitReaction()
    {
        if (isHit) return;
        isHit = true;
        animator.SetBool("isHit", true);
        Invoke(nameof(ResetHit), 0.5f);
    }

    private void ResetHit()
    {
        isHit = false;
        animator.SetBool("isHit", false);
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        animator.SetBool("isDead", true);
        PhotonNetwork.Destroy(gameObject);
    }
}
