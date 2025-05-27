// Assets/Scripts/Common/DebuffController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// DOT/Slow/Bind를 모든 객체(몬스터·플레이어)에 적용하는 공통 컨트롤러
/// </summary>
public class DebuffController : MonoBehaviour
{
    /* ───── 캐시 ───── */
    IDamageable damageable;      // 체력 인터페이스 (필수)
    NavMeshAgent agent;          // 몬스터 전용
    IMovable movable;            // 플레이어 전용 
    Coroutine dotCo, slowCo, bindCo;

    /* ──────────────── 초기화 ──────────────── */
    void Awake()
    {
        damageable = GetComponent<IDamageable>();      
        if (damageable == null)
            Debug.LogWarning($"{name}에 IDamageable이 없습니다!", this);

        agent = GetComponent<NavMeshAgent>();        
        movable = GetComponent<IMovable>();           
    }


    /* ────────────────  퍼블릭 API  ──────────────── */

    public void ApplyDebuff(SpecialEffectType type, float duration, float value)
    {
        switch (type)
        {
            case SpecialEffectType.Dot: Restart(ref dotCo, DamageOverTime(duration, value)); break;
            case SpecialEffectType.Slow: Restart(ref slowCo, ApplySlow(duration, value)); break;
            case SpecialEffectType.Bind: Restart(ref bindCo, ApplyBind(duration)); break;
        }
    }

    /* ──────────────── 구현부 ──────────────── */

    // 1) 지속 피해
    IEnumerator DamageOverTime(float dur, float dps)
    {
        for (float t = 0f; t < dur; t += 1f)
        {
            damageable?.TakeDamage(dps, transform.position, AttackerType.Debuff);
            yield return new WaitForSeconds(1f);
        }
    }

    // 2) 이동 속도 감소 (rate = 0.3 → 30 % 감소)
    IEnumerator ApplySlow(float dur, float rate)
    {
        /* NavMeshAgent */
        bool hasAgent = agent != null;
        float originalAgentSpeed = 0f;
        if (hasAgent)
        {
            originalAgentSpeed = agent.speed;
            agent.speed *= 1f - rate;
        }

        /* IMovable (플레이어) */
        bool hasMovable = movable != null;
        float originalMoveSpeed = 0f;
        if (hasMovable)
        {
            originalMoveSpeed = movable.MoveSpeed;
            movable.MoveSpeed *= 1f - rate;
        }

        yield return new WaitForSeconds(dur);

        if (hasAgent) agent.speed = originalAgentSpeed;
        if (hasMovable) movable.MoveSpeed = originalMoveSpeed;
    }

    // 3) 이동/입력 완전 봉인
    IEnumerator ApplyBind(float dur)
    {
        bool hasAgent = agent != null;
        bool hasMovable = movable != null;

        if (hasAgent) agent.isStopped = true;
        if (hasMovable) movable.StopMove(true);

        yield return new WaitForSeconds(dur);

        if (hasAgent) agent.isStopped = false;
        if (hasMovable) movable.StopMove(false);
    }

    /* ──────────────── 헬퍼 ──────────────── */
    void Restart(ref Coroutine co, IEnumerator routine)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(routine);
    }
}
