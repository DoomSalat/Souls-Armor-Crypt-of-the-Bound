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

	private Rigidbody2D _rigidbody;
	private Collider2D _collider;
	private RigidbodyType2D _originalBodyType;
	private RigidbodyConstraints2D _originalConstraints;
	private bool _canBounce = true;
	private Sequence _bounceSequence;

	public bool IsBouncing => _isBouncing;

	public event System.Action OnBounceStarted;
	public event System.Action<float, Ease> OnBounceEnded;

	public void ResetBounceState()
	{
		_isBouncing = false;
		_canBounce = true;

		_bounceSequence?.Kill();
		DOTween.Kill(this);

		_rigidbody.bodyType = _originalBodyType;
		_rigidbody.constraints = _originalConstraints;
		_rigidbody.linearVelocity = Vector2.zero;
	}

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<Collider2D>();

		_originalBodyType = _rigidbody.bodyType;
		_originalConstraints = _rigidbody.constraints;

		_collider.isTrigger = true;
	}

	private void Start()
	{
		_particleSpawner?.SetSoulType(SoulType.Blue);
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

	public void UpdateSoulType(SoulType soulType)
	{
		_particleSpawner?.SetSoulType(soulType);
	}

	public void ManualBounce(Collider2D target)
	{
		if (_canBounce == false)
			return;

		BounceFromTarget(target);
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

		float rotationAmount = CalculateRotationAmount(wall, bounceDirection);

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

	private void BounceFromTarget(Collider2D target)
	{
		_isBouncing = true;
		_canBounce = false;

		OnBounceStarted?.Invoke();

		_bounceSequence?.Kill();
		DOTween.Kill(this);

		Vector2 bounceDirection = CalculateTargetBounceDirection(target);
		_lastBounceDirection = bounceDirection;
		float adaptiveBounceDistance = _baseBounceDistance;

		if (_spawnParticlesOnBounce && _particleSpawner != null)
		{
			Vector3 impactPoint = GetTargetImpactPoint(target);
			Vector3 targetNormal = CalculateTargetNormal(target);
			_particleSpawner.SpawnWallImpactEffect(impactPoint, targetNormal);
		}

		_rigidbody.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody.linearVelocity = Vector2.zero;

		Vector2 startPosition = _rigidbody.position;
		Vector2 targetPosition = startPosition + bounceDirection * adaptiveBounceDistance;

		_bounceSequence = DOTween.Sequence();

		Tween moveTween = _rigidbody.DOMove(targetPosition, _bounceDuration)
			.SetEase(_bounceEase);

		float rotationAmount = CalculateTargetRotationAmount(target, bounceDirection);

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

	private float CalculateRotationAmount(Collider2D wall, Vector2 bounceDirection)
	{
		Vector2 swordCenter = _collider.bounds.center;
		Vector2 impactPoint = wall.ClosestPoint(swordCenter);

		Vector2 impactVector = impactPoint - swordCenter;
		Vector2 localImpactVector = transform.InverseTransformDirection(impactVector);

		float rotationDirection;

		if (localImpactVector.y > 0)
		{
			rotationDirection = -1f;
		}
		else
		{
			rotationDirection = 1f;
		}

		return rotationDirection * _rotationSpeed;
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

	private Vector2 CalculateTargetBounceDirection(Collider2D target)
	{
		Vector2 targetCenter = target.bounds.center;
		Vector2 swordCenter = _collider.bounds.center;
		Vector2 directionFromTarget = (swordCenter - targetCenter).normalized;

		if (directionFromTarget == Vector2.zero)
		{
			float randomAngle = Random.Range(0f, MaxRotation) * Mathf.Deg2Rad;
			directionFromTarget = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
		}

		return directionFromTarget;
	}

	private float CalculateTargetRotationAmount(Collider2D target, Vector2 bounceDirection)
	{
		Vector2 swordCenter = _collider.bounds.center;
		Vector2 impactPoint = target.ClosestPoint(swordCenter);

		Vector2 impactVector = impactPoint - swordCenter;
		Vector2 localImpactVector = transform.InverseTransformDirection(impactVector);

		float rotationDirection;

		if (localImpactVector.y > 0)
		{
			rotationDirection = -1f;
		}
		else
		{
			rotationDirection = 1f;
		}

		return rotationDirection * _rotationSpeed;
	}

	private Vector3 GetTargetImpactPoint(Collider2D target)
	{
		Vector3 swordCenter = _collider.bounds.center;
		Vector3 closestPoint = target.ClosestPoint(swordCenter);

		return Vector3.Lerp(swordCenter, closestPoint, Half);
	}

	private Vector3 CalculateTargetNormal(Collider2D target)
	{
		Vector3 swordCenter = _collider.bounds.center;
		Vector3 targetCenter = target.bounds.center;

		Vector3 normal = (swordCenter - targetCenter).normalized;

		if (normal == Vector3.zero)
		{
			normal = _lastBounceDirection.normalized;
		}

		return normal;
	}
}