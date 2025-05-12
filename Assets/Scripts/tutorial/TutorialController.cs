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
		// ?��?�� ?��?��리얼?�� Exit() 메소?�� ?���?
		if ( currentTutorial != null )
		{
			currentTutorial.Exit();
		}

		// 마�??�? ?��?��리얼?�� 진행?��?���? CompletedAllTutorials() 메소?�� ?���?
		if ( currentIndex >= tutorials.Count-1 )
		{
			CompletedAllTutorials();
			return;
		}

		// ?��?�� ?��?��리얼 과정?�� currentTutorial�? ?���?
		currentIndex ++;
		currentTutorial = tutorials[currentIndex];

		// ?���? 바�?? ?��?��리얼?�� Enter() 메소?�� ?���?
		currentTutorial.Enter();
	}

	public void CompletedAllTutorials()
	{
		currentTutorial = null;

		// ?��?�� ?��?��?�� ?��?�� 종류�? ?��?��?�� ?�� 코드 추�?? ?��?��
		// ?��?��?�� ?�� ?��?��

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

