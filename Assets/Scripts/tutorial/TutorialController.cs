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
		// ?˜„?¬ ?Šœ?† ë¦¬ì–¼?˜ Exit() ë©”ì†Œ?“œ ?˜¸ì¶?
		if ( currentTutorial != null )
		{
			currentTutorial.Exit();
		}

		// ë§ˆì??ë§? ?Šœ?† ë¦¬ì–¼?„ ì§„í–‰?–ˆ?‹¤ë©? CompletedAllTutorials() ë©”ì†Œ?“œ ?˜¸ì¶?
		if ( currentIndex >= tutorials.Count-1 )
		{
			CompletedAllTutorials();
			return;
		}

		// ?‹¤?Œ ?Šœ?† ë¦¬ì–¼ ê³¼ì •?„ currentTutorialë¡? ?“±ë¡?
		currentIndex ++;
		currentTutorial = tutorials[currentIndex];

		// ?ƒˆë¡? ë°”ë?? ?Šœ?† ë¦¬ì–¼?˜ Enter() ë©”ì†Œ?“œ ?˜¸ì¶?
		currentTutorial.Enter();
	}

	public void CompletedAllTutorials()
	{
		currentTutorial = null;

		// ?–‰?™ ?–‘?‹?´ ?—¬?Ÿ¬ ì¢…ë¥˜ê°? ?˜?—ˆ?„ ?•Œ ì½”ë“œ ì¶”ê?? ?‘?„±
		// ?˜„?¬?Š” ?”¬ ? „?™˜

		Debug.Log("Complete All");

		if ( !nextSceneName.Equals("") )
		{
            PlayerPrefs.SetInt("Tutorial", 1);
            PlayerPrefs.Save();
            PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene(nextSceneName);
		}
	}
}

