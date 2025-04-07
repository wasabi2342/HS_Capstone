using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerRunTimeData
{
    public float attackPower;
    public float attackSpeed;
    public float moveSpeed;
    public float cooldownReductionPercent;
    public float abilityPower;
    public float currentHealth;
    public SkillWithLevel[] skillWithLevel;

    //private static readonly string filePath = Path.Combine(Application.persistentDataPath, "playerData.json");
    private static readonly string filePath = "PlayerRunTimeData.json";
    public PlayerRunTimeData(CharacterStats characterStats)
    {
        attackPower = characterStats.attackPower;
        attackSpeed = characterStats.attackSpeed;
        moveSpeed = characterStats.moveSpeed;
        cooldownReductionPercent = characterStats.cooldownReductionPercent;
        abilityPower = characterStats.abilityPower;
        currentHealth = characterStats.maxHP;

        skillWithLevel = new SkillWithLevel[(int)Skills.Max];
        for(int i = 0 ; i < skillWithLevel.Length; i++)
        {
            skillWithLevel[i] = new SkillWithLevel(DataManager.Instance.skillList[characterStats.skillDatasIndex[i]], 0);
        }
    }

    public void SaveToJsonFile()
    {
        // PhotonNetwork.CurrentRoom.CustomProperties[PhotonNetwork.LocalPlayer.ActorNumber + "RunTimeData"] = JsonUtility.ToJson(this); 

        File.WriteAllText(filePath, JsonUtility.ToJson(this, true));
        Debug.Log($"PlayerRunTimeData �����: {filePath}");
    }

    public void LoadFromJsonFile()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PlayerRunTimeData loadedData = JsonUtility.FromJson<PlayerRunTimeData>(json);

            attackPower = loadedData.attackPower;
            attackSpeed = loadedData.attackSpeed;
            moveSpeed = loadedData.moveSpeed;
            cooldownReductionPercent = loadedData.cooldownReductionPercent;
            abilityPower = loadedData.abilityPower;
            currentHealth = loadedData.currentHealth;
            skillWithLevel = loadedData.skillWithLevel;

            Debug.Log($"PlayerRunTimeData �ε��: {json}");
        }
        else
        {
            Debug.Log("����� �����Ͱ� ���� �⺻���� ����մϴ�.");
        }
    }

    public void DeleteRunTimeData()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"PlayerRunTimeData ������: {filePath}");
        }
        else
        {
            Debug.Log("������ ������ �����ϴ�.");
        }
    }
}
