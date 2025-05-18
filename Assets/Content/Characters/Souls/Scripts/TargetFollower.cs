using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TargetFollower : MonoBehaviour
{
	[SerializeField, MinValue(0)] private float _moveSpeed = 2f;
	[SerializeField, MinValue(0)] private float _minDistance = 0.1f;

	private Rigidbody2D _rigidbody;
	private Vector2 _moveDirection;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	public void TryFollow(Transform target)
	{
		if (target == null)
		{
			StopMovement();

			return;
		}

		UpdateMoveDirection(target.position);
		ApplyMovement();
	}

	private void UpdateMoveDirection(Vector2 targetPosition)
	{
		Vector2 currentPosition = _rigidbody.position;
		float distanceSqr = (targetPosition - currentPosition).sqrMagnitude;

		if (distanceSqr <= _minDistance * _minDistance)
		{
			_moveDirection = Vector2.zero;
		}
		else
		{
			_moveDirection = (targetPosition - currentPosition).normalized;
		}
	}

	private void ApplyMovement()
	{
		_rigidbody.linearVelocity = _moveDirection * _moveSpeed;
	}

	private void StopMovement()
	{
		_rigidbody.linearVelocity = Vector2.zero;
	}
}
