using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class PhotonNavMeshAgentView : MonoBehaviourPun, IPunObservable
{
    private NavMeshAgent agent;
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            agent.Warp(networkPosition);
            transform.rotation = networkRotation;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(agent.nextPosition);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
