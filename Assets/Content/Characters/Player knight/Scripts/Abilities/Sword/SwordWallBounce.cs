using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class SwordWallBounce : MonoBehaviour
{
	private const float MaxRotation = 360f;
	private const float Half = 0.5f;

	[SerializeField] private LayerMask _wallLayer;

	[Header("Bounce Settings")]
	[SerializeField, MinValue(0)] private float _baseBounceDistance = 2f;
	[SerializeField, MinValue(0)] private float _bounceMultiplier = 1.5f;
	[SerializeField, MinValue(0)] private float _maxBounceDistance = 5f;
	[SerializeField, MinValue(0)] private float _bounceDuration = 0.3f;
	[SerializeField] private Ease _bounceEase = Ease.OutBack;
	[SerializeField, MinValue(0)] private float _rotationSpeed = 10f;

	[Header("Spring Recovery")]
	[SerializeField, MinValue(0)] private float _springRecoveryTime = 0.5f;
	[SerializeField] private Ease _springRecoveryEase = Ease.InQuad;

	[Header("Particle Effects")]
	[SerializeField] private SpawnerBounceParticles _particleSpawner;
	[SerializeField] private bool _spawnParticlesOnBounce = true;

	[Header("Debug")]
	[ShowInInspector, ReadOnly] private bool _isBouncing;
	[ShowInInspector, ReadOnly] private Vector2 _lastBounceDirection;
	[ShowInInspector, ReadOnly] private float _lastPenetrationDepth;

	private Rigidbody2D _rigidbody;
	private Collider2D _collider;
	private RigidbodyType2D _originalBodyType;
	private RigidbodyConstraints2D _originalConstraints;
	private bool _canBounce = true;
	private Sequence _bounceSequence;

	public bool IsBouncing => _isBouncing;

	public event System.Action OnBounceStarted;
	public event System.Action<float, Ease> OnBounceEnded;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<Collider2D>();

		_originalBodyType = _rigidbody.bodyType;
		_originalConstraints = _rigidbody.constraints;

		_collider.isTrigger = true;
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (_canBounce == false || IsWallLayer(other) == false)
			return;

		BounceFromWall(other);
	}

	private void OnDestroy()
	{
		_bounceSequence?.Kill();
		DOTween.Kill(this);
	}

	private bool IsWallLayer(Collider2D collider)
	{
		int objectLayer = collider.gameObject.layer;

		return (_wallLayer.value & (1 << objectLayer)) != 0;
	}

	private void BounceFromWall(Collider2D wall)
	{
		_isBouncing = true;
		_canBounce = false;

		OnBounceStarted?.Invoke();

		_bounceSequence?.Kill();
		DOTween.Kill(this);

		Vector2 bounceDirection = CalculateBounceDirection(wall);
		_lastBounceDirection = bounceDirection;
		float penetrationDepth = CalculatePenetrationDepth(wall);
		float adaptiveBounceDistance = CalculateAdaptiveBounceDistance(penetrationDepth);
		_lastPenetrationDepth = penetrationDepth;

		if (_spawnParticlesOnBounce && _particleSpawner != null)
		{
			Vector3 impactPoint = GetImpactPoint(wall);
			Vector3 wallNormal = CalculateWallNormal(wall);
			_particleSpawner.SpawnWallImpactEffect(impactPoint, wallNormal);
		}

		_rigidbody.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody.linearVelocity = Vector2.zero;

		Vector2 startPosition = _rigidbody.position;
		Vector2 targetPosition = startPosition + bounceDirection * adaptiveBounceDistance;

		_bounceSequence = DOTween.Sequence();

		Tween moveTween = _rigidbody.DOMove(targetPosition, _bounceDuration)
			.SetEase(_bounceEase);

		float rotationDirection;
		if (Mathf.Abs(bounceDirection.y) > Mathf.Abs(bounceDirection.x))
		{
			rotationDirection = bounceDirection.y > 0 ? -1f : 1f;
		}
		else
		{
			rotationDirection = bounceDirection.x > 0 ? 1f : -1f;
		}

		float rotationAmount = rotationDirection * Random.Range(_rotationSpeed * Half, _rotationSpeed);

		Tween rotateTween = transform.DORotate(Vector3.forward * rotationAmount, _bounceDuration, RotateMode.LocalAxisAdd)
			.SetEase(_bounceEase);

		_bounceSequence.Append(moveTween)
			.Join(rotateTween)
			.OnComplete(() =>
			{
				_rigidbody.bodyType = _originalBodyType;
				_rigidbody.constraints = _originalConstraints;
				_rigidbody.linearVelocity = Vector2.zero;

				_canBounce = true;
				_isBouncing = false;
				OnBounceEnded?.Invoke(_springRecoveryTime, _springRecoveryEase);
			});

		_bounceSequence.Play();
	}

	private Vector2 CalculateBounceDirection(Collider2D wall)
	{
		Vector2 wallCenter = wall.bounds.center;
		Vector2 swordCenter = _collider.bounds.center;
		Vector2 directionFromWall = (swordCenter - wallCenter).normalized;

		if (directionFromWall == Vector2.zero)
		{
			float randomAngle = Random.Range(0f, MaxRotation) * Mathf.Deg2Rad;
			directionFromWall = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
		}

		return directionFromWall;
	}

	private float CalculatePenetrationDepth(Collider2D wall)
	{
		Bounds swordBounds = _collider.bounds;
		Bounds wallBounds = wall.bounds;

		float overlapX = Mathf.Max(0, Mathf.Min(swordBounds.max.x, wallBounds.max.x) - Mathf.Max(swordBounds.min.x, wallBounds.min.x));
		float overlapY = Mathf.Max(0, Mathf.Min(swordBounds.max.y, wallBounds.max.y) - Mathf.Max(swordBounds.min.y, wallBounds.min.y));

		return Mathf.Min(overlapX, overlapY);
	}

	private float CalculateAdaptiveBounceDistance(float penetrationDepth)
	{
		float adaptiveDistance = _baseBounceDistance + (penetrationDepth * _bounceMultiplier);
		return Mathf.Min(adaptiveDistance, _maxBounceDistance);
	}

	private Vector3 GetImpactPoint(Collider2D wall)
	{
		Vector3 swordCenter = _collider.bounds.center;
		Vector3 closestPoint = wall.ClosestPoint(swordCenter);

		return Vector3.Lerp(swordCenter, closestPoint, Half);
	}

	private Vector3 CalculateWallNormal(Collider2D wall)
	{
		Vector3 swordCenter = _collider.bounds.center;
		Vector3 wallCenter = wall.bounds.center;

		Vector3 normal = (swordCenter - wallCenter).normalized;

		if (normal == Vector3.zero)
		{
			normal = _lastBounceDirection.normalized;
		}

		return normal;
	}
}