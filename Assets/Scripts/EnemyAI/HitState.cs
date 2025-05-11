using UnityEngine;
using System.Collections;
using Photon.Pun;

public class HitState : BaseState
{
    Coroutine stunCo;
    Vector3 knockbackVelocity;          
    const float KNOCKBACK_DURATION = .5f;
    float knockbackTime;

    public HitState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        FaceLastAttacker();
        SetAgentStopped(true);
        FlashSprite();
        ApplyKnockbackFromLastAttacker();
        fsm.PlayDirectionalAnim("Hit");

        /* 체력 0 -> 즉시 DeadState */
        if (fsm.currentHP <= 0)
        {
            fsm.TransitionToState(typeof(DeadState));
            return;
        }

        /* 스턴 루틴 */
        if (PhotonNetwork.IsMasterClient)
            stunCo = fsm.StartCoroutine(Stun());

    }
    void FaceLastAttacker()
    {
        float dx = fsm.LastHitPos.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)
            fsm.ForceFacing(dx);   // + → 오른쪽, – → 왼쪽
    }

    public override void Execute()
    {
        /* 넉백 동안 이동 */
        if (knockbackTime > 0f)
        {
            float t = Time.deltaTime;
            fsm.transform.position += knockbackVelocity * t;
            knockbackTime -= t;
        }

        fsm.PlayDirectionalAnim("Hit");
    }

    IEnumerator Stun()
    {
        yield return new WaitForSeconds(status.hitStunTime);
        if (!PhotonNetwork.IsMasterClient) yield break;

        fsm.TransitionToState(
            fsm.Target ? typeof(ChaseState) : typeof(WanderState));
    }

    /* ───────── Aux Methods ───────── */

    void SpawnBloodFx()                                           
    {
        var prefab = (fsm.CurrentFacing >= 0)
            ? fsm.BloodFxRight : fsm.BloodFxLeft;
        var slash = (fsm.CurrentFacing >= 0)
            ? fsm.SlashFxRight : fsm.SlashFxLeft;

        if (prefab) Object.Instantiate(prefab,
            fsm.transform.position + Vector3.down * 3f, Quaternion.identity);
        if (slash) Object.Instantiate(slash,
            fsm.transform.position + Vector3.down * 3f, Quaternion.identity);
    }

    void FlashSprite()                                          
    {
        if (!fsm.SR) return;
        fsm.StartCoroutine(CoFlash());
        IEnumerator CoFlash()
        {
            var c = fsm.SR.color;
            fsm.SR.color = new Color(1f, .3f, .3f);
            yield return new WaitForSeconds(.1f);
            if (fsm) fsm.SR.color = c;
        }
    }

    void ApplyKnockbackFromLastAttacker()                                      
    {
        if (fsm.Target == null) return;
        Vector3 dir = (fsm.transform.position - fsm.LastHitPos).normalized;
        dir.y = 0f;
        dir.z = 0f;
        knockbackVelocity = dir * status.hitKnockbackStrength;
        knockbackTime = KNOCKBACK_DURATION;
    }

    public override void Exit()
    {
        if (stunCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(stunCo);
    }
}
