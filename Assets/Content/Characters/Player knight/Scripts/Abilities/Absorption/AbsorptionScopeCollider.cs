using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D), typeof(Rigidbody2D))]
public class AbsorptionScopeCollider : MonoBehaviour
{
	private const float OrientationY = 90f;
	private const float Half = 0.5f;

	[SerializeField, Required]
	private Transform _target;

	[SerializeField, MinValue(0f)]
	private float _maxColliderLength = 5f;

	[SerializeField, MinValue(0f)]
	private float _rotationSpeed = 360f;

	private Rigidbody2D _rigidbody;
	private CapsuleCollider2D _capsuleCollider;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_capsuleCollider = GetComponent<CapsuleCollider2D>();
	}

	public void UpdateCollider()
	{
		if (_target == null)
			return;

		Vector2 scopePosition = transform.position;
		Vector2 targetPosition = _target.position;

		// Вычисляем направление и расстояние
		Vector2 direction = (targetPosition - scopePosition).normalized;
		float distanceToTarget = Vector2.Distance(scopePosition, targetPosition);

		// Ограничиваем длину коллайдера
		distanceToTarget = Mathf.Min(distanceToTarget, _maxColliderLength);

		// Устанавливаем размер коллайдера
		_capsuleCollider.size = new Vector2(_capsuleCollider.size.x, distanceToTarget);

		// Смещаем центр коллайдера к середине между прицелом и целью
		_capsuleCollider.offset = direction * (distanceToTarget * Half);

		// Вычисляем угол поворота
		float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - OrientationY;

		// Плавно поворачиваем через Rigidbody2D
		float currentAngle = _rigidbody.rotation;
		float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, _rotationSpeed * Time.fixedDeltaTime);
		_rigidbody.MoveRotation(newAngle);
	}

	public void SetTarget(Transform newTarget)
	{
		_target = newTarget;
	}
}