using UnityEngine;
using Sirenix.OdinInspector;

namespace SpawnerSystem
{
	public abstract class SpawnerBase : MonoBehaviour, ISpawner
	{
		private const float MinWeight = 0.01f;
		private const float DefaultWeight = 1f;

		[SerializeField] protected EnemyPrefabByKind[] _prefabsByKind;
		[SerializeField, Required] protected Transform _inactiveContainer;
		[SerializeField, Min(0)] private int _prewarmPerPrefab = 10;
		[SerializeField] private bool _prewarmOnInit = true;

		protected ISpawnStrategy _spawnStrategy;
		protected SpawnerDependencies _dependencies;

		public virtual void Init(SpawnerDependencies dependencies)
		{
			_dependencies = dependencies;

			if (_prewarmOnInit)
			{
				PrewarmPrefabs();
			}
		}

		public virtual int Spawn(SpawnSection section)
		{
			return SpawnWithStrategy(section);
		}

		public virtual int Spawn(SpawnSection section, int tierIndex) => Spawn(section);

		public virtual int Spawn(SpawnSection section, EnemyKind enemyKind)
		{
			return SpawnWithStrategy(section, enemyKind);
		}

		public virtual PooledEnemy SpawnAtPosition(SpawnSection section, EnemyKind enemyKind, Vector3 position)
		{
			PooledEnemy prefabToSpawn = GetPrefabForKind(enemyKind);

			if (prefabToSpawn == null)
			{
				Debug.LogWarning($"SpawnerBase: No prefab configured for enemy kind {enemyKind}");
				return null;
			}

			PooledEnemy pooledEnemy = SpawnEnemyAt(position, enemyKind, prefabToSpawn);
			SetupSpawned(pooledEnemy, section, enemyKind);
			return pooledEnemy;
		}

		protected virtual int SpawnWithStrategy(SpawnSection section, EnemyKind? enemyKind = null)
		{
			if (_spawnStrategy == null)
			{
				Debug.LogWarning("SpawnerBase: No spawn strategy configured!");
				return 0;
			}

			EnemyKind kindToSpawn = enemyKind ?? ChooseKindWeightedByTokens(_dependencies.Tokens);
			PooledEnemy prefabToSpawn = GetPrefabForKind(kindToSpawn);

			if (prefabToSpawn == null)
			{
				Debug.LogWarning($"SpawnerBase: No prefab configured for enemy kind {kindToSpawn}");
				return 0;
			}

			int spawnCount = _spawnStrategy.GetSpawnCount(section);
			int spawnedCount = 0;

			for (int i = 0; i < spawnCount; i++)
			{
				Vector3 spawnPosition = _spawnStrategy.CalculateSpawnPosition(section, _dependencies);

				bool handledByStrategy = _spawnStrategy.OnBeforeSpawn(spawnPosition, prefabToSpawn, section, kindToSpawn);

				if (!handledByStrategy)
				{
					PooledEnemy pooledEnemy = SpawnEnemyAt(spawnPosition, kindToSpawn, prefabToSpawn);
					SetupSpawned(pooledEnemy, section, kindToSpawn);
					_spawnStrategy.OnAfterSpawn(pooledEnemy, spawnPosition, section, kindToSpawn);
				}

				spawnedCount++;
			}

			return spawnedCount;
		}

		public virtual int GetTierCount() => 0;

		public virtual Transform GetInactiveContainer()
		{
			return _inactiveContainer;
		}

		protected static EnemyKind ChooseKindWeightedByTokens(SpawnerTokens tokens)
		{
			// Временно возвращаем только Soul, пока не настроены другие префабы
			return EnemyKind.Soul;

			// TODO: Раскомментировать когда будут настроены префабы для всех типов
			/*
			EnemyKind[] kinds = { EnemyKind.Soul, EnemyKind.SoulVase, EnemyKind.Skelet, EnemyKind.Knight };
			float totalWeight = 0f;
			float[] weights = new float[kinds.Length];

			for (int index = 0; index < kinds.Length; index++)
			{
				float weight = Mathf.Max(MinWeight, tokens.GetKindWeight(kinds[index]));
				weights[index] = weight;
				totalWeight += weight;
			}

			float randomRoll = Random.value * totalWeight;
			for (int index = 0; index < kinds.Length; index++)
			{
				randomRoll -= weights[index];
				if (randomRoll <= 0f)
					return kinds[index];
			}

			return kinds[kinds.Length - 1];
			*/
		}

		protected PooledEnemy GetPrefabForKind(EnemyKind kind)
		{
			int count = 0;
			for (int i = 0; i < (_prefabsByKind?.Length ?? 0); i++)
				if (_prefabsByKind[i].Kind == kind) count++;

			if (count == 0)
				return null;

			int pickIndex = UnityEngine.Random.Range(0, count);
			for (int i = 0; i < _prefabsByKind.Length; i++)
			{
				if (_prefabsByKind[i].Kind != kind)
					continue;

				if (pickIndex == 0)
					return _prefabsByKind[i].Prefab;

				pickIndex--;
			}

			return null;
		}

		protected PooledEnemy SpawnEnemyAt(Vector3 position, EnemyKind kind, PooledEnemy prefab)
		{
			if (prefab == null)
			{
				Debug.LogError($"SpawnerBase.SpawnEnemyAt: prefab is null for kind {kind}");
				return null;
			}

			PooledEnemy pooledEnemy = _dependencies.EnemyPool.GetPooled(prefab, position, Quaternion.identity);
			return pooledEnemy;
		}

		protected void SetupSpawned(PooledEnemy spawned, SpawnSection section, EnemyKind kind)
		{
			SetupEnemySpawn(spawned, section, kind);
			SetupSoulSpawnerRequested(spawned);
			SetupSkeletThrow(spawned);
		}

		private void SetupEnemySpawn(PooledEnemy spawned, SpawnSection section, EnemyKind kind)
		{
			spawned.SetupForSpawn(_dependencies.Tokens, section, _dependencies.EnemyPool.GetPlayerTarget(), kind, _inactiveContainer, _dependencies.EnemyPool.GetStatusMachine());
		}

		private void SetupSoulSpawnerRequested(PooledEnemy spawned)
		{
			if (spawned.TryGetComponent<SoulSpawnerRequested>(out var soulSpawnerRequested))
			{
				soulSpawnerRequested.Initialize(_dependencies.SoulSpawnRequestHandler);
			}
		}

		private void SetupSkeletThrow(PooledEnemy spawned)
		{
			if (spawned.TryGetComponent<SkeletThrow>(out var skeletThrow))
			{
				skeletThrow.Initialize(_dependencies.ThrowSpawner);
			}
		}

		private void PrewarmPrefabs()
		{
			for (int i = 0; i < _prefabsByKind.Length; i++)
			{
				var entry = _prefabsByKind[i];
				var prefab = entry.Prefab;

				for (int j = 0; j < _prewarmPerPrefab; j++)
				{
					var pooled = _dependencies.EnemyPool.GetPooled(prefab, Vector3.zero, Quaternion.identity);
					pooled.gameObject.SetActive(false);
					pooled.transform.SetParent(_inactiveContainer, false);
				}
			}
		}
	}
}


