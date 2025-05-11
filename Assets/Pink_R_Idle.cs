using UnityEngine;

public class Pink_R_Idle : StateMachineBehaviour
{
    const float transitionDuration = 0.1f;
    bool hasTransitioned;

    // R_Idle ���� �ø��� ����
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasTransitioned = false;
        // �Ķ���� �ʱ�ȭ
        animator.SetBool("ultimate", false);
        animator.SetBool("Pre-Attack", false);
        animator.SetBool("Pre-Input", false);
        animator.SetBool("CancleState", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("run", false);
        animator.SetBool("revive", false);

        // ���� ����
        var pc = animator.GetComponent<PinkPlayerController>();
        pc.currentState = PinkPlayerState.R_Idle;
    }

    // �� �����Ӹ��� run �Ķ���͸� üũ�ؼ�
    // run==true �� base layer�� Run���� CrossFade
    // run==false �� �ٽ� R_Idle�� CrossFade
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bool runParam = animator.GetBool("run");
        var pc = animator.GetComponent<PinkPlayerController>();

        // 1) move �Է��� �ͼ� ���� ���� �� �� ��� �� Run
        if (!hasTransitioned && runParam)
        {
            animator.CrossFade("Run", transitionDuration, 0);      // base layer(0) Run����
            pc.currentState = PinkPlayerState.Run;
            hasTransitioned = true;
        }
        // 2) �̹� Run���� ���̵� ���¿��ٰ� run �Ķ���Ͱ� ������ �� �ٽ� R_Idle
        else if (hasTransitioned && !runParam)
        {
            // stateInfo.shortNameHash�� �� Behaviour�� ���� "R_Idle" state�� �ؽð�
            animator.CrossFade(stateInfo.shortNameHash, transitionDuration, layerIndex);
            pc.currentState = PinkPlayerState.R_Idle;
            hasTransitioned = false;
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

