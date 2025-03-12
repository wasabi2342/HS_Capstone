using System;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirmPanel : UIBase
{
    [SerializeField]
    private Text message;
    [SerializeField]
    private Button confirmButton;
    [SerializeField]
    private Button cancelButton;

    public void Init(Action confirmEvent, Action cancelEvent, string message)
    {
        if(confirmEvent ==  null)
        {
            Debug.Log("¾×¼Ç ³Î");
        }
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => confirmEvent.Invoke());
        cancelButton.onClick.AddListener(() => cancelEvent.Invoke());
        this.message.text = message;
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }
}
