using Photon.Pun;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerInRoom;

    public static RoomManager Instance {  get; private set; }

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        PhotonNetwork.Instantiate(playerInRoom.name, Vector3.zero, Quaternion.identity);
    }
}
