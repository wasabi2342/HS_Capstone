using Photon.Pun;
using System.Collections;
using UnityEngine;

public enum InteractablePlace
{
    none,
    upgrade,
    selectCharacter,
    changeSkill,
    trainingRoom
}

public class RoomMovement : MonoBehaviourPun
{

    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float portalCooldown;

    private bool canUsePortal;
    private bool isInPortal;
    private Vector3 portalExitPosition;

    private bool canStartGame;

    private InteractablePlace nowPlace;

    void Start()
    {
        if (!photonView.IsMine)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            Destroy(transform.GetComponentInChildren<Camera>().gameObject);
        }
        canUsePortal = true;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            float h = Input.GetAxisRaw("Horizontal");

            transform.Translate(new Vector3(h, 0, 0) * moveSpeed * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (canUsePortal && isInPortal)
                {
                    UsePortal();
                }
                else if(canStartGame && !RoomManager.Instance.isEnteringStage)
                {
                    if (!RoomManager.Instance.IsPlayerInRestrictedArea())
                    {
                        RoomManager.Instance.InteractWithDungeonNPC();
                    }
                    else
                    {
                        UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("��� �÷��̾ ������ ���;� �մϴ�.");
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (photonView.IsMine)
        {
            // �±׸� ���� �۵�������� �����ϱ�
            Portal portal = collision.GetComponent<Portal>();
            if (portal != null)
            {
                isInPortal = true;
                portalExitPosition = portal.PortalExitPos();
                Debug.Log("��Ż�� ����");
                Debug.Log(portalExitPosition);
            }
            else
            {
                canStartGame = true;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (photonView.IsMine)
        {
            // �±׸� ���� �۵�������� �����ϱ�
            Portal portal = collision.GetComponent<Portal>();
            if (portal != null)
            {
                isInPortal = true;
                portalExitPosition = portal.PortalExitPos();
                Debug.Log("��Ż�� ����");
                Debug.Log(portalExitPosition);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (photonView.IsMine)
        {
            // �±׸� ���� �۵�������� �����ϱ�
            Portal portal = collision.GetComponent<Portal>();
            if (portal != null)
            {
                isInPortal = false;
                Debug.Log("��Ż Ż��");
            }
            else
            {
                canStartGame = false;
            }
        }
    }

    public void UpdateNowPlace(InteractablePlace place)
    {
        nowPlace = place;
        if(nowPlace == InteractablePlace.none)
        {
            RoomManager.Instance.ExitRestrictedArea(gameObject.GetComponent<PhotonView>().ViewID);
        }
        else
        {
            RoomManager.Instance.EnterRestrictedArea(gameObject.GetComponent<PhotonView>().ViewID);
        }
    }
}
