using Sirenix.OdinInspector;
using UnityEngine;

namespace BlockAnimationSystem
{
	[ExecuteAlways]
	public class TargetFollower : MonoBehaviour
	{
		private const int FallbackTargetIndexOffset = 1;

		[Header("Target Configuration")]
		[SerializeField] private Transform[] _targetObjects;
		[SerializeField] private GameObject[] _activationTriggers;

		[Header("Position Tracking")]
		[SerializeField] private bool _trackPosition = true;
		[SerializeField] private bool _trackZAxis;
		[SerializeField] private Vector3 _positionOffset;
		[SerializeField][MinValue(0)] private float _positionSmoothingFactor = 0;
		[SerializeField] private bool _useLocalOffset;
		[SerializeField] private bool _calculateOffsetOnStart = false;

		[Header("Rotation Tracking")]
		[SerializeField] private bool _enableRotationTracking;
		[SerializeField] private float _rotationOffsetAngle;
		[SerializeField][MinValue(0)] private float _rotationSmoothingFactor = 0;

		[Header("Scale Tracking")]
		[SerializeField] private bool _enableScaleTracking;
		[SerializeField] private Vector3 _scaleOffset;
		[SerializeField][MinValue(0)] private float _scaleSmoothingFactor = 0;

		[Header("Component Configuration")]
		[SerializeField] private Transform _localTransform;

		private Transform _activeTarget;

		private void OnEnable()
		{
			InitializeComponents();
			UpdateTransform(instant: true);
		}

		private void Awake()
		{
			InitializeComponents();
		}

		private void LateUpdate()
		{
			if (IsConfigurationValid())
			{
				UpdateTransform();
			}
		}

		[ContextMenu(nameof(InitializeComponents))]
		private void InitializeComponents()
		{
			_localTransform ??= transform;

			if (_calculateOffsetOnStart && HasValidTargets())
			{
				_positionOffset = _localTransform.position - _targetObjects[0].position;
			}
		}

		private bool IsConfigurationValid()
		{
			return _targetObjects is { Length: > 0 } && _targetObjects[0] != null &&
				   _activationTriggers is { Length: > 0 } && _activationTriggers[0] != null;
		}

		private bool HasValidTargets()
		{
			return _targetObjects is { Length: > 0 } && _targetObjects[0] != null;
		}

		private void UpdateTransform(bool instant = false)
		{
			if (_targetObjects == null || _targetObjects.Length == 0)
				return;

			_activeTarget = SelectActiveTarget();
			UpdatePosition(instant);
			UpdateRotation(instant);
			UpdateScale(instant);
		}

		private void UpdatePosition(bool instant)
		{
			if (!_trackPosition)
				return;

			Vector3 targetPosition = ComputeTargetPosition();

			if (!_trackZAxis)
			{
				targetPosition.z = _localTransform.position.z;
			}

			if (_positionSmoothingFactor > 0 && Application.isPlaying && !instant)
			{
				_localTransform.position = Vector3.Lerp(_localTransform.position, targetPosition, _positionSmoothingFactor * Time.deltaTime);
			}
			else
			{
				_localTransform.position = targetPosition;
			}
		}

		private void UpdateRotation(bool instant)
		{
			if (_enableRotationTracking)
			{
				Quaternion targetRotation = _activeTarget.rotation * Quaternion.Euler(0, 0, _rotationOffsetAngle);

				if (_rotationSmoothingFactor > 0 && Application.isPlaying && !instant)
				{
					_localTransform.rotation = Quaternion.Lerp(_localTransform.rotation, targetRotation, _rotationSmoothingFactor * Time.deltaTime);
				}
				else
				{
					_localTransform.rotation = targetRotation;
				}
			}
		}

		private void UpdateScale(bool instant)
		{
			if (_enableScaleTracking)
			{
				Vector3 targetScale = _activeTarget.localScale + _scaleOffset;

				if (_scaleSmoothingFactor > 0 && Application.isPlaying && !instant)
				{
					_localTransform.localScale = Vector3.Lerp(_localTransform.localScale, targetScale, _scaleSmoothingFactor * Time.deltaTime);
				}
				else
				{
					_localTransform.localScale = targetScale;
				}
			}
		}

		private Vector3 ComputeTargetPosition()
		{
			if (_useLocalOffset)
			{
				return _activeTarget.position +
					   _activeTarget.right * _positionOffset.x +
					   _activeTarget.up * _positionOffset.y +
					   _activeTarget.forward * _positionOffset.z;
			}
			return _activeTarget.position + _positionOffset;
		}

		private Transform SelectActiveTarget()
		{
			for (int i = 0; i < _activationTriggers.Length && i < _targetObjects.Length; i++)
			{
				if (_activationTriggers[i].activeInHierarchy)
				{
					return _targetObjects[i];
				}
			}

			return _targetObjects[^FallbackTargetIndexOffset]; // Return last target as fallback
		}
	}
}