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
    [SerializeField] GameObject NPCIconPrefab;    // ★ 스테이지 보상 문(문 닫힌 아이콘 등)

    /* ────────────── Tag 옵션 ────────────── */
    [Header("Auto-Bind 옵션")]
    [Tooltip("씬 안의 ‘MinimapCamera’ 태그를 가진 카메라를 자동으로 물고 옵니다.")]
    [SerializeField] string minimapCameraTag = "MinimapCamera";


    /* ────────────── Internals ────────────── */
    // { 오브젝트 식별 ID → 아이콘 } 맵
    readonly Dictionary<int, GameObject> iconPool = new();

    void Awake()
    {
        BindMinimapCamera();                           // 최초 1회
        SceneManager.sceneLoaded += OnSceneLoaded;     // 씬 전환감시
    }
    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
    void OnSceneLoaded(Scene s, LoadSceneMode mode) => BindMinimapCamera();

    void BindMinimapCamera()
    {
        // 인스펙터에서 지정된 경우 패스
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
        // 3) RenderTexture 가 달린 첫번째 카메라 (최후 수단)
        if (minimapCamera == null)
        {
            foreach (var cam in FindObjectsOfType<Camera>())
            {
                if (cam.targetTexture != null) { minimapCamera = cam; break; }
            }
        }

        // RawImage에 텍스처 바인딩
        if (minimapCamera != null)
            minimapImage.texture = minimapCamera.targetTexture;
        else
            Debug.LogWarning("[UIMinimapPanel] MinimapCamera를 찾지 못했습니다.");
    }

    void Start() => StartCoroutine(CoRefreshIcons());

    IEnumerator CoRefreshIcons()
    {
        while (true)
        {
            RefreshIcons();
            yield return new WaitForSeconds(0.1f);   // 0.1초 주기
        }
    }

    /* ────────────── Main update ────────────── */
    void RefreshIcons()
    {
        /* 1) 이번 프레임에 필요한 아이콘(Transform/Prefab) 수집 */
        Dictionary<int, (Transform trans, GameObject prefab)> wanted = new();

        // 플레이어 아이콘
        foreach (var playerGO in GameObject.FindGameObjectsWithTag("Player"))
        {
            var pv = playerGO.GetComponent<PhotonView>();
            if (pv == null || pv.ViewID <= 0) continue;
            var prefab = pv.IsMine ? playerLocalIconPrefab : playerRemoteIconPrefab;
            wanted[pv.ViewID] = (playerGO.transform, prefab);
        }

        // 몬스터 아이콘
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

        /* 2) 아이콘 생성·재사용 및 위치 갱신 */
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

            // 월드 → 뷰포트 → UI좌표
            Vector3 vp = minimapCamera.WorldToViewportPoint(target.position);
            vp.x = Mathf.Clamp01(vp.x);
            vp.y = Mathf.Clamp01(vp.y);

            float x = (vp.x - 0.5f) * width;
            float y = (vp.y - 0.5f) * height;
            icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }

        /* 3) 더 이상 필요 없는 아이콘 제거 */
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
