using UnityEngine;

public class HomingBulletController : MonoBehaviour
{
    [Header("���� ���(�÷��̾�)")]
    public Transform target;        // �÷��̾� Transform
    [Header("�̵� �ӵ� (������ ����)")]
    public float speed = 2f;
    [Header("�Ѿ� ���� (��)")]
    public float lifeTime = 10f;

    void Start()
    {
        // ���� �ð��� ������ �Ѿ� ����
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target == null) return;

        // Ÿ��(�÷��̾�) ������ ���� ����
        Vector3 direction = (target.position - transform.position).normalized;

        // õõ�� �̵�
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // �ð������� �÷��̾ �ٶ󺸰� �ϰ� ������:
        // transform.rotation = Quaternion.LookRotation(direction);
    }
}
