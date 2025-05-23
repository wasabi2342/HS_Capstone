using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Stack<UIBase> uiStack = new Stack<UIBase>();

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

    void Start()
    {
        OpenPanel<UiStartPanel>();
        
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

    public void OpenPanel<T>() where T : UIBase
    {
        string prefabName = typeof(T).Name;
        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");

        GameObject panelInstance = Instantiate(prefab);
        panelInstance.transform.SetParent(transform, false);

        ClosePeekUI();

        uiStack.Push(panelInstance.GetComponent<T>());
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
            uiStack.Peek().gameObject.GetComponent<CanvasGroup  >().interactable = false;

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
        if(uiStack.Peek() is UIIngameMainPanel)
        {
            OpenPopupPanel<UISkillInfoPanel>();
        }
        else if(uiStack.Peek() is UISkillInfoPanel)
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
        if (scene.name.StartsWith("Stage"))
        {
            UIManager.Instance.CloseAllUI();
            UIManager.Instance.OpenPanel<UIIngameMainPanel>();
            //ReturnLocalPlayer().GetComponent<WhitePlayercontroller_event>().isInVillage = false;
        }
    }
}
