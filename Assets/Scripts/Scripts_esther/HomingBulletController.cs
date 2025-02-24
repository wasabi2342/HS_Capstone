using Photon.Pun;
using UnityEngine;

/// <summary>
/// �÷��̾�(Ÿ��)�� ���� õõ�� �����ϴ� źȯ.
/// Player Transform�� ���� ���, ��Ÿ�ӿ� ���� �÷��̾ ã�� �Ҵ�(����).
/// </summary>
[RequireComponent(typeof(Collider))]
public class HomingBulletController : MonoBehaviour
{
    [Header("���� ���(�÷��̾�)")]
    [Tooltip("��Ÿ�ӿ� Instantiate�Ǵ� Player��� ����μ���. Start���� �ڵ����� ã���ϴ�.")]
    public Transform target;

    [Header("�̵� �ӵ� (������ ����)")]
    public float speed = 2f;

    [Header("�Ѿ� ���� (��)")]
    public float lifeTime = 10f;

    void Start()
    {
        // ���� �ð��� ������ �Ѿ� ����
        Destroy(gameObject, lifeTime);

        // ���� target�� �̸� �Ҵ���� �ʾҴٸ�, ���� �÷��̾� Transform�� ã�� �Ҵ�
        if (target == null)
        {
            FindLocalPlayerTransform();
        }
    }

    void Update()
    {
        if (target == null) return;

        // Ÿ��(�÷��̾�) ������ ���� ����
        Vector3 direction = (target.position - transform.position).normalized;

        // õõ�� �̵�
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // �ð������� �÷��̾ �ٶ󺸰� �ϰ� �ʹٸ�:
        // transform.rotation = Quaternion.LookRotation(direction);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // �ʿ� �� ������ ����
                // player.TakeDamage(damage);
                Debug.Log("[HomingBullet] �÷��̾�� �浹!");
            }

            // �Ѿ� �ı�
            Destroy(gameObject);
        }
        else
        {
            // �ʿ� �� �ٸ� �浹 ó��
        }
    }

    /// <summary>
    /// Photon���� Instantiate�� '���� �÷��̾�'�� ã�� target�� �Ҵ�
    /// </summary>
    private void FindLocalPlayerTransform()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (var p in players)
        {
            if (p.photonView != null && p.photonView.IsMine)
            {
                target = p.transform;
                Debug.Log("[HomingBullet] ���� �÷��̾ �����ϵ��� �����߽��ϴ�.");
                return;
            }
        }

        Debug.LogWarning("[HomingBullet] ���� �÷��̾ ã�� ���߽��ϴ�. " +
                         "���� ���� �÷��̾ �����ϴ��� Ȯ���ϼ���.");
    }
}
