using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Sword : MonoBehaviour, IKnockbackProvider
{
	[SerializeField, Required] private InputReader _activeButton;
	[SerializeField, Required] private SpringJoint2D _targetController;
	[SerializeField, Required] private SmoothLook _eye;
	[SerializeField, MinValue(0)] private float _knockbackForceMultiplier = 2f;

	private Rigidbody2D _rigidbody;

	private float _currentSpeed;
	private Vector2 _previousPosition;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	private void Start()
	{
		DeactiveFollow();
	}

	private void OnEnable()
	{
		_activeButton.InputActions.Player.Sword.performed += context => ActiveFollow();
		_activeButton.InputActions.Player.Sword.canceled += context => DeactiveFollow();
	}

	private void OnDisable()
	{
		_activeButton.InputActions.Player.Sword.performed -= context => ActiveFollow();
		_activeButton.InputActions.Player.Sword.canceled -= context => DeactiveFollow();
	}

	private void FixedUpdate()
	{
		Vector2 currentPosition = _rigidbody.position;
		_currentSpeed = (currentPosition - _previousPosition).magnitude / Time.fixedDeltaTime;
		_previousPosition = currentPosition;
	}

	public void CalculateKnockback(Collider2D hitCollider, Collider2D other, out Vector2 direction, out float force)
	{
		direction = CalculateKnockbackDirection(hitCollider, other);
		force = CalculateKnockbackForce();
	}

	private void ActiveFollow()
	{
		_targetController.enabled = true;
		_eye.SetFollowing(true);
	}

	private void DeactiveFollow()
	{
		_targetController.enabled = false;
		_eye.SetFollowing(false);
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
