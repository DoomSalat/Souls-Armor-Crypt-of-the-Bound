using System.Collections.Generic;
using StatusSystem;
using UnityEngine;
using UnityEngine.Pool;

namespace SpawnerSystem
{
	public class EnemyPool : MonoBehaviour
	{
		[SerializeField] private int _defaultCapacity = 8;
		[SerializeField] private int _maxSize = 64;
		[SerializeField] private Transform _container;

		private Transform _playerTarget;
		private StatusMachine _statusMachine;
		private Dictionary<PooledEnemy, ISpawner> _prefabToSpawner = new();
		private Dictionary<PooledEnemy, Transform> _prefabToInactiveContainer = new();

		private readonly Dictionary<PooledEnemy, ObjectPool<PooledEnemy>> _prefabToPool = new();

		public Transform GetPlayerTarget() => _playerTarget;
		public StatusMachine GetStatusMachine() => _statusMachine;

		public void RegisterPrefab(PooledEnemy prefab, ISpawner spawner)
		{
			_prefabToSpawner[prefab] = spawner;
			_prefabToInactiveContainer[prefab] = spawner.GetInactiveContainer();
		}

		public void Initialize(Transform playerTarget, StatusMachine statusMachine)
		{
			_playerTarget = playerTarget;
			_statusMachine = statusMachine;
		}

		public PooledEnemy Get(PooledEnemy prefab, Vector3 position, Quaternion rotation)
		{
			if (prefab == null)
			{
				Debug.LogError("EnemyPool.Get: prefab is null!");
				return null;
			}

			ObjectPool<PooledEnemy> pooledObjectPool = GetOrCreatePool(prefab);
			PooledEnemy pooledInstance = pooledObjectPool.Get();

			Transform instanceTransform = pooledInstance.transform;
			instanceTransform.SetPositionAndRotation(position, rotation);

			instanceTransform.SetParent(_container, false);
			pooledInstance.gameObject.SetActive(true);

			SubscribeToDeathEvent(pooledInstance);

			return pooledInstance;
		}

		public PooledEnemy GetPooled(PooledEnemy prefab, Vector3 position, Quaternion rotation)
		{
			return Get(prefab, position, rotation);
		}

		public void PrewarmPool(PooledEnemy prefab, int count)
		{
			if (count <= 0 || prefab == null)
				return;

			ObjectPool<PooledEnemy> pool = GetOrCreatePool(prefab);
			var createdObjects = new List<PooledEnemy>();

			for (int i = 0; i < count; i++)
			{
				PooledEnemy instance = pool.Get();
				createdObjects.Add(instance);
			}

			foreach (var obj in createdObjects)
			{
				pool.Release(obj);
			}
		}

		public void Release(PooledEnemy pooled)
		{
			if (pooled == null || pooled.PrefabOrigin == null)
				return;

			UnsubscribeFromDeathEvent(pooled);

			if (_prefabToPool.TryGetValue(pooled.PrefabOrigin, out var pooledObjectPool))
			{
				pooledObjectPool.Release(pooled);
			}
			else
			{
				Destroy(pooled.gameObject);
			}
		}

		private void SubscribeToDeathEvent(PooledEnemy pooledInstance)
		{
			if (pooledInstance.TryGetComponent<EnemyDamage>(out var enemyDamage))
			{
				enemyDamage.DeathCompleted += OnEnemyDeathCompleted;
			}
		}

		private void UnsubscribeFromDeathEvent(PooledEnemy pooledInstance)
		{
			if (pooledInstance.TryGetComponent<EnemyDamage>(out var enemyDamage))
			{
				enemyDamage.DeathCompleted -= OnEnemyDeathCompleted;
			}
		}

		private void OnEnemyDeathCompleted(EnemyDamage enemyDamage)
		{
			if (enemyDamage.TryGetComponent<PooledEnemy>(out var pooledObject))
			{
				if (pooledObject.TryGetComponent<EnemySpawnMeta>(out var spawnMeta))
				{
					Transform inactiveContainer = spawnMeta.InactiveParent;
					if (inactiveContainer != null)
					{
						pooledObject.transform.SetParent(inactiveContainer, false);
					}
				}

				Release(pooledObject);
			}
		}

		private ObjectPool<PooledEnemy> GetOrCreatePool(PooledEnemy prefab)
		{
			if (_prefabToPool.TryGetValue(prefab, out var existingPool))
				return existingPool;

			var newPool = new ObjectPool<PooledEnemy>(
				createFunc: () => CreateInstance(prefab),
				actionOnGet: OnGet,
				actionOnRelease: OnRelease,
				actionOnDestroy: OnDestroyInstance,
				collectionCheck: false,
				defaultCapacity: _defaultCapacity,
				maxSize: _maxSize
			);

			_prefabToPool[prefab] = newPool;
			return newPool;
		}

		private PooledEnemy CreateInstance(PooledEnemy prefab)
		{
			PooledEnemy pooledInstance = Instantiate(prefab);

			Transform inactiveContainer = _prefabToInactiveContainer.TryGetValue(prefab, out var container) ? container : _container;
			if (inactiveContainer != null)
			{
				pooledInstance.transform.SetParent(inactiveContainer, false);
			}

			pooledInstance.Initialize(this, prefab);

			if (pooledInstance.TryGetComponent<EnemyDamage>(out var enemyDamage))
			{
				enemyDamage.InitializeCreate(_statusMachine);
			}

			if (pooledInstance.TryGetComponent<IFollower>(out var follower))
			{
				follower.SetTarget(_playerTarget);
			}

			if (_prefabToSpawner.TryGetValue(prefab, out var spawner))
			{
				spawner.InitializeComponents(pooledInstance);
			}

			pooledInstance.gameObject.SetActive(false);

			return pooledInstance;
		}

		private void OnGet(PooledEnemy pooledInstance)
		{
			if (pooledInstance.TryGetComponent<EnemyDamage>(out var enemyDamage))
			{
				enemyDamage.InitializeCreate(_statusMachine);
			}

			if (pooledInstance.TryGetComponent<IFollower>(out var follower))
			{
				follower.SetTarget(_playerTarget);
			}
		}

		private void OnRelease(PooledEnemy pooledInstance)
		{
			if (pooledInstance == null)
				return;

			Transform targetContainer = GetTargetContainer(pooledInstance);
			pooledInstance.transform.SetParent(targetContainer, false);

			pooledInstance.gameObject.SetActive(false);
		}

		private void OnDestroyInstance(PooledEnemy pooledInstance)
		{
			if (pooledInstance != null)
				Destroy(pooledInstance.gameObject);
		}

		private Transform GetTargetContainer(PooledEnemy pooledInstance)
		{
			if (_prefabToInactiveContainer.TryGetValue(pooledInstance.PrefabOrigin, out var inactiveContainer))
			{
				return inactiveContainer ?? _container;
			}

			if (pooledInstance.TryGetComponent<EnemySpawnMeta>(out var spawnMeta))
			{
				return spawnMeta.InactiveParent ?? _container;
			}

			return _container;
		}
	}
}
