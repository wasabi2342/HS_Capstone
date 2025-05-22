using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DebuffController : MonoBehaviour
{
    private EnemyFSM fsm;
    private Coroutine dotCo, slowCo, bindCo;

    private void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
    }
    public void ApplyDebuff(SpecialEffectType type, float duration, float value)
    {
        switch (type)
        {
            case SpecialEffectType.Dot:
                if (dotCo != null) StopCoroutine(dotCo);
                dotCo = StartCoroutine(DamageOverTime(duration, value));
                break;

            case SpecialEffectType.Slow:
                if (slowCo != null) StopCoroutine(slowCo);
                slowCo = StartCoroutine(ApplySlow(duration, value));
                break;

            case SpecialEffectType.Bind:
                if (bindCo != null) StopCoroutine(bindCo);
                bindCo = StartCoroutine(ApplyBind(duration));
                break;
        }
    }

    private IEnumerator DamageOverTime(float duration, float dps)
    {
        float timer = 0f;
        while (timer < duration)
        {
            fsm.TakeDamage(dps, transform.position, AttackerType.Debuff);
            yield return new WaitForSeconds(1f);
            timer += 1f;
        }
    }

    private IEnumerator ApplySlow(float duration, float slowRate)
    {
        var agent = GetComponent<NavMeshAgent>();
        if (agent == null) yield break;

        float originalSpeed = agent.speed;
        agent.speed *= (1f - slowRate);
        yield return new WaitForSeconds(duration);
        agent.speed = originalSpeed;
    }

    private IEnumerator ApplyBind(float duration)
    {
        var agent = GetComponent<NavMeshAgent>();
        if (agent == null) yield break;

        agent.isStopped = true;
        yield return new WaitForSeconds(duration);
        agent.isStopped = false;
    }
}
