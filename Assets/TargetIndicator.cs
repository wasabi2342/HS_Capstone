using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    private Camera cam;                     // 주 카메라
    [SerializeField]
    private RectTransform arrowUI;         // UI 화살표
    [SerializeField]
    private Canvas canvas;                 // 캔버스 (Screen Space - Overlay 권장)
    [SerializeField]
    private float edgeRadius = 200f;       // 중심에서 떨어질 거리 (Off-screen 화살표 위치)

    private Transform target;               // 따라갈 오브젝트

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
            // 오브젝트가 화면 안에 있으면: 해당 위치에 화살표 표시
            Vector3 screenPos = cam.WorldToScreenPoint(target.position);
            arrowUI.position = screenPos;
            arrowUI.rotation = Quaternion.identity;
        }
        else
        {
            // 화면 중심
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            Vector3 screenPos = cam.WorldToScreenPoint(target.position);
            Vector3 dir = (screenPos - screenCenter).normalized;

            // 화면 경계 (화면 테두리에 화살표가 붙게 계산)
            float canvasWidth = canvas.pixelRect.width;
            float canvasHeight = canvas.pixelRect.height;

            float arrowHalfWidth = arrowUI.rect.width / 2f;
            float arrowHalfHeight = arrowUI.rect.height / 2f;

            // 최대 거리 계산 (캔버스 경계 안쪽으로 화살표가 위치하게)
            float maxX = (canvasWidth / 2f) - arrowHalfWidth;
            float maxY = (canvasHeight / 2f) - arrowHalfHeight;

            // 방향벡터에 따라 비율을 이용해 경계점 계산
            float xFactor = maxX / Mathf.Abs(dir.x);
            float yFactor = maxY / Mathf.Abs(dir.y);
            float factor = Mathf.Min(xFactor, yFactor);

            // 화면 중심에서 방향벡터에 따라 factor만큼 이동
            Vector3 arrowPos = screenCenter + dir * factor;

            // 위치 및 회전 적용
            arrowUI.position = arrowPos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrowUI.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
}
