// ServantChaseState.cs
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ServantChaseState : ServantBaseState
{
    const float Z_ALIGN_TOL = 0.1f;  // Z 정렬 허용 오차

    public ServantChaseState(ServantFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        if (agent)
        {
            SetAgentStopped(false);
            float mult = fsm.chaseMultiplier > 0 ? fsm.chaseMultiplier : 1f;
            agent.speed = fsm.moveSpeed * mult;
        }
        PlayDirectionalAnim("Walk");
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (fsm.TargetEnemy == null || !fsm.TargetEnemy.gameObject.activeInHierarchy)
        {
            fsm.TransitionToState(typeof(ServantWanderState));
            return;
        }

        // 거리 계산
        float xDiff = fsm.TargetEnemy.position.x - transform.position.x;
        float xAbs = Mathf.Abs(xDiff);
        float zDiff = Mathf.Abs(fsm.TargetEnemy.position.z - transform.position.z);
        float atkR = fsm.attackRange;

        // Z 정렬
        if (zDiff > Z_ALIGN_TOL)
        {
            // X 너무 가깝다면 우회
            if (xAbs <= Z_ALIGN_TOL)
            {
                float detour = atkR * 4f * Mathf.Sign(fsm.CurrentFacing);
                Vector3 dest = transform.position;
                dest.x += detour;
                dest.z = fsm.TargetEnemy.position.z;
                if (!agent.pathPending)
                    agent.SetDestination(dest);
                PlayDirectionalAnim("Walk");
                return;
            }
            // Z축으로 이동
            RefreshFacingToTarget();
            agent.stoppingDistance = 0f;
            Vector3 mid = transform.position;
            mid.z = fsm.TargetEnemy.position.z;
            if (!agent.pathPending)
                agent.SetDestination(mid);
            PlayDirectionalAnim("Walk");
            return;
        }

        // X 접근
        agent.stoppingDistance = atkR;
        Vector3 aim = fsm.TargetEnemy.position;
        aim.z = transform.position.z;
        if (!agent.pathPending)
            agent.SetDestination(aim);

        // 사거리 도달 시 공격
        if ((fsm.TargetEnemy.position - transform.position).sqrMagnitude <= atkR * atkR)
        {
            agent.isStopped = true;
            fsm.TransitionToState(typeof(ServantAttackState));
            return;
        }

        PlayDirectionalAnim("Walk");
    }

    public override void Exit() { }
}
