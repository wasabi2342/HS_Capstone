using UnityEngine;

public class WhitePlayerController_AttackStack : MonoBehaviour
{
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController 컴포넌트가 없습니다!");
        }
    }

    // Attack_1 관련 애니메이션 이벤트 함수들
    public void OnAttack1StartupEnd()
    {
        playerController.OnAttack1StartupEnd();
    }

    public void OnAttack1DamageStart()
    {
        playerController.OnAttack1DamageStart();
    }

    public void OnAttack1DamageEnd()
    {
        playerController.OnAttack1DamageEnd();
    }

    public void OnAttack1AllowNextInput()
    {
        playerController.OnAttack1AllowNextInput();
    }

    public void OnAttack1RecoveryEnd()
    {
        playerController.OnAttack1RecoveryEnd();
    }

    public void OnAttack1AnimationEnd()
    {
        playerController.OnAttack1AnimationEnd();
    }

    // Attack_2 관련 애니메이션 이벤트 함수들
    public void OnAttack2StartupFrame1End()
    {
        playerController.OnAttack2StartupFrame1End();
    }

    public void OnAttack2StartupFrame2End()
    {
        playerController.OnAttack2StartupFrame2End();
    }

    public void OnAttack2DamageStart()
    {
        playerController.OnAttack2DamageStart();
    }

    public void OnAttack2DamageEnd()
    {
        playerController.OnAttack2DamageEnd();
    }

    public void OnAttack2AllowNextInput()
    {
        playerController.OnAttack2AllowNextInput();
    }

    public void OnAttack2RecoveryEnd()
    {
        playerController.OnAttack2RecoveryEnd();
    }

    public void OnAttack2AnimationEnd()
    {
        playerController.OnAttack2AnimationEnd();
    }
}
