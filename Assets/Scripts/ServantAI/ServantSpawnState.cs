// ServantSpawnState.cs
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ServantSpawnState : ServantBaseState
{
    [Header("Spawn Settings")]
    public float spawnAnimDuration = 1.2f;
    public float damageRadius = 3f;
    public int damageAmount = 20;
    public LayerMask enemyLayerMask = 1 << 8; // “Enemy” 레이어

    public ServantSpawnState(ServantFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        // 1) 스폰 중 무적, 이동 정지
        agent.isStopped = true;
        fsm.IsInvincible = true;
        if (fsm.Anim != null)
        {
            fsm.pv.RPC(nameof(ServantFSM.RPC_PlayClip), RpcTarget.Others, "Spawn");
            fsm.Anim.Play("Spawn", 0);
        }

        // 3) 스폰 위치 범위 내 즉시 데미지
        var hits = Physics.OverlapSphere(transform.position, damageRadius, enemyLayerMask);
        foreach (var c in hits)
            if (c.GetComponentInParent<IDamageable>() is IDamageable d)
                d.TakeDamage(damageAmount, AttackerType.PinkPlayer);

        // 4) 기다림 후 분기
        if (PhotonNetwork.IsMasterClient)
            fsm.StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(spawnAnimDuration);

        // 무적 해제
        fsm.IsInvincible = false;

        // 주변에 적이 있으면 곧장 공격, 없으면 Wander로
        if(fsm.TargetEnemy != null && Vector3.SqrMagnitude(fsm.TargetEnemy.position - fsm.transform.position)<=fsm.attackRange * fsm.attackRange)
            fsm.TransitionToState(typeof(ServantWaitCoolState));
        else
            fsm.TransitionToState(typeof(ServantIdleState));
    }

    public override void Execute() { }
    public override void Exit() { }
}
