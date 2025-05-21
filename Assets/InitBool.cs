using Photon.Pun;
using UnityEngine;

public class InitBool : StateMachineBehaviour
{
    [SerializeField]
    private string parameter;
    [SerializeField]
    private bool boolValue;

    private PhotonView photonView;

    private ParentPlayerController parentPlayerController;
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
        animator.SetBool(parameter, boolValue);
        Debug.Log(parameter + animator.GetBool(parameter));
        if (parentPlayerController == null)
        {
            parentPlayerController = animator.GetComponent<ParentPlayerController>();
        }
        if (photonView == null)
        {
            photonView = animator.GetComponent<PhotonView>();
        }
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        //if (photonView.IsMine)
        //{
        //    parentPlayerController.SetBoolParameter(parameter, boolValue);
        //}
        animator.speed = 1.0f;
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
