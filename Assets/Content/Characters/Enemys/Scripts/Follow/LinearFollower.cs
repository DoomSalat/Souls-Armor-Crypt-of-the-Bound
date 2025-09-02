using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LinearFollower : MonoBehaviour, IFollower
{
	private const float DefaultDistance = 0f;
	private const float MinimumVectorMagnitude = 0.01f;

	[SerializeField, ReadOnly] private Transform _target;

	[Header("Linear Movement")]
	[SerializeField, MinValue(0)] private float _moveSpeed = 2f;
	[SerializeField, MinValue(0)] private float _minDistance = 0.1f;
	[SerializeField, Range(0f, 1f)] private float _directionSmoothing = 0.85f;

	[Header("Debug")]
	[SerializeField] private bool _debugMovement = false;

	private Rigidbody2D _rigidbody;
	private Vector2 _moveDirection;
	private bool _hasReachedTarget;

	private Vector2 _groupInfluence;
	private float _groupInfluenceStrength;
	private bool _controlOverridden;

	public bool IsMovementEnabled => enabled;
	public Vector2 Direction => _moveDirection;
	public Vector2 Velocity => _rigidbody.linearVelocity;
	public Transform Target => _target;
	public bool IsControlOverridden => _controlOverridden;

	public event System.Action TargetReached;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	public void SetTarget(Transform target)
	{
		_target = target;
		_hasReachedTarget = false;

		if (enabled == false)
			return;

		if (target == null)
		{
			StopMovement();
			return;
		}
	}

	public bool TryGetDistanceToTarget(out float distance)
	{
		if (_target == null)
		{
			distance = DefaultDistance;
			return false;
		}

		Vector2 diff = _rigidbody.position - (Vector2)_target.position;
		distance = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
		return true;
	}

	public void PauseMovement()
	{
		if (_controlOverridden)
			return;

		enabled = false;
		StopMovement();
	}

	public void ResumeMovement()
	{
		if (_controlOverridden)
			return;

		enabled = true;
	}

	public void EnableMovement()
	{
		enabled = true;
		_hasReachedTarget = false;
	}

	public void DisableMovement()
	{
		enabled = false;
		StopMovement();
		_hasReachedTarget = false;
	}

	public void TryFollow()
	{
		if (_target == null || enabled == false)
			return;

		if (_controlOverridden == false)
		{
			UpdateMoveDirection(_target.position);
		}

		ApplyMovement();
	}

	public void AddInfluence(Vector2 influence, float strength)
	{
		_groupInfluence = influence;
		_groupInfluenceStrength = strength;
	}

	public void SetControlOverride(bool isOverridden)
	{
		_controlOverridden = isOverridden;

		if (isOverridden)
		{
			enabled = true;
			StopMovement();
		}
		else
		{
			_groupInfluence = Vector2.zero;
			_groupInfluenceStrength = 0f;
		}
	}

	public void SetMoveSpeed(float speed)
	{
		_moveSpeed = speed;
	}

	public void StopMovement()
	{
		_rigidbody.linearVelocity = Vector2.zero;
	}

	private void UpdateMoveDirection(Vector2 targetPosition)
	{
		if (enabled == false)
			return;

		Vector2 currentPosition = _rigidbody.position;
		float distanceSqr = (targetPosition - currentPosition).sqrMagnitude;

		if (distanceSqr <= _minDistance * _minDistance)
		{
			_moveDirection = Vector2.zero;

			if (_hasReachedTarget == false)
			{
				_hasReachedTarget = true;
				TargetReached?.Invoke();
			}
		}
		else
		{
			_moveDirection = (targetPosition - currentPosition).normalized;
		}
	}

	private void ApplyMovement()
	{
		Vector2 targetVelocity = Vector2.zero;

		if (_controlOverridden)
		{
			targetVelocity = _groupInfluence.normalized * _groupInfluenceStrength;

			if (_debugMovement)
			{
				Debug.Log($"[{nameof(LinearFollower)}] ApplyMovement: {_groupInfluence} - {_groupInfluenceStrength}");
			}
		}
		else
		{
			if (_moveDirection.sqrMagnitude > MinimumVectorMagnitude)
			{
				targetVelocity = _moveDirection.normalized * _moveSpeed;
			}
		}

		float currentSpeed = _rigidbody.linearVelocity.magnitude;

		if (targetVelocity.sqrMagnitude < Mathf.Epsilon)
		{
			_rigidbody.linearVelocity = Vector2.Lerp(_rigidbody.linearVelocity, Vector2.zero, _directionSmoothing);
		}
		else
		{
			Vector2 currentDirection = _rigidbody.linearVelocity.normalized;
			Vector2 newDirection = Vector2.Lerp(currentDirection, targetVelocity.normalized, _directionSmoothing);

			float targetSpeed = targetVelocity.magnitude;
			float smoothedSpeed = Mathf.Lerp(currentSpeed, targetSpeed, _directionSmoothing);

			_rigidbody.linearVelocity = newDirection * smoothedSpeed;
		}

		_groupInfluence = Vector2.zero;
		_groupInfluenceStrength = 0f;
	}
}
