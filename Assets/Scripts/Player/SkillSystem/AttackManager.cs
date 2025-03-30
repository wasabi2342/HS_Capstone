using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun; // 네임스페이스 추가

public class AttackManager : MonoBehaviour
{
    private Dictionary<Skills, AttackAction> attackActions = new Dictionary<Skills, AttackAction>();
    private WhitePlayerController playerController;
    
    public void Initialize(WhitePlayerController controller)
    {
        playerController = controller;
        RegisterDefaultAttacks();
    }
    
    private void RegisterDefaultAttacks()
    {
        // 기본 공격 (마우스 좌클릭)
        var basicAttack = new AttackAction(Skills.Mouse_L, "BasicAttack", WhitePlayerState.BasicAttack);
        basicAttack.OnDamageStart = (controller) => controller.OnAttack1DamageStart();
        basicAttack.OnAnimationEnd = (controller) => controller.OnAttackAnimationEnd();
        
        // 가드/패리 (마우스 우클릭)
        var guardAction = new AttackAction(Skills.Mouse_R, "Guard", WhitePlayerState.Guard);
        guardAction.OnActionStart = (controller) => controller.EnterInvincibleState();
        guardAction.OnAnimationEnd = (controller) => {
            controller.ExitInvincibleState();
            controller.OnAttackAnimationEnd();
        };
        
        // 대시 (스페이스바)
        var dashAction = new AttackAction(Skills.Space, "Dash", WhitePlayerState.Dash);
        dashAction.OnActionStart = (controller) => {
            controller.animator.SetBool("dash", true);
            if (PhotonNetwork.IsConnected)
            {
                controller.photonView.RPC("SyncBoolParameter", RpcTarget.Others, "dash", true);
            }
            controller.StartCoroutine(controller.DoDash(new Vector3(controller.moveInput.x, 0, 0)));
        };
        
        // 특수 공격 (쉬프트)
        var specialAttack = new AttackAction(Skills.Shift_L, "SpecialAttack", WhitePlayerState.Skill);
        specialAttack.OnDamageStart = (controller) => controller.OnSkillCollider();
        specialAttack.damageMultiplier = 1.7f;
        specialAttack.OnAnimationEnd = (controller) => {
            controller.OffSkillCollider();
            controller.OnAttackAnimationEnd();
        };
        
        // 궁극기 (R)
        var ultimateAttack = new AttackAction(Skills.R, "UltimateAttack", WhitePlayerState.Ultimate);
        ultimateAttack.OnActionStart = (controller) => controller.EnterInvincibleState();
        ultimateAttack.OnDamageStart = (controller) => controller.CreateUltimateEffect();
        ultimateAttack.damageMultiplier = 2.5f;
        ultimateAttack.enableCameraShake = true;
        ultimateAttack.cameraShakeIntensity = 2.0f;
        ultimateAttack.OnAnimationEnd = (controller) => {
            controller.ExitInvincibleState();
            controller.OnAttackAnimationEnd();
        };
        
        // 액션 등록
        RegisterAttack(basicAttack);
        RegisterAttack(guardAction);
        RegisterAttack(dashAction);
        RegisterAttack(specialAttack);
        RegisterAttack(ultimateAttack);
    }
    
    public void RegisterAttack(AttackAction action)
    {
        if (attackActions.ContainsKey(action.skillType))
        {
            attackActions[action.skillType] = action;
        }
        else
        {
            attackActions.Add(action.skillType, action);
        }
    }
    
    public bool ExecuteAction(Skills skillType)
    {
        if (!attackActions.TryGetValue(skillType, out AttackAction action))
            return false;
            
        // 사용 가능 여부 확인
        if (!playerController.IsSkillReady(skillType))
            return false;
            
        // 액션이 유효한지 확인 (죽음 상태 체크 등)
        if (playerController.currentState == WhitePlayerState.Death)
            return false;
            
        // 우선순위 체크
        if (playerController.nextState > action.targetState)
            return false;
            
        // 실행
        action.Execute(playerController);
        return true;
    }
    
    // 콤보 공격용 다음 단계 실행 메서드
    public void AdvanceCombo(int currentCombo)
    {
        // 콤보에 따라 다른 데미지 판정 적용
        switch (currentCombo)
        {
            case 1:
                playerController.OnAttack2DamageStart();
                break;
            case 2:
                playerController.OnAttack3DamageStart();
                break;
            case 3:
                playerController.OnAttack4DamageStart();
                break;
        }
    }
}
