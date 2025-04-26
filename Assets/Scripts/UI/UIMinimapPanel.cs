using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UIMinimapPanel : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Camera minimapCamera;          // 미니맵 전용 카메라
    [SerializeField] RawImage minimapImage;         // 미니맵 RenderTexture를 띄우는 RawImage
    [SerializeField] RectTransform iconsRoot;       // 아이콘을 자식으로 둘 RectTransform

    [Header("Icon Prefabs")]
    [SerializeField] GameObject playerLocalIconPrefab;   // 로컬 플레이어용 파란 원
    [SerializeField] GameObject playerRemoteIconPrefab;  // 원격 플레이어용 초록 원
    [SerializeField] GameObject monsterIconPrefab;       // 몬스터용 빨강 원

    // { 오브젝트 식별 ID → 아이콘 } 맵
    Dictionary<int, GameObject> iconPool = new Dictionary<int, GameObject>();

    void LateUpdate()
    {
        RefreshIcons();
    }

    void RefreshIcons()
    {
        // 1) 이번 프레임에 필요할 아이콘 목록을 만듭니다.
        var wanted = new Dictionary<int, (Transform trans, GameObject prefab)>();

        // --- 플레이어 아이콘: Tag("Player") 로 필터링 ---
        foreach (var playerGO in GameObject.FindGameObjectsWithTag("Player"))
        {
            var pv = playerGO.GetComponent<PhotonView>();
            if (pv == null || pv.ViewID <= 0) continue;

            var prefab = pv.IsMine
                ? playerLocalIconPrefab
                : playerRemoteIconPrefab;

            wanted[pv.ViewID] = (playerGO.transform, prefab);
        }

        // 모든 몬스터: Tag("Enemy")로 필터링
        foreach (var monster in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            int id = monster.GetInstanceID();
            wanted[id] = (monster.transform, monsterIconPrefab);
        }

        // 2) 아이콘 생성 또는 재사용 & 위치 갱신
        var rect = minimapImage.rectTransform.rect;
        float width = rect.width;
        float height = rect.height;

        foreach (var kv in wanted)
        {
            int id = kv.Key;
            Transform trans = kv.Value.trans;
            GameObject prefab = kv.Value.prefab;
            GameObject icon;

            // 아이콘 풀에 없으면 새로 생성
            if (!iconPool.TryGetValue(id, out icon))
            {
                icon = Instantiate(prefab, iconsRoot);
                iconPool[id] = icon;
            }

            // 월드 → 뷰포트 → UI 좌표 변환
            Vector3 vp = minimapCamera.WorldToViewportPoint(trans.position);
            vp.x = Mathf.Clamp01(vp.x);
            vp.y = Mathf.Clamp01(vp.y);

            float x = (vp.x - 0.5f) * width;
            float y = (vp.y - 0.5f) * height;

            icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }

        // 3) 더 이상 필요 없는 아이콘은 제거
        var toRemove = new List<int>();
        foreach (var existingId in iconPool.Keys)
            if (!wanted.ContainsKey(existingId))
                toRemove.Add(existingId);

        foreach (var id in toRemove)
        {
            Destroy(iconPool[id]);
            iconPool.Remove(id);
        }
    }
}
