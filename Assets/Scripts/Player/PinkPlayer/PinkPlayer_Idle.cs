using Photon.Pun;
using UnityEngine;

public class PinkPlayer_Idle : StateMachineBehaviour
{
    PinkPlayerController pinkPlayerController;
    PhotonView photonView;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pinkPlayerController == null)
            pinkPlayerController = animator.GetComponent<PinkPlayerController>();
        pinkPlayerController.currentState = PinkPlayerState.Idle;
        animator.SetBool("Pre-Attack", false);
        animator.SetBool("Pre-Input", false);
        animator.SetBool("CancleState", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("run", false);
        animator.SetBool("revive", false);
        animator.SetInteger("CounterStack", 0);
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pinkPlayerController == null)
            pinkPlayerController = animator.GetComponent<PinkPlayerController>();
        if (photonView == null)
            photonView = animator.GetComponent<PhotonView>();
        if (!photonView.IsMine)
            return;
        switch (pinkPlayerController.nextState)
        {
            case PinkPlayerState.Idle:
                break;
            case PinkPlayerState.Run:
                animator.SetBool("run", true);
                pinkPlayerController.currentState = PinkPlayerState.Run;
                break;
            case PinkPlayerState.BasicAttack:
                pinkPlayerController.attackStack++;
                Debug.Log(pinkPlayerController.attackStack);
                animator.SetInteger("AttackStack", pinkPlayerController.attackStack);
                pinkPlayerController.SetIntParameter("AttackStack", pinkPlayerController.attackStack);
                pinkPlayerController.AttackStackUpdate?.Invoke(pinkPlayerController.attackStack);
                animator.SetBool("basicattack", true);
                pinkPlayerController.SetBoolParameter("basicattack", true);
                pinkPlayerController.currentState = PinkPlayerState.BasicAttack;
                break;
            //case PinkPlayerState.Guard:
            //    animator.SetBool("guard", true);
            //    pinkPlayerController.SetBoolParameter("guard", true);
            //    pinkPlayerController.currentState = WhitePlayerState.Guard;
            //    break;
            case PinkPlayerState.Skill:
                animator.SetBool("skill", true);
                pinkPlayerController.SetBoolParameter("skill", true);
                pinkPlayerController.currentState = PinkPlayerState.Skill;
                break;
            case PinkPlayerState.Ultimate:
                animator.SetBool("ultimate", true);
                pinkPlayerController.SetBoolParameter("ultimate", true);
                pinkPlayerController.currentState = PinkPlayerState.Ultimate;
                break;
            case PinkPlayerState.Dash:
                animator.SetTrigger("dash");
                pinkPlayerController.currentState = PinkPlayerState.Dash;
                break;

        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pinkPlayerController == null)
            pinkPlayerController = animator.GetComponent<PinkPlayerController>();

        pinkPlayerController.nextState = PinkPlayerState.Idle;
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
