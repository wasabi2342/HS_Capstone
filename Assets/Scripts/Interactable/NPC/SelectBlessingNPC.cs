using UnityEngine;
using UnityEngine.InputSystem;

public class SelectBlessingNPC : MonoBehaviour, IInteractable
{
    private bool canIneract = true;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canIneract)
        {
            canIneract = false;
            UIManager.Instance.OpenPopupPanel<UISelectBlessingPanel>();
        }
    }
}
