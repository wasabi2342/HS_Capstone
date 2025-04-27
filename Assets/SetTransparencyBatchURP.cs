// File: Scripts/SetTransparencyBatchURP.cs
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class SetTransparencyBatchURP : MonoBehaviour
{
    [Header("�����ϰ� ���� ������Ʈ ����Ʈ")]
    [SerializeField]
    private List<GameObject> targetObjects = new List<GameObject>();

    [Range(0f, 1f)]
    [Header("���İ� (0:�������� ~ 1:����������)")]
    public float transparency = 0.5f;

    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") &&
            (!PhotonNetwork.InRoom ||
             (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            MakeAllTransparent();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") &&
            (!PhotonNetwork.InRoom ||
             (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            RestoreOriginalMaterials();
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

            if (!originalMaterials.ContainsKey(obj))
            {
                originalMaterials[obj] = meshRenderer.sharedMaterial; // ���� ����
            }

            Material originalMaterial = meshRenderer.sharedMaterial;
            Material transparentMaterial = new Material(originalMaterial); // ����

            meshRenderer.material = transparentMaterial; // ������ ����

            if (transparentMaterial.shader.name.Contains("Universal Render Pipeline/Lit"))
            {
                transparentMaterial.SetFloat("_Surface", 1); // Transparent ����
                transparentMaterial.SetOverrideTag("RenderType", "Transparent");
                transparentMaterial.renderQueue = 3000;

                transparentMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                transparentMaterial.DisableKeyword("_SURFACE_TYPE_OPAQUE");

                Color color = transparentMaterial.color;
                color.a = transparency;
                transparentMaterial.color = color;
            }
            else
            {
                Debug.LogWarning($"{obj.name} �� Shader�� URP Lit�� �ƴմϴ�.");
            }
        }
    }

    private void RestoreOriginalMaterials()
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

            if (originalMaterials.ContainsKey(obj))
            {
                meshRenderer.sharedMaterial = originalMaterials[obj]; // ���� ����
            }
        }

        originalMaterials.Clear();
    }
}
