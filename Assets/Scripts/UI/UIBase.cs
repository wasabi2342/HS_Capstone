using Photon.Pun;
using System;
using UnityEngine;

public abstract class UIBase : MonoBehaviourPunCallbacks
{
    public Action onClose;

    public virtual void OnDisable()
    {
        onClose?.Invoke();
    }

    public virtual void Init()
    {

    }
}
