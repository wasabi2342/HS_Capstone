
using UnityEngine;

public class CenterPointInitializer : MonoBehaviour
{
    public Transform playerTransform;
    public float interactionRadius = 2.5f; // �÷��̾� ���� ��ȣ�ۿ� ����
    public Transform[] centerPoints; // 8�� �߽���

    void Start()
    {
        if (playerTransform == null || centerPoints == null || centerPoints.Length != 8)
        {
            Debug.LogError("�÷��̾� Transform �Ǵ� CenterPoints�� �ùٸ��� �������� �ʾҽ��ϴ�.");
            return;
        }

        // 8���� ���� ���� (360���� 8�� �������� ����)
        float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

        for (int i = 0; i < centerPoints.Length; i++)
        {
            float radians = angles[i] * Mathf.Deg2Rad; // ������ �������� ��ȯ
            Vector3 offset = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * interactionRadius;
            centerPoints[i].position = playerTransform.position + offset;
        }
    }

    void Update()
    {
        // �÷��̾ �̵��ϸ� �߽����� ���� �̵�
        for (int i = 0; i < centerPoints.Length; i++)
        {
            float radians = i * 45f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * interactionRadius;
            centerPoints[i].position = playerTransform.position + offset;
        }
    }
}
