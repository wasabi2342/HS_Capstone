using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIMinimapPanel : UIBase
{
    /* ���������������������������� Refs ���������������������������� */
    [Header("Refs")]
    [SerializeField] Camera minimapCamera;          // �̴ϸ� ���� ī�޶�
    [SerializeField] RawImage minimapImage;         // �̴ϸ� RenderTexture�� ���� RawImage
    [SerializeField] RectTransform iconsRoot;       // �������� �ڽ����� �� RectTransform

    /* ���������������������������� Icon Prefabs ���������������������������� */
    [Header("Icon Prefabs")]
    [SerializeField] GameObject playerLocalIconPrefab;   // ���� �÷��̾�� �Ķ� ��
    [SerializeField] GameObject playerRemoteIconPrefab;  // ���� �÷��̾�� �ʷ� ��
    [SerializeField] GameObject monsterIconPrefab;       // ���Ϳ� ���� ��
    [SerializeField] GameObject rewardDoorIconPrefab;    // ���� �� ������

    /* ���������������������������� Tag / Layer �ɼ� ���������������������������� */
    [Header("Auto-Bind / Filter �ɼ�")]
    [Tooltip("�� ���� ��MinimapCamera�� �±׸� ���� ī�޶� �ڵ����� ���� �ɴϴ�.")]
    [SerializeField] string minimapCameraTag = "MinimapCamera";

    [Tooltip("���� �� �����տ� ������ Tag �̸�")]
    [SerializeField] string rewardDoorTag = "RewardDoor";

    [Space(4)]
    [Tooltip("�̴ϸʿ��� ������ ��Servant�� ���̾� �̸�")]
    [SerializeField] string servantLayerName = "Servant";

    /* ���������������������������� Internals ���������������������������� */
    readonly Dictionary<int, GameObject> iconPool = new();
    int servantLayer = -1;

    /* ���������������������������� Unity LifeCycle ���������������������������� */
    void Awake()
    {
        servantLayer = LayerMask.NameToLayer(servantLayerName);   // �������� ������ -1
        BindMinimapCamera();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
    void OnSceneLoaded(Scene s, LoadSceneMode mode) => BindMinimapCamera();

    void Start() => StartCoroutine(CoRefreshIcons());

    /* ���������������������������� Bind Cam ���������������������������� */
    void BindMinimapCamera()
    {
        if (minimapCamera != null) return;

        // 1) �±� �˻�
        if (!string.IsNullOrEmpty(minimapCameraTag))
        {
            var camGO = GameObject.FindGameObjectWithTag(minimapCameraTag);
            if (camGO) minimapCamera = camGO.GetComponent<Camera>();
        }
        // 2) �̸� �˻� (fallback)
        if (minimapCamera == null)
        {
            var camGO = GameObject.Find("MinimapCamera");
            if (camGO) minimapCamera = camGO.GetComponent<Camera>();
        }
        // 3) RenderTexture �޸� ī�޶� �˻� (���� ����)
        if (minimapCamera == null)
        {
            foreach (var cam in FindObjectsOfType<Camera>())
            {
                if (cam.targetTexture != null) { minimapCamera = cam; break; }
            }
        }

        if (minimapCamera != null)
            minimapImage.texture = minimapCamera.targetTexture;
        else
            Debug.LogWarning("[UIMinimapPanel] MinimapCamera�� ã�� ���߽��ϴ�.");
    }

    /* ���������������������������� �ڷ�ƾ ���������������������������� */
    IEnumerator CoRefreshIcons()
    {
        while (true)
        {
            RefreshIcons();
            yield return new WaitForSeconds(0.1f);
        }
    }

    /* ���������������������������� ���� ���� ���������������������������� */
    void RefreshIcons()
    {
        /* 1) �̹� �����ӿ� �ʿ��� �����ܵ� ���� */
        Dictionary<int, (Transform tr, GameObject prefab)> wanted = new();

        // �÷��̾� (��, Servant ���̾�� ����)
        foreach (var playerGO in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (servantLayer != -1 && playerGO.layer == servantLayer) continue; // �� ����

            var pv = playerGO.GetComponent<PhotonView>();
            if (pv == null || pv.ViewID <= 0) continue;

            var prefab = pv.IsMine ? playerLocalIconPrefab : playerRemoteIconPrefab;
            wanted[pv.ViewID] = (playerGO.transform, prefab);
        }

        // ����
        foreach (var monster in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            int id = monster.GetInstanceID();
            wanted[id] = (monster.transform, monsterIconPrefab);
        }

        // ���� ��
        if (!string.IsNullOrEmpty(rewardDoorTag) && rewardDoorIconPrefab != null)
        {
            foreach (var door in GameObject.FindGameObjectsWithTag(rewardDoorTag))
            {
                int id = door.GetInstanceID();
                wanted[id] = (door.transform, rewardDoorIconPrefab);
            }
        }

        /* 2) ������ ����/���� & ��ġ ���� */
        Rect rect = minimapImage.rectTransform.rect;
        float w = rect.width, h = rect.height;

        foreach (var kv in wanted)
        {
            int id = kv.Key;
            Transform target = kv.Value.tr;
            GameObject prefab = kv.Value.prefab;

            if (!iconPool.TryGetValue(id, out var icon) || icon == null)
            {
                icon = Instantiate(prefab, iconsRoot);
                iconPool[id] = icon;
            }

            Vector3 vp = minimapCamera.WorldToViewportPoint(target.position);
            vp.x = Mathf.Clamp01(vp.x);
            vp.y = Mathf.Clamp01(vp.y);
            icon.GetComponent<RectTransform>().anchoredPosition =
                new Vector2((vp.x - 0.5f) * w, (vp.y - 0.5f) * h);
        }

        /* 3) �� �̻� �ʿ� ���� ������ ���� */
        List<int> obsolete = new();
        foreach (var id in iconPool.Keys)
            if (!wanted.ContainsKey(id))
                obsolete.Add(id);

        foreach (var id in obsolete)
        {
            Destroy(iconPool[id]);
            iconPool.Remove(id);
        }
    }
}
