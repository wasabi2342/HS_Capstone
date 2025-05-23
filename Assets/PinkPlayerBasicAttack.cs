using UnityEngine;

public class PinkPlayerBasicAttack1 : StateMachineBehaviour
{
    PinkPlayerController pinkPlayerController;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pinkPlayerController == null)
            pinkPlayerController = animator.GetComponent<PinkPlayerController>();
        
        pinkPlayerController.attackStack = 1;
        Debug.Log(pinkPlayerController.attackStack);
        animator.SetInteger("AttackStack", pinkPlayerController.attackStack);
        pinkPlayerController.SetIntParameter("AttackStack", pinkPlayerController.attackStack);
        pinkPlayerController.AttackStackUpdate?.Invoke(pinkPlayerController.attackStack);
        pinkPlayerController.CreateBasicAttackEffect();

        animator.SetBool("AttackContinue", false);
        animator.SetBool("Pre-Attack", false);
        animator.SetBool("Pre-Input", false);
        animator.SetBool("CancleState", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("run", false);
        animator.SetBool("revive", false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pinkPlayerController == null)
            pinkPlayerController = animator.GetComponent<PinkPlayerController>();

        if (pinkPlayerController.nextState == PinkPlayerState.BasicAttack)
        {
            animator.SetBool("AttackContinue", true);
            pinkPlayerController.SetBoolParameter("AttackContinue", true);
            pinkPlayerController.currentState = PinkPlayerState.BasicAttack;
            pinkPlayerController.nextState = PinkPlayerState.Idle;
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
