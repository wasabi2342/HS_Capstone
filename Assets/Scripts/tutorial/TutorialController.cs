using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TutorialController : MonoBehaviour
{
    [SerializeField]
     private List<TutorialBase> tutorias;

    [SerializeField]
    private string nextSceneName = "";

    private TutorialBase currentTutorial;
    private int currentTutorialIndex = 0;
    private void Start()
    {
        SetNextTutorial();
    }

    public void SetNextTutorial()
    {
        if (currentTutorial != null)
        {
            currentTutorial.Exit();
        }

        if (currentTutorialIndex >= tutorias.Count)
        {
            CompletedAllTutorials();
            return;
        }
        // 다음 튜토리얼 
        currentTutorialIndex++;
        currentTutorial = tutorias[currentTutorialIndex];
        
        currentTutorial.Enter();
    }
        public void CompletedAllTutorials()
    {
        currentTutorial = null;
        // 모든 튜토리얼을 완료한 후 다음 씬으로 이동

        Debug.Log("complete All");

        if( !nextSceneName.Equals(""))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("No next scene name provided.");
        }
    }
}
