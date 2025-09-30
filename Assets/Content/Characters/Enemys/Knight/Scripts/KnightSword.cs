using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody2D))]
public class KnightSword : MonoBehaviour
{
	private const float MinDistanceThreshold = 0.1f;
	private const float RotationPrecision = 0.5f;
	private const float FullRotationDegrees = 180f;
	private const float PhysicsSqrtMultiplier = 2f;

	[Header("Sword Settings")]
	[SerializeField, MinValue(0)] private float _orbitRadius = 2f;
	[SerializeField, MinValue(0)] private float _blockingRange = 1.5f;

	[Header("Components")]
	[SerializeField, Required] private Transform _pivotPoint;
	[SerializeField, Required] private Transform _swordBlockPoint;
	[SerializeField, Required] private ParticleSystem _particleSystemSoul;
	[Space]
	[SerializeField] private Collider2D[] _colliders;
	[SerializeField, Required] private HitBox _hitBox;
	[SerializeField, Required] private SwordWallBounce _swordWallBounce;

	[Header("Physics")]
	[SerializeField, MinValue(0)] private float _moveSpeed = 15f;
	[SerializeField, MinValue(0)] private float _moveAcceleration = 30f;
	[SerializeField, MinValue(0)] private float _arrivalThreshold = 0.05f;
	[SerializeField, Range(0f, 1f)] private float _minForceMultiplier = 0.1f;
	[Space]
	[SerializeField, MinValue(0)] private float _rotationSpeed = 360f;
	[SerializeField, MinValue(0)] private float _rotationAcceleration = 720f;
	[SerializeField, MinValue(0)] private float _maxAngularVelocity = 720f;
	[Space]
	[SerializeField, MinValue(0)] private float _rotationSlowdownAngle = 15f;
	[SerializeField, Range(0f, 1f)] private float _rotationSlowdownStrength = 0.1f;

	[Header("Debug")]
	[SerializeField] private Transform _debugTargetPlayer;
	[SerializeField] private Sword _debugPlayerSword;
	[Button("Initialize Debug Mode")]
	private void InitializeDebugModeButton()
	{
		if (Application.isPlaying)
		{
			InitializeDebugMode();
		}
		else
		{
			_debugTargetPlayer = FindFirstObjectByType<Player>().transform;
			_debugPlayerSword = FindFirstObjectByType<Sword>();
		}
	}

	private Rigidbody2D _rigidbody;
	private Transform _target;
	private Sword _playerSword;

	private bool _isControllEnabled = false;
	private bool _isBlockingMode = false;
	private bool _isRecoveringFromBounce = false;
	private Vector2 _orbitCenter;
	private Vector2 _targetPosition;
	private float _targetRotation;
	private Vector2 _bestPosition;
	private bool _isTrackingPlayer = true;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	private void OnEnable()
	{
		_hitBox.Hitted += OnHitBoxHitted;
		_swordWallBounce.OnBounceStarted += OnBounceStarted;
		_swordWallBounce.OnBounceEnded += OnBounceEnded;
	}

	private void OnDisable()
	{
		_hitBox.Hitted -= OnHitBoxHitted;
		_swordWallBounce.OnBounceStarted -= OnBounceStarted;
		_swordWallBounce.OnBounceEnded -= OnBounceEnded;
	}

	private void Update()
	{
		if (_isControllEnabled && _target != null && !_isRecoveringFromBounce)
		{
			UpdateSwordTargets();
			CheckForPlayerSwordBlocking();
		}
	}

	private void FixedUpdate()
	{
		if (_isControllEnabled && _target != null && !_isRecoveringFromBounce)
		{
			ApplyPhysicsMovement();
		}
	}

	public void Enable(Transform target)
	{
		gameObject.SetActive(true);

		foreach (var collider in _colliders)
		{
			if (collider != null)
				collider.enabled = true;
		}

		_rigidbody.simulated = true;

		if (!gameObject.activeInHierarchy)
		{
			gameObject.SetActive(true);
		}

		_target = target;
		_isControllEnabled = true;
		_orbitCenter = _pivotPoint.position;

		_particleSystemSoul.Play();
	}

	public void Disable()
	{
		_rigidbody.linearVelocity = Vector2.zero;
		_rigidbody.angularVelocity = 0f;
		_rigidbody.simulated = false;

		foreach (var collider in _colliders)
		{
			if (collider != null)
				collider.enabled = false;
		}

		_target = null;
		_isControllEnabled = false;
		_isBlockingMode = false;

		gameObject.SetActive(false);
	}

