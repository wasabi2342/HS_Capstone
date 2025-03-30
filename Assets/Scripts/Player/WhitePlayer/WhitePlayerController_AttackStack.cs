using UnityEngine;

public class WhitePlayerController_AttackStack : MonoBehaviour
{
    private WhitePlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<WhitePlayerController>();
    }

    // 이 함수는 애니메이션 이벤트에서 호출됩니다.
    public void AdvanceAttack()
    {
        if (playerController != null)
        {
            playerController.AdvanceAttackCombo();
        }
    }

    // Attack_1 애니메이션 이벤트 함수들
    public void OnAttack1StartupEnd()
    {
        // 필요한 경우 구현
    }

    public void OnAttack1DamageStart()
    {
        if (playerController != null)
            playerController.OnAttack1DamageStart();
    }

    public void OnAttack1DamageEnd()
    {
        if (playerController != null)
            playerController.OnAttack1DamageEnd();
    }

    public void OnAttack1AllowNextInput()
    {
        if (playerController != null)
            playerController.OnAttack1AllowNextInput();
    }

    public void OnAttack1RecoveryEnd()
    {
        // 필요한 경우 구현
    }

    public void OnAttack1AnimationEnd()
    {
        if (playerController != null)
            playerController.OnAttack1AnimationEnd();
    }

    // Attack_2 애니메이션 이벤트 함수들
    public void OnAttack2StartupFrame1End()
    {
        // 필요한 경우 구현
    }

    public void OnAttack2StartupFrame2End()
    {
        // 필요한 경우 구현
    }

    public void OnAttack2DamageStart()
    {
        if (playerController != null)
            playerController.OnAttack2DamageStart();
    }

    public void OnAttack2DamageEnd()
    {
        if (playerController != null)
            playerController.OnAttack2DamageEnd();
    }

    public void OnAttack2AllowNextInput()
    {
        if (playerController != null)
            playerController.OnAttack2AllowNextInput();
    }

    public void OnAttack2RecoveryEnd()
    {
        // 필요한 경우 구현
    }

    public void OnAttack2AnimationEnd()
    {
        if (playerController != null)
            playerController.OnAttack2AnimationEnd();
    }
}
