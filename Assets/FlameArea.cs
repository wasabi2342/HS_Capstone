using NUnit.Framework;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameArea : MonoBehaviourPun
{
    [SerializeField]
    private float damage;
    [SerializeField]
    private List<GameObject> effectList = new List<GameObject>();

    public void Init(float damage, float duration)
    {
        this.damage = damage;

        StartCoroutine(GenerateEffect(duration));

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("RPC_Init", RpcTarget.Others, duration);
        }
    }

    [PunRPC]
    public void RPC_Init(float duration)
    {
        StartCoroutine(GenerateEffect(duration));
    }

    private IEnumerator GenerateEffect(float duration)
    {
        for (int i = 0; i < effectList.Count; i++)
        {
            bool isLast = (i == effectList.Count - 1);
            StartCoroutine(DestroyAfterduration(effectList[i], duration, isLast));
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator DestroyAfterduration(GameObject effect, float duration, bool isLast)
    {
        effect.SetActive(true);

        yield return new WaitForSeconds(duration);

        effect.SetActive(false);

        if (isLast)
        {
            if (PhotonNetwork.IsConnected && photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        Debug.Log("Fire Area TriggerEnter");

        if (other.CompareTag("Enemy"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, transform.position);
            }
        }
    }
}
