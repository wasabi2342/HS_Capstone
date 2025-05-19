using UnityEngine;

public class ServantSpawnAttack : MonoBehaviour, IMonsterAttack
{
    [SerializeField] GameObject spawnCollider;   // �ݶ��̴� ������Ʈ

    public void SetColliderRight() => ShiftCollider(+1);
    public void SetColliderLeft() => ShiftCollider(-1);
    public void OnAttackAnimationEndEvent() => DisableAttack();

    void ShiftCollider(int dir)     // +1 �� ������, -1 �� ����
    {
        //
    }

    /* IMonsterAttack ���� */
    public void EnableAttack() => spawnCollider.SetActive(true);
    public void DisableAttack() => spawnCollider.SetActive(false);
    public void Attack(Transform target) { /* �ʿ�� ��Ʈ����Ʈ */ }
    public void SetDirection(float s)
    {
        if (s >= 0f) SetColliderRight();
        else SetColliderLeft();
    }
}
