using UnityEngine;

public class WhitePlayerController_AttackStack : MonoBehaviour
{
    private WhitePlayerController whitePlayerController;

    private void Awake()
    {
        whitePlayerController = GetComponent<WhitePlayerController>();
        if (whitePlayerController == null)
        {
            Debug.LogError("WhitePlayerController ������Ʈ�� �����ϴ�!");
        }
    }

    // Attack_1 ���� �ִϸ��̼� �̺�Ʈ �Լ���
    public void OnAttack1StartupEnd()
    {
        whitePlayerController.OnAttack1StartupEnd();
    }

    public void OnAttack1DamageStart()
    {
        whitePlayerController.OnAttack1DamageStart();
    }

    public void OnAttack1DamageEnd()
    {
        whitePlayerController.OnAttack1DamageEnd();
    }

    public void OnAttack1AllowNextInput()
    {
        whitePlayerController.OnAttack1AllowNextInput();
    }

    public void OnAttack1RecoveryEnd()
    {
        whitePlayerController.OnAttack1RecoveryEnd();
    }

    public void OnAttack1AnimationEnd()
    {
        whitePlayerController.OnAttack1AnimationEnd();
    }

    // Attack_2 ���� �ִϸ��̼� �̺�Ʈ �Լ���
    public void OnAttack2StartupFrame1End()
    {
        whitePlayerController.OnAttack2StartupFrame1End();
    }

    public void OnAttack2StartupFrame2End()
    {
        whitePlayerController.OnAttack2StartupFrame2End();
    }

    public void OnAttack2DamageStart()
    {
        whitePlayerController.OnAttack2DamageStart();
    }

    public void OnAttack2DamageEnd()
    {
        whitePlayerController.OnAttack2DamageEnd();
    }

    public void OnAttack2AllowNextInput()
    {
        whitePlayerController.OnAttack2AllowNextInput();
    }

    public void OnAttack2RecoveryEnd()
    {
        whitePlayerController.OnAttack2RecoveryEnd();
    }

    public void OnAttack2AnimationEnd()
    {
        whitePlayerController.OnAttack2AnimationEnd();
    }
}
