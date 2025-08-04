using UnityEngine;
using Sirenix.OdinInspector;

public class BodyLightingAbility : MonoBehaviour, IAbilityBody
{
	[SerializeField, Required] private LightningSpawner _lightningSpawnerPrefab;

	[Header("Found")]
	[SerializeField] private float _lightningRadius = 8f;
	[SerializeField] private LayerMask _enemyLayerMask = -1;

	private LightningSpawner _lightningSpawner;
	private float _damageAmount = 1;

	private Collider2D[] _collidersBuffer = new Collider2D[20];

	public bool HasVisualEffects => true;

	public void Initialize()
	{

	}

	public void InitializeVisualEffects(Transform effectsParent)
	{
		_lightningSpawner = Instantiate(_lightningSpawnerPrefab, effectsParent.transform.position, effectsParent.transform.rotation, effectsParent);
		_lightningSpawner.Initialize();
	}

	public void Activate()
	{
		Vector3 targetPosition;
		bool foundEnemy;

		Collider2D closestEnemy = FoundOverlapCircleUtilits.FindClosestEnemy(transform.position, _lightningRadius, _enemyLayerMask, _collidersBuffer);

		if (closestEnemy != null)
		{
			targetPosition = closestEnemy.transform.position;
			foundEnemy = true;
			ApplyDamageToEnemy(closestEnemy);
		}
		else
		{
			targetPosition = GetRandomTargetPosition();
			foundEnemy = false;
		}

		_lightningSpawner.SpawnLightning(targetPosition, foundEnemy);
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
