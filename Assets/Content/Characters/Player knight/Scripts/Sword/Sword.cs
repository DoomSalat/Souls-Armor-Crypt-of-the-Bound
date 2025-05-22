using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Sword : MonoBehaviour, IKnockbackProvider
{
	[SerializeField, Required] private SmoothLook _eye;
	[SerializeField, Required] private Rigidbody2DLocalAxisLimiter _localAxisLimiter;
	[SerializeField, MinValue(0)] private float _knockbackForceMultiplier = 2f;

	[Header("Follow deactivation")]
	[SerializeField, MinValue(0)] private float _deactiveDamping = 1f;
	[SerializeField, MinValue(0)] private float _followRadius = 2f;
	[SerializeField, MinValue(0)] private float _springForce = 10f;
	[SerializeField, MinValue(0)] private float _dampingForce = 5f;

	private Rigidbody2D _rigidbody;
	private Transform _parentPocket;
	[ShowInInspector, ReadOnly] private Vector2 _pocketOffset;
	private float _currentSpeed;
	private Vector2 _previousPosition;
	private float _rigidbodySaveDampingLinear;

	private bool _isActive;
	private bool _isInRadius;
	private bool _hasValidOffset;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_parentPocket = transform.parent;
		_rigidbodySaveDampingLinear = _rigidbody.linearDamping;
	}

	private void Start()
	{
		DeactiveFollow();
	}

	private void FixedUpdate()
	{
		if (_isActive)
		{
			Vector2 currentPosition = _rigidbody.position;
			_currentSpeed = (currentPosition - _previousPosition).magnitude / Time.fixedDeltaTime;
			_previousPosition = currentPosition;

			_localAxisLimiter.UpdateLimit();
		}
		else
		{
			UpdateFollowPosition();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, _followRadius);
	}

	public void UpdateLook(Transform target)
	{
		if (_isActive)
			_eye.LookAt(target.position);
		else
			_eye.LookAt();
	}

	private void UpdateFollowPosition()
	{
		float distanceToPocket = Vector2.Distance(transform.position, _parentPocket.position);
		bool wasInRadius = _isInRadius;
		_isInRadius = distanceToPocket <= _followRadius;

		if (_isInRadius && wasInRadius == false)
		{
			UpdatePocketOffset();
		}

		if (_isInRadius && _hasValidOffset)
		{
			Vector2 targetPosition = _parentPocket.TransformPoint(_pocketOffset);
			Vector2 currentPosition = _rigidbody.position;
			Vector2 direction = (targetPosition - currentPosition).normalized;
			float distance = Vector2.Distance(currentPosition, targetPosition);

			Vector2 springForce = direction * distance * _springForce;
			Vector2 dampingForce = -_rigidbody.linearVelocity * _dampingForce;

			_rigidbody.AddForce(springForce + dampingForce, ForceMode2D.Force);
		}
	}

	private void UpdatePocketOffset()
	{
		_pocketOffset = _parentPocket.InverseTransformPoint(transform.position);
		_hasValidOffset = true;
	}

	public void ActiveFollow()
	{
		_isActive = true;
		_rigidbody.linearDamping = _rigidbodySaveDampingLinear;
		_rigidbody.linearVelocity = Vector2.zero;

		transform.SetParent(null);
		_hasValidOffset = false;
	}

	public void DeactiveFollow()
	{
		_isActive = false;
		_rigidbody.linearDamping = _deactiveDamping;
		_rigidbody.linearVelocity = Vector2.zero;

		transform.SetParent(_parentPocket);

		if (_isInRadius)
		{
			UpdatePocketOffset();
		}
		else
		{
			_hasValidOffset = false;
		}
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
