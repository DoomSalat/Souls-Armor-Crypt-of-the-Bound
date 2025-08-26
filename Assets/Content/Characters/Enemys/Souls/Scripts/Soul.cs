using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using SpawnerSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Soul : MonoBehaviour, ISpawnInitializable
{
	[Header("Components")]
	[SerializeField, Required] private MonoBehaviour _followLogic;
	[SerializeField, Required] private SoulAttractor _soulAttractor;
	[SerializeField, Required] private SoulAnimator _soulAnimator;
	[SerializeField, Required] private ObjectDepthZ _objectDepthZ;

	[Header("Hit")]
	[SerializeField, Required] private HitBox _hitBox;
	[SerializeField, Required] private HurtBox _hurtBox;
	[SerializeField, Required] private EnemyDamage _soulDamage;
	[SerializeField, Required] private KnockbackReceiver _knockbackReceiver;
	[SerializeField, MinValue(0)] private float _knockbackForce = 10f;

	private Rigidbody2D _rigidbody;
	private Collider2D _collider;
	private IFollower _follower;

	private bool _isAttracted = false;
	private bool _isEndAttraction = false;
	private event System.Action _attractionCompleted;

	private WaitUntil _isKnockedBackWaitUntil;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<Collider2D>();
		_follower = _followLogic.GetComponent<IFollower>();

		_soulDamage.Initialize(_collider, _hitBox, _hurtBox);

		_isKnockedBackWaitUntil = new WaitUntil(() => _knockbackReceiver.IsKnockedBack == false);
	}

	private void OnEnable()
	{
		_soulAttractor.AttractionCompleted += OnAttractionCompletedInternal;

		_hitBox.Hitted += OnHitTarget;

		_soulAnimator.DeathExplosionStarted += OnStartDeathExplosion;
		_soulAnimator.DeathExplosionEnded += OnEndDeathExplosion;

		_soulDamage.DeathRequested += OnDeathRequested;
	}

	private void OnDisable()
	{
		_soulAttractor.AttractionCompleted -= OnAttractionCompletedInternal;

		_hitBox.Hitted -= OnHitTarget;

		_soulAnimator.DeathExplosionStarted -= OnStartDeathExplosion;
		_soulAnimator.DeathExplosionEnded -= OnEndDeathExplosion;

		_soulDamage.DeathRequested -= OnDeathRequested;
	}

	private void Update()
	{
		if (_isAttracted)
		{
			if (_isEndAttraction == false)
			{
				_soulAnimator.AbsorptionDirection(_rigidbody.linearVelocity);
			}
			else
			{
				_soulAnimator.AbsorptionDirection(Vector2.up);
			}
		}
	}

	private void FixedUpdate()
	{
		if (_soulDamage.IsDead == false && _knockbackReceiver.IsKnockedBack == false)
		{
			_follower.TryFollow();
		}
	}

	private void OnValidate()
	{
		if (_followLogic != null)
		{
			if (_followLogic is IFollower follower)
			{
				_follower = follower;
			}
			else
			{
				_follower = null;
				_followLogic = null;
				Debug.LogError($"[{nameof(Soul)}] TargetFollower is not a IFollower on {gameObject.name}!");
			}
		}
	}

	public void SpawnInitializate(bool enableCollisions = true)
	{
		_soulAnimator.Reset();
		_rigidbody.linearVelocity = Vector2.zero;
		_objectDepthZ.enabled = true;

		_soulDamage.ClearStatus();
		_soulDamage.ResetDeathState();

		if (enableCollisions)
		{
			_soulDamage.EnableCollisions();
		}

		_soulAttractor.StopAttraction();
		_isAttracted = false;
		_isEndAttraction = false;
		_attractionCompleted = null;

		_follower.EnableMovement();
	}

	public void ApplySpawnKnockback(Vector2 direction, float force)
	{
		_soulDamage.DisableCollisions();
		_follower.DisableMovement();

		_knockbackReceiver.ApplyKnockback(direction, force);
		StartCoroutine(EnableCollisionsAfterKnockback());
	}

	private IEnumerator EnableCollisionsAfterKnockback()
	{
		while (_knockbackReceiver.IsKnockedBack)
		{
			yield return null;
		}

		if (_soulDamage.IsDead == false && _isAttracted == false)
		{
			_soulDamage.EnableCollisions();
			_follower.EnableMovement();
		}
	}

	public void StartAttraction(Transform target, Action AttractionCompleted)
	{
		_objectDepthZ.enabled = false;
		transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z);

		_attractionCompleted = AttractionCompleted;

		_soulDamage.DisableCollisions();
		_follower.DisableMovement();
		_soulDamage.ClearStatus();

		_isAttracted = true;
		_soulAnimator.AbsorptionDirection(_rigidbody.linearVelocity);

		_soulAttractor.StartAttraction(target);
	}

	public void OnAbsorptionCompleted()
	{
		ForceDeath();
	}

	private void OnAttractionCompletedInternal()
	{
		_isEndAttraction = true;

		_attractionCompleted?.Invoke();
		_attractionCompleted = null;
	}

	private void OnHitTarget(Collider2D targetCollider, DamageData damageData)
	{
		if (_soulDamage.IsDead || _isAttracted)
			return;

		_follower.PauseMovement();

		Vector2 directionFromPlayer = (transform.position - targetCollider.transform.position).normalized;
		_knockbackReceiver.ApplyKnockback(directionFromPlayer, _knockbackForce);

		StartCoroutine(DisableTargetFollowerTemporarily());
	}

	private IEnumerator DisableTargetFollowerTemporarily()
	{
		yield return _isKnockedBackWaitUntil;

		if (_soulDamage.IsDead == false && _isAttracted == false)
		{
			_follower.ResumeMovement();
		}
	}

	public void ForceDeath()
	{
		_soulDamage.DisableCollisions();
		_follower.DisableMovement();
		_soulAnimator.PlayDeath();
	}

	private void OnDeathRequested(DamageData damageData)
	{
		_soulAnimator.PlayDeath();
		_soulDamage.DisableCollisions();
		_knockbackReceiver.ApplyKnockback(damageData.KnockbackDirection, damageData.KnockbackForce);
	}

	private void OnStartDeathExplosion()
	{
		_rigidbody.linearVelocity = Vector2.zero;
	}

	private void OnEndDeathExplosion()
	{
		_rigidbody.linearVelocity = Vector2.zero;
		_soulAnimator.Reset();
		_soulDamage.CompleteDeath();
	}
}
