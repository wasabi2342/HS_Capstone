using UnityEngine;

[System.Serializable]
public class DoorSpawnSetting
{
    // ���� ������ ��ġ
    public Vector3 position;
    // ���� ������ ȸ���� (�⺻���� Quaternion.identity)
    public Quaternion rotation = Quaternion.identity;
    // ���� �Ҵ�� ���� Ÿ�� 
    public RewardType rewardType;    
}
