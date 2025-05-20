using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum InputKey
{
    mouseL,
    mouseR,
    space,
    shift,
    r,
    MAX
}

public class UISkillInfoPanel : UIBase
{
    [SerializeField]
    private UISkillDataSlot[] dataSlots = new UISkillDataSlot[5];

    private Action<InputAction.CallbackContext> closeUIAction;

    private void Start()
    {
        Init();
    }
    
    public override void Init()
    {
        var blessings = RoomManager.Instance.ReturnLocalPlayer().GetComponentInChildren<PlayerBlessing>().ReturnSkillWithLevel();
        for (int i = 0; i < (int)Skills.Max; i++)
        {
            if (blessings[i].level == 0)
            {
                dataSlots[i].Init(blessings[i]);
            }
            else
            {
                dataSlots[i].Init(blessings[i]);
            }
        }

        closeUIAction = CloseUI;
        InputManager.Instance.PlayerInput.actions["Tab"].performed += closeUIAction;
    }

    public void CloseUI(InputAction.CallbackContext ctx)
    {
        if(UIManager.Instance.ReturnPeekUI() as UISkillInfoPanel)
        {
            UIManager.Instance.ClosePeekUI();
            InputManager.Instance.ChangeDefaultMap(InputDefaultMap.Player);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (InputManager.Instance != null && InputManager.Instance.PlayerInput != null && closeUIAction != null)
        {
            InputManager.Instance.PlayerInput.actions["Tab"].performed -= closeUIAction;
        }
    }
}
