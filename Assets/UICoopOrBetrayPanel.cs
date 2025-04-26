using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private void Start()
    {
        Init();
    }

    private void OnDisable()
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.Player);
    }

    public override void Init()
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);
        
        // 이전 선택 초기화
        ExitGames.Client.Photon.Hashtable resetProps = new ExitGames.Client.Photon.Hashtable
        {
            { "coopChoice", null } // null로 설정하면 사실상 초기화처럼 동작
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

        coopButton.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.Linear);
        betrayButton.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(0.1f);

        RectTransform coopRect = coopButton.GetComponent<RectTransform>();
        RectTransform betrayRect = betrayButton.GetComponent<RectTransform>();

        Vector2 coopOriginPos = coopRect.anchoredPosition;
        Vector2 betrayOriginPos = betrayRect.anchoredPosition;

        // coopButton 왼쪽 이동 후 돌아오기
        Sequence coopSeq = DOTween.Sequence();
        coopSeq.Append(coopRect.DOAnchorPos(coopOriginPos + Vector2.left * 200f, 0.1f).SetEase(Ease.OutQuad))
               .AppendInterval(0.2f)
               .Append(coopRect.DOAnchorPos(coopOriginPos, 0.2f).SetEase(Ease.InBack));

        // betrayButton 오른쪽 이동 후 돌아오기
        Sequence betraySeq = DOTween.Sequence();
        betraySeq.Append(betrayRect.DOAnchorPos(betrayOriginPos + Vector2.right * 200f, 0.1f).SetEase(Ease.OutQuad))
                 .AppendInterval(0.2f)
                 .Append(betrayRect.DOAnchorPos(betrayOriginPos, 0.2f).SetEase(Ease.InBack));

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
        // 글리치 효과 지속 시간만큼 반복
        float elapsedTime = 0f;

        int i = 0;

        while (elapsedTime < 0.5f)
        {
            target.sprite = sprites[i % sprites.Length];
            target.SetNativeSize();

            yield return new WaitForSeconds(0.1f); // 0.1초 간격으로 변경

            elapsedTime += 0.1f;
            i++;
        }

        // 글리치 효과 후, 스프라이트 유지
        // 원하시는 스프라이트를 남겨놓거나 마지막으로 보여준 스프라이트를 계속 유지하려면 추가할 수 있습니다.
        // 예를 들어, 마지막 스프라이트로 유지할 수 있습니다.
        target.sprite = sprites[sprites.Length - 1];
        target.SetNativeSize();
    }

    private void OnChoiceMade(bool choice)
    {
        Debug.Log("버튼 눌림: " + (choice ? "협력" : "배신"));

        // CustomProperties에 선택 저장
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "coopChoice", choice }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 버튼 비활성화
        coopButton.interactable = false;
        betrayButton.interactable = false;
        coopConfirmButton.interactable = false;
        betrayConfirmButton.interactable = false;
        resetInfoPanelButton.interactable = false;

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
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
