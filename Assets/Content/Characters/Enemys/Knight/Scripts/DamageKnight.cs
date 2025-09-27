using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class DamageKnight : EnemyDamage
{
	[Header("Death Settings")]
	[SerializeField, MinValue(0)] private float _deathEndedDelay = 1f;
	[SerializeField, MinValue(0)] private float _bodySoulDelay = 3f;
	[Space]
	[SerializeField, MinValue(0)] private float _bodySoulSpawnOffset = 0.5f;
	[SerializeField, MinValue(0)] private float _bodySoulSpawnForce = 5f;
	[SerializeField, MinValue(0)] private float _bodySoulSpawnForceAmount = 1.5f;
	[Space]
	[SerializeField, Required] private Transform _swordSoulSpawnPoint;
	[SerializeField, Required] private Transform _bodySoulSpawnPoint;

	private KnightAnimator _knightAnimator;
	private KnightSword _knightSword;
	private SoulSpawnerRequested _soulSpawner;

	private SoulType _soulTypeSword;
	private SoulType _soulTypeBodyFirst;
	private SoulType _soulTypeBodySecond;

	private bool _deathAnimationEnded = false;

	private WaitForSeconds _deathEndedDelayWait;
	private WaitForSeconds _bodySoulDelayWait;
	private WaitUntil _animationEndedWaitUntil;

	private void Awake()
	{
		InitializeWait();
	}

	private void OnDisable()
	{
		if (_knightAnimator != null)
			_knightAnimator.DeathEnded -= OnDeathAnimationEnded;
	}

	public void InitializeComponents(KnightAnimator knightAnimator, KnightSword knightSword, SoulSpawnerRequested soulSpawner)
	{
		_knightAnimator = knightAnimator;
		_knightSword = knightSword;
		_soulSpawner = soulSpawner;
	}

	public SoulType[] RandomSoulInside()
	{
		SoulType[] allSoulTypes = (SoulType[])System.Enum.GetValues(typeof(SoulType));
		SoulType[] availableSoulTypes = System.Array.FindAll(allSoulTypes, soulType =>
			soulType != SoulType.Purple && soulType != SoulType.None);

		_soulTypeSword = availableSoulTypes[Random.Range(0, availableSoulTypes.Length)];
		_soulTypeBodyFirst = availableSoulTypes[Random.Range(0, availableSoulTypes.Length)];
		_soulTypeBodySecond = availableSoulTypes[Random.Range(0, availableSoulTypes.Length)];

		return new SoulType[] { _soulTypeSword, _soulTypeBodyFirst, _soulTypeBodySecond };
	}

	private void InitializeWait()
	{
		_deathEndedDelayWait = new WaitForSeconds(_deathEndedDelay);
		_bodySoulDelayWait = new WaitForSeconds(_bodySoulDelay);
		_animationEndedWaitUntil = new WaitUntil(() => _deathAnimationEnded);
	}

	private void OnDeathAnimationEnded()
	{
		_deathAnimationEnded = true;
		_knightAnimator.DeathEnded -= OnDeathAnimationEnded;
	}

	protected override void Death(DamageData damageData)
	{
		_deathAnimationEnded = false;

		base.Death(damageData);

		StartCoroutine(DeathSequenceCoroutine());
	}

	private IEnumerator DeathSequenceCoroutine()
	{
		_knightAnimator.DeathEnded += OnDeathAnimationEnded;

		yield return _animationEndedWaitUntil;
		yield return _deathEndedDelayWait;

		SpawnSwordSoul();

		yield return _bodySoulDelayWait;

		SpawnBodySouls();
		CompleteDeath();
	}

	private void SpawnSwordSoul()
	{
		DamageData swordSoulDamageData = new DamageData(
			_bodySoulSpawnForceAmount,
			DamageType.Physical,
			Vector2.zero,
			0
		);

		_knightSword.Disable();
		_soulSpawner.RequestSoulSpawn(swordSoulDamageData, _swordSoulSpawnPoint.position, _soulTypeSword);
	}

	private void SpawnBodySouls()
	{
		SpawnLeftBodySoul();
		SpawnRightBodySoul();
	}

	private void SpawnLeftBodySoul()
	{
		Vector2 leftImpulse = Vector2.left * _bodySoulSpawnForce;
		DamageData leftSoulDamageData = new DamageData(
			_bodySoulSpawnForceAmount,
			DamageType.Physical,
			leftImpulse,
			_bodySoulSpawnForce
		);

		Vector3 leftPosition = _bodySoulSpawnPoint.position + Vector3.left * _bodySoulSpawnOffset;
		_soulSpawner.RequestSoulSpawn(leftSoulDamageData, leftPosition, _soulTypeBodyFirst);
	}

	private void SpawnRightBodySoul()
	{
		Vector2 rightImpulse = Vector2.right * _bodySoulSpawnForce;
		DamageData rightSoulDamageData = new DamageData(
			_bodySoulSpawnForceAmount,
			DamageType.Physical,
			rightImpulse,
			_bodySoulSpawnForce
		);

		Vector3 rightPosition = _bodySoulSpawnPoint.position + Vector3.right * _bodySoulSpawnOffset;
		_soulSpawner.RequestSoulSpawn(rightSoulDamageData, rightPosition, _soulTypeBodySecond);
	}

	[Button(nameof(ManualDeathRequest))]
	private void ManualDeathRequest()
	{
		Death(new DamageData(0, DamageType.Physical, Vector2.zero, 0));
	}
}
