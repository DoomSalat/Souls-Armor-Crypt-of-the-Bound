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

	private Rigidbody2D _rigidbody;
	private Camera _mainCamera;
	private bool _isFollowing = false;
	private Transform _target;

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

		if (follow == false)
		{
			_rigidbody.linearVelocity = Vector2.zero;
		}
	}

	public void SetTarget(Transform target)
	{
		_target = target;
		_isFollowing = false;
	}

	private bool CanMove()
	{
		return _isFollowing || _target != null;
	}

	private Vector2 GetTargetPosition()
	{
		if (_target != null)
			return _target.position;

		Vector3 mouseScreenPosition = InputUtilits.GetMouseClampPosition();
		Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);
		return mouseWorldPosition;
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
		return Vector3.MoveTowards(currentPosition, targetPosition, _moveSpeed * Time.fixedDeltaTime);
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
