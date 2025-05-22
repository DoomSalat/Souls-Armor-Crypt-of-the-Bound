using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Sword : MonoBehaviour, IKnockbackProvider
{
	[SerializeField, Required] private SmoothLook _eye;
	[SerializeField, MinValue(0)] private float _knockbackForceMultiplier = 2f;

	private Rigidbody2D _rigidbody;

	private float _currentSpeed;
	private Vector2 _previousPosition;

	private bool _isActive;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	private void Start()
	{
		DeactiveFollow();
	}

	public void UpdateLook(Transform target)
	{
		if (_isActive)
			_eye.LookAt(target.position);
		else
			_eye.LookAt();
	}

	private void FixedUpdate()
	{
		Vector2 currentPosition = _rigidbody.position;
		_currentSpeed = (currentPosition - _previousPosition).magnitude / Time.fixedDeltaTime;
		_previousPosition = currentPosition;
	}

	public void ActiveFollow()
	{
		_isActive = true;
	}

	public void DeactiveFollow()
	{
		_isActive = false;
	}

	public void CalculateKnockback(Collider2D hitCollider, Collider2D other, out Vector2 direction, out float force)
	{
		direction = CalculateKnockbackDirection(hitCollider, other);
		force = CalculateKnockbackForce();
	}

	private Vector2 CalculateKnockbackDirection(Collider2D hitCollider, Collider2D other)
	{
		Vector2 closestPointOnEnemy = other.ClosestPoint(hitCollider.bounds.center);
		Vector2 closestPointOnWeapon = hitCollider.ClosestPoint(other.bounds.center);
		Vector2 knockbackDirection = (closestPointOnWeapon - closestPointOnEnemy).normalized;

		if (knockbackDirection.sqrMagnitude < 0.001f)
		{
			knockbackDirection = (other.bounds.center - hitCollider.bounds.center).normalized;

			if (knockbackDirection.sqrMagnitude < 0.001f && _rigidbody != null)
			{
				knockbackDirection = _rigidbody.linearVelocity.normalized;
			}
		}

		return knockbackDirection;
	}

	private float CalculateKnockbackForce()
	{
		return _currentSpeed * _knockbackForceMultiplier;
	}
}
