using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor.U2D.Animation;
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
    [SerializeField]
    private CharacterStats characterData;
    
    private Vector2 inputMoveDir;

    private bool canUsePortal;
    private Vector3 portalExitPosition;

    private InteractableObject nowObject;

    private bool canControl;
    private bool isInRoom;

    public Action startFillGauge;
    public Action<bool> canelFillGauge;

    public GameObject changeCharacterPrefab;
    public Dictionary<InputKey, (Blessing blessing, int level)> blessings;

    public Action<UIIcon, float> updateUIAction;
    public Action<UIIcon, Color> updateUIOutlineAction;

    void Start()
    {
        if (!photonView.IsMine && PhotonNetwork.InRoom)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.black;
        }
        canUsePortal = true;
        canControl = true;
        isInRoom = true;
    }

    void Update()
    {
        if (photonView.IsMine || !PhotonNetwork.InRoom)
        {
            if (!canControl)
                return;
            if (!isInRoom)
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
                    if (!isInRoom) // 훈련소로 화면 전환
                    {
                        RoomManager.Instance.EnterRestrictedArea(GetComponent<PhotonView>().ViewID);
                        EnterTrainingRoom();
                        isInRoom = true;
                    }
                    else // 훈련소에서 나오기
                    {
                        RoomManager.Instance.ExitRestrictedArea(GetComponent<PhotonView>().ViewID);
                        ExitTrainingRoom();
                        isInRoom = false;
                    }
                    break;
                case InteractableObject.mannequin:
                    if(context.started)
                    {
                        startFillGauge?.Invoke();
                    }
                    else if(context.canceled)
                    {
                        canelFillGauge?.Invoke(false);
                    }
                    else if(context.performed)
                    {
                        canelFillGauge?.Invoke(true);
                        //RoomManager.Instance.CreateCharacter(changeCharacterPrefab, transform.position, transform.rotation);
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

    public string ReturnName()
    {
        return characterData.name;
    }

    public void InitBlessing()
    {
        blessings = new Dictionary<InputKey, (Blessing blessing, int level)>();

        for(int i = 0; i < 5; i++)
        {
            blessings.Add((InputKey)i, (Blessing.none, 0));
        }
    }
}
