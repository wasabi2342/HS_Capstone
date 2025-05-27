// Assets/Scripts/Common/DebuffController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

/// <summary>
/// DOT / Slow / Bind ─ 몬스터·플레이어 공용 디버프 컨트롤러
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class DebuffController : MonoBehaviour
{
    /* ───── 캐시 ───── */
    IDamageable dmg;
    NavMeshAgent agent;
    IMovable mv;
    Animator anim;
    PhotonView pv;

    Coroutine dotCo, slowCo, bindCo;

    void Awake()
    {
        dmg = GetComponent<IDamageable>();
        agent = GetComponent<NavMeshAgent>();
        mv = GetComponent<IMovable>();
        anim = GetComponent<Animator>();
        pv = GetComponent<PhotonView>();
    }

    /* ────────────────  퍼블릭 API  ──────────────── */
    public void ApplyDebuff(SpecialEffectType type, float dur, float val)
    {
        if (!pv.IsMine) return;                   // 권한 있는 클라이언트만 로직

        switch (type)
        {
            case SpecialEffectType.Dot: Restart(ref dotCo, Co_DOT(dur, val)); break;
            case SpecialEffectType.Slow: Restart(ref slowCo, Co_Slow(dur, val)); break;
            case SpecialEffectType.Bind: Restart(ref bindCo, Co_Bind(dur)); break;
        }
    }

    /* ──────────────── DOT ──────────────── */
    IEnumerator Co_DOT(float dur, float dps)
    {
        for (float t = 0f; t < dur; t += 1f)
        {
            dmg?.TakeDamage(dps, transform.position, AttackerType.Debuff);
            yield return new WaitForSeconds(1f);
        }
    }

    /* ──────────────── Slow ──────────────── */
    IEnumerator Co_Slow(float dur, float rate)
    {
        float baseAgentSpd = agent ? agent.speed : 0f;
        float baseMoveSpd = mv != null ? mv.MoveSpeed : 0f;

        for (float t = 0f; t < dur; t += 0.1f)
        {
            if (agent) agent.speed = baseAgentSpd * (1f - rate);
            if (mv != null) mv.MoveSpeed = baseMoveSpd * (1f - rate);
            yield return new WaitForSeconds(0.1f);
        }

        if (agent) agent.speed = baseAgentSpd;
        if (mv != null) mv.MoveSpeed = baseMoveSpd;
    }

    /* ──────────────── Bind ────────────────
       1. 모든 이동 정지
       2. Animator.speed = 0 (RPC로 전체 동기화)
    ─────────────────────────────────────── */
    // ... (DOT / Slow 부분은 이전과 동일)

    IEnumerator Co_Bind(float dur)
    {
        float originalAnimSpeed = anim ? anim.speed : 1f;

        float rpcTick = 0f;                 // 모든 클라이언트에 주기적 재전송
        float t = 0f;
        while (t < dur)
        {
            /* ───────── 1) 이동 완전 정지 ───────── */
            if (agent) agent.isStopped = true;
            if (mv != null) mv.StopMove(true);

            /* ───────── 2) 애니메이션 멈춤 ───────── */
            if (anim && anim.speed != 0f) anim.speed = 0f;     // Owner 쪽 즉시 보정

            /*     다른 클라이언트도 상태-엔트리에서 speed=1 로 덮어쓰므로
                   0.2 초마다 RPC 로 ‘다시 0’ 을 강제 적용한다                */
            rpcTick += Time.deltaTime;
            if (rpcTick >= 0.2f)
            {
                pv.RPC(nameof(RPC_SetAnimSpeed), RpcTarget.AllBuffered, 0f);
                rpcTick = 0f;
            }

            yield return null;
            t += Time.deltaTime;
        }

        /* ───── 복구 ───── */
        if (agent) agent.isStopped = false;
        if (mv != null) mv.StopMove(false);
        pv.RPC(nameof(RPC_SetAnimSpeed), RpcTarget.AllBuffered, originalAnimSpeed);
    }


    /* ──────────────── RPC ──────────────── */
    [PunRPC]
    void RPC_SetAnimSpeed(float spd)
    {
        if (anim != null) anim.speed = spd;
    }

    /* ──────────────── 헬퍼 ──────────────── */
    void Restart(ref Coroutine co, IEnumerator next)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(next);
    }
}
