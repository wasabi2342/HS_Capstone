using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private PhotonView pv;

    [SerializeField]private int health = 100;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();

        if (pv.IsMine)
        {
            ExitGames.Client.Photon.Hashtable playerInfo = new ExitGames.Client.Photon.Hashtable();
            playerInfo.Add("ViewID", pv.ViewID);
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerInfo);
        }
    }

    void Update()
    {
        if (pv.IsMine)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");
            Vector3 movement = new Vector3(moveX, 0, moveZ).normalized;

            rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!pv.IsMine) return; // 네트워크 플레이어가 아니라면 무시

        health -= damage;
        Debug.Log($"[피격] 플레이어 체력 감소: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[사망] 플레이어 사망!");
        gameObject.SetActive(false);
    }
}
