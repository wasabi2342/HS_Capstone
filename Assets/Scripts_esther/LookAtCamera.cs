using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }
}
