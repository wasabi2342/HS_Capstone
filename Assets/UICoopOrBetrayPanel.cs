using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CoopType
{
    defaultType,
    PVPType
}

public class UICoopOrBetrayPanel : UIBase
{
    [SerializeField]
    private Button coopButton;
    [SerializeField]
    private Button betrayButton;
    [SerializeField]
    private Button coopConfirmButton;
    [SerializeField]
    private Button betrayConfirmButton;
    [SerializeField]
    private Button resetInfoPanelButton;
    [SerializeField]
    private List<RectTransform> labelList = new List<RectTransform>();
    [SerializeField]
    private List<Vector3> labelPosList = new List<Vector3>();
    [SerializeField]
    private List<Vector3> labelStartPosList = new List<Vector3>();

    [SerializeField]
    private RectTransform cross1;
    [SerializeField]
    private RectTransform cross2;
    [SerializeField]
    private Vector3 cross1TargetPos;
    [SerializeField]
    private Vector3 cross2TargetPos;
    [SerializeField]
    private Image crossImage;

    [SerializeField]
    private Sprite[] coopButtonSprites;
    [SerializeField]
    private Sprite[] betrayButtonSprites;
    [SerializeField]
    private Sprite[] crossSprites;

    [SerializeField]
    private UICoopOrBetrayInfoPanel coopInfoPanel;
    [SerializeField]
    private UICoopOrBetrayInfoPanel betrayInfoPanel;
    [SerializeField]
    private TextMeshProUGUI coopHeaderText;
    [SerializeField]
    private TextMeshProUGUI coopBodyText;
    [SerializeField]
    private TextMeshProUGUI betaryHeaderText;
    [SerializeField]
    private TextMeshProUGUI betrayBodyText;

    private CoopType coopType;

