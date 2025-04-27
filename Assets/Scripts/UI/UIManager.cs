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

    private Canvas canvas;
    private CanvasScaler scaler;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        canvas = GetComponent<Canvas>();
        scaler = GetComponent<CanvasScaler>();
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
        canvas.worldCamera = camera;
    }

    void Start()
    {
        //OpenPanel<UiStartPanel>();
        OpenPanel<UILogoPanel>();

        string nickname = PlayerPrefs.GetString("Nickname");

        if (nickname != "")
        {
            PhotonNetworkManager.Instance.SetNickname(nickname);
        }
        else
        {
            OpenPopupPanel<UISetNicknamePanel>();
        }
    }

    public T OpenPanel<T>(bool additive = false) where T : UIBase
    {
        string prefabName = typeof(T).Name;
        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");

        GameObject panelInstance = Instantiate(prefab, transform, false);
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
        if (uiStack.Count > 0)
        {
            Destroy(uiStack.Pop().gameObject);
            CloseAllUI();
        }
    }

    public T OpenPopupPanel<T>() where T : UIBase
    {
        if (uiStack.Count > 0)
            uiStack.Peek().gameObject.GetComponent<CanvasGroup>().interactable = false;

        string prefabName = typeof(T).Name;
        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");

        GameObject panelInstance = Instantiate(prefab);
        panelInstance.transform.SetParent(transform, false);

        T popup = panelInstance.GetComponent<T>();
        uiStack.Push(popup);

        return popup;
    }

    public void OnSkillInfo()
    {
        if (uiStack.Peek() is UIIngameMainPanel)
        {
            OpenPopupPanel<UISkillInfoPanel>();
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
        CloseAllUI();
        if (scene.name.StartsWith("Level")||scene.name=="StageTest1")
        {
            OpenPanel<UIIngameMainPanel>();
            OpenPanel<UIMinimapPanel>(additive: true);
        }
    }

    public void CloseAllAndOpen<T>() where T : UIBase
    {
        CloseAllUI();
        OpenPanel<T>();
    }

    public void SetCanvasSize(Vector2 scale)
    {
        scaler.referenceResolution = scale;
    }
}
