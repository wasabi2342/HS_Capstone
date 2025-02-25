using Photon.Pun;
using UnityEngine;

public class Bar : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private bool isLeftEntry;

    private Color color = Color.white;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PhotonView>().IsMine)
            {
                if (isLeftEntry)
                    color.a = Mathf.Clamp((transform.position.x - other.transform.position.x) / 2, 0f, 1f);
                else
                    color.a = Mathf.Clamp((other.transform.position.x - transform.position.x) / 2, 0f, 1f);
                spriteRenderer.color = color;   
            }
        }
    }
}
