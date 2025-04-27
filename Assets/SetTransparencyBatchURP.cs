using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class SetTransparencyBatchURP : MonoBehaviour
{
    [Header("�����ϰ� ���� ������Ʈ ����Ʈ")]
    [SerializeField]
    private List<GameObject> targetObjects = new List<GameObject>();

    [Range(0f, 1f)]
    [Header("���İ� (0:�������� ~ 1:������)")]
    public float transparency = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Interactable") && (!PhotonNetwork.InRoom ||
         (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine))))
        {
            MakeAllTransparent();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Interactable") && (!PhotonNetwork.InRoom ||
         (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine))))
        {
            MakeAllOpaque();
        }
    }

    private void MakeAllTransparent()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj == null) continue;

            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogWarning($"{obj.name} �� MeshRenderer�� �����ϴ�.");
                continue;
            }

            Material originalMaterial = meshRenderer.sharedMaterial;
            Material instanceMaterial = new Material(originalMaterial);
            meshRenderer.material = instanceMaterial;

            if (instanceMaterial.shader.name.Contains("Universal Render Pipeline/Lit"))
            {
                instanceMaterial.SetFloat("_Surface", 1); // Transparent
                instanceMaterial.SetFloat("_Blend", 0); // Alpha Blend
                instanceMaterial.SetOverrideTag("RenderType", "Transparent");
                instanceMaterial.renderQueue = 3000;

                instanceMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                instanceMaterial.DisableKeyword("_SURFACE_TYPE_OPAQUE");

                Color color = instanceMaterial.color;
                color.a = transparency;
                instanceMaterial.color = color;
            }
            else
            {
                Debug.LogWarning($"{obj.name} �� Shader�� URP Lit�� �ƴմϴ�.");
            }
        }
    }
    private void MakeAllOpaque()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj == null) continue;

            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogWarning($"{obj.name} �� MeshRenderer�� �����ϴ�.");
                continue;
            }

            Material material = meshRenderer.material;

            if (material.shader.name.Contains("Universal Render Pipeline/Lit"))
            {
                material.SetFloat("_Surface", 0); // Opaque
                material.SetOverrideTag("RenderType", "Opaque");
                material.renderQueue = -1;

                material.EnableKeyword("_SURFACE_TYPE_OPAQUE");
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");

                Color color = material.color;
                color.a = 1.0f;
                material.color = color;
            }
            else
            {
                Debug.LogWarning($"{obj.name} �� Shader�� URP Lit�� �ƴմϴ�.");
            }
        }
    }
}