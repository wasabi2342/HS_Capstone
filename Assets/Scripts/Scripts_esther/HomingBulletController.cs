using Photon.Pun;
using UnityEngine;

/// <summary>
/// 플레이어(타겟)을 향해 천천히 추적하는 탄환.
/// Player Transform이 없을 경우, 런타임에 로컬 플레이어를 찾아 할당(예시).
/// </summary>
[RequireComponent(typeof(Collider))]
public class HomingBulletController : MonoBehaviour
{
    [Header("추적 대상(플레이어)")]
    [Tooltip("런타임에 Instantiate되는 Player라면 비워두세요. Start에서 자동으로 찾습니다.")]
    public Transform target;

    [Header("이동 속도 (느리게 추적)")]
    public float speed = 2f;

    [Header("총알 수명 (초)")]
    public float lifeTime = 10f;

    void Start()
    {
        // 일정 시간이 지나면 총알 삭제
        Destroy(gameObject, lifeTime);

        // 만약 target이 미리 할당되지 않았다면, 로컬 플레이어 Transform을 찾아 할당
        if (target == null)
        {
            FindLocalPlayerTransform();
        }
    }

    void Update()
    {
        if (target == null) return;

        // 타겟(플레이어) 쪽으로 방향 벡터
        Vector3 direction = (target.position - transform.position).normalized;

        // 천천히 이동
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // 시각적으로 플레이어를 바라보게 하고 싶다면:
        // transform.rotation = Quaternion.LookRotation(direction);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 필요 시 데미지 로직
                // player.TakeDamage(damage);
                Debug.Log("[HomingBullet] 플레이어와 충돌!");
            }

            // 총알 파괴
            Destroy(gameObject);
        }
        else
        {
            // 필요 시 다른 충돌 처리
        }
    }

    /// <summary>
    /// Photon으로 Instantiate된 '로컬 플레이어'를 찾아 target에 할당
    /// </summary>
    private void FindLocalPlayerTransform()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (var p in players)
        {
            if (p.photonView != null && p.photonView.IsMine)
            {
                target = p.transform;
                Debug.Log("[HomingBullet] 로컬 플레이어를 추적하도록 설정했습니다.");
                return;
            }
        }

        Debug.LogWarning("[HomingBullet] 로컬 플레이어를 찾지 못했습니다. " +
                         "씬에 로컬 플레이어가 존재하는지 확인하세요.");
    }
}
