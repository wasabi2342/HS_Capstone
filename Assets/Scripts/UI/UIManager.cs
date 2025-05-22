using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Stack<UIBase> uiStack = new Stack<UIBase>();

    [SerializeField]
    private Canvas overlayCanvas;
    [SerializeField]
    private Canvas cameraCanvas;
    [SerializeField]
    private TargetIndicator targetIndicator;

    private CanvasScaler scaler;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetRenderCamera(Camera camera)
    {
        cameraCanvas.worldCamera = camera;
        targetIndicator.SetCamera(camera);
    }

    void Start()
    {
        //OpenPanel<UiStartPanel>();
        OpenPanelInOverlayCanvas<UILogoPanel>();
    }

    public T OpenPanelInOverlayCanvas<T>(bool additive = false) where T : UIBase
    {
        string prefabName = typeof(T).Name;
        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");

        GameObject panelInstance = Instantiate(prefab, overlayCanvas.transform, false);
        T panel = panelInstance.GetComponent<T>();

        if (!additive) ClosePeekUI();

        uiStack.Push(panel);
        return panel;
    }

    public T OpenPanelInCameraCanvas<T>(bool additive = false) where T : UIBase
    {
        string prefabName = typeof(T).Name;
        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");

        GameObject panelInstance = Instantiate(prefab, cameraCanvas.transform, false);
        T panel = panelInstance.GetComponent<T>();

        if (!additive) ClosePeekUI();

        uiStack.Push(panel);
        return panel;
    }

    public void ClosePeekUI()
    {
        if (uiStack.Count > 0)
        {
            Destroy(uiStack.Pop().gameObject);
        }

        if (uiStack.Count > 0)
        {
            uiStack.Peek().gameObject.GetComponent<CanvasGroup>().interactable = true;
        }
    }

    public void CloseAllUI()
    {
        while (uiStack.Count > 0)
        {
            UIBase ui = uiStack.Pop();

            if (ui != null && ui.gameObject != null)
            {
                Destroy(ui.gameObject);
            }
        }
    }

    public T OpenPopupPanelInOverlayCanvas<T>() where T : UIBase
    {
        if (uiStack.Count > 0)
            uiStack.Peek().gameObject.GetComponent<CanvasGroup>().interactable = false;

        string prefabName = typeof(T).Name;
        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");

        GameObject panelInstance = Instantiate(prefab);
        panelInstance.transform.SetParent(overlayCanvas.transform, false);

        T popup = panelInstance.GetComponent<T>();
        uiStack.Push(popup);

        return popup;
    }

    public T OpenPopupPanelInCameraCanvas<T>() where T : UIBase
    {
        if (uiStack.Count > 0)
            uiStack.Peek().gameObject.GetComponent<CanvasGroup>().interactable = false;

        string prefabName = typeof(T).Name;
        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");

        GameObject panelInstance = Instantiate(prefab);
        panelInstance.transform.SetParent(cameraCanvas.transform, false);

        T popup = panelInstance.GetComponent<T>();
        uiStack.Push(popup);

        return popup;
    }

    public void OnSkillInfo()
    {
        if (uiStack.Peek() is UIIngameMainPanel)
        {
            OpenPopupPanelInOverlayCanvas<UISkillInfoPanel>();
        }
        else if (uiStack.Peek() is UISkillInfoPanel)
        {
            ClosePeekUI();
        }
    }

    public UIBase ReturnPeekUI()
    {
        if (uiStack.Count > 0)
            return uiStack.Peek();
        else
            return null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OffTargetIndicator();

        CloseAllUI();
        if (scene.name.StartsWith("Level")||scene.name=="StageTest1" || scene.name == "Tutorial" || scene.name == "PvP")
        {
            OpenPanelInOverlayCanvas<UIIngameMainPanel>();
            OpenPanelInOverlayCanvas<UIMinimapPanel>(additive: true);
        }
        else if(scene.name == "Restart")
        {
            CloseAllUI();
            OpenPanelInOverlayCanvas<UiStartPanel>();
        }
    }

    public void CloseAllAndOpen<T>() where T : UIBase
    {
        CloseAllUI();
        OpenPanelInOverlayCanvas<T>();
    }

    public void OnTargetIndicator(Transform target)
    {
        targetIndicator.SetTarget(target);
        targetIndicator.gameObject.SetActive(true);
    }

    public void OffTargetIndicator()
    {
        targetIndicator.gameObject.SetActive(false);
    }
}
