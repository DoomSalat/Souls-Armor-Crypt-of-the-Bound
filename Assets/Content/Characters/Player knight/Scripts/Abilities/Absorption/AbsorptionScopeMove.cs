using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AbsorptionScopeMove : MonoBehaviour
{
	private const float DefaultMoveFactor = 1f;
	private const float MinDistanceToTarget = 0.01f;
	private const float PreciseMovementDistance = 0.5f;

	[SerializeField, MinValue(0f)] private float _moveSpeed = 10f;
	[SerializeField, MinValue(0.01f)] private float _smoothingDistance = 0.5f;
	[SerializeField, Range(0f, 1f)] private float _minSpeedFactor = 0.1f;

	[Header("Predictive Targeting")]
	[SerializeField, MinValue(0f)] private float _minLeadDistance = 0.3f;
	[SerializeField, MinValue(0f)] private float _maxLeadDistance = 2f;
	[SerializeField, MinValue(0f)] private float _leadMultiplier = 0.8f;
	[SerializeField, MinValue(0.01f)] private float _minTargetSpeed = 0.1f;
	[SerializeField, MinValue(1f)] private float _speedBoostMultiplier = 1.5f;
	[SerializeField, MinValue(1f)] private float _maxSpeedForMaxLead = 10f;

	private Rigidbody2D _rigidbody;
	private Camera _mainCamera;
	private bool _isFollowing = false;
	private Transform _target;

	private Vector2 _previousTargetPosition;
	private Vector2 _targetVelocity;
	private bool _hasValidPreviousPosition = false;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_mainCamera = Camera.main;
	}

	public void Move()
	{
		if (CanMove() == false)
			return;

		Vector2 targetPosition = GetTargetPosition();
		Vector2 currentPosition = _rigidbody.position;
		float distanceToTarget = (targetPosition - currentPosition).magnitude;

		if (IsReachedTarget(distanceToTarget))
			return;

		Vector2 newPosition = CalculateNewPosition(currentPosition, targetPosition, distanceToTarget);
		ApplyVelocity(currentPosition, newPosition, distanceToTarget);
	}

	public void SetFollowing(bool follow)
	{
		_isFollowing = follow;
		_target = null;
		_hasValidPreviousPosition = false;

		if (follow == false)
		{
			_rigidbody.linearVelocity = Vector2.zero;
		}
	}

	public void SetTarget(Transform target)
	{
		_target = target;
		_isFollowing = false;
		_hasValidPreviousPosition = false;

		if (_target != null)
		{
			_previousTargetPosition = _target.position;
		}
	}

	private bool CanMove()
	{
		return _isFollowing || _target != null;
	}

	private Vector2 GetTargetPosition()
	{
		if (_target != null)
		{
			Vector2 currentTargetPosition = _target.position;

			UpdateTargetVelocity(currentTargetPosition);

			Vector2 predictiveOffset = CalculatePredictiveOffset();

			return currentTargetPosition + predictiveOffset;
		}

		Vector3 mouseScreenPosition = InputUtilits.GetMouseClampPosition();
		Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);
		return mouseWorldPosition;
	}

	private void UpdateTargetVelocity(Vector2 currentTargetPosition)
	{
		if (_hasValidPreviousPosition)
		{
			_targetVelocity = (currentTargetPosition - _previousTargetPosition) / Time.fixedDeltaTime;
		}
		else
		{
			_targetVelocity = Vector2.zero;
			_hasValidPreviousPosition = true;
		}

		_previousTargetPosition = currentTargetPosition;
	}

	private Vector2 CalculatePredictiveOffset()
	{
		float targetSpeed = _targetVelocity.magnitude;

		if (targetSpeed < _minTargetSpeed)
		{
			return Vector2.zero;
		}

		Vector2 targetDirection = _targetVelocity.normalized;

		float speedProgress = Mathf.Clamp01((targetSpeed - _minTargetSpeed) / (_maxSpeedForMaxLead - _minTargetSpeed));
		float leadDistance = Mathf.Lerp(_minLeadDistance, _maxLeadDistance, speedProgress * _leadMultiplier);

		Vector2 predictiveOffset = targetDirection * leadDistance;

		Vector2 currentPosition = _rigidbody.position;
		Vector2 currentTargetPosition = _target.position;
		float distanceToTarget = (currentTargetPosition - currentPosition).magnitude;

		float maxOffsetMagnitude = Mathf.Max(distanceToTarget * 0.5f, _maxLeadDistance);

		if (predictiveOffset.magnitude > maxOffsetMagnitude)
		{
			predictiveOffset = predictiveOffset.normalized * maxOffsetMagnitude;
		}

		return predictiveOffset;
	}

	private bool IsReachedTarget(float distanceToTarget)
	{
		if (distanceToTarget <= MinDistanceToTarget)
		{
			_rigidbody.linearVelocity = Vector2.zero;
			return true;
		}

		return false;
	}

	private Vector2 CalculateNewPosition(Vector2 currentPosition, Vector2 targetPosition, float distanceToTarget)
	{
		float adaptiveSpeed = CalculateAdaptiveSpeed();

		return Vector3.MoveTowards(currentPosition, targetPosition, adaptiveSpeed * Time.fixedDeltaTime);
	}

	private float CalculateAdaptiveSpeed()
	{
		float targetSpeed = _targetVelocity.magnitude;

		if (targetSpeed > _minTargetSpeed)
		{
			float speedBoost = 1f + (targetSpeed * _speedBoostMultiplier / _moveSpeed);
			return _moveSpeed * speedBoost;
		}

		return _moveSpeed;
	}

	private void ApplyVelocity(Vector2 currentPosition, Vector2 newPosition, float distanceToTarget)
	{
		if (distanceToTarget > PreciseMovementDistance)
		{
			ApplyFastMovement(currentPosition, newPosition);
		}
		else
		{
			ApplyPreciseMovement(currentPosition, newPosition);
		}
	}

	private void ApplyFastMovement(Vector2 currentPosition, Vector2 newPosition)
	{
		_rigidbody.linearVelocity = (newPosition - currentPosition) / Time.fixedDeltaTime;
	}

	private void ApplyPreciseMovement(Vector2 currentPosition, Vector2 newPosition)
	{
		float speedFactor = SmoothSpeedFactor(currentPosition, newPosition);
		_rigidbody.linearVelocity = (newPosition - currentPosition) * speedFactor / Time.fixedDeltaTime;
	}

	private float SmoothSpeedFactor(Vector2 currentPosition, Vector2 targetPosition)
	{
		float distanceSqr = (targetPosition - currentPosition).sqrMagnitude;
		float smoothingDistanceSqr = _smoothingDistance * _smoothingDistance;

		if (distanceSqr >= smoothingDistanceSqr)
			return DefaultMoveFactor;

		float time = distanceSqr / smoothingDistanceSqr;
		return Mathf.Lerp(_minSpeedFactor, DefaultMoveFactor, time);
	}
}
