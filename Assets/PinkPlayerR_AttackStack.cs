using Photon.Pun;
using UnityEngine;

public class PinkPlayerR_AttackStack : StateMachineBehaviour
{

    private PinkPlayerController pinkPlayerController;
    private PhotonView photonView;


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (pinkPlayerController == null)
            pinkPlayerController = animator.GetComponent<PinkPlayerController>();

        animator.SetBool("Pre-Attack", false);
        animator.SetBool("Pre-Input", false);
        animator.SetBool("CancleState", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("basicattack", false);
        animator.SetBool("AttackContinue", false);

        //pinkPlayerController.R_attackStack ++;
        //animator.SetInteger("R_attackStack", pinkPlayerController.R_attackStack);

        pinkPlayerController.currentState = PinkPlayerState.R_hit;

        if (pinkPlayerController.nextState == PinkPlayerState.R_hit)
            pinkPlayerController.nextState = PinkPlayerState.R_Idle;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (photonView == null)
            photonView = animator.GetComponent<PhotonView>();
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
            return;
        if (pinkPlayerController == null)
            pinkPlayerController = animator.GetComponent<PinkPlayerController>();

        if (pinkPlayerController.nextState == PinkPlayerState.R_hit)
        {
            animator.SetBool("AttackContinue", true);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Pre-Attack", false);
        animator.SetBool("Pre-Input", false);
        animator.SetBool("CancleState", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("basicattack", false);
        animator.SetBool("AttackContinue", false);
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
