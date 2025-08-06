using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneActivator : MonoBehaviour
{
	[SerializeField] private SceneActivatorFade _fade;

	public event Action FadeCompleted;

	private void Start()
	{
		_fade.FadeInCompleted += OnFadeInCompleted;
		_fade.StartFadeIn();
	}

	private void OnDestroy()
	{
		if (_fade != null)
			_fade.FadeInCompleted -= OnFadeInCompleted;
	}

	[ContextMenu(nameof(RestartScene))]
	public void RestartScene()
	{
		_fade.StartFadeOut(() =>
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		});
	}

	private void OnFadeInCompleted()
	{
		FadeCompleted?.Invoke();
	}
}