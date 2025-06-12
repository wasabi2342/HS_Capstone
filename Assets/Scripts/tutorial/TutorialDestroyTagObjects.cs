using UnityEngine;

public class TutorialDestroyTagObjects : TutorialBase
{
	[SerializeField]
	private	WhitePlayerController	playerController;
	[SerializeField]
	private	GameObject[]		objectList;
	[SerializeField]
	private	string				tagName;

	public override void Enter()
	{
		// 플레이어의 이동, 공격이 가능하도록 설정 여기서 그 스킬 만 쓸수있게도 설정가능


		// 파괴해야할 오브젝트들을 활성화
		for ( int i = 0; i < objectList.Length; ++ i )
		{
			objectList[i].SetActive(true);
		}
	}

	public override void Execute(TutorialController controller)
	{
		GameObject[] objects = GameObject.FindGameObjectsWithTag(tagName);

		if ( objects.Length == 0 )
		{
			controller.SetNextTutorial();
		}
	}

	public override void Exit()
	{

	}
}

