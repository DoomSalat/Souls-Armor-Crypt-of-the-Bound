using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SpawnerSystem
{
	public abstract class SpawnerBase : MonoBehaviour, ISpawner
	{
		[SerializeField] protected EnemyData[] _enemysData = new EnemyData[0];
		[SerializeField, Required] protected Transform _inactiveContainer;
		[SerializeField, Min(0)] private int _prewarmPerPrefab = 10;
		[SerializeField] private bool _prewarmOnInit = true;

		private Dictionary<EnemyKind, EnemyData> _enemyDataLookup;

		protected ISpawnStrategy _spawnStrategy;
		protected SpawnerDependencies _dependencies;

		private List<PooledEnemy> _activeEnemies = new List<PooledEnemy>();

		public event Action<PooledEnemy> EnemyReturnedToPool;

		public virtual void Init(SpawnerDependencies dependencies)
		{
			_dependencies = dependencies;

			BuildEnemyDataLookup();
			RegisterPrefabs();

			if (_prewarmOnInit)
			{
				PrewarmPrefabs();
			}
		}

		protected virtual void OnEnemyReturnedToPool(PooledEnemy enemy)
		{
			_activeEnemies.Remove(enemy);

			EnemyReturnedToPool?.Invoke(enemy);
		}

		private void RegisterPrefabs()
		{
			for (int i = 0; i < _enemysData.Length; i++)
			{
				var entry = _enemysData[i];
				if (entry?.Prefab != null)
				{
					_dependencies.EnemyPool.RegisterPrefab(entry.Prefab, this);

					var prefab = entry.Prefab;
					prefab.ReturnedToPool += OnEnemyReturnedToPool;
				}
			}
		}

		public virtual int Spawn(SpawnSection section)
		{
			return SpawnWithStrategy(section);
		}

		public virtual int Spawn(SpawnSection section, EnemyData enemyData)
		{
			if (enemyData == null)
			{
				Debug.LogWarning("SpawnerBase: EnemyData is null!");
				return 0;
			}

			return SpawnWithStrategy(section, enemyData.EnemyKind);
		}

		public virtual int Spawn(SpawnSection section, EnemyKind enemyKind)
		{
			return SpawnWithStrategy(section, enemyKind);
		}

		public virtual PooledEnemy SpawnAtPosition(SpawnSection section, EnemyData enemyData, Vector3 position)
		{
			if (enemyData == null)
			{
				Debug.LogWarning("SpawnerBase: EnemyData is null!");
				return null;
			}

			PooledEnemy prefabToSpawn = enemyData.Prefab;

			if (prefabToSpawn == null)
			{
				Debug.LogWarning($"SpawnerBase: No prefab configured for enemy kind {enemyData.EnemyKind}");
				return null;
			}

			PooledEnemy pooledEnemy = SpawnEnemyAt(position, enemyData.EnemyKind, prefabToSpawn);
			SetupEnemySpawn(pooledEnemy, section, enemyData.EnemyKind);

			return pooledEnemy;
		}

		public virtual PooledEnemy SpawnAtPosition(SpawnSection section, EnemyKind enemyKind, Vector3 position)
		{
			EnemyData enemyData = GetEnemyData(enemyKind);
			if (enemyData == null)
			{
				Debug.LogWarning($"SpawnerBase: No EnemyData found for enemy kind {enemyKind}");
				return null;
			}

			return SpawnAtPosition(section, enemyData, position);
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
					SetupEnemySpawn(pooledEnemy, section, kindToSpawn);
					_spawnStrategy.OnAfterSpawn(pooledEnemy, spawnPosition, section, kindToSpawn);
				}

				spawnedCount++;
			}

			return spawnedCount;
		}

		public virtual int GetEnemyDataCount() => _enemysData?.Length ?? 0;

		public virtual EnemyData GetEnemyData(EnemyKind enemyKind)
		{
			if (_enemyDataLookup == null)
				BuildEnemyDataLookup();

			return _enemyDataLookup.TryGetValue(enemyKind, out var data) ? data : null;
		}

		public virtual EnemyData[] GetAllEnemyData()
		{
			return _enemysData?.Where(data => data != null).ToArray() ?? new EnemyData[0];
		}

		public virtual EnemyKind[] GetAllEnemyKinds()
		{
			return _enemysData?.Where(data => data != null).Select(data => data.EnemyKind).ToArray() ?? new EnemyKind[0];
		}

		public virtual Transform GetInactiveContainer()
		{
			return _inactiveContainer;
		}

		public virtual ISpawnStrategy GetSpawnStrategy()
		{
			return _spawnStrategy;
		}

		public virtual PooledEnemy[] GetActiveEnemies()
		{
			_activeEnemies.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);

			return _activeEnemies.ToArray();
		}

		protected EnemyKind ChooseKindWeightedByTokens(SpawnerTokens tokens)
		{
			var availableKinds = GetAllEnemyKinds();
			if (availableKinds.Length == 0)
				return EnemyKind.Soul;

			float totalWeight = 0f;
			float[] weights = new float[availableKinds.Length];

			for (int index = 0; index < availableKinds.Length; index++)
			{
				EnemyData enemyData = GetEnemyData(availableKinds[index]);
				float weight = enemyData?.SectionWeight ?? 1f;
				weights[index] = weight;
				totalWeight += weight;
			}

			if (totalWeight <= 0f)
				return availableKinds[0];

			float randomRoll = UnityEngine.Random.value * totalWeight;
			for (int index = 0; index < availableKinds.Length; index++)
			{
				randomRoll -= weights[index];
				if (randomRoll <= 0f)
					return availableKinds[index];
			}

			return availableKinds[availableKinds.Length - 1];
		}

		protected PooledEnemy GetPrefabForKind(EnemyKind kind)
		{
			EnemyData enemyData = GetEnemyData(kind);
			return enemyData?.Prefab;
		}

		private void BuildEnemyDataLookup()
		{
			_enemyDataLookup = new Dictionary<EnemyKind, EnemyData>();

			if (_enemysData != null)
			{
				foreach (var enemyData in _enemysData)
				{
					if (enemyData != null)
					{
						_enemyDataLookup[enemyData.EnemyKind] = enemyData;
					}
				}
			}
		}

		protected PooledEnemy SpawnEnemyAt(Vector3 position, EnemyKind kind, PooledEnemy prefab)
		{
			var spawnedEnemy = _dependencies.EnemyPool.GetPooled(prefab, position, Quaternion.identity);

			if (spawnedEnemy != null)
			{
				_activeEnemies.Add(spawnedEnemy);
			}

			return spawnedEnemy;
		}

		public virtual void SetupEnemySpawn(PooledEnemy spawned, SpawnSection section, EnemyKind kind)
		{
			spawned.SetupForSpawn(_dependencies.Tokens, section, _dependencies.EnemyPool.GetPlayerTarget(), kind, _inactiveContainer, _dependencies.EnemyPool.GetStatusMachine());
			InitializeComponents(spawned);
		}

		private void PrewarmPrefabs()
		{
			var prefabsByKind = new Dictionary<EnemyKind, List<PooledEnemy>>();

			for (int i = 0; i < _enemysData.Length; i++)
			{
				var entry = _enemysData[i];
				if (entry?.Prefab != null)
				{
					if (!prefabsByKind.ContainsKey(entry.EnemyKind))
					{
						prefabsByKind[entry.EnemyKind] = new List<PooledEnemy>();
					}

					prefabsByKind[entry.EnemyKind].Add(entry.Prefab);
				}
			}

			foreach (var kvp in prefabsByKind)
			{
				var kind = kvp.Key;
				var prefabs = kvp.Value;

				int instancesPerPrefab = _prewarmPerPrefab / prefabs.Count;
				int remainingInstances = _prewarmPerPrefab % prefabs.Count;

				for (int i = 0; i < prefabs.Count; i++)
				{
					var prefab = prefabs[i];
					int countForThisPrefab = instancesPerPrefab + (i < remainingInstances ? 1 : 0);

					PrewarmPrefabInInactiveContainer(prefab, countForThisPrefab);
				}
			}
		}

		private void PrewarmPrefabInInactiveContainer(PooledEnemy prefab, int count)
		{
			_dependencies.EnemyPool.PrewarmPool(prefab, count);
		}

		public void InitializeComponents(PooledEnemy pooled)
		{
			if (pooled.TryGetComponent<SoulSpawnerRequested>(out var soulSpawnerRequested))
			{
				soulSpawnerRequested.Initialize(_dependencies.SoulSpawnRequestHandler);
			}

			if (pooled.TryGetComponent<BaseSkeletThrow>(out var skeletThrow))
			{
				skeletThrow.Initialize(_dependencies.ThrowSpawner);
			}

			if (pooled.TryGetComponent<Knight>(out var knight))
			{
				knight.InitializePlayerSword(_dependencies.PlayerSword);
			}
		}
	}
}


