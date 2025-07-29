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
	[SerializeField] private TargetFollower _targetFollower;

	[Header("General")]
	[SerializeField, MinValue(0)] private float _speedSeek = 0.5f;
	[SerializeField, MinValue(0)] private float _lifeTime = 2f;
	[SerializeField, MinValue(0)] private float _fadeOutDuration = 0.5f;
	[SerializeField, MinValue(0)] private float _colliderSeekRadius = 4;
	[SerializeField, MinValue(0)] private float _colliderShrinkAmount = 0.3f;

	[Header("Trail Settings")]
	[SerializeField, MinValue(0)] private float _initialTrailTime = 0.6f;
	[SerializeField, MinValue(0)] private float _followingTrailTime = 0.2f;
	[SerializeField, MinValue(0)] private float _trailTransitionDuration = 0.1f;

	private int _damageAmount = 1;
	private HurtBox _targetHurtBox;
	private Transform _targetTransform;
	private float _currentLifeTime;
	private bool _isFollowingTarget;
	private bool _isDying;
	private Vector2 _initialDirection;
	private float _originalColliderRadius;
	private Coroutine _trailTransitionCoroutine;

	private void Update()
	{
		if (_isDying)
			return;

		_currentLifeTime += Time.deltaTime;

		if (_isFollowingTarget == false && _currentLifeTime >= _lifeTime)
		{
			Die();
			return;
		}

		if (_isFollowingTarget && _targetTransform != null)
		{
			if (_targetTransform.gameObject.activeInHierarchy)
			{
				_targetFollower.TryFollow(_targetTransform);
			}
			else
			{
				Die();
			}
		}
	}

	private void FixedUpdate()
	{
		if (_isDying || _isFollowingTarget)
			return;

		_rigidbody.linearVelocity = _initialDirection * _speedSeek;
	}

	public void Initialize(Vector2 direction)
	{
		_targetFollower.SetMoveState(false);
		_targetFollower.StopMovement();

		_initialDirection = direction.normalized;

		_targetHurtBox = null;
		_targetTransform = null;

		_rigidbody.linearVelocity = _initialDirection * _speedSeek;
		_trail.Clear();
		_currentLifeTime = 0f;
		_isFollowingTarget = false;
		_isDying = false;
		_targetHurtBox = null;
		_targetTransform = null;

		_circleCollider.radius = _colliderSeekRadius;
		_trail.time = _initialTrailTime;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (_isDying)
			return;

		if (collision.TryGetComponent(out HurtBox hurtBox) && hurtBox.TryGetComponent(out FactionTag factionTag))
		{
			if (factionTag.Faction == Faction.Enemy)
			{
				if (_isFollowingTarget == false)
				{
					_targetFollower.SetMoveState(true);
					StartFollowingTarget(hurtBox);

					_circleCollider.radius = _originalColliderRadius - _colliderShrinkAmount;
				}
				else if (hurtBox == _targetHurtBox)
				{
					ReachTarget();
				}
			}
		}
	}

	private void StartFollowingTarget(HurtBox hurtBox)
	{
		_targetHurtBox = hurtBox;
		_targetTransform = hurtBox.transform;
		_isFollowingTarget = true;

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

	private void ReachTarget()
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
		if (_isFollowingTarget)
			_targetFollower.TryFollow(null);

		_isFollowingTarget = false;

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

		gameObject.SetActive(false);
	}
}
