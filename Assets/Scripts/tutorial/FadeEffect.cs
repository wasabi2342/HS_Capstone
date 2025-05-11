using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class FadeEffect : MonoBehaviour
{
	// 페이드 효과가 끝났을 때 호출하고 싶은 메소드를 등록, 호출하는 이벤트 클래스
	[System.Serializable]
	private	class FadeEvent : UnityEvent { }
	private	FadeEvent onFadeEvent = new FadeEvent();

	[SerializeField]
	[Range(0.01f, 10f)]
	private	float			fadeTime;		// 페이드 되는 시간
	[SerializeField]
	private	AnimationCurve	fadeCurve;		// 페이드 효과가 적용되는 알파 값을 곡선의 값으로 설정
	private	Image			fadeImage;		// 페이드 효과에 사용되는 검은 바탕 이미지

	private void Awake()
	{
		fadeImage = GetComponent<Image>();
	}

	public void FadeIn(UnityAction action)
	{
		StartCoroutine(Fade(action, 1, 0));
	}

	public void FadeOut(UnityAction action)
	{
		StartCoroutine(Fade(action, 0, 1));
	}

	private IEnumerator Fade(UnityAction action, float start, float end)
	{
		// action 메소드를 이벤트에 등록
		onFadeEvent.AddListener(action);
		
		float current = 0.0f;
		float percent = 0.0f;

		while ( percent < 1 )
		{
			current += Time.deltaTime;
			percent = current / fadeTime;

			Color color		= fadeImage.color;
			color.a			= Mathf.Lerp(start, end, fadeCurve.Evaluate(percent));
			fadeImage.color	= color;

			yield return null;
		}

		// action 메소드를 실행
		onFadeEvent.Invoke();

		// action 메소드를 이벤트에서 제거
		onFadeEvent.RemoveListener(action);
	}
}

