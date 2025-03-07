using UnityEngine;

public class WhitePlayerController_Guard : MonoBehaviour
{
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController 컴포넌트가 없습니다!");
        }
    }

    // 가드 모션 트리거 (우클릭 시 호출)
    public void TriggerGuard()
    {
        playerController.HandleGuard();
    }

    // 패링 모션 트리거 ( 가드 중 좌클릭으로 패링 및 반격)
    public void TriggerParry()
    {
        playerController.HandleParry();
    }

    // 발도(반격) 이벤트 (애니메이션 이벤트 혹은 코루틴 내 호출)
    public void OnCounterAttackEvent()
    {
        playerController.OnCounterAttackEvent();
    }
}
