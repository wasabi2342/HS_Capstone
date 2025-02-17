using UnityEngine;
using System.Collections;

public class AttackBox : MonoBehaviour
{
    private EnemyStateController enemyState;
    private bool canAttack = true; // 공격 가능 상태

    void Start()
    {
        enemyState = GetComponentInParent<EnemyStateController>();
        if (enemyState == null)
        {
            Debug.LogError("[오류] EnemyStateController를 찾을 수 없습니다!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canAttack) return; // 공격 가능할 때만 실행
        if (!gameObject.activeSelf) return; // AttackBox가 비활성화 상태라면 실행 X

        if (other.CompareTag("Player"))
        {
            Debug.Log("[공격 준비] 플레이어 감지! 공격 대기...");
            StartCoroutine(AttackSequence(other));
        }
    }

    IEnumerator AttackSequence(Collider player)
    {
        canAttack = false;
        enemyState.StopMovement(); // 공격 준비 → 몬스터 멈춤

        yield return new WaitForSeconds(enemyState.status.waitCool); // 대기 후 공격

        Debug.Log("[공격] 몬스터가 공격 실행!");
        player.GetComponent<PlayerController>().TakeDamage(enemyState.status.dmg);

        yield return new WaitForSeconds(enemyState.status.attackCool); // 쿨타임 대기

        enemyState.ResumeMovement(); // 다음 공격 가능 → 몬스터 이동 재개
        canAttack = true;
    }
}
