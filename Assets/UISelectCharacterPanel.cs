using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class UISelectCharacterPanel : UIBase
{
    [SerializeField]
    private List<CharacterStats> characterDatas = new List<CharacterStats>();
    [SerializeField]
    private Button characterButton;
    [SerializeField]
    private RectTransform content;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        for (int i = 0; i < characterDatas.Count; i++)
        {
            int index = i;
            Button button = Instantiate(characterButton);
            button.transform.SetParent(content.transform, false);
            button.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprite/" + characterDatas[index].name);
            button.onClick.AddListener(() => UIManager.Instance.OpenPopupPanel<UICharacterInfoPanel>().Init(characterDatas[index]));
        }
    }

}
