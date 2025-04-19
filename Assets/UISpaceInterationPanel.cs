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

    private Action<bool> action;

    private float timeRemaining = 10f;
    private float decayTimer = 1f;
    private bool isRunning = false;

    private float blinkTimer = 0f;
    private bool isBright = true;

    private void OnDisable()
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.Player);
    }

    public void Init(Action<bool> action = null)
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);
        InputManager.Instance.PlayerInput.actions["Space"].started += ctx => SpaceInput(ctx);

        this.action = action;
        fillImage.fillAmount = 0;

        timeRemaining = 15f;
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

        // 매 1초마다 감소
        if (decayTimer <= 0f)
        {
            fillImage.fillAmount -= 0.1f;
            fillImage.fillAmount = Mathf.Max(fillImage.fillAmount, 0f);
            decayTimer = 1f;
        }

        // 0.5초마다 아이콘 색 전환
        if (icon != null && blinkTimer <= 0f)
        {
            icon.color = isBright ? Color.gray : Color.white;
            isBright = !isBright;
            blinkTimer = 0.5f;
        }

        // 타이머 종료
        if (timeRemaining <= 0f)
        {
            isRunning = false;
            InputManager.Instance.PlayerInput.actions["Space"].started -= SpaceInput;

            bool success = fillImage.fillAmount >= 0.9f;

            action?.Invoke(success);

            UIManager.Instance.ClosePeekUI();
        }
    }

    public void SetPanelPosition(Vector3 worldPos)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay) return;

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPos);

        RectTransform panelRT = GetComponent<RectTransform>();
        RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPoint, null, out anchoredPos);

        panelRT.anchoredPosition = anchoredPos;
    }
}
