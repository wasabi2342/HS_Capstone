using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;

enum Characters
{
    WhitePlayer,
    PinkPlayer,
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }

        skillList = LoadSkillCsv("CSV/Blessing_Table");
        effectList = LoadSpecialEffectCsv("CSV/Special_Table");
        linkList = LoadBlessingEffectLinkCsv("CSV/Bless_Special_Table");
        basicAttackComboDatas = LoadComboCsv("CSV/Norm_Attack_Table");
        r_AttackComboDatas = LoadComboCsv2("CSV/R_Attack_Table");
    }

    private void Start()
    {
        settingDataPath = Path.Combine(Application.persistentDataPath, "setting.json");
        LoadSettingData();
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
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = line.Split(',');
            if (values.Length < 10)
            {
                Debug.LogWarning($"줄 {i} 건너뜀 - 값 부족: {line}");
                continue;
            }

            try
            {
                Debug.Log($"줄 {i} 파싱 시도: {line}");

                var data = ScriptableObject.CreateInstance<SkillData>();
                data.ID = int.Parse(values[0]);
                data.Devil = int.Parse(values[1]);
                data.Bind_Key = int.Parse(values[2]);
                data.Character = int.Parse(values[3]);
                data.Blessing_name = values[4];
                data.Bless_Discript = values[5];

                data.AttackDamageCoefficient = float.Parse(values[6].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                data.AbilityPowerCoefficient = float.Parse(values[7].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                data.Cooldown = float.Parse(values[8].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                data.Stack = int.Parse(values[9]);

                list.Add(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"줄 {i} 파싱 중 오류 발생: {line}\n에러: {e.Message}");
            }
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

    private List<R_AttackComboData> LoadComboCsv2(string resourcePath)
    {
        var list = new List<R_AttackComboData>();
        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        if (csvFile == null) return list;

        var lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Trim().Split(',');
            if (values.Length < 4) continue;

            var data = ScriptableObject.CreateInstance<R_AttackComboData>();
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

    public void SaveSettingData()
    {
        string json = JsonUtility.ToJson(settingData, true);
        File.WriteAllText(settingDataPath, json);
        Debug.Log($"[DataManager] Setting saved to {settingDataPath}");
    }

    public void LoadSettingData()
    {
        if (File.Exists(settingDataPath))
        {
            string json = File.ReadAllText(settingDataPath);
            JsonUtility.FromJsonOverwrite(json, settingData);
            ApplySettings();
            Debug.Log("[DataManager] Setting loaded.");
        }
        else
        {
            Debug.Log("[DataManager] No setting file found. Creating default.");
            SaveSettingData(); // 초기 저장
        }
    }

    private void ApplySettings()
    {
        AudioManager.Instance.SetMasterVolume(settingData.masterVolume);

        Screen.SetResolution(
            settingData.resolution.x,
            settingData.resolution.y,
            settingData.screenMode
        );

        // 추가로 사운드 매니저가 있다면 볼륨 적용
        // SoundManager.Instance?.SetVolumes(settingData.bgmVolume, settingData.sfxVolume);
    }

}

