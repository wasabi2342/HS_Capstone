using System.Collections.Generic;
using UnityEngine;

public class UISelectBlessingPanel : UIBase
{

    Dictionary<Skills, (Blessings, int)> newBlessings = new Dictionary<Skills, (Blessings, int)>();

    void Start()
    {
        var dic = RoomManager.Instance.ReturnLocalPlayer().GetComponent<PlayerBlessing>().playerBlessingDic;

        for (int i = 0; i < 3; i++)
        {
            while (true)
            {
                int randomKey = Random.Range(0, (int)Skills.Max);
                if (dic.ContainsKey((Skills)randomKey))
                {
                    var newBlessingPair = (dic[(Skills)randomKey].Item1, dic[(Skills)randomKey].Item2 + 1);
                    if (newBlessings.ContainsKey((Skills)randomKey) && newBlessings.ContainsValue(newBlessingPair))
                    {
                        continue;
                    }
                    else
                    {
                        newBlessings[(Skills)randomKey] = newBlessingPair;
                        break;
                    }
                }
                else
                {
                    int randomBlessing = Random.Range(0, (int)Blessings.Max);
                    var newBlessingPair = ((Blessings)randomBlessing, 1);
                    if (newBlessings.ContainsKey((Skills)randomKey) && newBlessings.ContainsValue(newBlessingPair))
                    {
                        continue;
                    }
                    else
                    {
                        newBlessings[(Skills)randomKey] = newBlessingPair;
                        break;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
