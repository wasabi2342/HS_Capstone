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
        savePath = Path.Combine(Application.dataPath, "Resources/skillData.json"); // 나중에 경로 바꿔야함
        LoadData();
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"데이터 저장 완료: {savePath}");
    }

    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            loadDatas = JsonUtility.FromJson<LoadDatas>(json);
            Debug.Log("데이터 로드 완료");
        }
        else
        {
            Debug.Log("저장된 데이터 없음, 기본 데이터 생성");
            InitializeDefaultData();
            SaveData();
        }
    }

    private void InitializeDefaultData()
    {
        // 배열을 생성하고 초기화
        loadDatas = new LoadDatas
        {
            characterBlessingSkillDatas = new CharacterBlessingSkillData[(int)Characters.Max]
        };

        // WhitePlayer에 대한 데이터 초기화
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
                        skillName = "파이어볼",
                        skillInfo = "불덩이를 날려 적을 공격합니다.",
                        attackCoefficient = 1.5f,
                        levelUpIncrease = 0.2f,
                        attackSpeed = 1.0f,
                        shieldAmount = 0,
                        cooldown = 5.0f,
                        healAmount = 0
                    },
                    new SkillData
                    {
                        skillName = "힐",
                        skillInfo = "체력을 회복합니다.",
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

