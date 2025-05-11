using System.Collections;
using UnityEngine;

public class ScarecrowAttack : IState
{
    readonly ScarecrowFSM fsm;
    Coroutine co;
    public ScarecrowAttack(ScarecrowFSM f) { fsm = f; }

    public void Enter()
    {
        fsm.PlayAnim("Attack");
        co = fsm.StartCoroutine(Routine());
    }
    public void Execute() { }
    public void Exit() 
    {
        if (co != null) fsm.StopCoroutine(co);
    }
    /* ── Attack 애니 끝나면 Idle로 복귀 ── */
    IEnumerator Routine()
    {
        // 애니메이션 길이(attackDur) 만큼 대기
        yield return new WaitForSeconds(fsm.attackDur);

        /* 다음 주기를 위해 Idle로 전환.
           attackEnabled 값은 그대로 유지되므로
           Idle → attackInterval(3초) 경과 → Attack … 반복 */
        fsm.TransitionTo(typeof(ScarecrowIdle));

        // Idle로 넘어가면 Idle.Execute()에서 attackTimer가 다시 돌기 시작
    }
}
