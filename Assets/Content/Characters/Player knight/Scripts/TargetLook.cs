using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class TargetLook : MonoBehaviour
{
	[SerializeField] private Vector2 _handFaceDirection = Vector2.right;

	[Header("Animation Settings")]
	[SerializeField, MinValue(0)] private float _rotationDuration = 0.3f;
	[SerializeField, MinValue(0)] private float _positionDuration = 0.2f;
	[SerializeField] private Ease _rotationEase = Ease.OutQuart;
	[SerializeField] private Ease _positionEase = Ease.OutQuart;

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

	public void ActivateTracking(float customZOffset = 999f)
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
		var instanceId = GetInstanceID().ToString();
		_tweenId = $"TargetLook_{instanceId}";
	}

	private void AnimateToActivePosition(float customZOffset = 999f)
	{
		KillTweens();

		var newPosition = _originalPosition;

		if (customZOffset != 999f)
		{
			newPosition.z += customZOffset;
		}
		else
		{
			newPosition.z += _activeZOffset;
		}

		_handTransform.DOLocalMove(newPosition, _positionDuration)
			.SetEase(_positionEase)
			.SetId(_tweenId);
	}

	private void AnimateToOriginalTransform()
	{
		KillTweens();

		_handTransform.DOLocalMove(_originalPosition, _positionDuration)
			.SetEase(_positionEase)
			.SetId(_tweenId);

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
			finalAngle = finalAngle + 180f;
		}
		else if (isFlippedX)
		{
			finalAngle = 180f - finalAngle;
		}
		else if (isFlippedY)
		{
			finalAngle = -finalAngle;
		}

		var targetRotation = Quaternion.Euler(0, 0, finalAngle);

		DOTween.Kill(_tweenId + "_rotation");

		_handTransform.DOLocalRotateQuaternion(targetRotation, _rotationDuration)
			.SetEase(_rotationEase)
			.SetId(_tweenId + "_rotation");
	}

	private void KillTweens()
	{
		DOTween.Kill(_tweenId);
		DOTween.Kill(_tweenId + "_rotation");
	}
}