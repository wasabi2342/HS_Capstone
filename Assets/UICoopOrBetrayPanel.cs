using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICoopOrBetrayPanel : UIBase
{
    [SerializeField]
    private Button coopButton;
    [SerializeField]
    private Button betrayButton;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        // ���� ���� �ʱ�ȭ
        Hashtable resetProps = new Hashtable
        {
            { "coopChoice", null } // null�� �����ϸ� ��ǻ� �ʱ�ȭó�� ����
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(resetProps);

        coopButton.onClick.AddListener(() => OnChoiceMade(true));
        betrayButton.onClick.AddListener(() => OnChoiceMade(false));
    }

    private void OnChoiceMade(bool choice)
    {
        Debug.Log("��ư ����: " + (choice ? "����" : "���"));

        // CustomProperties�� ���� ����
        Hashtable props = new Hashtable
        {
            { "coopChoice", choice }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // ��ư ��Ȱ��ȭ
        coopButton.interactable = false;
        betrayButton.interactable = false;

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
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
            PhotonNetworkManager.Instance.photonView.RPC("RPC_ApplyPlayerBuff", RpcTarget.All, 1.5f);
        }
        else if (betrayCount == choices.Count)
        {
            Debug.Log("���� ���!");
            // �������� ����� �ο�
            PhotonNetworkManager.Instance.photonView.RPC("RPC_ApplyMonsterBuff", RpcTarget.All, 1.5f);
        }
        else
        {
            Debug.Log("�Ϻ� ���");
            // ����� ����� ����
            foreach (var pair in choices)
            {
                if (!pair.Value)
                {
                    PhotonNetworkManager.Instance.photonView.RPC("PopupBlessingPanel", pair.Key);
                }
                else
                {
                    PhotonNetworkManager.Instance.photonView.RPC("PopupDialogPanel", pair.Key, "�������� ����߽��ϴ�.");
                }
            }
        }
    }
}
