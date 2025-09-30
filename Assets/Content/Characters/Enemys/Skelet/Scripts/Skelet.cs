using UnityEngine;
using Sirenix.OdinInspector;
using SpawnerSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Skelet : MonoBehaviour, ISpawnInitializable
{
	private const float MinVelocity = 0.01f;

	[Header("Components")]
	[SerializeField, Required] private MonoBehaviour _followLogic;
	[SerializeField, Required] private SkeletAnimator _animator;
	[SerializeField, Required] private BaseSkeletThrow _throw;
	[SerializeField, Required] private CreatureFlip _creatureFlip;
	[SerializeField, Required] private SoulSpawnerRequested _soulSpawner;

	[Header("Attack Settings")]
	[SerializeField, MinValue(0)] private float _attackDistance = 5f;
	[SerializeField, MinValue(0)] private float _optimalDistance = 4f;
	[SerializeField, MinValue(0)] private float _attackCooldown = 2f;

	[Header("Hit")]
	[SerializeField, Required] private HurtBox _hurtBox;
	[SerializeField, Required] private DamageSkelet _damage;

	[Header("Debug")]
	[SerializeField] private bool _debug = false;

	private Rigidbody2D _rigidbody;
	private Collider2D _collider;
	private IFollower _follower;
	private IGroupController _groupController;

	private AttackState _attackState = AttackState.Ready;
	private bool _hasSpawnedSoul = false;
	private Coroutine _attackCooldownRoutine;
	private WaitForSeconds _attackCooldownWait;

	private enum AttackState
	{
		Ready,
		Attacking,
		Cooldown
	}

	public bool IsCanAttack => _attackState != AttackState.Attacking && _damage.IsDead == false;

	public System.Action Attacked;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<Collider2D>();
		_follower = _followLogic.GetComponent<IFollower>();
		TryGetComponent(out _groupController);

		_damage.Initialize(_collider, null, _hurtBox);

		_attackCooldownWait = new WaitForSeconds(_attackCooldown);
	}

	private void OnEnable()
	{
		_animator.Throwed += OnThrowAttack;
		_animator.ThrowEnded += ThrowEnd;
		_animator.SpawnSoul += OnSpawnSoul;
		_animator.DeathEnded += OnDeathAnimationComplete;
		_damage.DeathRequested += OnDeathRequested;
	}

	private void OnDisable()
	{
		_animator.Throwed -= OnThrowAttack;
		_animator.ThrowEnded -= ThrowEnd;
		_animator.SpawnSoul -= OnSpawnSoul;
		_animator.DeathEnded -= OnDeathAnimationComplete;
		_damage.DeathRequested -= OnDeathRequested;
	}

	private void Update()
	{
		if (_follower.TryGetDistanceToTarget(out float distanceToTarget))
		{
			if (distanceToTarget <= _optimalDistance)
			{
				_follower.PauseMovement();
			}
			else
			{
				_follower.ResumeMovement();
			}
		}

		if (_attackState != AttackState.Attacking)
		{
			UpdateFlipDirection();
		}

		if (_damage.IsDead || _attackState == AttackState.Attacking)
		{
			return;
		}

		UpdateMoveAnimation();

		if (_follower.TryGetDistanceToTarget(out float distance))
		{
			UpdateAttack(distance);
		}
	}

	private void FixedUpdate()
	{
		if (_follower.IsMovementEnabled && _follower.TryGetDistanceToTarget(out float distance))
		{
			UpdateMovement(distance);
		}
	}

	public void StartAttack()
	{
		_attackState = AttackState.Attacking;

		_follower.PauseMovement();

		_animator.StopWalk();
		_animator.PlayThrow();
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

	private void UpdateAttack(float distanceToTarget)
	{
		if (_groupController != null && !_groupController.IsGroupLeader)
			return;

		if (_attackState == AttackState.Ready && distanceToTarget <= _attackDistance)
		{
			StartAttack();
		}
	}

	private void UpdateMovement(float distanceToTarget)
	{
		if (_attackState == AttackState.Attacking)
		{
			return;
		}

		_follower.TryFollow();

		if (_debug)
		{
			Debug.Log($"[{nameof(Skelet)}] UpdateMovement: {distanceToTarget}");
		}
	}

	private void UpdateMoveAnimation()
	{
		if (_follower.Velocity.magnitude > MinVelocity)
		{
			_animator.PlayWalk();
		}
		else
		{
			_animator.StopWalk();
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

		if (Application.isPlaying)
		{
			_attackCooldownWait = new WaitForSeconds(_attackCooldown);
		}
	}

	private void OnThrowAttack()
	{
		_throw.Attack(_follower.Target.position);
		Attacked?.Invoke();
	}

	private void ThrowEnd()
	{
		_attackState = AttackState.Cooldown;

		_follower.ResumeMovement();
		_animator.PlayWalk();

		if (_attackCooldownRoutine != null)
		{
			StopCoroutine(_attackCooldownRoutine);
		}

		_attackCooldownRoutine = StartCoroutine(AttackCooldownRoutine());
	}

	private IEnumerator AttackCooldownRoutine()
	{
		yield return _attackCooldownWait;

		_attackState = AttackState.Ready;
		_attackCooldownRoutine = null;
	}

	private void OnDeathRequested(DamageData damageData)
	{
		_animator.PlayDeath();
		_follower.DisableMovement();
		_damage.DisableCollisions();
	}

	private void OnSpawnSoul()
	{
		if (!_hasSpawnedSoul)
		{
			_hasSpawnedSoul = true;
			_soulSpawner.RequestSoulSpawn(new DamageData(0, DamageType.Physical, Vector2.zero, 0), transform.position);
		}
	}

	private void OnDeathAnimationComplete()
	{
		_damage.CompleteDeath();
	}

	public void SpawnInitializate(bool enableCollisions = true)
	{
		_animator.Reset();
		_rigidbody.linearVelocity = Vector2.zero;

		_damage.EnableCollisions();
		_damage.ClearStatus();
		_damage.ResetDeathState();

		_follower.EnableMovement();
		_animator.PlayIdle();
		_animator.PlayWalk();

		_attackState = AttackState.Ready;
		_hasSpawnedSoul = false;

		if (_attackCooldownRoutine != null)
		{
			StopCoroutine(_attackCooldownRoutine);
			_attackCooldownRoutine = null;
		}
	}
}
