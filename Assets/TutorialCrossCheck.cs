using UnityEngine;
using System.Collections;
public class TutorialCrossCheck : TutorialBase
{
[SerializeField]
	private	string targetObjectName = "NPC";  // 찾을 오브젝트 이름

	[SerializeField]
	private	bool isCompleted = false;
	private bool isInitialized = false;
	private GameObject targetObject = null;
	private SelectBlessingNPC blessingNPC = null;

	public override void Enter()
	{
		StartCoroutine(FindTargetObject());
	}
	
	private IEnumerator FindTargetObject()
	{
		// 타겟 오브젝트를 찾을 때까지 반복
		while (!isInitialized)
		{
			targetObject = null;
			
			// 1. 태그로 찾기
			GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("NPC");
			if (objectsWithTag.Length > 0)
			{
				targetObject = objectsWithTag[0];
				Debug.Log("태그로 오브젝트를 찾았습니다.");
			}
			
			// 2. 이름으로 찾기
			if (targetObject == null)
			{				// 씬의 모든 GameObjects 중에서 이름에 targetObjectName이 포함된 것 찾기
				foreach (GameObject obj in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
				{
					if (obj.name.Contains(targetObjectName))
					{
						targetObject = obj;
						Debug.Log($"이름 '{targetObjectName}'이 포함된 오브젝트를 찾았습니다: {obj.name}");
						break;
					}
				}
			}
			
			
			if (targetObject != null)
			{
                // SelectBlessingNPC 컴포넌트 가져오기
                blessingNPC = targetObject.GetComponent<SelectBlessingNPC>();
                if (blessingNPC == null)
                {
                    Debug.LogWarning("SelectBlessingNPC 컴포넌트를 찾을 수 없습니다.");
                }
                isInitialized = true;
            }	
			else
			{
				Debug.Log($"타겟 오브젝트를 찾는 중...");
				yield return new WaitForSeconds(0.5f); // 0.5초마다 검색
			}
		}
	}
	
	public override void Execute(TutorialController controller)
	{
		
	

		if (isCompleted)
		{
			controller.SetNextTutorial();
		}
	}

	public override void Exit()
	{
	}
}
