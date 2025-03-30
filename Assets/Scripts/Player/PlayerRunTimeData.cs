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
    public BlessingInfo[] blessingInfo;

    //private static readonly string filePath = Path.Combine(Application.persistentDataPath, "playerData.json");
    private static readonly string filePath = "PlayerRunTimeData.json";
    public PlayerRunTimeData(float attackPower, float attackSpeed, float moveSpeed, float cooldownReductionPercent, float abilityPower, float maxtHealth)
    {
        this.attackPower = attackPower;
        this.attackSpeed = attackSpeed;
        this.moveSpeed = moveSpeed;
        this.cooldownReductionPercent = cooldownReductionPercent;
        this.abilityPower = abilityPower;
        this.currentHealth = maxtHealth;
        blessingInfo = new BlessingInfo[(int)Skills.Max];
        for(int i = 0 ; i < blessingInfo.Length; i++)
        {
            blessingInfo[i] = new BlessingInfo(Blessings.None, 0);
        }
    }

    public void SaveToJsonFile()
    {
        // PhotonNetwork.CurrentRoom.CustomProperties[PhotonNetwork.LocalPlayer.ActorNumber + "RunTimeData"] = JsonUtility.ToJson(this); 

        File.WriteAllText(filePath, JsonUtility.ToJson(this, true));
        Debug.Log($"PlayerRunTimeData 저장됨: {filePath}");
    }

    public void LoadFromJsonFile()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PlayerRunTimeData loadedData = JsonUtility.FromJson<PlayerRunTimeData>(json);

            this.attackPower = loadedData.attackPower;
            this.attackSpeed = loadedData.attackSpeed;
            this.moveSpeed = loadedData.moveSpeed;
            this.cooldownReductionPercent = loadedData.cooldownReductionPercent;
            this.abilityPower = loadedData.abilityPower;
            this.currentHealth = loadedData.currentHealth;
            blessingInfo = loadedData.blessingInfo;

            Debug.Log($"PlayerRunTimeData 로드됨: {json}");
        }
        else
        {
            Debug.Log("저장된 데이터가 없어 기본값을 사용합니다.");
        }
    }

    public void DeleteRunTimeData()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"PlayerRunTimeData 삭제됨: {filePath}");
        }
        else
        {
            Debug.Log("삭제할 파일이 없습니다.");
        }
    }
}
