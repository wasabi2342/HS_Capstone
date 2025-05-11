using UnityEngine;
using System.Collections;
public class TutorialReSpawn : TutorialBase
{
    [SerializeField]
	private	string targetObjectName = "BlessingNPC";  // 찾을 오브젝트 이름

	[SerializeField]
	private	Vector3	endPosition;
	private	bool isCompleted = false;
	private bool isInitialized = false;

	public override void Enter()
	{
		StartCoroutine(FindTargetObject());
	}
	
	private IEnumerator FindTargetObject()
	{
		// 타겟 오브젝트를 찾을 때까지 반복
		while (!isInitialized)
		{
			GameObject targetObject = null;
			
			// 1. 태그로 찾기
			GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(targetObjectName);
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
			
			// 오브젝트를 찾았다면 Transform 가져오기
			if (targetObject != null)
			{
				Transform transform = targetObject.transform;
				Debug.Log($"타겟 오브젝트 '{targetObject.name}' 찾음");
				
				// 위치 이동 시작 (직접 변환)
				StartCoroutine(MoveTransform(transform));
				isInitialized = true;
			}
			else
			{
				Debug.Log($"타겟 오브젝트를 찾는 중...");
				yield return new WaitForSeconds(0.5f); // 0.5초마다 검색
			}
		}
	}
	
	private IEnumerator MoveTransform(Transform transform)
	{
		if (transform != null)
		{
			// 현재 위치에서 목표 위치로 이동
			Vector3 startPosition = transform.position;
			float elapsed = 0f;
			float duration = 0.1f; // 이동에 걸리는 시간 (초)
			
			while (elapsed < duration)
			{
				transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
				elapsed += Time.deltaTime;
				yield return null;
			}
			
			// 최종 위치 설정
			transform.position = endPosition;
			isCompleted = true;
		}
		else
		{
			Debug.LogError("Transform이 null입니다");
			isCompleted = true; // 에러가 발생해도 진행되도록 함
		}
	}

	public override void Execute(TutorialController controller)
	{
		if (isCompleted == true)
		{
			controller.SetNextTutorial();
		}
	}

	public override void Exit()
	{
	}
}
