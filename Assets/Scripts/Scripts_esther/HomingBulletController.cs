using UnityEngine;

public class HomingBulletController : MonoBehaviour
{
    [Header("추적 대상(플레이어)")]
    public Transform target;        // 플레이어 Transform
    [Header("이동 속도 (느리게 추적)")]
    public float speed = 2f;
    [Header("총알 수명 (초)")]
    public float lifeTime = 10f;

    void Start()
    {
        // 일정 시간이 지나면 총알 삭제
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target == null) return;

        // 타겟(플레이어) 쪽으로 방향 벡터
        Vector3 direction = (target.position - transform.position).normalized;

        // 천천히 이동
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // 시각적으로 플레이어를 바라보게 하고 싶으면:
        // transform.rotation = Quaternion.LookRotation(direction);
    }
}
