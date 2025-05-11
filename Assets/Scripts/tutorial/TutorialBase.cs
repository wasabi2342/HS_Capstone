using UnityEngine;

public abstract class TutorialBase : MonoBehaviour
{
    public abstract void Enter(); // Abstract method to start the tutorial
    public abstract void Execute(TutorialController controller);   // Abstract method to end the tutorial
    public abstract void Exit(); // Abstract method to check if the tutorial is active

}
