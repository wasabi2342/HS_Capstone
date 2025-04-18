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
        // 이전 선택 초기화
        Hashtable resetProps = new Hashtable
        {
            { "coopChoice", null } // null로 설정하면 사실상 초기화처럼 동작
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(resetProps);

        coopButton.onClick.AddListener(() => OnChoiceMade(true));
        betrayButton.onClick.AddListener(() => OnChoiceMade(false));
    }

    private void OnChoiceMade(bool choice)
    {
        Debug.Log("버튼 눌림: " + (choice ? "협력" : "배신"));

        // CustomProperties에 선택 저장
        Hashtable props = new Hashtable
        {
            { "coopChoice", choice }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 버튼 비활성화
        coopButton.interactable = false;
        betrayButton.interactable = false;

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!changedProps.ContainsKey("coopChoice")) return;

        // 모든 남은 플레이어에 대해 확인
        var currentPlayers = PhotonNetwork.CurrentRoom.Players.Values;
        var choices = new Dictionary<Player, bool>();

        foreach (var player in currentPlayers)
        {
            if (player.CustomProperties.TryGetValue("coopChoice", out object value))
            {
                choices[player] = (bool)value;
            }
        }

        // 아직 선택 안한 사람 있으면 대기
        if (choices.Count < currentPlayers.Count) return;

        // 결과 계산
        int coopCount = 0;
        int betrayCount = 0;

        foreach (var choice in choices.Values)
        {
            if (choice) coopCount++;
            else betrayCount++;
        }

        if (coopCount == choices.Count)
        {
            Debug.Log("전원 협력!");
            // 전원에게 버프 부여
            PhotonNetworkManager.Instance.photonView.RPC("RPC_ApplyPlayerBuff", RpcTarget.All, 1.5f);
        }
        else if (betrayCount == choices.Count)
        {
            Debug.Log("전원 배신!");
            // 전원에게 디버프 부여
            PhotonNetworkManager.Instance.photonView.RPC("RPC_ApplyMonsterBuff", RpcTarget.All, 1.5f);
        }
        else
        {
            Debug.Log("일부 배신");
            // 배신한 사람만 버프
            foreach (var pair in choices)
            {
                if (!pair.Value)
                {
                    PhotonNetworkManager.Instance.photonView.RPC("PopupBlessingPanel", pair.Key);
                }
                else
                {
                    PhotonNetworkManager.Instance.photonView.RPC("PopupDialogPanel", pair.Key, "누군가가 배신했습니다.");
                }
            }
        }
    }
}
