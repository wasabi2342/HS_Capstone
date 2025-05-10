using UnityEngine;

public class TutorialMonsterAttack : TutorialBase
{
    [SerializeField]
	private	WhitePlayerController	playerController;
    
    [SerializeField] 
    ScarecrowFSM scarecrow;
	
  [SerializeField]
  private float targetHP = 49000000f; // 특정 HP 값 (이 HP 이하로 내려가면 다음 튜토리얼로 넘어감)

	public override void Enter()
	{
		// 플레이어 이동 가능
        //scarecrow.HP = 49000000f; // 스켈레톤의 체력을 초기화
		// Trigger 오브젝트 활성화
		
    }
	public override void Execute(TutorialController controller)
	{
	

		// scarecrow의 체력이 목표 HP 이하면 다음 튜토리얼로 진행
		if (scarecrow != null && scarecrow.HP < targetHP)
		{
			controller.SetNextTutorial();
		}
	}

	public override void Exit()
	{

	}

}


