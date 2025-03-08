using System.Collections.Generic;
using UnityEngine;

public enum Skills { Mouse_L, Mouse_R, Space, Shift_L, R }
public enum Blessings { Crocell, Gremory, Paymon }
public class PlayerBlessing : MonoBehaviour
{
    public Dictionary<Skills, (Blessings, int)> playerBlessingDic = new Dictionary<Skills, (Blessings, int)>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
