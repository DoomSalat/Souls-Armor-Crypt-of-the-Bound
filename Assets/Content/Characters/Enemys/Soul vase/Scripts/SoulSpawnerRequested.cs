using UnityEngine;
using SpawnerSystem;

public class SoulSpawnerRequested : MonoBehaviour
{
	[SerializeField] private SoulType _soulType;
	[Space]
	[SerializeField] private MonoBehaviour _soulSpawnerLogic;

	private ISoulSpawnRequestHandler _spawnRequestHandler;

	public event System.Action<PooledEnemy> SpawnedSoul;

	private void Awake()
	{
		if (_soulSpawnerLogic != null)
		{
			if (_soulSpawnerLogic.TryGetComponent<ISoulSpawnRequestHandler>(out var handler))
			{
				_spawnRequestHandler = handler;
			}
			else
			{
				Debug.LogWarning($"[{nameof(SoulSpawnerRequested)}] SoulSpawnerRequestHandler not found on {_soulSpawnerLogic.gameObject.name}");
				_soulSpawnerLogic = null;
			}
		}
	}

	private void OnValidate()
	{
		if (_soulSpawnerLogic != null)
		{
			if (_soulSpawnerLogic.TryGetComponent<ISoulSpawnRequestHandler>(out var handler))
			{
				_spawnRequestHandler = handler;
			}
			else
			{
				Debug.LogWarning($"[{nameof(SoulSpawnerRequested)}] SoulSpawnerRequestHandler not found on {_soulSpawnerLogic.gameObject.name}");
				_soulSpawnerLogic = null;
			}
		}
	}

	public void Initialize(ISoulSpawnRequestHandler spawnRequestHandler)
	{
		_spawnRequestHandler = spawnRequestHandler;
	}

	public void RequestSoulSpawn(DamageData damageData, Vector3 spawnPosition, SoulType soulType = SoulType.None)
	{
		if (_spawnRequestHandler != null)
		{
			spawnPosition.z = transform.position.z;

			if (soulType == SoulType.None)
			{
				soulType = _soulType;
			}

			_spawnRequestHandler.RequestSoulSpawn(soulType, spawnPosition, damageData, OnSoulSpawned);
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
