using Photon.Pun;
using UnityEngine;

public class InitBool : StateMachineBehaviour
{
    [SerializeField]
    private string parameter;
    [SerializeField]
    private bool boolValue;

    private PhotonView photonView;

    private WhitePlayerController whitePlayerController;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (photonView == null)
        {
            photonView = animator.GetComponent<PhotonView>();
        }
        if (!photonView.IsMine)
        {
            return;
        }
        if (whitePlayerController == null)
        {
            whitePlayerController = animator.GetComponent<WhitePlayerController>();
        }
        animator.SetBool(parameter, boolValue);
        whitePlayerController.SetBoolParameter(parameter, boolValue);
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
