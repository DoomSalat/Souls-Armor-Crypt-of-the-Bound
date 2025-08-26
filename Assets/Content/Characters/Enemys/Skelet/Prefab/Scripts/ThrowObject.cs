using UnityEngine;
using CustomPool;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ThrowObject : MonoBehaviour, IPool, IPoolReference
{
	private const float FullProgress = 1f;

	[SerializeField, Required] private PhysicsRotate _physicsRotate;
	[SerializeField, Required] private Rigidbody2D _rigidbody;
	[SerializeField, Required] private HitBox _hitBox;
	[SerializeField, Required] private SimpleDamage _damage;

	[Header("Throw Settings")]
	[SerializeField] private float _speed = 10f;
	[SerializeField] private float _endDistance = 10f;
	[SerializeField] private LayerMask _wallLayerMask = 1;

	private ObjectPool<ThrowObject> _pool;
	private Vector2 _startPosition;
	private Vector3 _direction;
	private bool _isActive;

	private void OnEnable()
	{
		_hitBox.Hitted += OnHitTarget;
		SubscribeToDamageEvents();
	}

	private void OnDisable()
	{
		_hitBox.Hitted -= OnHitTarget;
		UnsubscribeFromDamageEvents();
	}

	private void FixedUpdate()
	{
		if (!_isActive)
			return;

		Vector2 currentPosition = transform.position;
		float distanceSquared = (_startPosition - currentPosition).sqrMagnitude;
		float endDistanceSquared = _endDistance * _endDistance;

		float distanceProgress = FullProgress - (distanceSquared / endDistanceSquared);
		_physicsRotate.SetRotationProgress(distanceProgress);

		if (distanceSquared >= endDistanceSquared)
		{
			ReturnToPool();
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (((1 << other.gameObject.layer) & _wallLayerMask) != 0)
		{
			ReturnToPool();
		}
	}

	public void Initialize()
	{
		_isActive = false;
		_rigidbody.linearVelocity = Vector2.zero;
		_hitBox.enabled = false;
	}

	public void InitializeThrow(Vector3 direction)
	{
		_direction = direction.normalized;
		_direction.z = 0f;
		_direction = _direction.normalized;

		_startPosition = transform.position;
		_isActive = true;

		_rigidbody.linearVelocity = _direction * _speed;
		_hitBox.enabled = true;
	}

	public void ReturnToPool()
	{
		_pool.Release(this);
	}

	public void OnSpawnFromPool()
	{
		_isActive = true;
		_startPosition = transform.position;
		_rigidbody.linearVelocity = Vector2.zero;

		_hitBox.enabled = false;
		_physicsRotate.StartRotation();
	}

	public void OnReturnToPool()
	{
		_isActive = false;
		_rigidbody.linearVelocity = Vector2.zero;
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;

		_hitBox.enabled = false;
		_physicsRotate.StopRotation();
	}

	public void SetPool(object pool)
	{
		_pool = pool as ObjectPool<ThrowObject>;
	}

	private void SubscribeToDamageEvents()
	{
		_damage.DamageReceived += OnDamageReceived;
		_damage.StatusApplied += OnStatusApplied;
	}

	private void UnsubscribeFromDamageEvents()
	{
		_damage.DamageReceived -= OnDamageReceived;
		_damage.StatusApplied -= OnStatusApplied;
	}

	private void OnDamageReceived(SimpleDamage target, DamageData damageData)
	{
		ReturnToPool();
	}

	private void OnStatusApplied(SimpleDamage target, DamageType statusType)
	{
		ReturnToPool();
	}

	private void OnHitTarget(Collider2D collider, DamageData data)
	{
		ReturnToPool();
	}
}
