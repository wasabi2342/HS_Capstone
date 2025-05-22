// ───────────────── GhoulLongAttack.cs ─────────────────
using UnityEngine;
using System.Collections;
using Photon.Pun;
using System;

public class GhoulLongAttack : MonoBehaviourPun, IMonsterAttack
{
    [Header("Collider")]
    [SerializeField] GameObject weaponCollider;     // BoxCollider 포함 오브젝트

    [Header("Long-Lunge")]
    [SerializeField] float lungeDistance = 0.4f;    // 전진 거리
    [SerializeField] float lungeDuration = 0.1f;   // 이동 시간

    EnemyFSM fsm;
    bool isLunging;
    float facing = 1f;                              // +1 -> Right, –1 -> Left

    public string AnimKey => "Attack2";

    void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        weaponCollider.SetActive(false);
    }

    /* ---------- IMonsterAttack ---------- */
    public void SetDirection(float dir) => facing = Mathf.Sign(dir);

    public void Attack(Transform target)
    {
        if (!PhotonNetwork.IsMasterClient || isLunging) return;
        StartCoroutine(LungeRoutine());
    }

    IEnumerator LungeRoutine()
    {
        isLunging = true;
        Vector3 dir = Vector3.right * facing;
        float speed = lungeDistance / lungeDuration;
        float t = 0f;
        while (t < lungeDuration)
        {
            float dt = Time.deltaTime;
            fsm.transform.position += dir * speed * dt;   // MasterClient만 이동
            t += dt;
            yield return null;
        }
        isLunging = false;
    }

    public void EnableAttack() => weaponCollider.SetActive(true);   // 애니 이벤트
    public void DisableAttack() => weaponCollider.SetActive(false);  // 애니 이벤트

    // ── 애니 이벤트용 보조 ──
    public void SetColliderRight() => ShiftCollider(+1);
    public void SetColliderLeft() => ShiftCollider(-1);
    public void OnAttackAnimationEndEvent() => DisableAttack();

    void ShiftCollider(int sign)
    {
        if (!weaponCollider.TryGetComponent(out BoxCollider box)) return;
        Vector3 c = box.center; c.x = Mathf.Abs(c.x) * sign; box.center = c;
    }
}
