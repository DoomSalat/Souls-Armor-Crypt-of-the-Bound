using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

public class YellowSwordAbility : MonoBehaviour, IAbilitySword
{
	[SerializeField, Required] private LightningSpawner _lightningSpawnerPrefab;
	[SerializeField, Required] private float _cooldown = 1f;

	[Header("Found")]
	[SerializeField] private float _foundEnemyRadius = 1f;
	[SerializeField] private LayerMask _enemyLayerMask = -1;

	private LightningSpawner _lightningSpawner;
	private float _damageAmount = 1;
	private bool _isOnCooldown;
	private WaitForSeconds _cooldownWait;

	private Collider2D[] _collidersBuffer = new Collider2D[20];

	public bool HasVisualEffects => true;

	public void Initialize()
	{
		_cooldownWait = new WaitForSeconds(_cooldown);

		_isOnCooldown = false;
	}

	public void InitializeVisualEffects(Transform effectsParent)
	{
		_lightningSpawner = Instantiate(_lightningSpawnerPrefab, effectsParent.position, Quaternion.identity, effectsParent);
		_lightningSpawner.Initialize();
	}

	public void Activate()
	{
		if (_isOnCooldown)
		{
			return;
		}

		_isOnCooldown = true;
		StartCoroutine(CooldownCoroutine());

		Vector3 targetPosition;
		bool foundEnemy;

		Collider2D closestEnemy = FoundOverlapCircleUtilits.FindClosestEnemy(transform.position, _foundEnemyRadius, _enemyLayerMask, _collidersBuffer);

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

	public void Deactivate() { }

	private IEnumerator CooldownCoroutine()
	{
		yield return _cooldownWait;
		_isOnCooldown = false;
	}

	private void ApplyDamageToEnemy(Collider2D enemy)
	{
		var damageData = new DamageData(_damageAmount, DamageType.Physical, Vector2.zero, 0f);
		enemy.GetComponent<HurtBox>().ApplyDamage(damageData);
	}
}
