using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SkillData
{
    public Skills skillType;
    public float cooldownDuration;
    public float currentCooldown;
    public bool isReady = true;
    
    public SkillData(Skills type, float duration)
    {
        skillType = type;
        cooldownDuration = duration;
        currentCooldown = 0f;
        isReady = true;
    }
}

public class CooldownManager : MonoBehaviour
{
    private Dictionary<Skills, SkillData> skillDataDictionary = new Dictionary<Skills, SkillData>();
    private Dictionary<Skills, Coroutine> activeCoroutines = new Dictionary<Skills, Coroutine>();
    
    // 쿨다운 업데이트 이벤트
    public UnityEvent<Skills, float> OnCooldownUpdate = new UnityEvent<Skills, float>();
    // 쿨다운 완료 이벤트
    public UnityEvent<Skills> OnCooldownComplete = new UnityEvent<Skills>();

    private CharacterStats characterStats;
    private float cooldownReductionMultiplier = 1f;

    public void Initialize(CharacterStats stats)
    {
        characterStats = stats;
        cooldownReductionMultiplier = 1f - (stats.cooldownReductionPercent / 100f);
        
        // 스킬 데이터 초기화
        RegisterSkill(Skills.Mouse_L, stats.mouseLeftCooldown);
        RegisterSkill(Skills.Mouse_R, stats.mouseRightCooldown);
        RegisterSkill(Skills.Space, stats.spaceCooldown);
        RegisterSkill(Skills.Shift_L, stats.shiftCooldown);
        RegisterSkill(Skills.R, stats.ultimateCooldown);
    }

    public void RegisterSkill(Skills skillType, float cooldownDuration)
    {
        if (!skillDataDictionary.ContainsKey(skillType))
        {
            skillDataDictionary.Add(skillType, new SkillData(skillType, cooldownDuration));
        }
        else
        {
            skillDataDictionary[skillType].cooldownDuration = cooldownDuration;
        }
    }

    public bool IsSkillReady(Skills skillType)
    {
        if (skillDataDictionary.TryGetValue(skillType, out SkillData data))
        {
            return data.isReady;
        }
        Debug.LogWarning($"SkillType {skillType} is not registered in CooldownManager");
        return false;
    }

    public void StartCooldown(Skills skillType)
    {
        if (skillDataDictionary.TryGetValue(skillType, out SkillData data))
        {
            if (activeCoroutines.ContainsKey(skillType) && activeCoroutines[skillType] != null)
            {
                StopCoroutine(activeCoroutines[skillType]);
            }
            
            data.isReady = false;
            float adjustedCooldown = data.cooldownDuration * cooldownReductionMultiplier;
            activeCoroutines[skillType] = StartCoroutine(CooldownCoroutine(skillType, adjustedCooldown));
        }
        else
        {
            Debug.LogWarning($"Cannot start cooldown for unregistered skill: {skillType}");
        }
    }

    public void ResetCooldown(Skills skillType)
    {
        if (skillDataDictionary.TryGetValue(skillType, out SkillData data))
        {
            if (activeCoroutines.ContainsKey(skillType) && activeCoroutines[skillType] != null)
            {
                StopCoroutine(activeCoroutines[skillType]);
                activeCoroutines.Remove(skillType);
            }
            
            data.isReady = true;
            data.currentCooldown = 0f;
            OnCooldownUpdate.Invoke(skillType, 1f); // 1.0 = 100% ready
            OnCooldownComplete.Invoke(skillType);
        }
    }

    private IEnumerator CooldownCoroutine(Skills skillType, float cooldownDuration)
    {
        SkillData data = skillDataDictionary[skillType];
        data.currentCooldown = 0f;
        
        while (data.currentCooldown < cooldownDuration)
        {
            data.currentCooldown += Time.deltaTime;
            float progress = data.currentCooldown / cooldownDuration;
            OnCooldownUpdate.Invoke(skillType, progress);
            yield return null;
        }
        
        data.isReady = true;
        OnCooldownUpdate.Invoke(skillType, 1f);
        OnCooldownComplete.Invoke(skillType);
        
        if (activeCoroutines.ContainsKey(skillType))
        {
            activeCoroutines.Remove(skillType);
        }
    }

    public void UpdateCooldownReduction(float newCooldownReductionPercent)
    {
        cooldownReductionMultiplier = 1f - (newCooldownReductionPercent / 100f);
    }
}
