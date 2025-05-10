using UnityEngine;
using UnityEngine.InputSystem;

public class ReviveTutorial : GaugeInteraction
{
    [SerializeField]
    private Animator animator;

    public override void OnInteract(InputAction.CallbackContext ctx)
    {
        base.OnInteract(ctx);
    }

    protected override void OnCanceledEvent()
    {
        base.OnCanceledEvent();
    }

    protected override void OnPerformedEvent()
    {
        base.OnPerformedEvent();
        animator.SetTrigger("Revive");
        canvas.gameObject.SetActive(false);
    }

    protected override void OnStartedEvent()
    {
        base.OnStartedEvent();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }
}
