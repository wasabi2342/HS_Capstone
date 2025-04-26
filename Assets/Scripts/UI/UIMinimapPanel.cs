using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UIMinimapPanel : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Camera minimapCamera;          // �̴ϸ� ���� ī�޶�
    [SerializeField] RawImage minimapImage;         // �̴ϸ� RenderTexture�� ���� RawImage
    [SerializeField] RectTransform iconsRoot;       // �������� �ڽ����� �� RectTransform

    [Header("Icon Prefabs")]
    [SerializeField] GameObject playerLocalIconPrefab;   // ���� �÷��̾�� �Ķ� ��
    [SerializeField] GameObject playerRemoteIconPrefab;  // ���� �÷��̾�� �ʷ� ��
    [SerializeField] GameObject monsterIconPrefab;       // ���Ϳ� ���� ��

    // { ������Ʈ �ĺ� ID �� ������ } ��
    Dictionary<int, GameObject> iconPool = new Dictionary<int, GameObject>();

    void LateUpdate()
    {
        RefreshIcons();
    }

    void RefreshIcons()
    {
        // 1) �̹� �����ӿ� �ʿ��� ������ ����� ����ϴ�.
        var wanted = new Dictionary<int, (Transform trans, GameObject prefab)>();

        // --- �÷��̾� ������: Tag("Player") �� ���͸� ---
        foreach (var playerGO in GameObject.FindGameObjectsWithTag("Player"))
        {
            var pv = playerGO.GetComponent<PhotonView>();
            if (pv == null || pv.ViewID <= 0) continue;

            var prefab = pv.IsMine
                ? playerLocalIconPrefab
                : playerRemoteIconPrefab;

            wanted[pv.ViewID] = (playerGO.transform, prefab);
        }

        // ��� ����: Tag("Enemy")�� ���͸�
        foreach (var monster in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            int id = monster.GetInstanceID();
            wanted[id] = (monster.transform, monsterIconPrefab);
        }

        // 2) ������ ���� �Ǵ� ���� & ��ġ ����
        var rect = minimapImage.rectTransform.rect;
        float width = rect.width;
        float height = rect.height;

        foreach (var kv in wanted)
        {
            int id = kv.Key;
            Transform trans = kv.Value.trans;
            GameObject prefab = kv.Value.prefab;
            GameObject icon;

            // ������ Ǯ�� ������ ���� ����
            if (!iconPool.TryGetValue(id, out icon))
            {
                icon = Instantiate(prefab, iconsRoot);
                iconPool[id] = icon;
            }

            // ���� �� ����Ʈ �� UI ��ǥ ��ȯ
            Vector3 vp = minimapCamera.WorldToViewportPoint(trans.position);
            vp.x = Mathf.Clamp01(vp.x);
            vp.y = Mathf.Clamp01(vp.y);

            float x = (vp.x - 0.5f) * width;
            float y = (vp.y - 0.5f) * height;

            icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }

        // 3) �� �̻� �ʿ� ���� �������� ����
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
