using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class ChaseState : BaseState
{
    float atkChkT, destT;
    public ChaseState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent)
        {
            SetAgentStopped(false);
            agent.speed = status.moveSpeed *
                          (status.chaseSpeedMultiplier > 0 ? status.chaseSpeedMultiplier : 1f);
        }
        fsm.PlayDirectionalAnim("Walk");
        atkChkT = destT = 0f;
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!fsm.Target || !fsm.Target.gameObject.activeInHierarchy)
        {
            fsm.TransitionToState(typeof(WanderState));
            return;
        }

        // 1) 거리 계산
        float xDiff = fsm.Target.position.x - transform.position.x;
        float xAbs = Mathf.Abs(xDiff);
        float zDiff = fsm.GetZDiffAbs();
        float tol = fsm.zAlignTolerance;
        float atkR = status.attackRange;

        // 2) Z 정렬 단계: Z가 벌어져 있을 때만 시도
        if (zDiff > tol)
        {
            // 2-1) X 차이가 너무 작으면 우회 경로 설정
            if (xAbs <= tol)
            {
                float detourOffset = atkR * 4f * Mathf.Sign(fsm.CurrentFacing);
                // 횡방향으로 한 칸(attackRange) 만큼 비켜서 Z로 이동
                Vector3 detour = transform.position;
                detour.x += detourOffset;
                detour.z = fsm.Target.position.z;

                if (!agent.pathPending)
                    agent.SetDestination(detour);

                fsm.PlayDirectionalAnim("Walk");
                return;
            }
            RefreshFacingToTarget();
            // 2-2) X 차이가 충분히 크면 기존 Z 정렬
            if (agent.stoppingDistance != 0f)
                agent.stoppingDistance = 0f;

            Vector3 mid = transform.position;
            mid.z = fsm.Target.position.z;
            if (!agent.pathPending)
                agent.SetDestination(mid);

            fsm.PlayDirectionalAnim("Walk");
            return;
        }

        // 3) X 접근 단계 (사거리 기준)
        if (agent.stoppingDistance != atkR)
            agent.stoppingDistance = atkR;

        Vector3 dest = fsm.Target.position;
        dest.z = transform.position.z;
        if (!agent.pathPending)
            agent.SetDestination(dest);

        // 4) 사거리 안 도달 시 공격 준비 단계
        if (fsm.GetTarget2DDistSq() <= atkR * atkR)
        {
            fsm.Agent.isStopped = true;
            fsm.TransitionToState(typeof(WaitCoolState));
            return;
        }

        fsm.PlayDirectionalAnim("Walk");
    }

    public override void Exit() { }
}
