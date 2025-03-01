using UnityEngine;

[CreateAssetMenu(menuName = "Character Data")]
public class CharacterStats : ScriptableObject
{
    public string characterName;
    public int maxHP;
    public float attackPower;
    public float attackSpeed;
    public float moveSpeed;
    public float cooldownReductionPercent;
    public float abilityPower;
}