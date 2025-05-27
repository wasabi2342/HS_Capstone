using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIMinimapPanel : UIBase
{
    /* ────────────── Refs ────────────── */
    [Header("Refs")]
    [SerializeField] Camera minimapCamera;          // 미니맵 전용 카메라
    [SerializeField] RawImage minimapImage;         // 미니맵 RenderTexture를 띄우는 RawImage
    [SerializeField] RectTransform iconsRoot;       // 아이콘을 자식으로 둘 RectTransform

    /* ────────────── Icon Prefabs ────────────── */
    [Header("Icon Prefabs")]
    [SerializeField] GameObject playerLocalIconPrefab;   // 로컬 플레이어용 파란 원
    [SerializeField] GameObject playerRemoteIconPrefab;  // 원격 플레이어용 초록 원
    [SerializeField] GameObject monsterIconPrefab;       // 몬스터용 빨강 원
    [SerializeField] GameObject rewardDoorIconPrefab;    // 보상 문 아이콘

    /* ────────────── Tag / Layer 옵션 ────────────── */
    [Header("Auto-Bind / Filter 옵션")]
    [Tooltip("씬 안의 ‘MinimapCamera’ 태그를 가진 카메라를 자동으로 물고 옵니다.")]
    [SerializeField] string minimapCameraTag = "MinimapCamera";

    [Tooltip("보상 문 프리팹에 지정된 Tag 이름")]
    [SerializeField] string rewardDoorTag = "RewardDoor";

    [Space(4)]
    [Tooltip("미니맵에서 제외할 ‘Servant’ 레이어 이름")]
    [SerializeField] string servantLayerName = "Servant";

    /* ────────────── Internals ────────────── */
    readonly Dictionary<int, GameObject> iconPool = new();
    int servantLayer = -1;

    /* ────────────── Unity LifeCycle ────────────── */
    void Awake()
    {
        servantLayer = LayerMask.NameToLayer(servantLayerName);   // 존재하지 않으면 -1
        BindMinimapCamera();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
    void OnSceneLoaded(Scene s, LoadSceneMode mode) => BindMinimapCamera();

    void Start() => StartCoroutine(CoRefreshIcons());

    /* ────────────── Bind Cam ────────────── */
    void BindMinimapCamera()
    {
        if (minimapCamera != null) return;

        // 1) 태그 검색
        if (!string.IsNullOrEmpty(minimapCameraTag))
        {
            var camGO = GameObject.FindGameObjectWithTag(minimapCameraTag);
            if (camGO) minimapCamera = camGO.GetComponent<Camera>();
        }
        // 2) 이름 검색 (fallback)
        if (minimapCamera == null)
        {
            var camGO = GameObject.Find("MinimapCamera");
            if (camGO) minimapCamera = camGO.GetComponent<Camera>();
        }
        // 3) RenderTexture 달린 카메라 검색 (최후 수단)
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
            Debug.LogWarning("[UIMinimapPanel] MinimapCamera를 찾지 못했습니다.");
    }

    /* ────────────── 코루틴 ────────────── */
    IEnumerator CoRefreshIcons()
    {
        while (true)
        {
            RefreshIcons();
            yield return new WaitForSeconds(0.1f);
        }
    }

    /* ────────────── 메인 로직 ────────────── */
    void RefreshIcons()
    {
        /* 1) 이번 프레임에 필요한 아이콘들 수집 */
        Dictionary<int, (Transform tr, GameObject prefab)> wanted = new();

        // 플레이어 (단, Servant 레이어는 제외)
        foreach (var playerGO in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (servantLayer != -1 && playerGO.layer == servantLayer) continue; // ★ 제외

            var pv = playerGO.GetComponent<PhotonView>();
            if (pv == null || pv.ViewID <= 0) continue;

            var prefab = pv.IsMine ? playerLocalIconPrefab : playerRemoteIconPrefab;
            wanted[pv.ViewID] = (playerGO.transform, prefab);
        }

        // 몬스터
        foreach (var monster in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            int id = monster.GetInstanceID();
            wanted[id] = (monster.transform, monsterIconPrefab);
        }

        // 보상 문
        if (!string.IsNullOrEmpty(rewardDoorTag) && rewardDoorIconPrefab != null)
        {
            foreach (var door in GameObject.FindGameObjectsWithTag(rewardDoorTag))
            {
                int id = door.GetInstanceID();
                wanted[id] = (door.transform, rewardDoorIconPrefab);
            }
        }

        /* 2) 아이콘 생성/재사용 & 위치 갱신 */
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

        /* 3) 더 이상 필요 없는 아이콘 제거 */
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
