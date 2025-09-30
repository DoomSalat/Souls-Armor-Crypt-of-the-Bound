using UnityEngine;
using Sirenix.OdinInspector;
using SpawnerSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Knight : MonoBehaviour, ISpawnInitializable
{
	private const float MinVelocity = 0.01f;

	[Header("Components")]
	[SerializeField, Required] private MonoBehaviour _followLogic;
	[SerializeField, Required] private KnightAnimator _animator;
	[SerializeField, Required] private KnightSword _knightSword;
	[SerializeField, Required] private CreatureFlip _creatureFlip;
	[SerializeField, Required] private SoulSpawnerRequested _soulSpawner;

	[Header("Movement Settings")]
	[SerializeField, MinValue(0)] private float _optimalDistance = 3f;

	[Header("Soul Settings")]
	[SerializeField, Required] private SoulMaterialApplier _soulMaterialHead;
	[SerializeField, Required] private SoulMaterialApplier _soulMaterialSword;

	[Header("Hit")]
	[SerializeField, Required] private HurtBox _hurtBox;
	[SerializeField, Required] private DamageKnight _damage;

	[Header("Debug")]
	[SerializeField] private bool _debug = false;

	private Rigidbody2D _rigidbody;
	private Collider2D _collider;
	private IFollower _follower;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<Collider2D>();
		_follower = _followLogic.GetComponent<IFollower>();

		_damage.Initialize(_collider, null, _hurtBox);
		_damage.InitializeComponents(_animator, _knightSword, _soulSpawner);
	}

	private void OnEnable()
	{
		_damage.DeathRequested += OnDeathRequested;
	}

	private void OnDisable()
	{
		_damage.DeathRequested -= OnDeathRequested;
	}

	private void Update()
	{
		if (_damage.IsDead)
		{
			return;
		}

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

		UpdateFlipDirection();
		UpdateMoveAnimation();
	}

	private void FixedUpdate()
	{
		if (_follower.IsMovementEnabled && _follower.TryGetDistanceToTarget(out float distance))
		{
			UpdateMovement(distance);
		}
	}

	public void InitializePlayerSword(Sword playerSword)
	{
		_knightSword.InitializePlayerSword(playerSword);
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

	private void UpdateMovement(float distanceToTarget)
	{
		_follower.TryFollow();

		if (_debug)
		{
			Debug.Log($"[{nameof(Knight)}] UpdateMovement: {distanceToTarget}");
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
				Debug.LogError($"[{nameof(Knight)}] TargetFollower is not a IFollower on {gameObject.name}!");
			}
		}
	}

	private void OnDeathRequested(DamageData damageData)
	{
		_animator.PlayDeath();

		_follower.DisableMovement();
		_damage.DisableCollisions();
		_knightSword.StopLogic();
	}

	public void SpawnInitializate(bool enableCollisions = true)
	{
		_animator.Reset();
		_rigidbody.linearVelocity = Vector2.zero;

		_damage.EnableCollisions();
		_damage.ClearStatus();
		_damage.ResetDeathState();

		SoulType[] soulTypes = _damage.RandomSoulInside();
		_soulMaterialSword.ApplySoul(soulTypes[0]);
		_soulMaterialHead.ApplySoul(soulTypes[1]);

		_follower.EnableMovement();
		_animator.PlayIdle();
		_animator.PlayWalk();

		_knightSword.Enable(_follower.Target);
	}
}
