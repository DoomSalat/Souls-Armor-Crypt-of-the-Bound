using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TargetFollower : MonoBehaviour
{
	[SerializeField, ReadOnly] private Transform _target;

	[Header("Lineal Movement")]
	[SerializeField, MinValue(0)] private float _moveSpeed = 2f;
	[SerializeField, MinValue(0)] private float _minDistance = 0.1f;

	[Header("Force Movement")]
	[SerializeField] private bool _useForceMovement = false;
	[SerializeField, MinValue(0)] private float _acceleration = 10f;
	[SerializeField, MinValue(0)] private float _maxSpeed = 15f;

	private Rigidbody2D _rigidbody;
	private Vector2 _moveDirection;
	private bool _hasReachedTarget;

	public event System.Action TargetReached;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	public void TryFollow(Transform target)
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

	public void SetMoveState(bool isForceMovement)
	{
		_useForceMovement = isForceMovement;
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
		if (_useForceMovement)
		{
			Vector2 accelerationForce = _moveDirection * _acceleration;
			_rigidbody.AddForce(accelerationForce);

			if (_rigidbody.linearVelocity.magnitude > _maxSpeed)
			{
				_rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _maxSpeed;
			}
		}
		else
		{
			_rigidbody.linearVelocity = _moveDirection * _moveSpeed;
		}
	}
}
