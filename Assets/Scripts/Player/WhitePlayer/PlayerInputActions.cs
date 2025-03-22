using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }

    public PlayerInputActions()
    {
        // JSON 형식으로 Input Action Asset 생성
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""e0b4d8c4-1d1c-4f73-8c80-1c2c98d5a1a0"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""fb97b3e4-9b49-4e69-ae2f-83a1a9d9e5b1"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Dash"",
                    ""type"": ""Button"",
                    ""id"": ""d6fdf7f4-9a50-42e0-91e0-2d1c1b2a8e73"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Tap""
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""a3d2e9ea-3c0a-4f7e-9d2a-70c1cbd5a2e3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""BasicAttack"",
                    ""type"": ""Button"",
                    ""id"": ""2c4e8e4d-d5e0-42d0-bad5-31ed6e7c3c1b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SpecialAttack"",
                    ""type"": ""Button"",
                    ""id"": ""1d8c9e4f-8f0b-4d9e-9c50-1a2b3c4d5e6f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SkillAttack"",
                    ""type"": ""Button"",
                    ""id"": ""b4c8d9e0-5f1a-4e3f-8c7d-2e1f3a4b5c6d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""UltimateAttack"",
                    ""type"": ""Button"",
                    ""id"": ""c7d8e9f0-1a2b-3c4d-5e6f-7a8b9c0d1e2f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""NPCInteract"",
                    ""type"": ""Button"",
                    ""id"": ""d1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""StageProgress"",
                    ""type"": ""Button"",
                    ""id"": ""e3f4a5b6-c7d8-9e0f-1a2b-3c4d5e6f7a8b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TrapClear"",
                    ""type"": ""Button"",
                    ""id"": ""f5a6b7c8-d9e0-1a2b-3c4d-5e6f7a8b9c0d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""11111111-2222-3333-4444-555555555555"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""66666666-7777-8888-9999-aaaaaaaaaaaa"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""bbbbbbbb-cccc-dddd-eeee-ffffffffffff"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""11112222-3333-4444-5555-666677778888"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""9999aaaa-bbbb-cccc-dddd-eeeeffffffff"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""ffffffff-eeee-dddd-cccc-bbbb99998888"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""12345678-90ab-cdef-1234-567890abcdef"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": ""Tap"",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""abcdef12-3456-7890-abcd-ef1234567890"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""11223344-5566-7788-99aa-bbccddeeff00"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""BasicAttack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""00ffeedd-ccbb-aazz-9988-776655443322"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""SpecialAttack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""deadbeef-0000-1111-2222-333344445555"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""SkillAttack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""feedface-6666-7777-8888-9999aaaabbbb"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""UltimateAttack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cafebabe-1234-5678-9abc-def012345678"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""NPCInteract"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""facefeed-8765-4321-0fed-cba987654321"",
                    ""path"": ""<Keyboard>/g"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""StageProgress"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""beadbead-1111-2222-3333-444455556666"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""TrapClear"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // 액션 맵 및 액션들을 찾습니다.
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Dash = m_Player.FindAction("Dash", throwIfNotFound: true);
        m_Player_Pause = m_Player.FindAction("Pause", throwIfNotFound: true);
        m_Player_BasicAttack = m_Player.FindAction("BasicAttack", throwIfNotFound: true);
        m_Player_SpecialAttack = m_Player.FindAction("SpecialAttack", throwIfNotFound: true);
        m_Player_SkillAttack = m_Player.FindAction("SkillAttack", throwIfNotFound: true);
        m_Player_UltimateAttack = m_Player.FindAction("UltimateAttack", throwIfNotFound: true);
        m_Player_NPCInteract = m_Player.FindAction("NPCInteract", throwIfNotFound: true);
        m_Player_StageProgress = m_Player.FindAction("StageProgress", throwIfNotFound: true);
        m_Player_TrapClear = m_Player.FindAction("TrapClear", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    // IInputActionCollection 인터페이스 구현
    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Player 액션 맵 및 액션들
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Dash;
    private readonly InputAction m_Player_Pause;
    private readonly InputAction m_Player_BasicAttack;
    private readonly InputAction m_Player_SpecialAttack;
    private readonly InputAction m_Player_SkillAttack;
    private readonly InputAction m_Player_UltimateAttack;
    private readonly InputAction m_Player_NPCInteract;
    private readonly InputAction m_Player_StageProgress;
    private readonly InputAction m_Player_TrapClear;

    public struct PlayerActions
    {
        private PlayerInputActions m_Wrapper;
        public PlayerActions(PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Dash => m_Wrapper.m_Player_Dash;
        public InputAction @Pause => m_Wrapper.m_Player_Pause;
        public InputAction @BasicAttack => m_Wrapper.m_Player_BasicAttack;
        public InputAction @SpecialAttack => m_Wrapper.m_Player_SpecialAttack;
        public InputAction @SkillAttack => m_Wrapper.m_Player_SkillAttack;
        public InputAction @UltimateAttack => m_Wrapper.m_Player_UltimateAttack;
        public InputAction @NPCInteract => m_Wrapper.m_Player_NPCInteract;
        public InputAction @StageProgress => m_Wrapper.m_Player_StageProgress;
        public InputAction @TrapClear => m_Wrapper.m_Player_TrapClear;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
    }

    public PlayerActions Player => new PlayerActions(this);

    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnDash(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnBasicAttack(InputAction.CallbackContext context);
        void OnSpecialAttack(InputAction.CallbackContext context);
        void OnSkillAttack(InputAction.CallbackContext context);
        void OnUltimateAttack(InputAction.CallbackContext context);
        void OnNPCInteract(InputAction.CallbackContext context);
        void OnStageProgress(InputAction.CallbackContext context);
        void OnTrapClear(InputAction.CallbackContext context);
    }
}
