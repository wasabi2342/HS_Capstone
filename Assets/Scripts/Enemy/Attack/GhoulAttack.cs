using UnityEngine;
using System.Collections;
using Photon.Pun;

public class GhoulAttack : MonoBehaviourPun, IMonsterAttack
{
    [Header("Collider / FX")]
    [SerializeField] GameObject weaponCollider;      // 공격 콜라이더

    [Header("Lunge Settings")]
    [Tooltip("런지(전진) 거리")]
    [SerializeField] float lungeDistance = 0.2f;
    [Tooltip("런지에 걸리는 시간")]
    [SerializeField] float lungeDuration = 0.15f;

    /* ───────── 내부 상태 ───────── */
    float facing = 1f;          // +1 ⇒ Right, –1 ⇒ Left
    bool isLunging;
    EnemyFSM fsm;               // 위치 이동에 사용

    public string AnimKey => "Attack1";

    void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        weaponCollider.SetActive(false);            // 기본 비활성
    }

    /* ---------- IMonsterAttack ---------- */
    public void SetDirection(float dir) => facing = Mathf.Sign(dir);

    /// <summary>AttackState 코루틴에서 윈드업 후 호출</summary>
    public void Attack(Transform target)
    {
        if (!PhotonNetwork.IsMasterClient || isLunging) return;
        StartCoroutine(LungeRoutine());
    }

    public void EnableAttack() => weaponCollider.SetActive(true);
    public void DisableAttack() => weaponCollider.SetActive(false);

    /* ---------- Animation Event ---------- */
    public void SetColliderRight() => ShiftCollider(+1);
    public void SetColliderLeft() => ShiftCollider(-1);
    public void OnAttackAnimationEndEvent() => DisableAttack();

    /* ───────── 런지 구현 ───────── */
    IEnumerator LungeRoutine()
    {
        isLunging = true;

        Vector3 dir = Vector3.right * facing;
        float speed = lungeDistance / lungeDuration;
        float elapsed = 0f;

        while (elapsed < lungeDuration)
        {
            float dt = Time.deltaTime;
            fsm.transform.position += dir * speed * dt;   // Master → 네트워크로 동기화
            elapsed += dt;
            yield return null;
        }
        isLunging = false;
    }

    /* ───────── 히트 박스 X 방향 보정 ───────── */
    void ShiftCollider(int sign)
    {
        var box = weaponCollider.GetComponent<BoxCollider>();
        if (box == null) return;
        Vector3 c = box.center;
        c.x = Mathf.Abs(c.x) * sign;
        box.center = c;
    }
}
