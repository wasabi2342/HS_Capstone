using System;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public virtual void Init()
    {

    }

    public virtual void Init(Action confirmEvent, Action cancelEvent, string message)
    {

    }
}
