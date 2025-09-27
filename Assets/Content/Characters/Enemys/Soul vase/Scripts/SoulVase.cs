using UnityEngine;
using Sirenix.OdinInspector;
using SpawnerSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SoulVase : MonoBehaviour, ISpawnInitializable
{
	[Header("Components")]
	[SerializeField, Required] private SoulSpawnerRequested _soulSpawner;
	[SerializeField, Required] private EnemyDamage _enemyDamage;
	[SerializeField, Required] private SoulVaseAnimator _soulVaseAnimator;
	[SerializeField, Required] private MonoBehaviour _followerLogic;
	[SerializeField, Required] private KnockbackReceiver _knockbackReceiver;
	[SerializeField, Required] private CreatureFlip _creatureFlip;

	[Header("Hit")]
	[SerializeField, Required] private HitBox _hitBox;
	[SerializeField, Required] private HurtBox _hurtBox;
	[SerializeField, MinValue(0)] private float _knockbackForce = 10f;

	private Rigidbody2D _rigidbody;
	private Collider2D _collider;
	private IFollower _follower;
	private bool _hasSpawnedSoul = false;

	private WaitUntil _isKnockedBackWaitUntil;

	public bool IsBusy => _enemyDamage.IsDead || _knockbackReceiver.IsKnockedBack;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<Collider2D>();

		_follower = _followerLogic.GetComponent<IFollower>();

		_enemyDamage.Initialize(_collider, _hitBox, _hurtBox);

		_isKnockedBackWaitUntil = new WaitUntil(() => !_knockbackReceiver.IsKnockedBack);
	}

	private void OnEnable()
	{
		_hitBox.Hitted += OnHitTarget;
		_enemyDamage.DeathRequested += OnDeathRequested;
		_soulVaseAnimator.Ended += OnDeathAnimationComplete;
	}

	private void OnDisable()
	{
		_hitBox.Hitted -= OnHitTarget;
		_enemyDamage.DeathRequested -= OnDeathRequested;
		_soulVaseAnimator.Ended -= OnDeathAnimationComplete;
	}

	private void FixedUpdate()
	{
		if (_enemyDamage.IsDead == false)
		{
			_follower.TryFollow();
			UpdateFlipDirection();
		}
	}

	private void OnValidate()
	{
		if (_followerLogic != null)
		{
			if (_followerLogic is IFollower)
			{
				_follower = _followerLogic.GetComponent<IFollower>();
			}
			else
			{
				_followerLogic = null;
				Debug.LogError($"[{nameof(SoulVase)}] SoulVaseMovement is not a {nameof(IFollower)} on {gameObject.name}!");
			}
		}
	}

	private void UpdateFlipDirection()
	{
		Vector2 currentDirection = _follower.Direction;

		if (currentDirection.x > 0)
		{
			_creatureFlip.FlipRight();
		}
		else if (currentDirection.x < 0)
		{
			_creatureFlip.FlipLeft();
		}
	}

	public void SpawnInitializate(bool enableCollisions = true)
	{
		_rigidbody.linearVelocity = Vector2.zero;

		_soulVaseAnimator.Reset();
		_enemyDamage.EnableCollisions();
		_enemyDamage.ClearStatus();
		_enemyDamage.ResetDeathState();
		_hasSpawnedSoul = false;

		_follower.EnableMovement();
	}

	private void OnDeathRequested(DamageData damageData)
	{
		if (!_hasSpawnedSoul)
		{
			_hasSpawnedSoul = true;
			_soulVaseAnimator.PlayDeath();
			_soulSpawner.RequestSoulSpawn(damageData, transform.position);
		}

		_follower.DisableMovement();
		_enemyDamage.DisableCollisions();
	}

	private void OnDeathAnimationComplete()
	{
		_enemyDamage.CompleteDeath();
	}

	private void OnHitTarget(Collider2D targetCollider, DamageData damageData)
	{
		if (_enemyDamage.IsDead)
			return;

		_follower.PauseMovement();

		Vector2 directionFromPlayer = (transform.position - targetCollider.transform.position).normalized;

		_knockbackReceiver.ApplyKnockback(directionFromPlayer, _knockbackForce);

		StartCoroutine(DisableTargetFollowerTemporarily());
	}

	private IEnumerator DisableTargetFollowerTemporarily()
	{
		yield return _isKnockedBackWaitUntil;

		if (_enemyDamage.IsDead == false)
		{
			_follower.ResumeMovement();
		}
	}
}
