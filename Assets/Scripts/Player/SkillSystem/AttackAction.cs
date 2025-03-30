using System;
using UnityEngine;
using Photon.Pun; // 네임스페이스 추가

[Serializable]
public class AttackAction
{
    public Skills skillType;
    public string actionName;
    public float damageMultiplier = 1f;
    public bool useRightDirection = true;
    public bool enableCameraShake = false;
    public float cameraShakeIntensity = 1f;
    public WhitePlayerState targetState;
    
    // 콜백 함수들
    public Action<WhitePlayerController> OnActionStart;
    public Action<WhitePlayerController> OnAnimationStart;
    public Action<WhitePlayerController> OnDamageStart;
    public Action<WhitePlayerController> OnAnimationEnd;
    public Action<WhitePlayerController> OnActionEnd;
    
    public AttackAction(Skills type, string name, WhitePlayerState state) 
    {
        skillType = type;
        actionName = name;
        targetState = state;
    }
    
    // 공격 시작
    public void Execute(WhitePlayerController controller)
    {
        OnActionStart?.Invoke(controller);
        
        // 방향 설정
        if (useRightDirection)
        {
            Vector3 mousePos = controller.GetMouseWorldPosition();
            bool isRight = mousePos.x > controller.transform.position.x;
            controller.animator.SetBool("Right", isRight);
            
            if (PhotonNetwork.IsConnected)
            {
                controller.photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", isRight);
            }
        }
        
        // 상태 설정 및 애니메이션 실행
        controller.nextState = targetState;
        controller.animator.SetBool("Pre-Attack", true);
        controller.animator.SetBool("Pre-Input", true);
        
        if (PhotonNetwork.IsConnected)
        {
            controller.photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", true);
            controller.photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
        }
        
        // 쿨다운 시작
        controller.StartCooldown(skillType);
    }
}
