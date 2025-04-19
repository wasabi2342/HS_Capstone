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

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            GameObject newLever = Instantiate(lever, leverPos1.position, Quaternion.identity);
            newLever.GetComponent<DoorLever>().successLeverInteraction += SuccessLeverInteraction;
            leverCount++;
        }
        else
        {
            GameObject newLever = PhotonNetwork.Instantiate(lever.name, leverPos1.position, Quaternion.identity);
            newLever.GetComponent<DoorLever>().successLeverInteraction += SuccessLeverInteraction;
            leverCount++;

            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                newLever = PhotonNetwork.Instantiate(lever.name, leverPos2.position, Quaternion.identity);
                newLever.GetComponent<DoorLever>().successLeverInteraction += SuccessLeverInteraction;
                leverCount++;
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
