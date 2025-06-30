using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class SwordBladeVisualizer : MonoBehaviour
{
	private const float Half = 0.5f;

	[SerializeField, Required] private float _pulseDuration = 0.5f;
	[SerializeField, Required] private float _minAlpha = 0.3f;
	[SerializeField, Required] private float _maxAlpha = 1f;
	[SerializeField, Required] private float _fadeOutDuration = 1f;
	[SerializeField, Required] private Ease _pulseEase = Ease.InOutSine;
	[SerializeField, Required] private Ease _fadeEase = Ease.OutQuad;

	private SpriteRenderer _spriteRenderer;
	private Tween _currentTween;
	private bool _isMoving;

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Start()
	{
		_spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 0f);
	}

	private void OnDestroy()
	{
		_currentTween?.Kill();
	}

	public void StartMovingVisualization()
	{
		if (_isMoving)
			return;

		_isMoving = true;
		_currentTween?.Kill();

		StartPulse();
	}

	public void StopMovingVisualization()
	{
		if (_isMoving == false)
			return;

		_isMoving = false;
		_currentTween?.Kill();

		StartFadeOut();
	}

	private void StartPulse()
	{
		if (_isMoving == false)
			return;

		_currentTween = _spriteRenderer.DOFade(_maxAlpha, _pulseDuration * Half)
			.SetEase(_pulseEase)
			.OnComplete(() =>
			{
				if (_isMoving == false)
					return;

				_currentTween = _spriteRenderer.DOFade(_minAlpha, _pulseDuration * Half)
					.SetEase(_pulseEase)
					.OnComplete(StartPulse);
			});
	}

	private void StartFadeOut()
	{
		_currentTween = _spriteRenderer.DOFade(0f, _fadeOutDuration)
			.SetEase(_fadeEase);
	}
}