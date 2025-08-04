using Sirenix.OdinInspector;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class SwordKnockbackProvider : MonoBehaviour, IKnockbackProvider
{
	private const float MinKnockbackMagnitude = 0.001f;

	[SerializeField, Required] private SwordSpeedTracker _speedTracker;
	[SerializeField, MinValue(0)] private float _knockbackForceMultiplier = 2f;

	private Rigidbody2D _rigidbody;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
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

		if (knockbackDirection.sqrMagnitude < MinKnockbackMagnitude)
		{
			knockbackDirection = (other.bounds.center - hitCollider.bounds.center).normalized;

			if (knockbackDirection.sqrMagnitude < MinKnockbackMagnitude)
			{
				knockbackDirection = _rigidbody.linearVelocity.normalized;
			}
		}

		return knockbackDirection;
	}

	private float CalculateKnockbackForce()
	{
		return _speedTracker.CurrentSpeed * _knockbackForceMultiplier;
	}
}