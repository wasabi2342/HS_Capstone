using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

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

        skillList = LoadSkillCsv("CSV/skills.csv");
        effectList = LoadSpecialEffectCsv("CSV/effects.csv");
        linkList = LoadBlessingEffectLinkCsv("CSV/blessing_effect_links.csv");
    }

    private List<SkillData> LoadSkillCsv(string resourcePath)
    {
        var list = new List<SkillData>();
        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        if (csvFile == null) return list;

        var lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Trim().Split('\t');
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
            data.Stack = float.Parse(values[9]);

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
            var values = lines[i].Trim().Split('\t');
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
            var values = lines[i].Trim().Split('\t');
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
}

