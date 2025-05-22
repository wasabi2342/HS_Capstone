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
    [SerializeField] GameObject NPCIconPrefab;    // �� �������� ���� ��(�� ���� ������ ��)

    /* ���������������������������� Tag �ɼ� ���������������������������� */
    [Header("Auto-Bind �ɼ�")]
    [Tooltip("�� ���� ��MinimapCamera�� �±׸� ���� ī�޶� �ڵ����� ���� �ɴϴ�.")]
    [SerializeField] string minimapCameraTag = "MinimapCamera";


    /* ���������������������������� Internals ���������������������������� */
    // { ������Ʈ �ĺ� ID �� ������ } ��
    readonly Dictionary<int, GameObject> iconPool = new();

    void Awake()
    {
        BindMinimapCamera();                           // ���� 1ȸ
        SceneManager.sceneLoaded += OnSceneLoaded;     // �� ��ȯ����
    }
    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
    void OnSceneLoaded(Scene s, LoadSceneMode mode) => BindMinimapCamera();

    void BindMinimapCamera()
    {
        // �ν����Ϳ��� ������ ��� �н�
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
        // 3) RenderTexture �� �޸� ù��° ī�޶� (���� ����)
        if (minimapCamera == null)
        {
            foreach (var cam in FindObjectsOfType<Camera>())
            {
                if (cam.targetTexture != null) { minimapCamera = cam; break; }
            }
        }

        // RawImage�� �ؽ�ó ���ε�
        if (minimapCamera != null)
            minimapImage.texture = minimapCamera.targetTexture;
        else
            Debug.LogWarning("[UIMinimapPanel] MinimapCamera�� ã�� ���߽��ϴ�.");
    }

    void Start() => StartCoroutine(CoRefreshIcons());

    IEnumerator CoRefreshIcons()
    {
        while (true)
        {
            RefreshIcons();
            yield return new WaitForSeconds(0.1f);   // 0.1�� �ֱ�
        }
    }

    /* ���������������������������� Main update ���������������������������� */
    void RefreshIcons()
    {
        /* 1) �̹� �����ӿ� �ʿ��� ������(Transform/Prefab) ���� */
        Dictionary<int, (Transform trans, GameObject prefab)> wanted = new();

        // �÷��̾� ������
        foreach (var playerGO in GameObject.FindGameObjectsWithTag("Player"))
        {
            var pv = playerGO.GetComponent<PhotonView>();
            if (pv == null || pv.ViewID <= 0) continue;
            var prefab = pv.IsMine ? playerLocalIconPrefab : playerRemoteIconPrefab;
            wanted[pv.ViewID] = (playerGO.transform, prefab);
        }

        // ���� ������
        foreach (var monster in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            int id = monster.GetInstanceID();
            wanted[id] = (monster.transform, monsterIconPrefab);
        }
        foreach (var NPC in GameObject.FindGameObjectsWithTag("NPC"))
        {
            int id = NPC.GetInstanceID();
            wanted[id] = (NPC.transform, NPCIconPrefab);
        }

        /* 2) ������ ���������� �� ��ġ ���� */
        Rect rect = minimapImage.rectTransform.rect;
        float width = rect.width;
        float height = rect.height;

        foreach (var kv in wanted)
        {
            int id = kv.Key;
            Transform target = kv.Value.trans;
            GameObject prefab = kv.Value.prefab;

            if (!iconPool.TryGetValue(id, out var icon) || icon == null)
            {
                icon = Instantiate(prefab, iconsRoot);
                iconPool[id] = icon;
            }

            // ���� �� ����Ʈ �� UI��ǥ
            Vector3 vp = minimapCamera.WorldToViewportPoint(target.position);
            vp.x = Mathf.Clamp01(vp.x);
            vp.y = Mathf.Clamp01(vp.y);

            float x = (vp.x - 0.5f) * width;
            float y = (vp.y - 0.5f) * height;
            icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }

        /* 3) �� �̻� �ʿ� ���� ������ ���� */
        List<int> toRemove = new();
        foreach (var id in iconPool.Keys)
            if (!wanted.ContainsKey(id))
                toRemove.Add(id);

        foreach (var id in toRemove)
        {
            Destroy(iconPool[id]);
            iconPool.Remove(id);
        }
    }
}
