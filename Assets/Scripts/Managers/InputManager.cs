using UnityEngine;
using UnityEngine.InputSystem;

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
    
    public bool ChangeDefaultMap(string actionMap)
    {
        if (PlayerInput.actions.FindActionMap(actionMap) == null)
        {
            Debug.Log("actionMap¿Ã æ¯¿Ω");
            return false;
        }
        else
        {
            PlayerInput.SwitchCurrentActionMap(actionMap);
            return true;
        }
    }
}
