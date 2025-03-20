using Photon.Pun;
using UnityEngine;

public class WhitePlayer_Idle : StateMachineBehaviour
{
    WhitePlayerController whitePlayerController;
    PhotonView photonView;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(whitePlayerController == null)
            whitePlayerController = animator.GetComponent<WhitePlayerController>();
        whitePlayerController.currentState = WhitePlayerState.Idle;
        animator.SetBool("Pre-Attack", false);
        animator.SetBool("Pre-Input", false);
        animator.SetBool("CancleState", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("run", false);

    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch (whitePlayerController.nextState)
        {
            case WhitePlayerState.Idle:
                break;
            case WhitePlayerState.Run:
                animator.SetBool("run", true);
                whitePlayerController.currentState = WhitePlayerState.Run;
                break;
            case WhitePlayerState.BasicAttack:
                whitePlayerController.attackStack++;
                Debug.Log(whitePlayerController.attackStack);
                animator.SetInteger("AttackStack", whitePlayerController.attackStack);
                animator.SetTrigger("basicattack");
                whitePlayerController.currentState = WhitePlayerState.BasicAttack;
                break;
            case WhitePlayerState.Guard:
                animator.SetTrigger("guard");
                whitePlayerController.currentState = WhitePlayerState.Guard;
                break;
            case WhitePlayerState.Skill:
                animator.SetTrigger("skill");
                whitePlayerController.currentState = WhitePlayerState.Skill;
                break;
            case WhitePlayerState.Ultimate:
                animator.SetTrigger("ultimate");
                whitePlayerController.currentState = WhitePlayerState.Ultimate;
                break;
            case WhitePlayerState.Dash:
                animator.SetTrigger("dash");
                whitePlayerController.currentState = WhitePlayerState.Dash;
                break;

        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
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
