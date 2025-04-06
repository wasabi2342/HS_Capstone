using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;

enum Characters
{
    WhitePlayer,
    Max
}

public class DataManager : MonoBehaviour
{
    public List<SkillData> skillList;
    public List<SpecialEffectData> effectList;
    public List<BlessingEffectLinkData> linkList;
    public List<BasicAttackComboData> basicAttackComboDatas;

    public static DataManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(gameObject);

        skillList = LoadSkillCsv("CSV/skills");
        effectList = LoadSpecialEffectCsv("CSV/effects");
        linkList = LoadBlessingEffectLinkCsv("CSV/blessing_effect_links");
        basicAttackComboDatas = LoadComboCsv("CSV/BasicAttackCombo");
    }

    private List<SkillData> LoadSkillCsv(string resourcePath)
    {
        Debug.Log("LoadSkillCsv 호출");

        var list = new List<SkillData>();
        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        if (csvFile == null)
        {
            Debug.Log("파일 없음");
            return list;
        }

        var lines = csvFile.text.Split('\n');
        Debug.Log(csvFile.text);

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Trim().Split(',');
            Debug.Log(values);
            if (values.Length < 10) continue;

            var data = ScriptableObject.CreateInstance<SkillData>();
            data.ID = int.Parse(values[0]);
            data.Devil = int.Parse(values[1]);
            data.Bind_Key = int.Parse(values[2]);
            data.Character = int.Parse(values[3]);
            data.Blessing_name = values[4];
            data.Bless_Discript = values[5];
            data.AttackDamageCoefficient = float.Parse(values[6]);
            data.AbilityPowerCoefficient = float.Parse(values[7]);
            data.Cooldown = float.Parse(values[8]);
            data.Stack = int.Parse(values[9]);

            list.Add(data);
        }
        return list;
    }

    private List<SpecialEffectData> LoadSpecialEffectCsv(string resourcePath)
    {
        var list = new List<SpecialEffectData>();
        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        if (csvFile == null) return list;

        var lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Trim().Split(',');
            if (values.Length < 3) continue;

            var data = ScriptableObject.CreateInstance<SpecialEffectData>();
            data.ID = int.Parse(values[0]);
            data.EffectName = values[1];
            data.Description = values[2];

            list.Add(data);
        }
        return list;
    }

    private List<BlessingEffectLinkData> LoadBlessingEffectLinkCsv(string resourcePath)
    {
        var list = new List<BlessingEffectLinkData>();
        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        if (csvFile == null) return list;

        var lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Trim().Split(',');
            if (values.Length < 5) continue;

            var data = ScriptableObject.CreateInstance<BlessingEffectLinkData>();
            data.ID = int.Parse(values[0]);
            data.Blessing_ID = int.Parse(values[1]);
            data.SpecialEffect_ID = int.Parse(values[2]);
            data.Value = float.Parse(values[3]);
            data.Duration = float.Parse(values[4]);

            list.Add(data);
        }
        return list;
    }

    private List<BasicAttackComboData> LoadComboCsv(string resourcePath)
    {
        var list = new List<BasicAttackComboData>();
        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        if (csvFile == null) return list;

        var lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Trim().Split(',');
            if (values.Length < 4) continue;

            var data = ScriptableObject.CreateInstance<BasicAttackComboData>();
            data.ID = int.Parse(values[0]);
            data.Character = int.Parse(values[1]);
            data.Combo_Index = int.Parse(values[2]);
            data.Damage = float.Parse(values[3]);

            list.Add(data);
        }
        return list;
    }

    public SkillData FindSkillByBlessingKeyAndCharacter(int bindKey, int blessing, int character)
    {
        return skillList.Find(skill =>
        skill.Devil == blessing &&
        skill.Bind_Key == bindKey &&
        skill.Character == character
        );
    }

    public int FindLinkIDBySkillID(int skillID)
    {
        // 스킬 존재 확인
        SkillData skill = skillList.Find(s => s.ID == skillID);
        if (skill == null)
        {
            Debug.LogWarning($"Skill ID {skillID} not found.");
            return -1;
        }

        // 스킬 ID에 연결된 특수 효과 찾기
        BlessingEffectLinkData link = linkList.Find(l => l.Blessing_ID == skillID);
        if (link != null)
        {
            return link.ID;
        }

        return -1;
    }

    public int FindSpecialEffectIDBySkillID(int skillID)
    {
        // 스킬 존재 확인
        SkillData skill = skillList.Find(s => s.ID == skillID);
        if (skill == null)
        {
            Debug.LogWarning($"Skill ID {skillID} not found.");
            return -1;
        }

        // 스킬 ID에 연결된 특수 효과 찾기
        BlessingEffectLinkData link = linkList.Find(l => l.Blessing_ID == skillID);
        if (link != null)
        {
            return link.SpecialEffect_ID;
        }

        return -1;
    }

    public float FindDamageByCharacterAndComboIndex(int character, int comboIndex)
    {
        var combo = basicAttackComboDatas.Find(c => c.Character == character && c.Combo_Index == comboIndex);
        if (combo != null)
        {
            return combo.Damage;
        }

        Debug.LogWarning($"Combo not found for Character: {character}, ComboIndex: {comboIndex}");
        return -1f;
    }

}

