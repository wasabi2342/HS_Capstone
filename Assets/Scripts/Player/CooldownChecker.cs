using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CooldownChecker
{
    private float cooldownTime;
    private int maxStacks;
    private int currentStacks;
    private Coroutine coroutine;
    private float timer;
    private UnityEvent<float, float> onCooldownUpdate;
    public CooldownChecker(float cooldownTime, UnityEvent<float, float> onCooldownUpdate, int maxStacks = 1)
    {
        this.cooldownTime = cooldownTime;
        this.maxStacks = maxStacks;
        currentStacks = maxStacks;
        timer = 0f;
        this.onCooldownUpdate = onCooldownUpdate;
    }

    public bool CanUse()
    {
        return currentStacks > 0;
    }

    public void Use(MonoBehaviour mono)
    {
        if (currentStacks > 0)
        {
            currentStacks--;
            if (coroutine == null)
            {
                coroutine = mono.StartCoroutine(StartCooldown());
            }
        }
    }

    public void ResetCooldown(MonoBehaviour mono)
    {
        if (coroutine != null)
        {
            mono.StopCoroutine(coroutine);
            coroutine = null;
        }
        onCooldownUpdate?.Invoke(0, cooldownTime);
        currentStacks++;
        currentStacks = Mathf.Min(currentStacks, maxStacks);
    }

    public void ReduceCooldown(float time)
    {
        if (timer > 0f)
        {
            timer -= time;
        }
    }

    private IEnumerator StartCooldown()
    {
        timer = cooldownTime;
        while (currentStacks < maxStacks)
        {
            yield return null;
            timer -= Time.deltaTime;
            onCooldownUpdate?.Invoke(timer, cooldownTime);

            if (timer <= 0)
            {
                timer = cooldownTime;
                currentStacks++;
                onCooldownUpdate?.Invoke(0, cooldownTime);
            }
        }
        coroutine = null;
    }
}
