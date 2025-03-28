using UnityEngine;
using System.IO;
using System;

enum Characters
{
    WhitePlayer,
    Max
}

public class DataManager : MonoBehaviour
{

    public static DataManager Instance { get; private set; }

    public LoadDatas loadDatas;

    string savePath;

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
        savePath = Path.Combine(Application.dataPath, "Resources/skillData.json"); // ���߿� ��� �ٲ����
        LoadData();
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"������ ���� �Ϸ�: {savePath}");
    }

    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            loadDatas = JsonUtility.FromJson<LoadDatas>(json);
            Debug.Log("������ �ε� �Ϸ�");
        }
        else
        {
            Debug.Log("����� ������ ����, �⺻ ������ ����");
            InitializeDefaultData();
            SaveData();
        }
    }

    private void InitializeDefaultData()
    {
        // �迭�� �����ϰ� �ʱ�ȭ
        loadDatas = new LoadDatas
        {
            characterBlessingSkillDatas = new CharacterBlessingSkillData[(int)Characters.Max]
        };

        // WhitePlayer�� ���� ������ �ʱ�ȭ
        loadDatas.characterBlessingSkillDatas[(int)Characters.WhitePlayer] = new CharacterBlessingSkillData
        {
            blessingSkillDatas = new BlessingSkillData[]
            {
            new BlessingSkillData
            {
                skillDatas = new SkillData[]
                {
                    new SkillData
                    {
                        skillName = "���̾",
                        skillInfo = "�ҵ��̸� ���� ���� �����մϴ�.",
                        attackCoefficient = 1.5f,
                        levelUpIncrease = 0.2f,
                        attackSpeed = 1.0f,
                        shieldAmount = 0,
                        cooldown = 5.0f,
                        healAmount = 0
                    },
                    new SkillData
                    {
                        skillName = "��",
                        skillInfo = "ü���� ȸ���մϴ�.",
                        attackCoefficient = 0,
                        levelUpIncrease = 0.1f,
                        attackSpeed = 0,
                        shieldAmount = 0,
                        cooldown = 10.0f,
                        healAmount = 50
                    }
                }
            }
            }
        };
    }
}

