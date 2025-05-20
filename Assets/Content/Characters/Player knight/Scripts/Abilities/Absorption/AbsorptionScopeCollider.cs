using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D), typeof(Rigidbody2D))]
public class AbsorptionScopeCollider : MonoBehaviour
{
	private const float OrientationY = 90f;
	private const float Half = 0.5f;

	private Rigidbody2D _rigidbody;
	private CapsuleCollider2D _capsuleCollider;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_capsuleCollider = GetComponent<CapsuleCollider2D>();
	}

	public void UpdateCollider(Vector2 targetPosition)
	{
		Vector2 scopePosition = transform.position;
		Vector2 direction = CalculateDirection(scopePosition, targetPosition);
		float distanceToTarget = CalculateDistance(scopePosition, targetPosition);

		RotateTowardsTarget(direction);
		UpdateColliderPosition(direction, distanceToTarget);
	}

	private Vector2 CalculateDirection(Vector2 from, Vector2 to)
	{
		return (to - from).normalized;
	}

	private float CalculateDistance(Vector2 from, Vector2 to)
	{
		return (to - from).magnitude;
	}

	private void RotateTowardsTarget(Vector2 direction)
	{
		float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - OrientationY;
		_rigidbody.rotation = targetAngle;
	}

	private void UpdateColliderPosition(Vector2 direction, float distance)
	{
		float initialWidth = _capsuleCollider.size.x;
		_capsuleCollider.size = new Vector2(initialWidth, distance + initialWidth);
		_capsuleCollider.offset = new Vector2(0f, distance * Half);
	}
}