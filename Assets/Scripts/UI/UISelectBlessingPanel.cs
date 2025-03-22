using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectBlessingPanel : UIBase
{
    [SerializeField]
    private List<Button> buttons = new List<Button>();
    [SerializeField]
    private Button selectButton;

    private bool[] isFlipped;


    private Dictionary<Skills, BlessingInfo> newBlessings = new Dictionary<Skills, BlessingInfo>();

    private KeyValuePair<Skills, BlessingInfo> selectedBlessing;

    void Start()
    {
        Init();
    }

    private void PickUpBlessing()
    {
        var dic = RoomManager.Instance.ReturnLocalPlayer().GetComponent<PlayerBlessing>().ReturnBlessingDic();

        while (newBlessings.Count < 3)
        {
            int randomKey = Random.Range(0, (int)Skills.Max);
            Skills skill = (Skills)randomKey;

            if (newBlessings.ContainsKey(skill))
            {
                continue;
            }

            if (dic.ContainsKey(skill))
            {
                BlessingInfo newBlessing = dic[skill];
                newBlessing.level++;
                if (!newBlessings.ContainsValue(newBlessing))
                {
                    newBlessings[skill] = newBlessing;
                }
            }
            else
            {
                int randomBlessing = Random.Range(1, (int)Blessings.Max);
                BlessingInfo newBlessing = new((Blessings)randomBlessing, 1);
                if (!newBlessings.ContainsValue(newBlessing))
                {
                    newBlessings[skill] = newBlessing;
                }
            }
        }
        Debug.Log("newBlessings count: " + newBlessings.Count);
    }

    public override void Init()
    {
        isFlipped = new bool[buttons.Count];
        PickUpBlessing();
        int index = 0;
        foreach (var pair in newBlessings)
        {
            buttons[index].GetComponent<UISelectBlessingButton>().Init(pair);
            index++;
        }
        for (int i = 0; i < buttons.Count; i++)
        {
            int index2 = i;
            buttons[index2].onClick.AddListener(() =>
            {
                if (isFlipped[index2])
                {
                    for (int j = 0; j < buttons.Count; j++)
                    {
                        if (index2 == j)
                        {
                            UISelectBlessingButton button = buttons[j].GetComponent<UISelectBlessingButton>();
                            button.OutlineEnabled(true);
                            selectedBlessing = button.ReturnBlessing();
                        }
                        else
                        {
                            buttons[j].GetComponent<UISelectBlessingButton>().OutlineEnabled(false);
                        }
                    }
                }
                else
                {
                    buttons[index2].interactable = false;
                    buttons[index2].transform.DOLocalRotate(new Vector3(0, 90, 0), 0.3f).OnComplete(() =>
                    {
                        buttons[index2].GetComponent<UISelectBlessingButton>().SetEnabled();
                        buttons[index2].transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f).OnComplete(() =>
                        {
                            isFlipped[index2] = true;
                            buttons[index2].image.sprite = null;
                            buttons[index2].interactable = true;
                        });
                    });
                }
            });
        }

        selectButton.onClick.AddListener(SelectBleesing);
    }

    private void SelectBleesing()
    {
        if (selectedBlessing.Value.level != 0)
        {
            RoomManager.Instance.ReturnLocalPlayer().GetComponent<PlayerBlessing>().UpdateBlessing(selectedBlessing);
            UIManager.Instance.ClosePeekUI();
        }
    }
}
