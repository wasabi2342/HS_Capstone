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
    public LayerMask enemyLayerMask = 1 << 8; // ��Enemy�� ���̾�

    public ServantSpawnState(ServantFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        // 1) ���� �� ����, �̵� ����
        agent.isStopped = true;
        fsm.IsInvincible = true;
        if (fsm.Anim != null)
        {
            fsm.pv.RPC(nameof(ServantFSM.RPC_PlayClip), RpcTarget.Others, "Spawn");
            fsm.Anim.Play("Spawn", 0);
        }

        // 3) ���� ��ġ ���� �� ��� ������
        var hits = Physics.OverlapSphere(transform.position, damageRadius, enemyLayerMask);
        foreach (var c in hits)
            if (c.GetComponentInParent<IDamageable>() is IDamageable d)
                d.TakeDamage(damageAmount, AttackerType.PinkPlayer);

        // 4) ��ٸ� �� �б�
        if (PhotonNetwork.IsMasterClient)
            fsm.StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(spawnAnimDuration);

        // ���� ����
        fsm.IsInvincible = false;

        // �ֺ��� ���� ������ ���� ����, ������ Wander��
        if(fsm.TargetEnemy != null && Vector3.SqrMagnitude(fsm.TargetEnemy.position - fsm.transform.position)<=fsm.attackRange * fsm.attackRange)
            fsm.TransitionToState(typeof(ServantWaitCoolState));
        else
            fsm.TransitionToState(typeof(ServantIdleState));
    }

    public override void Execute() { }
    public override void Exit() { }
}
