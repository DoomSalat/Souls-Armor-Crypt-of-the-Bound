using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class GreenPoisonSeeker : MonoBehaviour
{
	[SerializeField] private Rigidbody2D _rigidbody;
	[SerializeField] private CircleCollider2D _circleCollider;
	[Space]
	[SerializeField] private TrailRenderer _trail;
	[SerializeField] private MonoBehaviour _targetFollowerObject;

	[Header("General")]
	[SerializeField, MinValue(0)] private float _speedSeek = 0.5f;
	[SerializeField, MinValue(0)] private float _lifeTime = 2f;
	[SerializeField, MinValue(0)] private float _lifeTimeAttack = 5f;
	[SerializeField, MinValue(0)] private float _fadeOutDuration = 0.5f;

	[Header("Collider")]
	[SerializeField, MinValue(0)] private float _colliderSeekRadius = 4;
	[SerializeField, MinValue(0)] private float _colliderShrinkAmount = 0.3f;

	[Header("Trail Settings")]
	[SerializeField, MinValue(0)] private float _initialTrailTime = 0.6f;
	[SerializeField, MinValue(0)] private float _followingTrailTime = 0.2f;
	[SerializeField, MinValue(0)] private float _trailTransitionDuration = 0.1f;

	private IFollower _targetFollower;
	private int _damageAmount = 1;
	private HurtBox _targetHurtBox;
	private Transform _targetTransform;
	private float _currentLifeTime;
	private float _currentEndLifeTime;
	private bool _isAttackFollowingTarget;
	private bool _isDying;
	private Vector2 _initialDirection;
	private Coroutine _trailTransitionCoroutine;

	public event System.Action<GreenPoisonSeeker> SeekerDestroyed;

	private void Awake()
	{
		_targetFollower = _targetFollowerObject as IFollower;
	}

	private void OnEnable()
	{
		_targetFollower.TargetReached += OnTargetMoveReached;
	}

	private void OnDisable()
	{
		_targetFollower.TargetReached -= OnTargetMoveReached;
	}

	private void Update()
	{
		if (_isDying)
			return;

		_currentLifeTime += Time.deltaTime;

		if (_isAttackFollowingTarget == false && _currentLifeTime >= _currentEndLifeTime)
		{
			Die();
			return;
		}

		if (_isAttackFollowingTarget && _targetTransform != null)
		{
			if (_targetTransform.gameObject.activeInHierarchy == false)
			{
				Die();
			}
		}
	}

	private void FixedUpdate()
	{
		if (CanFollow())
		{
			_targetFollower.TryFollow();
		}
		else
		{
			_rigidbody.linearVelocity = _initialDirection * _speedSeek;
		}
	}

	private void OnValidate()
	{
		if (_targetFollowerObject != null)
		{
			if (_targetFollowerObject.TryGetComponent(out IFollower follower))
			{
				_targetFollower = follower;
			}
			else
			{
				Debug.LogError($"[{nameof(GreenPoisonSeeker)}] Target follower is not a IFollower on {_targetFollowerObject.name}!");
				_targetFollowerObject = null;
			}
		}
	}

	private bool CanFollow()
	{
		return _isDying == false &&
				_targetTransform != null &&
				_targetTransform.gameObject.activeInHierarchy == true;
	}

	public void Initialize(Vector2 direction, Transform target = null)
	{
		_targetFollower.DisableMovement();
		_targetFollower.SetTarget(null);

		_initialDirection = direction.normalized;

		_targetHurtBox = null;
		_targetTransform = null;

		_rigidbody.linearVelocity = _initialDirection * _speedSeek;
		_trail.Clear();
		_currentLifeTime = 0f;
		_currentEndLifeTime = _lifeTime;
		_isAttackFollowingTarget = false;
		_isDying = false;
		_targetHurtBox = null;

		_circleCollider.radius = _colliderSeekRadius;
		_trail.time = _initialTrailTime;

		if (target != null)
		{
			_targetFollower.SetTarget(target);
			_targetTransform = target;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (_isDying)
			return;

		if (collision.TryGetComponent(out HurtBox hurtBox) && hurtBox.TryGetComponent(out FactionTag factionTag))
		{
			if (factionTag.IsTagged(Faction.Enemy))
			{
				if (_isAttackFollowingTarget == false)
				{
					StartFollowingTarget(hurtBox);

					_circleCollider.radius = _colliderShrinkAmount;
				}
				else if (hurtBox == _targetHurtBox)
				{
					ReachFollowTarget();
				}
			}
		}
	}

	private void StartFollowingTarget(HurtBox hurtBox)
	{
		_targetHurtBox = hurtBox;
		_targetTransform = hurtBox.transform;
		_targetFollower.SetTarget(_targetTransform);
		_targetFollower.EnableMovement();
		_isAttackFollowingTarget = true;
		_currentLifeTime = 0f;
		_currentEndLifeTime = _lifeTimeAttack;

		StartTrailTransition();
	}

	private void StartTrailTransition()
	{
		if (_trailTransitionCoroutine != null)
		{
			StopCoroutine(_trailTransitionCoroutine);
		}

		_trailTransitionCoroutine = StartCoroutine(TransitionTrailTime());
	}

	private IEnumerator TransitionTrailTime()
	{
		float startTime = _trail.time;
		float targetTime = _followingTrailTime;
		float elapsedTime = 0f;

		while (elapsedTime < _trailTransitionDuration)
		{
			elapsedTime += Time.deltaTime;
			float progress = elapsedTime / _trailTransitionDuration;

			_trail.time = Mathf.Lerp(startTime, targetTime, progress);

			yield return null;
		}

		_trail.time = targetTime;
	}

	private void OnTargetMoveReached()
	{
		if (_isAttackFollowingTarget == false)
		{
			Die();
		}
	}

	private void ReachFollowTarget()
	{
		if (_targetHurtBox != null)
		{
			_targetHurtBox.ApplyDamage(new DamageData(_damageAmount, DamageType.Poison, Vector2.zero, 0));
		}

		Die();
	}

	private void Die()
	{
		if (_isDying)
			return;

		_isDying = true;
		if (_isAttackFollowingTarget)
		{
			_targetFollower.SetTarget(null);
			_targetFollower.DisableMovement();
		}

		_isAttackFollowingTarget = false;

		if (_trailTransitionCoroutine != null)
		{
			StopCoroutine(_trailTransitionCoroutine);
			_trailTransitionCoroutine = null;
		}

		StartCoroutine(StopGradually());
	}

	private IEnumerator StopGradually()
	{
		float elapsedTime = 0f;
		Vector2 initialVelocity = _rigidbody.linearVelocity;

		while (elapsedTime < _fadeOutDuration)
		{
			elapsedTime += Time.deltaTime;
			float progress = elapsedTime / _fadeOutDuration;

			_rigidbody.linearVelocity = Vector2.Lerp(initialVelocity, Vector2.zero, progress);

			yield return null;
		}

		_rigidbody.linearVelocity = Vector2.zero;

		if (_trail != null)
		{
			while (_trail.time > 0)
			{
				_trail.time -= Time.deltaTime;
				yield return null;
			}
		}

		SeekerDestroyed?.Invoke(this);
	}
}
