using UnityEngine;

public class TutorialReviveCheck : TutorialBase
{
    [SerializeField]
	private	ReviveTutorial	reviveTutorial;


	public override void Enter()
	{
		
	}

	public override void Execute(TutorialController controller)
	{
        // ReviveTutorial 스크립트에서 revive 변수를 확인하여 튜토리얼 완료 여부 판단
        
		if ( reviveTutorial.revive == true )
		{
			controller.SetNextTutorial();
		}
	}

	public override void Exit()
	{

	}

	
}