    private void OnDisable()
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.Player);
    }

    public void Init(CoopType coopType)
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);

        this.coopType = coopType;

        // ���� ���� �ʱ�ȭ
        ExitGames.Client.Photon.Hashtable resetProps = new ExitGames.Client.Photon.Hashtable
         {
             { "coopChoice", null } // null�� �����ϸ� ��ǻ� �ʱ�ȭó�� ����
         };

        PhotonNetwork.LocalPlayer.SetCustomProperties(resetProps);

        StartCoroutine(StartAnimation());

        coopConfirmButton.onClick.AddListener(() => OnChoiceMade(true));
        betrayConfirmButton.onClick.AddListener(() => OnChoiceMade(false));

        coopButton.onClick.AddListener(() =>
        {
            if (!coopInfoPanel.gameObject.activeSelf)
                coopInfoPanel.gameObject.SetActive(true);
        });

        betrayButton.onClick.AddListener(() =>
        {
            if (!betrayInfoPanel.gameObject.activeSelf)
                betrayInfoPanel.gameObject.SetActive(true);
        });

        resetInfoPanelButton.onClick.AddListener(() =>
        {
            coopInfoPanel.gameObject.SetActive(false);
            betrayInfoPanel.gameObject.SetActive(false);
        });

        switch (coopType)
        {
            case CoopType.defaultType:
                coopHeaderText.text = "협력 시 얻는 효과";
                coopBodyText.text = "협력 성공\r\n: 공격력 1.5배 증가, 체력 20회복\r\n\r\n협력 실패\r\n: 보상 없음";
                betaryHeaderText.text = "배신 시 얻는 효과";
                betrayBodyText.text = "배신 성공\r\n: 무작위 가호를 획득 \r\n\r\n배신 실패\r\n: 몬스터의 공격력 1.5배 증가";
                break;
            case CoopType.PVPType:
                coopHeaderText.text = "협력 시 얻는 보상";
                coopBodyText.text = "협력하여 보상을 나누겠습니까?";
                betaryHeaderText.text = "배신 시 얻는 보상";
                betrayBodyText.text = "배신하여 보상을 독식하겠습니까?";
                break;
        }
    }

    private IEnumerator StartAnimation()
    {
        for (int i = 0; i < labelList.Count; i++)
        {
            labelList[i].DOAnchorPos(labelPosList[i], 1f).SetEase(Ease.InOutQuad);
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < labelList.Count; i++)
        {
            labelList[i].GetComponent<LabelController>().StartScroll();
        }

        cross2.DOAnchorPos(cross2TargetPos, 0.2f).SetEase(Ease.InQuad);

        yield return new WaitForSeconds(0.2f);

        cross1.DOAnchorPos(cross1TargetPos, 0.2f).SetEase(Ease.InQuad);

        yield return new WaitForSeconds(0.2f);

        //cross1.localScale = Vector3.one * 3f;
        //cross2.localScale = Vector3.one * 3f;

        //yield return new WaitForSeconds(0.1f);

        cross1.gameObject.SetActive(false);
        cross2.gameObject.SetActive(false);
        crossImage.gameObject.SetActive(true);

        //crossImage.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        //yield return new WaitForSeconds(0.3f);

        coopButton.interactable = false;
        betrayButton.interactable = false;

        coopButton.transform.DOScaleX(1f, 0.1f).SetEase(Ease.Linear);
        betrayButton.transform.DOScaleX(1f, 0.1f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(0.1f);

        RectTransform coopRect = coopButton.GetComponent<RectTransform>();
        RectTransform betrayRect = betrayButton.GetComponent<RectTransform>();

        Vector2 coopOriginPos = coopRect.anchoredPosition;
        Vector2 betrayOriginPos = betrayRect.anchoredPosition;

        // coopButton ���� �̵� �� ���ƿ���
        Sequence coopSeq = DOTween.Sequence();
        coopSeq.Append(coopRect.DOAnchorPos(coopOriginPos + Vector2.left * 200f, 0.1f).SetEase(Ease.OutQuad))
               .AppendInterval(0.2f)
               .Append(coopRect.DOAnchorPos(coopOriginPos, 0.2f).SetEase(Ease.OutBounce));

        // betrayButton ������ �̵� �� ���ƿ���
        Sequence betraySeq = DOTween.Sequence();
        betraySeq.Append(betrayRect.DOAnchorPos(betrayOriginPos + Vector2.right * 200f, 0.1f).SetEase(Ease.OutQuad))
                 .AppendInterval(0.2f)
                 .Append(betrayRect.DOAnchorPos(betrayOriginPos, 0.2f).SetEase(Ease.OutBounce));

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(GlitchEffect(coopButton.GetComponent<Image>(), coopButtonSprites));
        StartCoroutine(GlitchEffect(betrayButton.GetComponent<Image>(), betrayButtonSprites));
        StartCoroutine(GlitchEffect(crossImage, crossSprites));

        yield return new WaitForSeconds(0.5f);

        coopButton.interactable = true;
        betrayButton.interactable = true;
    }


    private IEnumerator GlitchEffect(Image target, Sprite[] sprites)
    {
        // �۸�ġ ȿ�� ���� �ð���ŭ �ݺ�
        float elapsedTime = 0f;

        int i = 0;

        while (elapsedTime < 0.5f)
        {
            target.sprite = sprites[i % sprites.Length];
            target.SetNativeSize();

            yield return new WaitForSeconds(0.1f); // 0.1�� �������� ����

            elapsedTime += 0.1f;
            i++;
        }

        // �۸�ġ ȿ�� ��, ��������Ʈ ����
        // ���Ͻô� ��������Ʈ�� ���ܳ��ų� ���������� ������ ��������Ʈ�� ��� �����Ϸ��� �߰��� �� �ֽ��ϴ�.
        // ���� ���, ������ ��������Ʈ�� ������ �� �ֽ��ϴ�.
        target.sprite = sprites[sprites.Length - 1];
        target.SetNativeSize();
    }

    private void OnChoiceMade(bool choice)
    {
        Debug.Log("��ư ����: " + (choice ? "����" : "���"));

        // CustomProperties�� ���� ����
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "coopChoice", choice }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        coopInfoPanel.gameObject.SetActive(false);
        betrayInfoPanel.gameObject.SetActive(false);

        if (choice)
        {
            betrayButton.transform.DOScaleX(0f, 0.1f).SetEase(Ease.Linear).OnComplete(EndSelect);
        }
        else
        {
            coopButton.transform.DOScaleX(0f, 0.1f).SetEase(Ease.Linear).OnComplete(EndSelect);
        }

        // ��ư ��Ȱ��ȭ
        coopButton.interactable = false;
        betrayButton.interactable = false;
        coopConfirmButton.interactable = false;
        betrayConfirmButton.interactable = false;
        resetInfoPanelButton.interactable = false;

    }

    private void EndSelect()
    {
        for (int i = 0; i < labelList.Count; i++)
        {
            labelList[i].GetComponent<LabelController>().StopScroll();
        }

        for (int i = 0; i < labelList.Count; i++)
        {
            labelList[i].DOAnchorPos(labelStartPosList[i], 1f).SetEase(Ease.InOutQuad);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!changedProps.ContainsKey("coopChoice")) return;

        // ��� ���� �÷��̾ ���� Ȯ��
        var currentPlayers = PhotonNetwork.CurrentRoom.Players.Values;
        var choices = new Dictionary<Player, bool>();

        foreach (var player in currentPlayers)
        {
            if (player.CustomProperties.TryGetValue("coopChoice", out object value))
            {
                choices[player] = (bool)value;
            }
        }

        // ���� ���� ���� ��� ������ ���
        if (choices.Count < currentPlayers.Count) return;

        // ��� ���
        int coopCount = 0;
        int betrayCount = 0;

        foreach (var choice in choices.Values)
        {
            if (choice) coopCount++;
            else betrayCount++;
        }

        if (coopCount == choices.Count)
        {
            Debug.Log("���� ����!");
            // �������� ���� �ο�
            switch (coopType)
            {
                case CoopType.defaultType:
                    PhotonNetworkManager.Instance.photonView.RPC("RPC_ApplyPlayerBuff", RpcTarget.All, 1.5f);
                    break;
                case CoopType.PVPType:
                    PhotonNetworkManager.Instance.photonView.RPC("RPC_ResetGame", RpcTarget.All, "모든 플레이어가 협력해 유물을 성공적으로 전달했습니다.\n 잠시 뒤 마을로 복귀합니다......");
                    break;
            }
        }
        else if (betrayCount == choices.Count)
        {
            Debug.Log("���� ���!");
            // �������� ����� �ο�
            switch (coopType)
            {
                case CoopType.defaultType:
                    PhotonNetworkManager.Instance.photonView.RPC("RPC_ApplyMonsterBuff", RpcTarget.All, 1.5f);
                    break;
                case CoopType.PVPType:
                    foreach (var pair in choices)
                    {
                        int teamId;
                        if (!pair.Value)
                        {
                            teamId = pair.Key.ActorNumber;
                        }
                        else
                        {
                            teamId = -1;
                        }
                        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
                        {
                            { "TeamId", teamId }
                        };

                        pair.Key.SetCustomProperties(props);
                    }
                    PhotonNetworkManager.Instance.GotoPVPArea();
                    break;
            }
        }
        else
        {
            Debug.Log("�Ϻ� ���");
            // ����� ����� ����
            switch (coopType)
            {
                case CoopType.defaultType:
                    foreach (var pair in choices)
                    {
                        if (!pair.Value)
                        {
                            PhotonNetworkManager.Instance.photonView.RPC("PopupBlessingPanel", pair.Key);
                        }
                        else
                        {
                            PhotonNetworkManager.Instance.photonView.RPC("PopupDialogPanel", pair.Key, "누군가 배신했습니다.");
                        }
                    }
                    break;
                case CoopType.PVPType:
                    foreach (var pair in choices)
                    {
                        int teamId;
                        if (!pair.Value)
                        {
                            teamId = pair.Key.ActorNumber;
                        }
                        else
                        {
                            teamId = -1;
                        }
                        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
                        {
                            { "TeamId", teamId }
                        };
                        pair.Key.SetCustomProperties(props);
                    }
                    PhotonNetworkManager.Instance.GotoPVPArea();
                    break;
            }
        }
    }
}