	public void StopLogic()
	{
		_rigidbody.linearVelocity = Vector2.zero;
		_rigidbody.angularVelocity = 0f;

		_particleSystemSoul.Stop();

		_target = null;
		_isControllEnabled = false;
		_isBlockingMode = false;
		_isRecoveringFromBounce = false;
	}

	private void OnHitBoxHitted(Collider2D other, DamageData damageData)
	{
		_swordWallBounce.ManualBounce(other);
	}

	private void OnBounceStarted()
	{
		_isRecoveringFromBounce = true;
	}

	private void OnBounceEnded(float recoveryTime, DG.Tweening.Ease recoveryEase)
	{
		_isRecoveringFromBounce = false;
	}

	public void InitializePlayerSword(Sword playerSword)
	{
		_playerSword = playerSword;
	}

	private void UpdateSwordTargets()
	{
		_orbitCenter = _pivotPoint.position;

		if (_isBlockingMode)
		{
			UpdateBlockingTargets();
		}
		else
		{
			UpdateAttackTargets();
		}
	}

	private void UpdateAttackTargets()
	{
		_isTrackingPlayer = true;

		Vector2 directionToPlayer = (_target.position - (Vector3)_orbitCenter).normalized;
		Vector2 bestDirection = directionToPlayer;

		Vector2 newBestPosition = _orbitCenter + bestDirection * _orbitRadius;

		Vector2 positionDifference = newBestPosition - _bestPosition;
		float sqrDistanceToNewPosition = positionDifference.sqrMagnitude;
		Vector2 currentPositionDifference = _bestPosition - (Vector2)transform.position;
		float sqrCurrentDistanceToTarget = currentPositionDifference.sqrMagnitude;

		float currentDistanceToTarget = Mathf.Sqrt(sqrCurrentDistanceToTarget);
		float sensitivityThreshold = _arrivalThreshold * (currentDistanceToTarget > _arrivalThreshold ? 0.2f : 1.0f);

		float distanceToNewPosition = Mathf.Sqrt(sqrDistanceToNewPosition);
		if (distanceToNewPosition > sensitivityThreshold)
		{
			_bestPosition = newBestPosition;
			_targetPosition = _bestPosition;
		}

		Vector2 directionFromCenterToPlayer = (_target.position - (Vector3)_orbitCenter).normalized;
		_targetRotation = Mathf.Atan2(directionFromCenterToPlayer.y, directionFromCenterToPlayer.x) * Mathf.Rad2Deg;
	}

