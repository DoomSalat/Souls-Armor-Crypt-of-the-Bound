using UnityEngine;
using SpawnerSystem;

public class SoulSpawnerRequested : MonoBehaviour
{
	[SerializeField] private SoulType _soulType;

	private ISoulSpawnRequestHandler _spawnRequestHandler;

	public event System.Action<PooledEnemy> SpawnedSoul;

	public void Initialize(ISoulSpawnRequestHandler spawnRequestHandler)
	{
		_spawnRequestHandler = spawnRequestHandler;
	}

	public void RequestSoulSpawn(DamageData damageData)
	{
		if (_spawnRequestHandler != null)
		{
			Vector3 spawnPosition = transform.position;
			spawnPosition.z = transform.position.z;

			_spawnRequestHandler.RequestSoulSpawn(_soulType, spawnPosition, damageData, OnSoulSpawned);
		}
		else
		{
			Debug.LogWarning($"[{nameof(SoulSpawnerRequested)}] Spawn request handler not initialized on {gameObject.name}");
		}
	}

	private void OnSoulSpawned(PooledEnemy spawnedSoul)
	{
		SpawnedSoul?.Invoke(spawnedSoul);
	}
}
