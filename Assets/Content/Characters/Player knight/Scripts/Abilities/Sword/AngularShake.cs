using UnityEngine;
using DG.Tweening;

public class AngularShake : MonoBehaviour
{
	[SerializeField] private float _strength = 15f;
	[SerializeField] private float _duration = 0.5f;
	[SerializeField] private int _vibrato = 10;
	[SerializeField] private float _randomness = 90f;
	[SerializeField] private bool _fadeOut = true;
	[Space]
	[SerializeField] private bool _playOnStart = false;

	private Tween _shakeTween;
	private Quaternion _initialRotate;

	private void Start()
	{
		if (_playOnStart)
		{
			Play();
		}
	}

	private void OnDestroy()
	{
		Stop();
	}

	[ContextMenu(nameof(Play))]
	public void Play()
	{
		_initialRotate = transform.localRotation;

		_shakeTween?.Kill();
		_shakeTween = transform.DOShakeRotation(_duration, new Vector3(0, 0, _strength), _vibrato, _randomness, _fadeOut)
			.SetEase(Ease.Linear)
			.SetLoops(-1);
	}

	[ContextMenu(nameof(Stop))]
	public void Stop()
	{
		_shakeTween?.Kill();
		transform.localRotation = _initialRotate;
	}

	public void SetUnscaledTime()
	{
		_shakeTween?.SetUpdate(true);
	}

	public void SetNormalTime()
	{
		_shakeTween?.SetUpdate(false);
	}
}
