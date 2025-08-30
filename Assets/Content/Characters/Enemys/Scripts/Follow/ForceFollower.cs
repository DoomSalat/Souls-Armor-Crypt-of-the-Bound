using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ForceFollower : MonoBehaviour, IFollower
{
	[SerializeField, ReadOnly] private Transform _target;

	[Header("Force Movement")]
	[SerializeField, MinValue(0)] private float _acceleration = 10f;
	[SerializeField, MinValue(0)] private float _maxSpeed = 15f;
	[SerializeField, MinValue(0)] private float _minDistance = 0.1f;

	private Rigidbody2D _rigidbody;
	private Vector2 _moveDirection;
	private bool _hasReachedTarget;

	private Vector2 _groupInfluence;
	private float _groupInfluenceStrength;

	public event System.Action TargetReached;

	public bool IsMovementEnabled => enabled;
	public Vector2 Direction => _moveDirection;
	public Transform Target => _target;

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

		UpdateMoveDirection(target.position);
		ApplyMovement();
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

		UpdateMoveDirection(_target.position);
		ApplyMovement();
	}

	public void AddInfluence(Vector2 influence, float strength)
	{
		_groupInfluence = influence;
		_groupInfluenceStrength = strength;
	}

	public void SetMoveSpeed(float speed)
	{
		_acceleration = speed;
	}

	public void StopMovement()
	{
		_rigidbody.linearVelocity = Vector2.zero;
	}

	public bool TryGetDistanceToTarget(out float distance)
	{
		if (_target == null)
		{
			distance = 0;
			return false;
		}

		Vector2 diff = _rigidbody.position - (Vector2)_target.position;
		distance = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
		return true;
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
		Vector2 accelerationForce = _moveDirection * _acceleration;
		_rigidbody.AddForce(accelerationForce);

		if (_groupInfluenceStrength > 0f)
		{
			Vector2 groupForce = _groupInfluence * _groupInfluenceStrength;
			_rigidbody.AddForce(groupForce, ForceMode2D.Force);
		}

		if (_rigidbody.linearVelocity.magnitude > _maxSpeed)
		{
			_rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _maxSpeed;
		}
	}
}
