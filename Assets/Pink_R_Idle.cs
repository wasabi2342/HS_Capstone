using UnityEngine;

public class Pink_R_Idle : StateMachineBehaviour
{
    const float transitionDuration = 0.1f;
    bool hasTransitioned;

    // R_Idle 진입 시마다 리셋
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasTransitioned = false;
        // 파라미터 초기화
        animator.SetBool("ultimate", false);
        animator.SetBool("Pre-Attack", false);
        animator.SetBool("Pre-Input", false);
        animator.SetBool("CancleState", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("run", false);
        animator.SetBool("revive", false);

        // 상태 갱신
        var pc = animator.GetComponent<PinkPlayerController>();
        pc.currentState = PinkPlayerState.R_Idle;
    }

    // 매 프레임마다 run 파라미터를 체크해서
    // run==true → base layer의 Run으로 CrossFade
    // run==false → 다시 R_Idle로 CrossFade
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bool runParam = animator.GetBool("run");
        var pc = animator.GetComponent<PinkPlayerController>();

        // 1) move 입력이 와서 아직 전이 안 된 경우 → Run
        if (!hasTransitioned && runParam)
        {
            animator.CrossFade("Run", transitionDuration, 0);      // base layer(0) Run으로
            pc.currentState = PinkPlayerState.Run;
            hasTransitioned = true;
        }
        // 2) 이미 Run으로 전이된 상태였다가 run 파라미터가 꺼지면 → 다시 R_Idle
        else if (hasTransitioned && !runParam)
        {
            // stateInfo.shortNameHash는 이 Behaviour가 붙은 "R_Idle" state의 해시값
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

