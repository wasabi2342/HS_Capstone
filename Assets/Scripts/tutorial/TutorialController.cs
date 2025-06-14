using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
	[SerializeField]
	private	List<TutorialBase>	tutorials;
	[SerializeField]
	private	string				nextSceneName = "";

	private TutorialBase		currentTutorial = null;
	private	int					currentIndex = -1;

	[SerializeField]
	private Transform blessingPos;
	[SerializeField]
	private Transform coopOrBetrayPos;
	[SerializeField]
	private GameObject selectBlessing;
	[SerializeField]
	private GameObject selectCoop;

	private void Start()
	{
		SetNextTutorial();

		GameObject obj = PhotonNetwork.Instantiate(selectCoop.name, coopOrBetrayPos.position, Quaternion.identity);
		obj.GetComponent<CoopOrBetray>().isInTutorial = true;

        PhotonNetwork.Instantiate(selectBlessing.name, blessingPos.position, Quaternion.identity);
    }

	private void Update()
	{
		if ( currentTutorial != null )
		{
			currentTutorial.Execute(this);
		}
	}

	public void SetNextTutorial()
	{
		// ??¬ ?? λ¦¬μΌ? Exit() λ©μ? ?ΈμΆ?
		if ( currentTutorial != null )
		{
			currentTutorial.Exit();
		}

		// λ§μ??λ§? ?? λ¦¬μΌ? μ§ν??€λ©? CompletedAllTutorials() λ©μ? ?ΈμΆ?
		if ( currentIndex >= tutorials.Count-1 )
		{
			CompletedAllTutorials();
			return;
		}

		// ?€? ?? λ¦¬μΌ κ³Όμ ? currentTutorialλ‘? ?±λ‘?
		currentIndex ++;
		currentTutorial = tutorials[currentIndex];

		// ?λ‘? λ°λ?? ?? λ¦¬μΌ? Enter() λ©μ? ?ΈμΆ?
		currentTutorial.Enter();
	}

	public void CompletedAllTutorials()
	{
		currentTutorial = null;

		// ?? ???΄ ?¬?¬ μ’λ₯κ°? ??? ? μ½λ μΆκ?? ??±
		// ??¬? ?¬ ? ?

		Debug.Log("Complete All");

		if ( !nextSceneName.Equals("") )
		{
            DataManager.Instance.settingData.tutorialCompleted = true;
            DataManager.Instance.SaveSettingData();
            PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene(nextSceneName);
		}
	}
}

