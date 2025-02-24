using System;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public Action onClose;

    protected virtual void OnDisable()
    {
        onClose?.Invoke();
    }

    public virtual void Init()
    {

    }

    public virtual void Init(Action confirmEvent, Action cancelEvent, string message)
    {

    }
}
