using UnityEngine;
using DG.Tweening;

public class AngularShake : MonoBehaviour
{
	[SerializeField] private float _strength = 15f; // Максимальный угол тряски (градусы)
	[SerializeField] private float _duration = 0.5f; // Длительность одной тряски
	[SerializeField] private int _vibrato = 10;     // Кол-во "вибраций" за _duration
	[SerializeField] private float _randomness = 90f; // Расброс направления в градусах
	[SerializeField] private bool _fadeOut = true;

	private Tween _shakeTween;
	private Quaternion _initialRotate;

	private void Start()
	{
		Play();
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
}
