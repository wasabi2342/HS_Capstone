using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using Unity.Cinemachine;
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
    mannequin
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

    public Action startFillGauge;
    public Action canelFillGauge;

    public GameObject changeCharacterPrefab;

    void Start()
    {
        if (!photonView.IsMine && PhotonNetwork.InRoom)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.black;
        }
        canUsePortal = true;
        canControl = true;
        isInTrainingRoom = false;
    }

    void Update()
    {
        if (photonView.IsMine || !PhotonNetwork.InRoom)
        {
            if (!canControl)
                return;
            if (!isInTrainingRoom)
                transform.Translate(new Vector3(inputMoveDir.x, 0, 0) * moveSpeed * Time.deltaTime);
            else
                transform.Translate(new Vector3(inputMoveDir.x, inputMoveDir.y, 0) * moveSpeed * Time.deltaTime);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (photonView.IsMine || !PhotonNetwork.InRoom)
            inputMoveDir = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (canControl && (!PhotonNetwork.InRoom || photonView.IsMine))
        {
            Debug.Log("OnInteract ȣ���!");
            switch (nowObject)
            {
                case InteractableObject.portal:
                    if (canUsePortal)
                    {
                        Debug.Log("��Ż ���");
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
                            UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("��� �÷��̾ ������ ���;� �մϴ�.");
                        }
                    }
                    break;
                case InteractableObject.upgrade: // ���׷��̵� UI�����ϱ�
                    break;
                case InteractableObject.selectCharacter: // ĳ���� ���� UI�����ϱ�
                    RoomManager.Instance.EnterRestrictedArea(GetComponent<PhotonView>().ViewID);
                    canControl = false;

                    UIManager.Instance.OpenPopupPanel<UISelectCharacterPanel>().onClose += () =>
                    {
                        RoomManager.Instance.ExitRestrictedArea(GetComponent<PhotonView>().ViewID);
                        canControl = true;
                    };
                    break;
                case InteractableObject.changeSkill: // ��ų���� UI �����ϱ�
                    break;
                case InteractableObject.trainingRoom:
                    if (!isInTrainingRoom) // �Ʒüҷ� ȭ�� ��ȯ
                    {
                        RoomManager.Instance.EnterRestrictedArea(GetComponent<PhotonView>().ViewID);
                        EnterTrainingRoom();
                        isInTrainingRoom = true;
                    }
                    else // �Ʒüҿ��� ������
                    {
                        RoomManager.Instance.ExitRestrictedArea(GetComponent<PhotonView>().ViewID);
                        ExitTrainingRoom();
                        isInTrainingRoom = false;
                    }
                    break;
                case InteractableObject.mannequin:
                    if(context.started)
                    {
                        startFillGauge?.Invoke();
                    }
                    else if(context.canceled)
                    {
                        canelFillGauge?.Invoke();
                    }
                    else if(context.performed)
                    {
                        RoomManager.Instance.CreateCharacter(changeCharacterPrefab);
                        Destroy(gameObject);
                    }
                    break;
            }
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
        sequence.Append(transform.DOMoveX(20, 0.5f));
        //sequence.Append(transform.DORotate(new Vector3(90, 0, 0), 1f, RotateMode.LocalAxisAdd));
        sequence.OnComplete(() => CanControl());
        sequence.Play();
    }

    private void ExitTrainingRoom()
    {
        canControl = false;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(transform.DOMoveZ(transform.position.z - 3, 1f));
        sequence.Append(transform.DORotate(new Vector3(0, 90, 0), 1f, RotateMode.LocalAxisAdd));
        sequence.Append(transform.DOMove(new Vector3(transform.position.x, -0.5f, 0), 0.5f));
        sequence.OnComplete(() => CanControl());
        sequence.Play();
    }

    private void CanControl()
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
        Debug.Log(nowObject);
    }
}
