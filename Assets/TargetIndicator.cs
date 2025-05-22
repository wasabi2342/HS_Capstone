using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    private Camera cam;                     // �� ī�޶�
    [SerializeField]
    private RectTransform arrowUI;         // UI ȭ��ǥ
    [SerializeField]
    private Canvas canvas;                 // ĵ���� (Screen Space - Overlay ����)
    [SerializeField]
    private float edgeRadius = 200f;       // �߽ɿ��� ������ �Ÿ� (Off-screen ȭ��ǥ ��ġ)

    private Transform target;               // ���� ������Ʈ

    public void SetCamera(Camera camera)
    {
        cam = camera;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void Update()
    {
        if(target == null)
        {
            UIManager.Instance.OffTargetIndicator();
            return;
        }

        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);
        bool isOnScreen = viewportPos.z > 0 &&
                          viewportPos.x >= 0 && viewportPos.x <= 1 &&
                          viewportPos.y >= 0 && viewportPos.y <= 1;

        if (isOnScreen)
        {
            // ������Ʈ�� ȭ�� �ȿ� ������: �ش� ��ġ�� ȭ��ǥ ǥ��
            Vector3 screenPos = cam.WorldToScreenPoint(target.position);
            arrowUI.position = screenPos;
            arrowUI.rotation = Quaternion.identity;
        }
        else
        {
            // ȭ�� �߽�
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            Vector3 screenPos = cam.WorldToScreenPoint(target.position);
            Vector3 dir = (screenPos - screenCenter).normalized;

            // ȭ�� ��� (ȭ�� �׵θ��� ȭ��ǥ�� �ٰ� ���)
            float canvasWidth = canvas.pixelRect.width;
            float canvasHeight = canvas.pixelRect.height;

            float arrowHalfWidth = arrowUI.rect.width / 2f;
            float arrowHalfHeight = arrowUI.rect.height / 2f;

            // �ִ� �Ÿ� ��� (ĵ���� ��� �������� ȭ��ǥ�� ��ġ�ϰ�)
            float maxX = (canvasWidth / 2f) - arrowHalfWidth;
            float maxY = (canvasHeight / 2f) - arrowHalfHeight;

            // ���⺤�Ϳ� ���� ������ �̿��� ����� ���
            float xFactor = maxX / Mathf.Abs(dir.x);
            float yFactor = maxY / Mathf.Abs(dir.y);
            float factor = Mathf.Min(xFactor, yFactor);

            // ȭ�� �߽ɿ��� ���⺤�Ϳ� ���� factor��ŭ �̵�
            Vector3 arrowPos = screenCenter + dir * factor;

            // ��ġ �� ȸ�� ����
            arrowUI.position = arrowPos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrowUI.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
}
