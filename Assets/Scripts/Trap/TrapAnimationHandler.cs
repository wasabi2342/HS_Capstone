using UnityEngine;

public class TrapAnimationHandler : MonoBehaviour
{
    public GameObject trapToDestroy; 

    public void DeleteSelf()
    {
        if (trapToDestroy != null)
        {
            Destroy(trapToDestroy);
        }
    }
}
