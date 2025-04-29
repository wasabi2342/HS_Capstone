using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class SetTransparencyBatchURP : MonoBehaviour
{
    [Header("투명하게 만들 오브젝트 리스트")]
    [SerializeField]
    private List<GameObject> targetObjects = new List<GameObject>();

    [Range(0f, 1f)]
    [Header("최대 알파값 (0:완전투명 ~ 1:완전불투명)")]
    public float maxTransparency = 0.5f;

    [Header("플레이어와 0이 되는 최소 거리")]
    public float minDistance = 1f;

    [Header("플레이어와 최대 알파가 되는 최대 거리")]
    public float maxDistance = 5f;

    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
    private Transform playerTransform = null;

    private void Update()
    {
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
            float currentAlpha = Mathf.Lerp(0f, maxTransparency, t);

            UpdateTransparency(currentAlpha);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") &&
            (!PhotonNetwork.InRoom || (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            playerTransform = other.transform;
            MakeAllTransparent(maxTransparency);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") &&
            (!PhotonNetwork.InRoom || (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            playerTransform = null;
            RestoreOriginalMaterials();
        }
    }

    private void ForceTransparentSettings(Material material)
    {
        // 이게 진짜 핵심
        material.SetFloat("_Surface", 1); // Transparent
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        material.SetInt("_ZWrite", 0); // ZWrite Off
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
    }

    private void MakeAllTransparent(float alpha)
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj == null) continue;

            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer == null) continue;

            if (!originalMaterials.ContainsKey(obj))
            {
                originalMaterials[obj] = meshRenderer.sharedMaterial;
            }

            Material originalMaterial = meshRenderer.sharedMaterial;
            Material transparentMaterial = new Material(originalMaterial);
            meshRenderer.material = transparentMaterial;

            if (transparentMaterial.shader.name.Contains("Universal Render Pipeline/Lit"))
            {
                ForceTransparentSettings(transparentMaterial);

                Color color = transparentMaterial.color;
                color.a = alpha;
                transparentMaterial.color = color;
            }
            else
            {
                Debug.LogWarning($"{obj.name} 의 Shader가 URP Lit이 아닙니다.");
            }
        }
    }

    private void UpdateTransparency(float alpha)
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj == null) continue;

            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer == null) continue;

            Material material = meshRenderer.material;
            if (material.shader.name.Contains("Universal Render Pipeline/Lit"))
            {
                Color color = material.color;
                color.a = alpha;
                material.color = color;

                ForceTransparentSettings(material);
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
