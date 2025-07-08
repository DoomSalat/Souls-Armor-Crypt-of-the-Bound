using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class TargetLook : MonoBehaviour
{
	private static readonly float DefaultZOffset = float.NaN;
	private const float HalfAngle = 180f;
	private const string AnimPrefix = "_rotation";

	[SerializeField] private Vector2 _handFaceDirection = Vector2.right;

	[Header("Animation Settings")]
	[SerializeField, MinValue(0)] private float _rotationDuration = 0.3f;
	[SerializeField] private Ease _rotationEase = Ease.OutQuart;

	[Header("Z Position Settings")]
	[SerializeField] private float _activeZOffset = -0.01f;

	[Header("Events")]
	[SerializeField] private UnityEvent _activated;
	[SerializeField] private UnityEvent _deactivated;

	private Transform _handTransform;
	private Transform _target;
	private Vector3 _originalPosition;
	private Quaternion _originalRotation;
	private bool _isTrackingActive;
	private string _tweenId;

	private void Awake()
	{
		_handTransform = transform;

		CacheOriginalTransform();
		InitializeTweenId();
	}

	private void Update()
	{
		if (_isTrackingActive && _target != null)
		{
			UpdateHandRotation();
		}
	}

	private void OnDestroy()
	{
		KillTweens();
	}

	public void SetTarget(Transform target)
	{
		_target = target;
	}

	public void ActivateTracking()
	{
		ActivateTracking(DefaultZOffset);
	}

	public void ActivateTracking(float customZOffset)
	{
		_isTrackingActive = true;
		AnimateToActivePosition(customZOffset);

		UpdateHandRotation();

		_activated?.Invoke();
	}

	public void DeactivateTracking()
	{
		_isTrackingActive = false;
		AnimateToOriginalTransform();

		_deactivated?.Invoke();
	}

	public void UpdateZPosition(float newZOffset)
	{
		if (_isTrackingActive == false)
			return;

		var newPosition = _originalPosition;
		newPosition.z += newZOffset;
	}

	private void CacheOriginalTransform()
	{
		_originalPosition = _handTransform.localPosition;
		_originalRotation = _handTransform.localRotation;
	}

	private void InitializeTweenId()
	{
		_tweenId = $"{nameof(TargetLook)}_{GetInstanceID()}";
	}

	private void AnimateToActivePosition(float customZOffset)
	{
		var newPosition = _originalPosition;

		if (float.IsNaN(customZOffset) == false)
		{
			newPosition.z += customZOffset;
		}
		else
		{
			newPosition.z += _activeZOffset;
		}

		_handTransform.localPosition = newPosition;
	}

	private void AnimateToOriginalTransform()
	{
		KillTweens();

		_handTransform.localPosition = _originalPosition;

		_handTransform.DOLocalRotateQuaternion(_originalRotation, _rotationDuration)
			.SetEase(_rotationEase)
			.SetId(_tweenId);
	}

	private void UpdateHandRotation()
	{
		var targetDirection = (_target.position - _handTransform.position).normalized;

		var targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
		var faceAngle = Mathf.Atan2(_handFaceDirection.y, _handFaceDirection.x) * Mathf.Rad2Deg;
		var finalAngle = targetAngle - faceAngle;

		var globalScale = _handTransform.lossyScale;
		bool isFlippedX = globalScale.x < 0;
		bool isFlippedY = globalScale.y < 0;

		if (isFlippedX && isFlippedY)
		{
			finalAngle = finalAngle + HalfAngle;
		}
		else if (isFlippedX)
		{
			finalAngle = HalfAngle - finalAngle;
		}
		else if (isFlippedY)
		{
			finalAngle = -finalAngle;
		}

		var targetRotation = Quaternion.Euler(0, 0, finalAngle);

		DOTween.Kill(_tweenId + AnimPrefix);

		_handTransform.DOLocalRotateQuaternion(targetRotation, _rotationDuration)
			.SetEase(_rotationEase)
			.SetId(_tweenId + AnimPrefix);
	}

	private void KillTweens()
	{
		DOTween.Kill(_tweenId);
		DOTween.Kill(_tweenId + AnimPrefix);
	}
}