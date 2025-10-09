using UnityEngine;
using Sirenix.OdinInspector;

public class GreenSwordAbility : MonoBehaviour, IAbilitySword
{
	[SerializeField, Required] private GreenPosionSeekerSpawner _greenPosionSeekerSpawnerPrefab;
	[SerializeField] private float _foundEnemyRadius = 5f;
	[SerializeField] private LayerMask _enemyLayerMask = -1;

	private GreenPosionSeekerSpawner _greenPosionSeekerSpawner;

	private Collider2D[] _collidersBuffer = new Collider2D[20];

	public bool HasVisualEffects => true;

	public void Initialize() { }

	public void InitializeVisualEffects(Transform effectsParent)
	{
		_greenPosionSeekerSpawner = Instantiate(_greenPosionSeekerSpawnerPrefab, effectsParent.position, Quaternion.identity, effectsParent);
		_greenPosionSeekerSpawner.Initialize();
	}

	public void InitializeVisualEffects(Transform effectsParent, SwordChargeEffect chargeEffect)
	{
		InitializeVisualEffects(effectsParent);
	}

	public void Activate()
	{
		int maxEnemies = _collidersBuffer.Length;
		Collider2D[] foundEnemies = FoundOverlapCircleUtilits.FindCircleEnemys(_greenPosionSeekerSpawner.transform.position, _foundEnemyRadius, _enemyLayerMask, _collidersBuffer, maxEnemies);

		int enemyCount = 0;
		for (int i = 0; i < foundEnemies.Length; i++)
		{
			if (foundEnemies[i] != null)
			{
				enemyCount++;
			}
		}

		if (enemyCount > 0)
		{
			_greenPosionSeekerSpawner.SetCount(enemyCount);
			_greenPosionSeekerSpawner.SpawnSeekers();
		}
	}

	public void Deactivate() { }
}
