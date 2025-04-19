using DG.Tweening;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractionManager : MonoBehaviour
{
    [SerializeField]
    private Transform leverPos1;
    [SerializeField]
    private Transform leverPos2;
    [SerializeField]
    private GameObject lever;
    [SerializeField]
    private GameObject doorLeft;
    [SerializeField]
    private GameObject doorRight;

    private int leverCount = 0;
    private int successLeverCount = 0;

    public static DoorInteractionManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            GameObject newLever = Instantiate(lever, leverPos1.position, Quaternion.identity);
            leverCount++;
        }
        else
        {
            leverCount = PhotonNetwork.CurrentRoom.PlayerCount > 1 ? 2 : 1;

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            GameObject newLever = PhotonNetwork.Instantiate(lever.name, leverPos1.position, Quaternion.identity);

            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                newLever = PhotonNetwork.Instantiate(lever.name, leverPos2.position, Quaternion.identity);
            }
        }
    }

    public void SuccessLeverInteraction()
    {
        successLeverCount++;

        if(successLeverCount >= leverCount)
        {
            OpenDoor();
        }
    }

    public void OpenDoor()
    {
        doorLeft.transform.DORotate(new Vector3(0f, -90f, 0f), 1f, RotateMode.LocalAxisAdd);
        doorRight.transform.DORotate(new Vector3(0f, 90f, 0f), 1f, RotateMode.LocalAxisAdd);
    }
}
