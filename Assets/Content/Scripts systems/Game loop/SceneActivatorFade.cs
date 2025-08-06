using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SceneActivatorFade : MonoBehaviour
{
	private const float EndBlackValue = 1f;

	[SerializeField] private float _fadeDuration = 1f;
	[SerializeField] private float _fadeInDelay = 0.5f;

	private CanvasGroup _blackScreen;

	public event Action FadeInCompleted;
	public event Action FadeOutCompleted;

	private void Awake()
	{
		_blackScreen = GetComponent<CanvasGroup>();
		_blackScreen.gameObject.SetActive(true);
	}

	public void StartFadeIn()
	{
		_blackScreen.alpha = EndBlackValue;
		_blackScreen.DOFade(0f, _fadeDuration).SetDelay(_fadeInDelay).OnComplete(() =>
		{
			_blackScreen.gameObject.SetActive(false);
			FadeInCompleted?.Invoke();
		}).SetUpdate(true);
	}

	public void StartFadeOut(Action completed = null)
	{
		_blackScreen.gameObject.SetActive(true);
		_blackScreen.alpha = 0f;

		_blackScreen.DOFade(EndBlackValue, _fadeDuration).OnComplete(() =>
		{
			FadeOutCompleted?.Invoke();
			completed?.Invoke();
		}).SetUpdate(true);
	}
}