using Photon.Pun;
using UnityEngine;

public class WhitePlayer_Idle : StateMachineBehaviour
{
    WhitePlayerController whitePlayerController;
    PhotonView photonView;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (photonView == null)
            photonView = animator.GetComponent<PhotonView>();
        //if (PhotonNetwork.IsConnected && !photonView.IsMine)
        //    return;

        if (whitePlayerController == null)
            whitePlayerController = animator.GetComponent<WhitePlayerController>();
        whitePlayerController.currentState = WhitePlayerState.Idle;
        animator.SetBool("basicattack", false);
        animator.SetBool("guard", false);
        animator.SetBool("skill", false);
        animator.SetBool("ultimate", false);
        animator.SetBool("dash", false);

        whitePlayerController.ExitInvincibleState();
        whitePlayerController.ExitSuperArmorState();

        animator.SetBool("Pre-Attack", false);
        animator.SetBool("Pre-Input", false);
        animator.SetBool("CancleState", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("run", false);
        animator.SetBool("revive", false);
        animator.SetInteger("CounterStack", 0);
        animator.SetBool("AttackContinue", false);
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (photonView == null)
            photonView = animator.GetComponent<PhotonView>();
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
            return;
        if (whitePlayerController == null)
            whitePlayerController = animator.GetComponent<WhitePlayerController>();

        var state = whitePlayerController.nextState;

        if (state == WhitePlayerState.Idle) return;

        switch (state)
        {
            case WhitePlayerState.Run:
                animator.SetBool("run", true);
                whitePlayerController.currentState = WhitePlayerState.Run;
                break;
            case WhitePlayerState.BasicAttack:
                whitePlayerController.attackStack++;
                Debug.Log(whitePlayerController.attackStack);
                animator.SetInteger("AttackStack", whitePlayerController.attackStack);
                whitePlayerController.AttackStackUpdate?.Invoke(whitePlayerController.attackStack);
                whitePlayerController.currentState = WhitePlayerState.BasicAttack;

                Vector3 mousePos = whitePlayerController.GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > whitePlayerController.transform.position.x);
                
                animator.SetBool("basicattack", true);
                if (PhotonNetwork.IsConnected)
                {
                    whitePlayerController.SetIntParameter("AttackStack", whitePlayerController.attackStack);
                    whitePlayerController.SetBoolParameter("basicattack", true);
                    whitePlayerController.SetBoolParameter("Right", mousePos.x > whitePlayerController.transform.position.x);
                }
                break;
            case WhitePlayerState.Guard:
                animator.SetBool("guard", true);
                whitePlayerController.currentState = WhitePlayerState.Guard;
                if (PhotonNetwork.IsConnected)
                    whitePlayerController.SetBoolParameter("guard", true);
                break;
            case WhitePlayerState.Skill:
                animator.SetBool("skill", true);
                whitePlayerController.currentState = WhitePlayerState.Skill;
                if (PhotonNetwork.IsConnected)
                    whitePlayerController.SetBoolParameter("skill", true);
                break;
            case WhitePlayerState.Ultimate:
                animator.SetBool("ultimate", true);
                whitePlayerController.currentState = WhitePlayerState.Ultimate;
                if (PhotonNetwork.IsConnected)
                    whitePlayerController.SetBoolParameter("ultimate", true);
                break;
            case WhitePlayerState.Dash:
                animator.SetBool("dash",true);
                whitePlayerController.currentState = WhitePlayerState.Dash;
                if (PhotonNetwork.IsConnected)
                    whitePlayerController.SetBoolParameter("dash", true);
                break;
        }

        // 상태 처리 후 Idle로 리셋해서 중복 처리 방지
        whitePlayerController.nextState = WhitePlayerState.Idle;
    }


    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (photonView == null)
            photonView = animator.GetComponent<PhotonView>();
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
            return;
        if (whitePlayerController == null)
            whitePlayerController = animator.GetComponent<WhitePlayerController>();

        whitePlayerController.nextState = WhitePlayerState.Idle;
        animator.SetBool("Pre-Attack", false);
        animator.SetBool("Pre-Input", false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
