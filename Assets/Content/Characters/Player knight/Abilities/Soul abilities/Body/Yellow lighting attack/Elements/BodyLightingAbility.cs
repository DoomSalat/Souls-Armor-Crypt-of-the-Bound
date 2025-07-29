using UnityEngine;
using Sirenix.OdinInspector;

public class BodyLightingAbility : MonoBehaviour, IAbilityBody
{
	[SerializeField, Required] private LightningSpawner _lightningSpawnerPrefab;

	[Header("Lightning Settings")]
	[SerializeField] private float _lightningRadius = 8f;
	[SerializeField] private LayerMask _enemyLayerMask = -1;

	private LightningSpawner _lightningSpawner;
	private float _damageAmount = 1;

#pragma warning disable 0414
	private Collider2D[] _collidersBuffer = new Collider2D[20];
#pragma warning restore 0414

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

		Collider2D closestEnemy = FindClosestEnemy();

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

	private Collider2D FindClosestEnemy()
	{
		Vector2 lightningCenter = transform.position;

#pragma warning disable 0618
		int colliderCount = Physics2D.OverlapCircleNonAlloc(lightningCenter, _lightningRadius, _collidersBuffer, _enemyLayerMask);
#pragma warning restore 0618

		float closestDistance = float.MaxValue;
		Collider2D closestEnemy = null;

		for (int i = 0; i < colliderCount; i++)
		{
			Collider2D collider = _collidersBuffer[i];
			if (IsEnemy(collider))
			{
				float distance = Vector2.Distance(transform.position, collider.transform.position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestEnemy = collider;
				}
			}
		}

		return closestEnemy;
	}

	private bool IsEnemy(Collider2D collider)
	{
		return collider.TryGetComponent<HurtBox>(out var hurtBox) &&
			   hurtBox.Faction != null &&
			   hurtBox.Faction.IsTagged(Faction.Enemy);
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
