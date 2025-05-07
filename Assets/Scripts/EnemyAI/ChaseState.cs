using UnityEngine;
using Photon.Pun;

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
        { fsm.TransitionToState(typeof(WanderState)); return; }

        float zDiff = fsm.GetZDiffAbs();

        /* ───── Z 맞추기 단계 ───── */
        if (zDiff > fsm.zAlignTolerance)
        {
            /* 멈추지 않도록 stopDist = 0 */
            if (agent.stoppingDistance != 0f)
                agent.stoppingDistance = 0f;

            /* Z 만 동일한 지점으로 이동 */
            Vector3 p = transform.position;
            p.z = fsm.Target.position.z;
            if (!agent.pathPending)
                agent.SetDestination(p);

            fsm.PlayDirectionalAnim("Walk");
            return;                      
        }

        /* ───── X 접근 단계 ───── */
        /* 이제 stopDist 을 attackRange 로 되돌림 */
        if (agent.stoppingDistance != fsm.EnemyStatusRef.attackRange)
            agent.stoppingDistance = fsm.EnemyStatusRef.attackRange;

        /* Z 고정, X 타깃으로 경로 지정 */
        Vector3 dest = fsm.Target.position;
        dest.z = transform.position.z;
        if (!agent.pathPending)
            agent.SetDestination(dest);

        /* 사정거리 안이면 멈추고 WaitCool */
        if (fsm.GetTarget2DDistSq() <=
            fsm.EnemyStatusRef.attackRange * fsm.EnemyStatusRef.attackRange)
        {
            if (fsm.debugMode)
                Debug.Log($"[Chase→WaitCool] dist={Mathf.Sqrt(fsm.GetTarget2DDistSq()):0.00}", fsm);

            fsm.Agent.isStopped=true;
            fsm.TransitionToState(typeof(WaitCoolState));
            return;
        }

        fsm.PlayDirectionalAnim("Walk");
    }



    public override void Exit() { }
}
