using UnityEngine;

public class WhitePlayerController_Guard : MonoBehaviour
{
    private WhitePlayerController whitePlayerController;

    private void Awake()
    {
        whitePlayerController = GetComponent<WhitePlayerController>();
        if (whitePlayerController == null)
        {
            Debug.LogError("WhitePlayerController ������Ʈ�� �����ϴ�!");
        }
    }
    public void StartGuard()
    {

    }
    //// ���� ��� Ʈ���� (��Ŭ�� �� ȣ��)
    //public void TriggerGuard()
    //{
    //    whitePlayerController.HandleGuard();
    //}

    //// �и� ��� Ʈ���� (���� �� ��Ŭ������ �и� �� �ݰ�)
    //public void TriggerParry()
    //{
    //    whitePlayerController.HandleParry();
    //}

    //// �ߵ�(�ݰ�) �̺�Ʈ (�ִϸ��̼� �̺�Ʈ Ȥ�� �ڷ�ƾ �� ȣ��)
    //public void OnCounterAttackEvent()
    //{
    //    whitePlayerController.OnCounterAttackEvent();
    //}
}
