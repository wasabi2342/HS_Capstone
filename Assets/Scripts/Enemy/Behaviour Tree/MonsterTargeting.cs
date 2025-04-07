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
        // (단, '필요하다면' Assassin 등 다른 타입에도 적용 가능)
        if (monsterType == MonsterType.Ranged && isAttackInitiated)
        {
            return;
        }

        // 현재 타겟이 유효한지 확인 (중도에 플레이어가 죽었거나 기절했는지 등)
        if (!IsTargetStillValid(CurrentTarget))
        {
            // 유효하지 않다면 즉시 타겟 해제 후 다음 기회에 재획득
            SetTarget(null);
        }

        if (CurrentTarget != null)
        {
            // 추적 중이면 시간을 누적
            chaseTimer += Time.deltaTime;

            // 일정 시간 넘으면 타겟 해제 → Wander
            if (chaseTimer >= retargetingTimeThreshold)
            {
                Debug.Log($"[{monsterType}] 추적 시간 만료. 타겟 해제 후 Wander로 돌아감.");
                SetTarget(null);
                cooldownTimer = targetingCooldown; // 바로 타겟 재획득 안 되도록 쿨타임 부여
            }
        }
        else
        {
            // 타겟이 없고 쿨타임이 0 이하라면 새 타겟을 탐색
            if (cooldownTimer <= 0f)
            {
                TryAcquireTarget();
            }
        }
    }

    /// <summary>
    /// 현재 몬스터 타입에 맞춰 플레이어 중 하나를 타겟으로 잡으려 시도
    /// </summary>
    public void TryAcquireTarget()
    {
        // 아직 쿨타임이 남아 있으면 그냥 리턴
        if (cooldownTimer > 0f)
            return;

        GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("Player");
        if (playerObjs.Length == 0)
            return;

        // 몬스터 타입별로 다른 타겟팅 규칙을 적용할 수 있음
        switch (monsterType)
        {
            case MonsterType.Basic:
                {
                    // 기본형: 가장 가까운 살아있는(Death, Stun 제외) 플레이어 중 임의로 하나 선택
                    List<Transform> candidates = new List<Transform>();
                    float minDist = float.MaxValue;

                    foreach (GameObject obj in playerObjs)
                    {
                        if (!IsPlayerValid(obj, out float dist))
                            continue;

                        // 더 가까운 플레이어가 있으면 리스트를 갈아치움
                        if (dist < minDist)
                        {
                            minDist = dist;
                            candidates.Clear();
                            candidates.Add(obj.transform);
                        }
                        // 비슷한 거리(±0.1f)면 후보 목록에 추가
                        else if (Mathf.Abs(dist - minDist) < 0.1f)
                        {
                            candidates.Add(obj.transform);
                        }
                    }

                    if (candidates.Count > 0)
                    {
                        Transform chosen = candidates[Random.Range(0, candidates.Count)];
                        SetTarget(chosen);
                    }
                    break;
                }

            case MonsterType.Assassin:
                {
                    // Assassin 예시: 가장 HP가 낮은 플레이어를 노린다거나, 특정 조건을 추가 가능
                    // 여기서는 "가장 가까운 플레이어" 로직을 그대로 사용해봄
                    float minDist = float.MaxValue;
                    Transform chosen = null;

                    foreach (GameObject obj in playerObjs)
                    {
                        if (!IsPlayerValid(obj, out float dist))
                            continue;
                        if (dist < minDist)
                        {
                            minDist = dist;
                            chosen = obj.transform;
                        }
                    }

                    if (chosen != null)
                        SetTarget(chosen);

                    break;
                }

            case MonsterType.Ranged:
                {
                    // Ranged 예시: 무작위 플레이어를 골라본다
                    // 다만 Death/Stun 제외
                    List<Transform> validPlayers = new List<Transform>();
                    foreach (GameObject obj in playerObjs)
                    {
                        if (IsPlayerValid(obj, out _))
                            validPlayers.Add(obj.transform);
                    }

                    if (validPlayers.Count > 0)
                    {
                        Transform chosen = validPlayers[Random.Range(0, validPlayers.Count)];
                        SetTarget(chosen);
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// 새 타겟을 설정하고, 쿨타임과 추적 시간 등을 초기화
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        // 이미 같은 타겟이면 무시
        if (CurrentTarget == newTarget)
            return;

        CurrentTarget = newTarget;
        cooldownTimer = targetingCooldown;
        chaseTimer = 0f;

        if (newTarget != null)
            Debug.Log($"[{monsterType}] 새 타겟 설정: {newTarget.name}");
        else
            Debug.Log($"[{monsterType}] 타겟 해제. (Wander 상태 복귀)");
    }

    /// <summary>
    /// 강제로 타겟을 갱신(바로 해제 후 재획득)
    /// </summary>
    public void ForceRetarget()
    {
        // 즉시 타겟을 null로 만든 뒤 TryAcquireTarget
        CurrentTarget = null;
        cooldownTimer = 0f;
        chaseTimer = 0f;
        TryAcquireTarget();
    }

    /// <summary>
    /// (원거리 공격 등) 공격이 시작되면 타겟 변경 잠금, 공격이 끝나면 풀어주기
    /// </summary>
    public void SetAttackInitiated(bool inProgress)
    {
        isAttackInitiated = inProgress;

        // 공격 종료 시점에 타겟이 없으면 다시 잡아본다
        if (!inProgress && CurrentTarget == null)
        {
            TryAcquireTarget();
        }
    }

    // ─────────────────────────────────────────────────────
    // 내부 유틸 함수들
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// 해당 플레이어 오브젝트가 사망/기절하지 않았고, 거리도 함께 구해주는 함수
    /// </summary>
    private bool IsPlayerValid(GameObject playerObj, out float distance)
    {
        distance = 0f;
        if (playerObj == null) return false;

        // WhitePlayerController 유무 확인
        var wpc = playerObj.GetComponent<WhitePlayerController>();
        if (wpc == null) return false;

        // 기절/사망 상태인지 확인
        if (wpc.currentState == WhitePlayerState.Death ||
            wpc.currentState == WhitePlayerState.Stun)
        {
            return false;
        }

        distance = Vector3.Distance(transform.position, playerObj.transform.position);
        return true;
    }

    /// <summary>
    /// 이미 잡혀 있는 타겟이 여전히 유효한지(죽지 않았는지) 체크
    /// </summary>
    private bool IsTargetStillValid(Transform target)
    {
        if (target == null) return false;
        // 혹시 오브젝트가 Destroy되었거나 비활성화 되었을 수 있으니 확인
        if (!target.gameObject.activeInHierarchy) return false;

        var wpc = target.GetComponent<WhitePlayerController>();
        if (wpc == null) return false;

        // 기절, 사망 상태도 무효
        if (wpc.currentState == WhitePlayerState.Death ||
            wpc.currentState == WhitePlayerState.Stun)
        {
            return false;
        }
        return true;
    }
}
