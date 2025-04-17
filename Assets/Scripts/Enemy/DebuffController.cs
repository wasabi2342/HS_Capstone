using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DebuffController : MonoBehaviour
{
    private EnemyAI enemyAI;
    private Coroutine dotCoroutine;
    private Coroutine slowCoroutine;
    private Coroutine bindCoroutine;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    public void ApplyDebuff(SpecialEffectType type, float duration, float value)
    {
        switch (type)
        {
            case SpecialEffectType.Dot:
                if (dotCoroutine != null) StopCoroutine(dotCoroutine);
                dotCoroutine = StartCoroutine(DamageOverTime(duration, value));
                break;

            case SpecialEffectType.Slow:
                if (slowCoroutine != null) StopCoroutine(slowCoroutine);
                slowCoroutine = StartCoroutine(ApplySlow(duration, value));
                break;

            case SpecialEffectType.Bind:
                if (bindCoroutine != null) StopCoroutine(bindCoroutine);
                bindCoroutine = StartCoroutine(ApplyBind(duration));
                break;
        }
    }

    private IEnumerator DamageOverTime(float duration, float dps)
    {
        float timer = 0f;
        while (timer < duration)
        {
            enemyAI.TakeDamage(dps);
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
