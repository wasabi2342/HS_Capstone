using UnityEngine;

[CreateAssetMenu(menuName = "Character Data")]
public class CharacterStats : ScriptableObject
{
    public string characterName;
    public int characterId;
    public int maxHP;
    public float attackPower;
    public float attackSpeed;
    public float moveSpeed;
    public float cooldownReductionPercent;
    public float abilityPower;
    //public float mouseLeftCooldown;
    //public float mouseRightCooldown;
    //public float shiftCooldown;
    //public float spaceCooldown;
    //public float ultimateCooldown;
    public int[] skillDatasIndex = new int[5];
}