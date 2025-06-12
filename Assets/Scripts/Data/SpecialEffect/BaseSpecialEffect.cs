using UnityEngine;

[CreateAssetMenu(fileName = "BaseSpecialEffect", menuName = "Scriptable Objects/BaseSpecialEffect")]
public abstract class BaseSpecialEffect : ScriptableObject
{
    protected float value;
    protected float duration;
    protected bool isFirstTime;
    protected bool isInstant;

    protected ParentPlayerController playerController;
    protected Collider collider;

    public virtual void Init(float value, float duration, ParentPlayerController playerController)
    {
        this.value = value;
        this.duration = duration;
        this.playerController = playerController;
    }

    public void InjectCollider(Collider collider)
    {
        this.collider = collider;
    }

    public bool IsInstant()
    {
        return isInstant;
    }

    public virtual void ApplyEffect()
    {
        Debug.Log("가호 효과 적용");
    }
}
