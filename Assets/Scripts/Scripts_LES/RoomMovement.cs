using DG.Tweening;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InteractableObject
{
    none,
    portal,
    gameStart,
    upgrade,
    selectCharacter,
    changeSkill,
    trainingRoom,
}

public class RoomMovement : MonoBehaviourPun
{

    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float portalCooldown;

    private bool canUsePortal;
    private Vector3 portalExitPosition;

    private InteractableObject nowObject;

    private bool canControl;
    private bool isInTrainingRoom;
    
    private Vector2 inputMoveDir;


    void Start()
    {
        if (!photonView.IsMine)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            Destroy(transform.GetComponentInChildren<Camera>().gameObject);
        }
        canUsePortal = true;
        canControl = true;
        isInTrainingRoom = false;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (!canControl)
                return;
            if(!isInTrainingRoom)
                transform.Translate(new Vector3(inputMoveDir.x, 0, 0) * moveSpeed * Time.deltaTime);
            else
                transform.Translate(new Vector3(inputMoveDir.x, inputMoveDir.y, 0) * moveSpeed * Time.deltaTime);
        }
    }

    private void OnMove(InputValue inputValue)
    {
        inputMoveDir = inputValue.Get<Vector2>();
        //transform.Translate(new Vector3(inputMoveDir.x, 0, 0) * moveSpeed * Time.deltaTime);
    }

    private void OnInteract(InputValue inputValue)
    {
        Debug.Log("OnInteract 호출됨!");
        switch (nowObject)
        {
            case InteractableObject.portal:
                if (canUsePortal)
                {
                    Debug.Log("포탈 사용");
                    UsePortal();
                }
                break;
            case InteractableObject.gameStart:
                if (!RoomManager.Instance.isEnteringStage)
                {
                    if (!RoomManager.Instance.IsPlayerInRestrictedArea())
                    {
                        RoomManager.Instance.InteractWithDungeonNPC().onClose += () => canControl = true;
                        canControl = false;
                    }
                    else
                    {
                        UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("모든 플레이어가 밖으로 나와야 합니다.");
                    }
                }
                break;
            case InteractableObject.upgrade: // 업그래이드 UI생성하기
                break;
            case InteractableObject.selectCharacter: // 캐릭터 선택 UI생성하기
                RoomManager.Instance.EnterRestrictedArea(GetComponent<PhotonView>().ViewID);
                canControl = false;

                UIManager.Instance.OpenPopupPanel<UISelectCharacterPanel>().onClose += () =>
                {
                    RoomManager.Instance.ExitRestrictedArea(GetComponent<PhotonView>().ViewID);
                    canControl = true;
                };
                break;
            case InteractableObject.changeSkill: // 스킬변경 UI 생성하기
                break;
            case InteractableObject.trainingRoom:
                if (!isInTrainingRoom) // 훈련소로 화면 전환
                {
                    RoomManager.Instance.EnterRestrictedArea(GetComponent<PhotonView>().ViewID);
                    EnterTrainingRoom();
                    isInTrainingRoom = true;
                }
                else // 훈련소에서 나오기
                {

                }
                break;
        }
    }

    private void UsePortal()
    {
        gameObject.transform.position = portalExitPosition;
        canUsePortal = false;
        StartCoroutine(PortalCooldownCheck(portalCooldown));
    }

    private IEnumerator PortalCooldownCheck(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canUsePortal = true;
    }

    private void EnterTrainingRoom()
    {
        canControl = false;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(transform.DORotate(new Vector3(0, -90, 0), 1f, RotateMode.LocalAxisAdd));
        sequence.Append(transform.DOMoveZ(transform.position.z + 3, 1f));
        sequence.Append(transform.DORotate(new Vector3(90, 0, 0), 1f, RotateMode.LocalAxisAdd));
        sequence.OnComplete(() => CompleteEnterTrainingRoom());
        sequence.Play();
    }

    private void CompleteEnterTrainingRoom()
    {
        canControl = true;
    }

    public void GetPortalExitPosition(Vector3 pos)
    {
        portalExitPosition = pos;
    }

    public void UpdateNowInteractable(InteractableObject obj)
    {
        nowObject = obj;
    }
}
