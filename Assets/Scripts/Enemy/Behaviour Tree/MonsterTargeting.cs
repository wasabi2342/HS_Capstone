using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public enum MonsterType
{
    Basic,      // 기본형: 1. 가장 가까운 유저, 2. 거리가 비슷하면 랜덤 선택, 재타겟팅: 추격 5초 이상
    Assassin,   // 암살형: 1. 체력이 가장 낮은 유저, 2. 조건 충족 안되면 랜덤 선택, 재타겟팅: 공격 시전할 때마다
    Ranged      // 원거리형: 1. 10m 내에 존재하는 가장 가까운 유저, 2. 없으면 랜덤 선택, 재타겟팅: 공격 시전 전까지
}

public class MonsterTargeting : MonoBehaviourPun
{
    [Header("타겟팅 설정")]
    public MonsterType monsterType = MonsterType.Basic;

    [Tooltip("타겟 쿨타임")]
    public float targetingCooldown = 3f;

    [Tooltip("재타겟팅 시간")]
    public float retargetingTimeThreshold = 5f;

    // Ranged 전용: 공격이 시전 중이면 타겟 변경을 막기 위한 플래그
    public bool isAttackInitiated = false;

    // 내부 타이머들
    private float cooldownTimer = 0f;
    private float chaseTimer = 0f;

    // 현재 타겟 (EnemyAI 등에서 참조)
    public Transform currentTarget { get; private set; }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // 쿨타임 타이머 갱신
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        switch (monsterType)
        {
            case MonsterType.Basic:
                // 타겟이 있으면 추격 타이머 증가, 없으면 새 타겟 탐색
                if (currentTarget != null)
                {
                    chaseTimer += Time.deltaTime;
                }
                else
                {
                    TryAcquireTarget();
                }
                // 추격 시간이 임계치 이상이면 재타겟팅 시도
                if (chaseTimer >= retargetingTimeThreshold && cooldownTimer <= 0f)
                {
                    TryAcquireTarget();
                    chaseTimer = 0f;
                }
                break;

            case MonsterType.Assassin:
                // 암살형은 공격 시마다 외부에서 ForceRetarget()을 호출하여 타겟을 갱신하도록 함
                if (currentTarget == null)
                {
                    TryAcquireTarget();
                }
                break;

            case MonsterType.Ranged:
                // 원거리형은 공격 시전 전까지 타겟 변경 허용
                if (!isAttackInitiated)
                {
                    if (currentTarget != null)
                    {
                        chaseTimer += Time.deltaTime;
                    }
                    else
                    {
                        TryAcquireTarget();
                    }
                    if (chaseTimer >= retargetingTimeThreshold && cooldownTimer <= 0f)
                    {
                        TryAcquireTarget();
                        chaseTimer = 0f;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 현재 몬스터의 위치를 기준으로 플레이어들을 검색해 타겟을 결정합니다.
    /// 각 몬스터 유형별 우선순위에 따라 타겟을 선택합니다.
    /// </summary>
    public void TryAcquireTarget()
    {
        if (cooldownTimer > 0f)
            return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
            return;

        switch (monsterType)
        {
            case MonsterType.Basic:
                {
                    // 기본형: 가장 가까운 플레이어 우선, 거리가 비슷하면 후보 중 랜덤 선택
                    List<GameObject> candidates = new List<GameObject>();
                    float minDistance = float.MaxValue;
                    foreach (var player in players)
                    {
                        float distance = Vector3.Distance(transform.position, player.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            candidates.Clear();
                            candidates.Add(player);
                        }
                        else if (Mathf.Abs(distance - minDistance) < 0.1f)
                        {
                            candidates.Add(player);
                        }
                    }
                    GameObject chosen = candidates[Random.Range(0, candidates.Count)];
                    SetTarget(chosen.transform);
                }
                break;
/*
            case MonsterType.Assassin:
                {
                    // 암살형: 체력이 가장 낮은 플레이어 우선, 없으면 랜덤 선택
                    List<GameObject> candidates = new List<GameObject>();
                    float lowestHP = float.MaxValue;
                    foreach (var player in players)
                    {
                        // 플레이어에 PlayerHealth 컴포넌트가 있어야 함
                        PlayerHealth ph = player.GetComponent<PlayerHealth>();
                        if (ph == null)
                            continue;

                        float hp = ph.currentHP;
                        if (hp < lowestHP)
                        {
                            lowestHP = hp;
                            candidates.Clear();
                            candidates.Add(player);
                        }
                        else if (Mathf.Abs(hp - lowestHP) < 0.1f)
                        {
                            candidates.Add(player);
                        }
                    }
                    if (candidates.Count > 0)
                    {
                        GameObject chosen = candidates[Random.Range(0, candidates.Count)];
                        SetTarget(chosen.transform);
                    }
                    else
                    {
                        // 조건에 맞는 플레이어가 없으면 랜덤 선택
                        GameObject chosen = players[Random.Range(0, players.Length)];
                        SetTarget(chosen.transform);
                    }
                }
                break;
*/
            case MonsterType.Ranged:
                {
                    // 원거리형: 10m 이내에 존재하는 플레이어 중 가장 가까운 유저 우선, 없으면 랜덤 선택
                    List<GameObject> withinRange = new List<GameObject>();
                    foreach (var player in players)
                    {
                        float distance = Vector3.Distance(transform.position, player.transform.position);
                        if (distance <= 10f)
                        {
                            withinRange.Add(player);
                        }
                    }
                    if (withinRange.Count > 0)
                    {
                        GameObject closest = null;
                        float minDistance = float.MaxValue;
                        foreach (var player in withinRange)
                        {
                            float distance = Vector3.Distance(transform.position, player.transform.position);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closest = player;
                            }
                        }
                        SetTarget(closest.transform);
                    }
                    else
                    {
                        // 10m 이내에 없으면 전체 플레이어 중 랜덤 선택
                        GameObject chosen = players[Random.Range(0, players.Length)];
                        SetTarget(chosen.transform);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 새로운 타겟을 설정하고 쿨타임 및 추격 타이머를 초기화합니다.
    /// </summary>
    /// <param name="newTarget">새로운 타겟 Transform</param>
    public void SetTarget(Transform newTarget)
    {
        if (currentTarget != newTarget)
        {
            currentTarget = newTarget;
            cooldownTimer = targetingCooldown;
            chaseTimer = 0f;
            Debug.Log($"[{photonView.ViewID}] 새 타겟 설정: {newTarget.name}");
        }
    }

    /// <summary>
    /// 암살형 몬스터의 경우, 공격 시전 시 외부에서 호출하여 강제 재타겟팅하도록 합니다.
    /// </summary>
    public void ForceRetarget()
    {
        currentTarget = null;
        cooldownTimer = 0f;
        chaseTimer = 0f;
        TryAcquireTarget();
    }

    /// <summary>
    /// 원거리형 몬스터의 경우, 공격 시작 시 타겟 변경을 막기 위해 호출합니다.
    /// </summary>
    /// <param name="flag">공격 시전 여부</param>
    public void SetAttackInitiated(bool flag)
    {
        isAttackInitiated = flag;
        if (!flag && currentTarget == null)
        {
            TryAcquireTarget();
        }
    }
}
