using UnityEngine;

public class TutorialEnemyAttack : TutorialBase
{    
    // RoomManager에서 로컬 플레이어를 가져오므로 SerializeField는 필요 없음
    private GameObject player;

    [SerializeField] 
    ScarecrowFSM scarecrow;

    private Animator playerAnimator;  // 플레이어의 애니메이터 컴포넌트
    private bool hasParried = false;  // 패리 여부 확인을 위한 플래그

    public override void Enter()
	{
        scarecrow.debugAttack=true;
        
        // RoomManager에서 로컬 플레이어 가져오기
        player = RoomManager.Instance.ReturnLocalPlayer();
        
        // 플레이어의 애니메이터 컴포넌트 가져오기
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            if (playerAnimator == null)
            {
                Debug.LogError("플레이어 오브젝트에 Animator 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogError("로컬 플레이어를 찾을 수 없습니다.");
        }
        
        hasParried = false;  // 패리 플래그 초기화
	}

	public override void Execute(TutorialController controller)
	{
        // 플레이어의 애니메이터에서 parry 파라미터 값 확인
        if (playerAnimator != null && !hasParried)
        {
            // 애니메이터에서 parry 파라미터 값 확인
            bool isParrying = playerAnimator.GetBool("parry");
            
            if (isParrying)
            {
                hasParried = true;  // 패리 성공 플래그 설정
                controller.SetNextTutorial();  // 다음 튜토리얼로 진행
            }
        }
	}

	public override void Exit()
	{
        // 필요한 경우 스케어크로우 공격 비활성화
        if (scarecrow != null)
        {
            scarecrow.SetAttackEnabled(false);
        }
	}
}