	private void UpdateBlockingTargets()
	{
		_isTrackingPlayer = false;

		if (_playerSword == null)
		{
			UpdateAttackTargets();
			return;
		}

		Vector2 directionToPlayerSword = (_playerSword.transform.position - (Vector3)_orbitCenter).normalized;
		Vector2 blockPointPosition = _orbitCenter + directionToPlayerSword * _orbitRadius;

		Vector2 currentBlockOffset = (_swordBlockPoint.position - (Vector3)transform.position);
		Vector2 handlePosition = blockPointPosition - currentBlockOffset;

		float distanceFromCenter = (handlePosition - _orbitCenter).magnitude;
		if (Mathf.Abs(distanceFromCenter - _orbitRadius) > 0.01f)
		{
			Vector2 directionFromCenter = (handlePosition - _orbitCenter).normalized;
			handlePosition = _orbitCenter + directionFromCenter * _orbitRadius;
		}

		Vector2 pivotToBlockDirection = (blockPointPosition - (Vector2)_pivotPoint.position).normalized;
		float pivotToBlockAngle = Mathf.Atan2(pivotToBlockDirection.y, pivotToBlockDirection.x) * Mathf.Rad2Deg;

		float currentAngle = transform.eulerAngles.z;
		float leftRotation = pivotToBlockAngle + 90f;
		float rightRotation = pivotToBlockAngle - 90f;

		float leftDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, leftRotation));
		float rightDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, rightRotation));

		_targetRotation = leftDifference < rightDifference ? leftRotation : rightRotation;
		_bestPosition = handlePosition;
		_targetPosition = _bestPosition;
	}

	private void ApplyPhysicsMovement()
	{
		ApplyMovement();
		ApplyRotation();
	}

	private void ApplyMovement()
	{
		Vector2 positionDifference = _targetPosition - (Vector2)transform.position;
		float sqrDistanceToTarget = positionDifference.sqrMagnitude;
		Vector2 directionToTarget = positionDifference.normalized;

		float forceMultiplier = CalculateMovementForceMultiplier(sqrDistanceToTarget);

		float targetSpeed = CalculateTargetSpeed(sqrDistanceToTarget, forceMultiplier);
		Vector2 targetVelocity = directionToTarget * targetSpeed;

		ApplySmoothMovement(targetVelocity, forceMultiplier);
	}

	private void ApplyRotation()
	{
		float currentAngle = transform.eulerAngles.z;
		float angleDifference = Mathf.DeltaAngle(currentAngle, _targetRotation);

		if (Mathf.Abs(angleDifference) < RotationPrecision)
		{
			_rigidbody.angularVelocity = 0f;
			return;
		}

		float targetAngularVelocity = CalculateTargetAngularVelocity(angleDifference);

		ApplySmoothRotation(targetAngularVelocity);
	}

	private float CalculateMovementForceMultiplier(float sqrDistanceToTarget)
	{
		float distanceToTarget = Mathf.Sqrt(sqrDistanceToTarget);
		float forceMultiplier = Mathf.Clamp01(distanceToTarget / _arrivalThreshold);

		if (distanceToTarget <= _arrivalThreshold * MinDistanceThreshold)
		{
			forceMultiplier = _minForceMultiplier;
		}

		return forceMultiplier;
	}

	private float CalculateTargetSpeed(float sqrDistanceToTarget, float forceMultiplier)
	{
		float distanceToTarget = Mathf.Sqrt(sqrDistanceToTarget);
		float maxSpeedAtDistance = Mathf.Sqrt(PhysicsSqrtMultiplier * _moveAcceleration * distanceToTarget);

		return Mathf.Min(_moveSpeed * forceMultiplier, maxSpeedAtDistance);
	}

	private void ApplySmoothMovement(Vector2 targetVelocity, float forceMultiplier)
	{
		Vector2 currentVelocity = _rigidbody.linearVelocity;
		Vector2 velocityDifference = targetVelocity - currentVelocity;
		float appliedForce = _moveAcceleration * forceMultiplier;

		if (velocityDifference.magnitude <= appliedForce * Time.fixedDeltaTime)
		{
			_rigidbody.linearVelocity = targetVelocity;
		}
		else
		{
			Vector2 velocityChange = velocityDifference.normalized * appliedForce * Time.fixedDeltaTime;
			_rigidbody.linearVelocity = currentVelocity + velocityChange;
		}
	}

	private float CalculateTargetAngularVelocity(float angleDifference)
	{
		float baseAngularVelocity = Mathf.Sign(angleDifference) * _rotationSpeed * Mathf.Clamp01(Mathf.Abs(angleDifference) / FullRotationDegrees);
		float slowdownFactor = CalculateRotationSlowdownFactor(Mathf.Abs(angleDifference));

		return baseAngularVelocity * slowdownFactor;
	}

	private float CalculateRotationSlowdownFactor(float angleDifference)
	{
		if (angleDifference >= _rotationSlowdownAngle)
		{
			return 1f;
		}

		float slowdownProgress = angleDifference / _rotationSlowdownAngle;
		return Mathf.Lerp(_rotationSlowdownStrength, 1f, slowdownProgress);
	}

	private void ApplySmoothRotation(float targetAngularVelocity)
	{
		float currentAngularVelocity = _rigidbody.angularVelocity;
		float speedChange = _rotationAcceleration * Time.fixedDeltaTime;

		if (Mathf.Abs(targetAngularVelocity - currentAngularVelocity) <= speedChange)
		{
			_rigidbody.angularVelocity = targetAngularVelocity;
		}
		else
		{
			float velocityChange = Mathf.Sign(targetAngularVelocity - currentAngularVelocity) * speedChange;
			float newVelocity = currentAngularVelocity + velocityChange;
			_rigidbody.angularVelocity = Mathf.Clamp(newVelocity, -_maxAngularVelocity, _maxAngularVelocity);
		}
	}

	private void CheckForPlayerSwordBlocking()
	{
		if (_playerSword == null)
			return;

		Vector2 directionToPlayerSword = _playerSword.transform.position - (Vector3)_orbitCenter;
		float sqrDistanceToPlayerSword = directionToPlayerSword.sqrMagnitude;
		float sqrBlockingRange = _blockingRange * _blockingRange;

		bool shouldBlock = sqrDistanceToPlayerSword <= sqrBlockingRange && _playerSword.gameObject.activeInHierarchy;

		if (shouldBlock != _isBlockingMode)
		{
			_isBlockingMode = shouldBlock;
		}
	}

	private void InitializeDebugMode()
	{
		if (_debugTargetPlayer != null && _debugPlayerSword != null)
		{
			_playerSword = _debugPlayerSword;
			Enable(_debugTargetPlayer);
		}
		else
		{
			Debug.LogWarning($"[{nameof(KnightSword)}] Debug mode enabled but _debugTarget or _debugPlayerSword is null!");
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_pivotPoint != null)
		{
			Gizmos.color = Color.blue;
			DrawWireCircle(_pivotPoint.position, _orbitRadius);

			Gizmos.color = Color.red;
			DrawWireCircle(_pivotPoint.position, _blockingRange);

			Gizmos.color = _isBlockingMode ? Color.yellow : Color.green;
			Gizmos.DrawRay(transform.position, transform.right * 1f);

			if (_swordBlockPoint != null)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere(_swordBlockPoint.position, 0.1f);
			}

			if (_target != null)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawLine(transform.position, _target.position);

				if (_playerSword != null)
				{
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine(transform.position, _playerSword.transform.position);
				}

				if (_isTrackingPlayer)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(_pivotPoint.position, _target.position);
				}
				else if (_playerSword != null)
				{
					Gizmos.color = Color.orange;
					Gizmos.DrawLine(_pivotPoint.position, _playerSword.transform.position);
				}

				if (_bestPosition != Vector2.zero)
				{
					Gizmos.color = Color.magenta;
					DrawWireSquare(_bestPosition, 0.2f);

					if (_isBlockingMode && _swordBlockPoint != null)
					{
						Vector2 directionToPlayerSword = (_playerSword.transform.position - (Vector3)_orbitCenter).normalized;
						Vector2 blockPointTarget = _orbitCenter + directionToPlayerSword * _orbitRadius;

						Gizmos.color = Color.blue;
						Gizmos.DrawSphere(blockPointTarget, 0.08f);

						Gizmos.color = Color.cyan;
						Gizmos.DrawLine(_bestPosition, blockPointTarget);

						Gizmos.color = Color.red;
						Gizmos.DrawRay(blockPointTarget, directionToPlayerSword * 0.5f);
					}
				}

				if (_targetPosition != Vector2.zero)
				{
					Gizmos.color = Color.yellow;
					Gizmos.DrawSphere(_targetPosition, 0.15f);

					Vector2 lookDirection = new Vector2(Mathf.Cos(_targetRotation * Mathf.Deg2Rad), Mathf.Sin(_targetRotation * Mathf.Deg2Rad));
					Gizmos.color = Color.red;
					Gizmos.DrawRay(_targetPosition, lookDirection * 0.5f);
				}
			}
		}
	}

	private void DrawWireSquare(Vector3 center, float size)
	{
		float halfSize = size * 0.5f;
		Vector3 topLeft = center + new Vector3(-halfSize, halfSize, 0);
		Vector3 topRight = center + new Vector3(halfSize, halfSize, 0);
		Vector3 bottomLeft = center + new Vector3(-halfSize, -halfSize, 0);
		Vector3 bottomRight = center + new Vector3(halfSize, -halfSize, 0);

		Gizmos.DrawLine(topLeft, topRight);
		Gizmos.DrawLine(topRight, bottomRight);
		Gizmos.DrawLine(bottomRight, bottomLeft);
		Gizmos.DrawLine(bottomLeft, topLeft);
	}

	private void DrawWireCircle(Vector3 center, float radius)
	{
		const int segments = 32;
		float angleStep = 360f / segments;
		Vector3 previousPoint = center + Vector3.right * radius;

		for (int i = 1; i <= segments; i++)
		{
			float angle = i * angleStep * Mathf.Deg2Rad;
			Vector3 currentPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
			Gizmos.DrawLine(previousPoint, currentPoint);
			previousPoint = currentPoint;
		}
	}
}