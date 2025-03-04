using System;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public Action onClose;

    protected virtual void OnDisable()
    {
        onClose?.Invoke();
    }

    public virtual void Init()
    {

    }
}
