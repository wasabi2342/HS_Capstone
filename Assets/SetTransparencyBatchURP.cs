// File: Scripts/SetTransparencyBatchURP.cs
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class SetTransparencyBatchURP : MonoBehaviour
{
    [Header("투명하게 만들 오브젝트 리스트")]
    [SerializeField]
    private List<GameObject> targetObjects = new List<GameObject>();

    [Range(0f, 1f)]
    [Header("알파값 (0:완전투명 ~ 1:완전불투명)")]
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
                Debug.LogWarning($"{obj.name} 에 MeshRenderer가 없습니다.");
                continue;
            }

            if (!originalMaterials.ContainsKey(obj))
            {
                originalMaterials[obj] = meshRenderer.sharedMaterial; // 원본 저장
            }

            Material originalMaterial = meshRenderer.sharedMaterial;
            Material transparentMaterial = new Material(originalMaterial); // 복제

            meshRenderer.material = transparentMaterial; // 복제본 적용

            if (transparentMaterial.shader.name.Contains("Universal Render Pipeline/Lit"))
            {
                transparentMaterial.SetFloat("_Surface", 1); // Transparent 설정
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
                Debug.LogWarning($"{obj.name} 의 Shader가 URP Lit이 아닙니다.");
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
                Debug.LogWarning($"{obj.name} 에 MeshRenderer가 없습니다.");
                continue;
            }

            if (originalMaterials.ContainsKey(obj))
            {
                meshRenderer.sharedMaterial = originalMaterials[obj]; // 원본 복구
            }
        }

        originalMaterials.Clear();
    }
}
