using UnityEngine.InputSystem;

public interface IInteractable
{
    void OnInteract(InputAction.CallbackContext context);
}
