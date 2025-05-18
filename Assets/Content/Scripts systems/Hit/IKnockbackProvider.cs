using UnityEngine;

public interface IKnockbackProvider
{
	public void CalculateKnockback(Collider2D hitCollider, Collider2D target, out Vector2 direction, out float force);
}