using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public enum MonsterType
{
    Basic,
    Assassin,
    Ranged
}

public class MonsterTargeting : MonoBehaviourPun
{
    [Header("타겟팅 설정")]
    public MonsterType monsterType = MonsterType.Basic;

    [Tooltip("타겟을 재선정하기까지의 쿨타임")]
    public float targetingCooldown = 3f;

    [Tooltip("추적 유지 시간(이 시간이 지나면 타겟 해제 후 Wander)")]
    public float retargetingTimeThreshold = 5f;

    // [Ranged] 공격이 진행 중일 때는 타겟 변경을 막고 싶을 때 사용
    private bool isAttackInitiated = false;

    // 내부 타이머
    private float cooldownTimer = 0f;  // 타겟 재획득 쿨타임
    private float chaseTimer = 0f;     // 현재 타겟을 얼마나 추적했는지 누적

    /// <summary>
    /// 현재 AI가 추적 중인 대상(플레이어)의 Transform
    /// </summary>
    public Transform CurrentTarget { get; private set; }

    private void Update()
    {
        // 마스터 클라이언트만 타겟 로직을 수행
        if (!PhotonNetwork.IsMasterClient) return;

        // 타겟 재지정 쿨타임 감소
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // Ranged 타입에서 공격 시전 중이면 타겟 변경 안 함
        if (monsterType == MonsterType.Ranged && isAttackInitiated)
            return;

        // 현재 타겟이 유효한지 확인
        if (!IsTargetStillValid(CurrentTarget))
        {
            SetTarget(null);   // 즉시 타겟 해제
        }
        if (CurrentTarget != null)
        {
            // 추적 시간 누적
            chaseTimer += Time.deltaTime;

            // 일정 시간 초과 → 타겟 해제
            if (chaseTimer >= retargetingTimeThreshold)
            {
                Debug.Log($"[{monsterType}] 추적 시간 만료. 타겟 해제 후 Wander로 돌아감.");
                SetTarget(null);
                cooldownTimer = targetingCooldown; // 바로 재획득 방지
            }
        }
        else if (cooldownTimer <= 0f)
        {
            TryAcquireTarget(); // 새 타겟 탐색
        }
    }

    /// <summary>
    /// 몬스터 타입별 규칙에 따라 플레이어를 선택
    /// </summary>
    public void TryAcquireTarget()
    {
        if (cooldownTimer > 0f) return;

        GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("Player");
        if (playerObjs.Length == 0) return;

        switch (monsterType)
        {
            case MonsterType.Basic:
                {
                    // 가장 가까운 살아있는 플레이어 후보 수집
                    List<Transform> candidates = new List<Transform>();
                    float minDist = float.MaxValue;

                    foreach (GameObject obj in playerObjs)
                    {
                        if (!IsPlayerValid(obj, out float dist)) continue;

                        if (dist < minDist - 0.1f)
                        {
                            minDist = dist;
                            candidates.Clear();
                            candidates.Add(obj.transform);
                        }
                        else if (Mathf.Abs(dist - minDist) < 0.1f)
                        {
                            candidates.Add(obj.transform);
                        }
                    }

                    if (candidates.Count > 0)
                        SetTarget(candidates[Random.Range(0, candidates.Count)]);
                    break;
                }

            case MonsterType.Assassin:
                {
                    // 예시: 가장 가까운 플레이어
                    float minDist = float.MaxValue;
                    Transform chosen = null;

                    foreach (GameObject obj in playerObjs)
                    {
                        if (!IsPlayerValid(obj, out float dist)) continue;
                        if (dist < minDist)
                        {
                            minDist = dist;
                            chosen = obj.transform;
                        }
                    }

                    if (chosen != null) SetTarget(chosen);
                    break;
                }

            case MonsterType.Ranged:
                {
                    // 무작위 살아있는 플레이어
                    List<Transform> valids = new List<Transform>();
                    foreach (GameObject obj in playerObjs)
                        if (IsPlayerValid(obj, out _)) valids.Add(obj.transform);

                    if (valids.Count > 0)
                        SetTarget(valids[Random.Range(0, valids.Count)]);
                    break;
                }
        }
    }

    public void SetTarget(Transform newTarget)
    {
        if (CurrentTarget == newTarget) return;

        CurrentTarget = newTarget;
        cooldownTimer = targetingCooldown;
        chaseTimer = 0f;

        Debug.Log(newTarget
            ? $"[{monsterType}] 새 타겟 설정: {newTarget.name}"
            : $"[{monsterType}] 타겟 해제. (Wander 상태 복귀)");
    }

    public void ForceRetarget()
    {
        CurrentTarget = null;
        cooldownTimer = 0f;
        chaseTimer = 0f;
        TryAcquireTarget();
    }

    public void SetAttackInitiated(bool inProgress)
    {
        isAttackInitiated = inProgress;
        if (!inProgress && CurrentTarget == null)
            TryAcquireTarget();
    }

    // ────────────────────────────────────────
    // 유틸 함수
    // ────────────────────────────────────────

    private bool IsPlayerValid(GameObject playerObj, out float distance)
    {
        distance = 0f;
        if (playerObj == null) return false;

        // WhitePlayerController 유무 확인 (주석 유지)
        // 실제 코드는 PlayerController 사용
        var pc = playerObj.GetComponent<PlayerController>();
        if (pc == null) return false;

        // 기절/사망 제외
        if (pc.CurrentState == PlayerState.Death ||
            pc.CurrentState == PlayerState.Stun)
            return false;

        distance = Vector3.Distance(transform.position, playerObj.transform.position);
        return true;
    }

    private bool IsTargetStillValid(Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return false;

        var pc = target.GetComponent<PlayerController>();
        if (pc == null) return false;

        return pc.CurrentState != PlayerState.Death &&
               pc.CurrentState != PlayerState.Stun;
    }
}
