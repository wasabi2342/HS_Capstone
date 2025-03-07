using UnityEngine;

public class WhitePlayerController_Guard : MonoBehaviour
{
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController ������Ʈ�� �����ϴ�!");
        }
    }

    // ���� ��� Ʈ���� (��Ŭ�� �� ȣ��)
    public void TriggerGuard()
    {
        playerController.HandleGuard();
    }

    // �и� ��� Ʈ���� ( ���� �� ��Ŭ������ �и� �� �ݰ�)
    public void TriggerParry()
    {
        playerController.HandleParry();
    }

    // �ߵ�(�ݰ�) �̺�Ʈ (�ִϸ��̼� �̺�Ʈ Ȥ�� �ڷ�ƾ �� ȣ��)
    public void OnCounterAttackEvent()
    {
        playerController.OnCounterAttackEvent();
    }
}
