using UnityEngine;
using System.Collections;

public class AttackBox : MonoBehaviour
{
    private EnemyStateController enemyState;
    private bool canAttack = true; // ���� ���� ����

    void Start()
    {
        enemyState = GetComponentInParent<EnemyStateController>();
        if (enemyState == null)
        {
            Debug.LogError("[����] EnemyStateController�� ã�� �� �����ϴ�!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canAttack) return; // ���� ������ ���� ����
        if (!gameObject.activeSelf) return; // AttackBox�� ��Ȱ��ȭ ���¶�� ���� X

        if (other.CompareTag("Player"))
        {
            Debug.Log("[���� �غ�] �÷��̾� ����! ���� ���...");
            StartCoroutine(AttackSequence(other));
        }
    }

    IEnumerator AttackSequence(Collider player)
    {
        canAttack = false;
        enemyState.StopMovement(); // ���� �غ� �� ���� ����

        yield return new WaitForSeconds(enemyState.status.waitCool); // ��� �� ����

        Debug.Log("[����] ���Ͱ� ���� ����!");
        player.GetComponent<PlayerController>().TakeDamage(enemyState.status.dmg);

        yield return new WaitForSeconds(enemyState.status.attackCool); // ��Ÿ�� ���

        enemyState.ResumeMovement(); // ���� ���� ���� �� ���� �̵� �簳
        canAttack = true;
    }
}
