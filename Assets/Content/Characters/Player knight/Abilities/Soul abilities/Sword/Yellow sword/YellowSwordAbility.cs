using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

public class YellowSwordAbility : MonoBehaviour, IAbilitySword
{
	private const int DefaultHitCount = 1;

	[SerializeField, Required] private LightningSpawner _lightningSpawnerPrefab;
	[SerializeField, Required] private float _chargeTime = 1f;

	[Header("Found")]
	[SerializeField] private float _foundEnemyRadius = 1f;
	[SerializeField] private LayerMask _enemyLayerMask = -1;

	[Header("Debug")]
	[SerializeField, ReadOnly] private bool _isCountingHits = false;
	[SerializeField, ReadOnly] private bool _isReadyToSpawn = false;

	private LightningSpawner _lightningSpawner;
	private SwordChargeEffect _chargeEffect;
	private float _damageAmount = 1;
	private int _hitCount = 0;
	private Coroutine _chargeCoroutine;
	private WaitForSeconds _chargeTimeWait;

	private Collider2D[] _collidersBuffer = new Collider2D[20];

	public bool HasVisualEffects => true;

	public void Initialize()
	{
		_chargeTimeWait = new WaitForSeconds(_chargeTime);
	}

	public void InitializeVisualEffects(Transform effectsParent)
	{
		_lightningSpawner = Instantiate(_lightningSpawnerPrefab, effectsParent.position, Quaternion.identity, effectsParent);
		_lightningSpawner.Initialize();
	}

	public void InitializeVisualEffects(Transform effectsParent, SwordChargeEffect chargeEffect)
	{
		_chargeEffect = chargeEffect;
		InitializeVisualEffects(effectsParent);
	}

	public void Activate()
	{
		if (_isReadyToSpawn)
		{
			SpawnLightningStrike();
			ResetState();
			return;
		}

		if (_isCountingHits == false)
		{
			StartHitCounting();
		}
		else
		{
			_hitCount++;
		}
	}

	public void Deactivate()
	{
		if (_chargeCoroutine != null)
		{
			StopCoroutine(_chargeCoroutine);
			_chargeCoroutine = null;
		}

		_chargeEffect.Stop();
		ResetState();
	}

	private void StartHitCounting()
	{
		_hitCount = DefaultHitCount;
		_isCountingHits = true;
		_isReadyToSpawn = false;

		if (_chargeCoroutine != null)
		{
			StopCoroutine(_chargeCoroutine);
		}

		_chargeCoroutine = StartCoroutine(ChargeTimeCoroutine());
	}

	private IEnumerator ChargeTimeCoroutine()
	{
		yield return _chargeTimeWait;

		_isCountingHits = false;
		_isReadyToSpawn = true;
		_chargeCoroutine = null;

		_chargeEffect.PlayCharged();
	}

	private void SpawnLightningStrike()
	{
		for (int i = 0; i < _hitCount; i++)
		{
			Vector3 targetPosition;
			bool foundEnemy;

			Collider2D closestEnemy = FoundOverlapCircleUtilits.FindClosestEnemy(_lightningSpawner.transform.position, _foundEnemyRadius, _enemyLayerMask, _collidersBuffer);

			if (closestEnemy != null)
			{
				targetPosition = closestEnemy.transform.position;
				foundEnemy = true;
				ApplyDamageToEnemy(closestEnemy);
			}
			else
			{
				targetPosition = transform.position;
				foundEnemy = false;
			}

			_lightningSpawner.SpawnLightning(targetPosition, foundEnemy);
		}

		_chargeEffect.Stop();
	}

	private void ResetState()
	{
		_hitCount = 0;
		_isCountingHits = false;
		_isReadyToSpawn = false;
	}

	private void ApplyDamageToEnemy(Collider2D enemy)
	{
		var damageData = new DamageData(_damageAmount, DamageType.Physical, Vector2.zero, 0f);
		enemy.GetComponent<HurtBox>().ApplyDamage(damageData);
	}
}
