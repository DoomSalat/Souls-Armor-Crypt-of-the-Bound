using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PlayerHandsTarget : MonoBehaviour
{
	[SerializeField, Required] private PlayerLimbs _playerLimbs;
	[SerializeField, Required] private SwordController _swordController;
	[SerializeField, Required] private PlayerKnightAnimator _playerKnightAnimator;

	[Header("Hand References")]
	[SerializeField, Required] private Transform _leftHandTransform;
	[SerializeField] private Vector2 _leftHandFaceDirection = Vector2.right;
	[SerializeField, Required] private Transform _rightHandTransform;
	[SerializeField] private Vector2 _rightHandFaceDirection = Vector2.left;

	[Header("Animation Settings")]
	[SerializeField, MinValue(0)] private float _rotationDuration = 0.3f;
	[SerializeField, MinValue(0)] private float _positionDuration = 0.2f;
	[SerializeField] private Ease _rotationEase = Ease.OutQuart;
	[SerializeField] private Ease _positionEase = Ease.OutQuart;

	[Header("Z Position Settings")]
	[SerializeField] private float _activeZOffset = -0.01f;

	[Header("Animation Events")]
	[SerializeField] private UnityEvent _leftHandActivated;
	[SerializeField] private UnityEvent _leftHandDeactivated;
	[SerializeField] private UnityEvent _rightHandActivated;
	[SerializeField] private UnityEvent _rightHandDeactivated;

	private Transform _swordTransform;

	private Vector3 _leftHandOriginalPosition;
	private Vector3 _rightHandOriginalPosition;
	private Quaternion _leftHandOriginalRotation;
	private Quaternion _rightHandOriginalRotation;

	private HandType _currentActiveHand = HandType.None;
	private readonly Dictionary<HandType, string> _tweenIds = new Dictionary<HandType, string>();
	private bool _isLogicActive;

	private enum HandType
	{
		None,
		Left,
		Right
	}

	private enum Direction
	{
		Down = 1,    // к камере
		Up = 2,      // от камеры  
		Left = 3,    // влево
		Right = 4    // вправо
	}

	private void Awake()
	{
		_swordTransform = _swordController.transform;

		CacheOriginalTransforms();
		InitializeTweenIds();
	}

	private void Start()
	{
		ValidateReferences();
	}

	public void ActivateLook()
	{
		_isLogicActive = true;
	}

	public void DeactivateLook()
	{
		_isLogicActive = false;
		DeactivateHandControl();
	}

	public void UpdateLook()
	{
		if (_isLogicActive && IsSwordControlActive())
		{
			UpdateHandControl();
		}
		else if (_isLogicActive == false)
		{
			DeactivateHandControl();
		}
	}

	private void OnDestroy()
	{
		KillAllTweens();
	}

	private void CacheOriginalTransforms()
	{
		_leftHandOriginalPosition = _leftHandTransform.localPosition;
		_rightHandOriginalPosition = _rightHandTransform.localPosition;
		_leftHandOriginalRotation = _leftHandTransform.localRotation;
		_rightHandOriginalRotation = _rightHandTransform.localRotation;
	}

	private void InitializeTweenIds()
	{
		var instanceId = GetInstanceID().ToString();
		_tweenIds[HandType.Left] = $"LeftHand_{instanceId}";
		_tweenIds[HandType.Right] = $"RightHand_{instanceId}";
	}

	private void ValidateReferences()
	{
		if (_leftHandTransform == null || _rightHandTransform == null)
		{
			Debug.LogError($"Hand transforms not assigned in {name}!", this);
		}

		if (_swordController == null)
		{
			Debug.LogError($"SwordController not assigned in {name}!", this);
		}

		if (_playerKnightAnimator == null)
		{
			Debug.LogError($"PlayerKnightAnimator not assigned in {name}!", this);
		}
	}

	private bool IsSwordControlActive()
	{
		var limbStates = _playerLimbs.LimbStates;
		return limbStates[LimbType.LeftArm] && limbStates[LimbType.RightArm] &&
			   limbStates[LimbType.Sword];
	}

	private void UpdateHandControl()
	{
		var nearestHand = DetermineNearestHand();

		if (nearestHand != _currentActiveHand)
		{
			SetActiveHand(nearestHand);
		}

		if (_currentActiveHand != HandType.None)
		{
			UpdateActiveHandRotation();
		}
	}

	private HandType DetermineNearestHand()
	{
		var playerDirection = GetPlayerDirection();
		var swordPosition = _swordTransform.position;
		var playerPosition = transform.position;
		var relativePosition = swordPosition - playerPosition;

		switch (playerDirection)
		{
			case Direction.Down:
				return relativePosition.x > 0 ? HandType.Left : HandType.Right;

			case Direction.Up:
				return relativePosition.x > 0 ? HandType.Right : HandType.Left;

			case Direction.Right:
				return relativePosition.y > 0 ? HandType.Right : HandType.Left;

			case Direction.Left:
				return relativePosition.y > 0 ? HandType.Left : HandType.Right;

			default:
				return HandType.None;
		}
	}

	private Direction GetPlayerDirection()
	{
		var directionValue = _playerKnightAnimator.GetDirection();
		return (Direction)directionValue;
	}

	private void SetActiveHand(HandType newActiveHand)
	{
		if (_currentActiveHand != HandType.None)
		{
			DeactivateHand(_currentActiveHand);
			InvokeHandDeactivatedEvent(_currentActiveHand);
		}

		_currentActiveHand = newActiveHand;

		if (_currentActiveHand != HandType.None)
		{
			ActivateHand(_currentActiveHand);
			InvokeHandActivatedEvent(_currentActiveHand);
		}
	}

	private void ActivateHand(HandType handType)
	{
		var handTransform = GetHandTransform(handType);
		var tweenId = _tweenIds[handType];

		DOTween.Kill(tweenId);

		var newPosition = GetOriginalHandPosition(handType);
		newPosition.z += _activeZOffset;

		handTransform.DOLocalMove(newPosition, _positionDuration)
			.SetEase(_positionEase)
			.SetId(tweenId);
	}

	private void DeactivateHand(HandType handType)
	{
		var handTransform = GetHandTransform(handType);
		var tweenId = _tweenIds[handType];

		DOTween.Kill(tweenId);

		var originalPosition = GetOriginalHandPosition(handType);
		var originalRotation = GetOriginalHandRotation(handType);

		handTransform.DOLocalMove(originalPosition, _positionDuration)
			.SetEase(_positionEase)
			.SetId(tweenId);

		handTransform.DOLocalRotateQuaternion(originalRotation, _rotationDuration)
			.SetEase(_rotationEase)
			.SetId(tweenId);
	}

	private void UpdateActiveHandRotation()
	{
		var activeHandTransform = GetHandTransform(_currentActiveHand);
		var swordDirection = (_swordTransform.position - activeHandTransform.position).normalized;
		var handFaceDirection = GetHandFaceDirection(_currentActiveHand);

		// Вычисляем угол между направлением лица руки и направлением к мечу
		var targetAngle = Mathf.Atan2(swordDirection.y, swordDirection.x) * Mathf.Rad2Deg;
		var faceAngle = Mathf.Atan2(handFaceDirection.y, handFaceDirection.x) * Mathf.Rad2Deg;
		var finalAngle = targetAngle - faceAngle;

		var targetRotation = Quaternion.Euler(0, 0, finalAngle);

		var tweenId = _tweenIds[_currentActiveHand];

		DOTween.Kill(tweenId + "_rotation");

		activeHandTransform.DOLocalRotateQuaternion(targetRotation, _rotationDuration)
			.SetEase(_rotationEase)
			.SetId(tweenId + "_rotation");
	}

	private void DeactivateHandControl()
	{
		if (_currentActiveHand != HandType.None)
		{
			DeactivateHand(_currentActiveHand);
			InvokeHandDeactivatedEvent(_currentActiveHand);
			_currentActiveHand = HandType.None;
		}
	}

	private void InvokeHandActivatedEvent(HandType handType)
	{
		switch (handType)
		{
			case HandType.Left:
				_leftHandActivated?.Invoke();
				break;
			case HandType.Right:
				_rightHandActivated?.Invoke();
				break;
		}
	}

	private void InvokeHandDeactivatedEvent(HandType handType)
	{
		switch (handType)
		{
			case HandType.Left:
				_leftHandDeactivated?.Invoke();
				break;
			case HandType.Right:
				_rightHandDeactivated?.Invoke();
				break;
		}
	}

	private Transform GetHandTransform(HandType handType)
	{
		return handType == HandType.Left ? _leftHandTransform : _rightHandTransform;
	}

	private Vector2 GetHandFaceDirection(HandType handType)
	{
		return handType == HandType.Left ? _leftHandFaceDirection : _rightHandFaceDirection;
	}

	private Vector3 GetOriginalHandPosition(HandType handType)
	{
		return handType == HandType.Left ? _leftHandOriginalPosition : _rightHandOriginalPosition;
	}

	private Quaternion GetOriginalHandRotation(HandType handType)
	{
		return handType == HandType.Left ? _leftHandOriginalRotation : _rightHandOriginalRotation;
	}

	private void KillAllTweens()
	{
		foreach (var tweenId in _tweenIds.Values)
		{
			DOTween.Kill(tweenId);
			DOTween.Kill(tweenId + "_rotation");
		}
	}
}