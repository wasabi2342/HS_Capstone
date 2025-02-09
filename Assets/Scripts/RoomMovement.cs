using Photon.Pun;
using System.Collections;
using UnityEngine;

public class RoomMovement : MonoBehaviourPun
{

    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float portalCooldown;

    private bool canUsePortal;
    private bool isInPortal;
    private Vector3 portalExitPosition;

    void Start()
    {
        if (!photonView.IsMine)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            Destroy(transform.GetComponentInChildren<Camera>().gameObject);
        }
        canUsePortal = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            float h = Input.GetAxisRaw("Horizontal");

            transform.Translate(new Vector3(h, 0, 0) * moveSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.UpArrow) && canUsePortal && isInPortal)
            {
                UsePortal();
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (photonView.IsMine)
        {
            Portal portal = collision.GetComponent<Portal>();
            if (portal != null)
            {
                isInPortal = false;
                Debug.Log("Æ÷Å» Å»Ãâ");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (photonView.IsMine)
        {
            Portal portal = collision.GetComponent<Portal>();
            if (portal != null)
            {
                isInPortal = true;
                portalExitPosition = portal.PortalExitPos();
                Debug.Log("Æ÷Å»¿¡ ´êÀ½");
                Debug.Log(portalExitPosition);
            }
        }
    }

}
