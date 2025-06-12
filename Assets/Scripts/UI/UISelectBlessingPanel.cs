using DG.Tweening;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISelectBlessingPanel : UIBase
{
    [SerializeField]
    private List<Button> buttons = new List<Button>();
    [SerializeField]
    private Button selectButton;

    private bool[] isFlipped;

    public bool isSelected = false;

    private List<SkillWithLevel> newBlessings = new List<SkillWithLevel>();

    private SkillWithLevel selectedBlessing;

    void Start()
    {
        Init();
    }

    private void PickUpBlessing()
    {
        var arr = RoomManager.Instance.ReturnLocalPlayer().GetComponent<PlayerBlessing>().ReturnSkillWithLevel();
        if (arr == null)
            Debug.Log("arr ��");
        for (int i = 0; i < arr.Length; i++)
        {
            Debug.Log(arr[i]);
        }
        while (newBlessings.Count < 3)
        {
            int randomKey = Random.Range(0, (int)Skills.Max);

            bool isDuplicate = false;

            for (int i = 0; i < newBlessings.Count; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (newBlessings[j].skillData.Bind_Key == randomKey)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if (isDuplicate)
                {
                    break;
                }
            }

            if (isDuplicate)
            {
                continue;
            }

            if (arr[randomKey].skillData.Devil != 0)
            {
                SkillWithLevel newBlessing = new SkillWithLevel(arr[randomKey].skillData, arr[randomKey].level);
                newBlessing.level++;
                if (!newBlessings.Contains(newBlessing))
                {
                    newBlessings.Add(newBlessing);
                }
            }
            else
            {
                int randomBlessing = Random.Range(1, (int)Blessings.Max);
                SkillWithLevel newBlessing = new SkillWithLevel(DataManager.Instance.FindSkillByBlessingKeyAndCharacter(randomKey, randomBlessing, arr[0].skillData.Character), 1);
                if (!newBlessings.Contains(newBlessing))
                {
                    newBlessings.Add(newBlessing);
                }
            }
        }
        Debug.Log("newBlessings count: " + newBlessings.Count);
    }

    public override void Init()
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);

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
                        isFlipped[index2] = true;

                        buttons[index2].transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f).OnComplete(() =>
                        {

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
        if (selectedBlessing == null)
        {
            return;
        }

        if (selectedBlessing.level != 0)
        {
            RoomManager.Instance.ReturnLocalPlayer().GetComponent<PlayerBlessing>().UpdateBlessing(selectedBlessing);
            InputManager.Instance.ChangeDefaultMap(InputDefaultMap.Player);
            UIManager.Instance.ClosePeekUI();
            isSelected = true;
        }
        
    }
}
