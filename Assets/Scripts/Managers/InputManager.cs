using UnityEngine;
using UnityEngine.InputSystem;

public enum InputDefaultMap
{
    Player,
    UI
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public PlayerInput PlayerInput { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(gameObject);

        PlayerInput = GetComponent<PlayerInput>();
    }

    public bool ChangeDefaultMap(InputDefaultMap actionMap)
    {
        switch (actionMap)
        {
            case InputDefaultMap.Player:
                PlayerInput.SwitchCurrentActionMap("Player");
                return true;
            case InputDefaultMap.UI:
                PlayerInput.SwitchCurrentActionMap("UI");
                return true;                
        }

        return false;
    }
}
