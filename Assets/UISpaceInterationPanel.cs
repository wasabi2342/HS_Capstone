using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UISpaceInterationPanel : UIBase
{
    [SerializeField]
    private Image fillImage;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Image timerImage;

    private Action<bool> action;

    private float timeRemaining = 10f;
    private float decayTimer = 1f;
    private bool isRunning = false;

    private float blinkTimer = 0f;
    private bool isBright = true;

    private System.Action<InputAction.CallbackContext> spaceInputAction;

    private void OnDisable()
    {
        if (InputManager.Instance != null && InputManager.Instance.PlayerInput != null && spaceInputAction != null)
        {
            InputManager.Instance.PlayerInput.actions["Space"].started -= spaceInputAction;
        }

        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.Player);
    }

    public void Init(Action<bool> action = null)
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);

        spaceInputAction = SpaceInput;
        InputManager.Instance.PlayerInput.actions["Space"].started += spaceInputAction;

        this.action = action;
        fillImage.fillAmount = 0;
        timerImage.fillAmount = 1f;

        timeRemaining = 10f;
        decayTimer = 1f;
        isRunning = true;

        icon.color = Color.white;
    }

    public void SpaceInput(InputAction.CallbackContext context)
    {
        if (!isRunning) return;

        fillImage.fillAmount += 0.15f;
        fillImage.fillAmount = Mathf.Min(fillImage.fillAmount, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;
        decayTimer -= Time.deltaTime;
        blinkTimer -= Time.deltaTime;

        if (timerImage != null)
        {
            timerImage.fillAmount = timeRemaining / 10f;
            timerImage.fillAmount = Mathf.Max(timerImage.fillAmount, 0f);
        }

        // 매 1초마다 감소
        if (decayTimer <= 0f)
        {
            fillImage.fillAmount -= 0.1f;
            fillImage.fillAmount = Mathf.Max(fillImage.fillAmount, 0f);
            decayTimer = 1f;
        }

        // ▶ 조기 성공 처리
        if (fillImage.fillAmount >= 0.9f)
        {
            CompleteInteraction(true);
            return;
        }

        // 타이머 종료
        if (timeRemaining <= 0f)
        {
            CompleteInteraction(fillImage.fillAmount >= 0.9f);
        }
    }

    private void CompleteInteraction(bool success)
    {
        if (!isRunning) return;

        isRunning = false;
        InputManager.Instance.PlayerInput.actions["Space"].started -= SpaceInput;

        action?.Invoke(success);
        UIManager.Instance.ClosePeekUI();
    }

    public void SetPanelPosition(Vector3 worldPos)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning("캔버스가 Screen Space - Overlay 모드가 아닙니다.");
            return;
        }

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPos);

        RectTransform panelRT = GetComponent<RectTransform>();
        RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPoint, null, out anchoredPos);

        panelRT.anchoredPosition = anchoredPos;
    }
}
