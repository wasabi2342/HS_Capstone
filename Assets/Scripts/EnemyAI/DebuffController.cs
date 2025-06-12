
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class DebuffController : MonoBehaviour
{
    /* ───── 타입 캐시 ───── */
    IDamageable damageable;
    NavMeshAgent agent;      // 몬스터만 있을 수도
    IMovable movable;    // 플레이어만 있을 수도
    Animator anim;
    PhotonView pv;

    /* ───── 코루틴 핸들 ───── */
    Coroutine dotCo, slowCo, bindCo;

    /* ───── 속도 원본 백업 ───── */
    float baseAgentSpeed = -1f;   // −1f ⇒ 아직 미초기화
    float baseMoveSpeed = -1f;

    /* ─────────────────────────────────────── 초기화 ─────────────────────────────────────── */
    void Awake()
    {
        damageable = GetComponent<IDamageable>();
        agent = GetComponent<NavMeshAgent>();
        movable = GetComponent<IMovable>();
        anim = GetComponent<Animator>();
        pv = GetComponent<PhotonView>();
    }

    /* ───────────────────────────────── 디버프 적용 API ───────────────────────────────── */
    /// <summary>
    /// <para>type  : SpecialEffectType.Dot / Slow / Bind</para>
    /// <para>dur   : 지속시간(초)</para>
    /// <para>value : DOT 은 초당 데미지, Slow 는 rate(0.3 = 30 %↓), Bind 는 0</para>
    /// </summary>
    public void ApplyDebuff(SpecialEffectType type, float dur, float value)
    {
        if (!pv.IsMine) return;                     // 권한 없는 복제본은 무시

        switch (type)
        {
            case SpecialEffectType.Dot:
                Restart(ref dotCo, Co_DOT(dur, value));
                break;

            case SpecialEffectType.Slow:
                RestoreBaseSpeed();                 // 겹칠 때 기준 속도 복구
                Restart(ref slowCo, Co_Slow(dur, Mathf.Clamp01(value)));
                break;

            case SpecialEffectType.Bind:
                Restart(ref bindCo, Co_Bind(dur));
                break;
        }
    }

    /* ───────────────────────────── DOT ───────────────────────────── */
    IEnumerator Co_DOT(float dur, float dps)
    {
        for (float t = 0f; t < dur; t += 1f)
        {
            damageable?.TakeDamage(dps, transform.position, AttackerType.Debuff);
            yield return new WaitForSeconds(1f);
        }
    }

    /* ───────────────────────────── Slow ───────────────────────────── */
    IEnumerator Co_Slow(float dur, float rate)
    {
        /* 최초 한 번만 원본 캐싱 */
        if (baseAgentSpeed < 0f && agent) baseAgentSpeed = agent.speed;
        if (baseMoveSpeed < 0f && movable != null) baseMoveSpeed = movable.MoveSpeed;

        /* 지속적으로 감속 유지 (0.1 초 간격) */
        float t = 0f;
        while (t < dur)
        {
            if (agent) agent.speed = baseAgentSpeed * (1f - rate);
            if (movable != null) movable.MoveSpeed = baseMoveSpeed * (1f - rate);

            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }

        /* 완전 복구 */
        RestoreBaseSpeed();
        slowCo = null;
    }

    /* ───────────────────────────── Bind ───────────────────────────── */
    IEnumerator Co_Bind(float dur)
    {
        float originalAnimSpeed = anim ? anim.speed : 1f;

        float rpcTick = 0f;
        float t = 0f;
        while (t < dur)
        {
            /* 이동/입력 완전 정지 */
            if (agent) agent.isStopped = true;
            if (movable != null) movable.StopMove(true);

            /* 애니메이터 로컬 즉시 보정 */
            if (anim && anim.speed != 0f) anim.speed = 0f;

            /* 0.2 초 주기로 ‘speed = 0’ RPC 재적용 → FSM Enter() 덮어쓰기 대응 */
            rpcTick += Time.deltaTime;
            if (rpcTick >= 0.2f)
            {
                pv.RPC(nameof(RPC_SetAnimSpeed), RpcTarget.AllBuffered, 0f);
                rpcTick = 0f;
            }

            yield return null;
            t += Time.deltaTime;
        }

        /* 복구 */
        if (agent) agent.isStopped = false;
        if (movable != null) movable.StopMove(false);
        pv.RPC(nameof(RPC_SetAnimSpeed), RpcTarget.AllBuffered, originalAnimSpeed);
        bindCo = null;
    }

    [PunRPC]
    void RPC_SetAnimSpeed(float spd)
    {
        if (anim) anim.speed = spd;
    }

    /* ───────────────────────────── 헬퍼 ───────────────────────────── */
    void Restart(ref Coroutine co, IEnumerator routine)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(routine);
    }

    void RestoreBaseSpeed()
    {
        if (baseAgentSpeed >= 0f && agent) agent.speed = baseAgentSpeed;
        if (baseMoveSpeed >= 0f && movable != null) movable.MoveSpeed = baseMoveSpeed;
    }
}
