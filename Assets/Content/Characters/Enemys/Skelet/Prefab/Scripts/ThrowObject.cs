using UnityEngine;
using CustomPool;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ThrowObject : MonoBehaviour, IPool, IPoolReference
{
	private const float FullProgress = 1f;

	[SerializeField, Required] private ThrowAnimator _animator;
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

		_animator.Ended += OnCrackedEnd;
	}

	private void OnDisable()
	{
		_hitBox.Hitted -= OnHitTarget;
		UnsubscribeFromDamageEvents();

		_animator.Ended -= OnCrackedEnd;
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
			_isActive = false;
			DamageReceived();
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (((1 << other.gameObject.layer) & _wallLayerMask) != 0)
		{
			_isActive = false;
			DamageReceived();
		}
	}

	public void Initialize()
	{
		_isActive = false;
		_rigidbody.linearVelocity = Vector2.zero;
		_hitBox.SetColliderEnabled(false);
	}

	public void InitializeThrow(Vector3 direction)
	{
		_direction = direction.normalized;
		_direction.z = 0f;
		_direction = _direction.normalized;

		_startPosition = transform.position;

		_rigidbody.linearVelocity = _direction * _speed;
		_physicsRotate.StartRotation();

		_animator.Reset();
	}

	public void OnSpawnFromPool()
	{
		_isActive = true;
		_startPosition = transform.position;
		_rigidbody.linearVelocity = Vector2.zero;

		_damage.EnableCollisions();
	}

	public void SetThrowParameters(float speed, float endDistance)
	{
		_speed = speed;
		_endDistance = endDistance;
	}

	public void SetPool(object pool)
	{
		_pool = pool as ObjectPool<ThrowObject>;
	}

	public void ReturnToPool()
	{
		_isActive = false;
		_rigidbody.linearVelocity = Vector2.zero;
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;

		_damage.DisableCollisions();
		_physicsRotate.StopRotation();

		OnReturnToPool();
	}

	public void OnReturnToPool()
	{
		_pool.Release(this);
	}

	private void OnCrackedEnd()
	{
		ReturnToPool();
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

	private void DamageReceived()
	{
		_damage.DisableCollisions();
		_rigidbody.linearVelocity = Vector2.zero;

		_physicsRotate.StopRotation();
		_animator.Crack();
	}

	private void OnDamageReceived(SimpleDamage target, DamageData damageData)
	{
		DamageReceived();
	}

	private void OnStatusApplied(SimpleDamage target, DamageType statusType)
	{
		DamageReceived();
	}

	private void OnHitTarget(Collider2D collider, DamageData data)
	{
		DamageReceived();
	}
}
