using UnityEngine;
using System.Collections;
public class TutoriaSelectBlessing : TutorialBase
{
   [SerializeField]
	private	string targetObjectName = "BlessingNPC";  // 찾을 오브젝트 이름

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
			GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("BlessingNPC");
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
		// 방법 1: SelectBlessingNPC 컴포넌트의 상호작용 가능 여부 확인
		if (blessingNPC != null)
		{
            // 추가한 public 메서드 사용
            bool canInteract = blessingNPC.CanInteract();
            if (!canInteract)
            {
                // 방법 2: UISelectBlessingPanel의 isSelected 확인
                UISelectBlessingPanel blessingPanel = FindAnyObjectByType<UISelectBlessingPanel>();
                if (blessingPanel != null && blessingPanel.isSelected)
                {
                    isCompleted = true;
                    Debug.Log("축복 선택 완료: 다음 튜토리얼로 진행합니다.");
                }
            }
		}

		if (isCompleted)
		{
			controller.SetNextTutorial();
		}
	}

	public override void Exit()
	{
	}
}
