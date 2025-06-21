using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;


public class DOTweenShakeEffect : MonoBehaviour
{
	[Title("Shake Settings")]
	[SerializeField] private bool _playOnStart = false;
	[SerializeField] private bool _playOnEnable = false;

	[Title("Shake Type")]
	[SerializeField] private ShakeType _shakeType = ShakeType.Position;

	[Title("Shake Parameters")]
	[SerializeField, Min(0f)] private float _duration = 1f;
	[SerializeField] private Vector3 _strength = Vector3.one;
	[SerializeField, Range(1, 50)] private int _vibrato = 10;
	[SerializeField, Range(0f, 180f)] private float _randomness = 90f;
	[SerializeField] private bool _snapping = false;
	[SerializeField] private bool _fadeOut = true;
	[SerializeField] private ShakeRandomnessMode _randomnessMode = ShakeRandomnessMode.Full;

	[Title("Animation Settings")]
	[SerializeField] private Ease _ease = Ease.OutQuad;
	[SerializeField, Min(0f)] private float _delay = 0f;
	[SerializeField] private bool _ignoreTimeScale = false;

	[Title("Loop Settings")]
	[SerializeField] private bool _isLooping = false;
	[SerializeField, ShowIf(nameof(_isLooping))] private int _loops = -1; // -1 = infinite
	[SerializeField, ShowIf(nameof(_isLooping))] private LoopType _loopType = LoopType.Restart;

	[Title("Events")]
	[SerializeField] private bool useEvents = false;
	[SerializeField, ShowIf(nameof(useEvents))] private UnityEngine.Events.UnityEvent _shakeCompleted;
	[SerializeField, ShowIf(nameof(useEvents))] private UnityEngine.Events.UnityEvent _shakeStarted;

	[Title("Target Override")]
	[SerializeField] private bool _useCustomTarget = false;
	[SerializeField, ShowIf(nameof(_useCustomTarget))] private Transform _customTarget;

	private Transform _targetTransform;
	private Vector3 _originalPosition;
	private Vector3 _originalRotation;
	private Vector3 _originalScale;
	private Tweener _currentTween;

	public enum ShakeType
	{
		Position,
		Rotation,
		Scale
	}

	private void Awake()
	{
		_targetTransform = _useCustomTarget && _customTarget != null ? _customTarget : transform;
		StoreOriginalValues();
	}

	private void Start()
	{
		if (_playOnStart)
		{
			PlayShake();
		}
	}

	private void OnEnable()
	{
		if (_playOnEnable && !_playOnStart)
		{
			PlayShake();
		}
	}

	private void OnDisable()
	{
		StopShake();
	}

	private void StoreOriginalValues()
	{
		if (_targetTransform != null)
		{
			_originalPosition = _targetTransform.localPosition;
			_originalRotation = _targetTransform.localEulerAngles;
			_originalScale = _targetTransform.localScale;
		}
	}

	[ContextMenu(nameof(PlayShake))]
	public void PlayShake()
	{
		if (_targetTransform == null)
		{
			Debug.LogWarning($"{nameof(DOTweenShakeEffect)}: Target transform is null!");
			return;
		}

		StopShake();

		if (useEvents && _shakeStarted != null)
			_shakeStarted.Invoke();

		switch (_shakeType)
		{
			case ShakeType.Position:
				_currentTween = _targetTransform.DOShakePosition(_duration, _strength, _vibrato, _randomness, _snapping, _fadeOut, _randomnessMode);
				break;

			case ShakeType.Rotation:
				_currentTween = _targetTransform.DOShakeRotation(_duration, _strength, _vibrato, _randomness, _fadeOut, _randomnessMode);
				break;

			case ShakeType.Scale:
				_currentTween = _targetTransform.DOShakeScale(_duration, _strength, _vibrato, _randomness, _fadeOut, _randomnessMode);
				break;
		}

		if (_currentTween != null)
		{
			_currentTween.SetEase(_ease)
					   .SetDelay(_delay)
					   .SetUpdate(_ignoreTimeScale);

			if (_isLooping)
			{
				_currentTween.SetLoops(_loops, _loopType);
			}

			if (useEvents && _shakeCompleted != null)
			{
				_currentTween.OnComplete(() => _shakeCompleted.Invoke());
			}
		}
	}

	[ContextMenu(nameof(StopShake))]
	public void StopShake()
	{
		if (_currentTween != null && _currentTween.IsActive())
		{
			_currentTween.Kill();
			_currentTween = null;
		}
	}

	public void ResetToOriginal()
	{
		StopShake();

		if (_targetTransform != null)
		{
			switch (_shakeType)
			{
				case ShakeType.Position:
					_targetTransform.localPosition = _originalPosition;
					break;

				case ShakeType.Rotation:
					_targetTransform.localEulerAngles = _originalRotation;
					break;

				case ShakeType.Scale:
					_targetTransform.localScale = _originalScale;
					break;
			}
		}
	}

	public void PlayShakeWithCustomParams(float customDuration, Vector3 customStrength, int customVibrato = 10, float customRandomness = 90f)
	{
		var originalDuration = _duration;
		var originalStrength = _strength;
		var originalVibrato = _vibrato;
		var originalRandomness = _randomness;

		_duration = customDuration;
		_strength = customStrength;
		_vibrato = customVibrato;
		_randomness = customRandomness;

		PlayShake();

		_duration = originalDuration;
		_strength = originalStrength;
		_vibrato = originalVibrato;
		_randomness = originalRandomness;
	}

	public bool IsShaking()
	{
		return _currentTween != null && _currentTween.IsActive();
	}

	private void OnDestroy()
	{
		StopShake();
	}

#if UNITY_EDITOR
	[ContextMenu(nameof(TestShakeInEditor))]
	private void TestShakeInEditor()
	{
		if (Application.isPlaying)
		{
			PlayShake();
		}
		else
		{
			Debug.Log("Test Shake доступен только в Play Mode");
		}
	}
#endif
}