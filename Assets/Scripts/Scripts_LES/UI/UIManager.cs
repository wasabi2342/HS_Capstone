using System.Collections.Generic;
using UnityEngine;

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
}
