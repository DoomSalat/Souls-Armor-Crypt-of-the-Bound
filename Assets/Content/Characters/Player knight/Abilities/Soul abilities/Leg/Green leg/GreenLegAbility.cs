using Sirenix.OdinInspector;
using UnityEngine;

public class GreenLegAbility : BaseLegAbility
{
	[SerializeField] private GreenPosionSeekerSpawner _greenPosionSeekerSpawnerPrefab;
	[SerializeField, MinValue(0)] private float _radius = 1.5f;
	[SerializeField] private LayerMask _enemyLayerMask = -1;

	private GreenPosionSeekerSpawner _greenPosionSeekerSpawner;
	private readonly Collider2D[] _collidersBuffer = new Collider2D[20];

	public override bool HasVisualEffects => true;

	public override void Initialize() { }
	public override void InitializeVisualEffects(Transform effectsParent)
	{
		_greenPosionSeekerSpawner = Instantiate(_greenPosionSeekerSpawnerPrefab, effectsParent.position, Quaternion.identity, effectsParent);
		_greenPosionSeekerSpawner.Initialize();
	}

	public override void Activate()
	{
		Collider2D enemyCollider = FindNearestEnemy();

		if (enemyCollider != null)
		{
			SendPoisonSeekersToEnemy(enemyCollider);
		}
	}

	public override void Deactivate() { }

	private Collider2D FindNearestEnemy()
	{
		Vector2 searchCenter = transform.position;

#pragma warning disable 0618
		int colliderCount = Physics2D.OverlapCircleNonAlloc(searchCenter, _radius, _collidersBuffer, _enemyLayerMask);
#pragma warning restore 0618

		return FindFirstEnemyInRange(colliderCount);
	}

	private Collider2D FindFirstEnemyInRange(int colliderCount)
	{
		for (int i = 0; i < colliderCount; i++)
		{
			Collider2D collider = _collidersBuffer[i];
			if (IsEnemy(collider))
			{
				return collider;
			}
		}

		return null;
	}

	private void SendPoisonSeekersToEnemy(Collider2D enemyCollider)
	{
		_greenPosionSeekerSpawner.Initialize(enemyCollider.transform);
		_greenPosionSeekerSpawner.SpawnSeekers();
	}

	private bool IsEnemy(Collider2D collider)
	{
		return collider.TryGetComponent<HurtBox>(out var hurtBox) &&
			   hurtBox.Faction != null &&
			   hurtBox.Faction.IsTagged(Faction.Enemy);
	}
}

