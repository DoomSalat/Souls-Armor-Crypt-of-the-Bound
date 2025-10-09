using UnityEngine;
using Sirenix.OdinInspector;

public class BodyLightingAbility : MonoBehaviour, IAbilityBody
{
	[SerializeField, Required] private LightningSpawner _lightningSpawnerPrefab;

	[Header("Found")]
	[SerializeField] private LayerMask _enemyLayerMask = -1;
	[SerializeField] private float _lightningRadius = 8f;
	[SerializeField] private int _foundMaxKills = 3;

	private Transform _effectCenter;
	private LightningSpawner _lightningSpawner;
	private float _damageAmount = 1;

	private Collider2D[] _collidersBuffer = new Collider2D[20];

	public bool HasVisualEffects => true;

	public void Initialize()
	{

	}

	public void InitializeVisualEffects(Transform effectsParent)
	{
		_effectCenter = effectsParent;
		_lightningSpawner = Instantiate(_lightningSpawnerPrefab, effectsParent.transform.position, effectsParent.transform.rotation, effectsParent);
		_lightningSpawner.Initialize();
	}

	public void Activate()
	{
		Vector3 targetPosition;

		Collider2D[] closestEnemys = FoundOverlapCircleUtilits.FindCircleEnemys(_effectCenter.position, _lightningRadius, _enemyLayerMask, _collidersBuffer, _foundMaxKills);

		int foundedEnemys = 0;

		for (int i = 0; i < _foundMaxKills; i++)
		{
			if (closestEnemys[i] != null)
			{
				ApplyDamageToEnemy(closestEnemys[i]);
				_lightningSpawner.SpawnLightning(closestEnemys[i].transform.position, true);
				foundedEnemys++;
			}
		}

		int extraLightnings = _foundMaxKills - foundedEnemys;
		for (int i = 0; i < extraLightnings; i++)
		{
			targetPosition = GetRandomTargetPosition();
			_lightningSpawner.SpawnLightning(targetPosition, false);
		}
	}

	private void ApplyDamageToEnemy(Collider2D enemy)
	{
		var damageData = new DamageData(_damageAmount, DamageType.Physical, Vector2.zero, 0f);
		enemy.GetComponent<HurtBox>().ApplyDamage(damageData);
	}

	private Vector3 GetRandomTargetPosition()
	{
		Vector2 randomDirection = Random.insideUnitCircle.normalized;
		return transform.position + (Vector3)(randomDirection * _lightningRadius);
	}

	public bool CanBlockDamage()
	{
		return false;
	}

	public void DamageBlocked() { }

	public void Deactivate() { }

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, _lightningRadius);
	}
#endif
}
