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

	private Rigidbody2D _rigidbody;
	private Vector2 _moveDirection;
	private bool _hasReachedTarget;

	private Vector2 _groupInfluence;
	private float _groupInfluenceStrength;
	private bool _controlOverridden;

	public bool IsMovementEnabled => enabled;
	public Vector2 Direction => _moveDirection;
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
		enabled = false;
		StopMovement();
	}

	public void ResumeMovement()
	{
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
		if (_controlOverridden)
		{
			if (_groupInfluence.sqrMagnitude > MinimumVectorMagnitude)
			{
				_rigidbody.linearVelocity = _groupInfluence.normalized * _groupInfluenceStrength;
			}
		}
		else
		{
			if (_moveDirection.sqrMagnitude > MinimumVectorMagnitude)
			{
				_rigidbody.linearVelocity = _moveDirection.normalized * _moveSpeed;
			}
			else
			{
				_rigidbody.linearVelocity = Vector2.zero;
			}
		}

		_groupInfluence = Vector2.zero;
		_groupInfluenceStrength = 0f;
	}
}
