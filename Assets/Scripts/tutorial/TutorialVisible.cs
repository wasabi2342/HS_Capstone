using UnityEngine;

[System.Serializable]
public struct Objects
{
	public GameObject visibleObject;
	public bool		  visible;
}

public class TutorialVisible : TutorialBase
{
	[SerializeField]
	private	Objects[]	objects;

	public override void Enter()
	{
		for ( int i = 0; i < objects.Length; ++ i )
		{
			objects[i].visibleObject.SetActive(objects[i].visible);
		}
	}

	public override void Execute(TutorialController controller)
	{
		controller.SetNextTutorial();
	}

	public override void Exit()
	{
	}
}

