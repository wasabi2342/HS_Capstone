using Photon.Pun;
using UnityEngine;

public class WhitePlayerBasicAttack : StateMachineBehaviour
{
    private WhitePlayerController whitePlayerController;
    private PhotonView photonView;

    [SerializeField]
    private int attackStack;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whitePlayerController == null)
            whitePlayerController = animator.GetComponent<WhitePlayerController>();

        animator.SetBool("Pre-Attack", false);
        animator.SetBool("Pre-Input", false);
        animator.SetBool("CancleState", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("AttackContinue", false);
        animator.SetBool("basicattack", false);
        whitePlayerController.attackStack = attackStack;
        animator.SetInteger("AttackStack", attackStack);
        whitePlayerController.currentState = WhitePlayerState.BasicAttack;
        whitePlayerController.nextState = WhitePlayerState.Idle;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (photonView == null)
            photonView = animator.GetComponent<PhotonView>();
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
            return;
        if (whitePlayerController == null)
            whitePlayerController = animator.GetComponent<WhitePlayerController>();

        if(whitePlayerController.nextState == WhitePlayerState.BasicAttack)
        {
            animator.SetBool("AttackContinue", true);
            if (PhotonNetwork.IsConnected)
            {
                whitePlayerController.SetIntParameter("AttackStack", whitePlayerController.attackStack);
                whitePlayerController.SetBoolParameter("AttackContinue", true);
            }
        }
        else
        {
            animator.SetBool("AttackContinue", false);
            if (PhotonNetwork.IsConnected)
            {
                whitePlayerController.SetIntParameter("AttackStack", whitePlayerController.attackStack);
                whitePlayerController.SetBoolParameter("AttackContinue", false);
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //
    //}

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
