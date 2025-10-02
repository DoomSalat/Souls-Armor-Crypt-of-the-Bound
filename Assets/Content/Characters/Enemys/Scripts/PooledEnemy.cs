using UnityEngine;
using StatusSystem;

namespace SpawnerSystem
{
	[RequireComponent(typeof(EnemySpawnMeta))]
	public class PooledEnemy : MonoBehaviour
	{
		public GameObject PrefabReference { get; private set; }
		private EnemyPool _ownerPool;
		public PooledEnemy PrefabOrigin { get; private set; }

		private EnemySpawnMeta _spawnMeta;
		private IFollower _follower;
		private Soul _soul;
		private bool _cached;

		public event System.Action<PooledEnemy> ReturnedToPool;

		public Soul Soul => _soul;
		public EnemySpawnMeta SpawnMeta => _spawnMeta;

		public void Initialize(EnemyPool pool, PooledEnemy prefabOrigin)
		{
			_ownerPool = pool;
			PrefabOrigin = prefabOrigin;
			PrefabReference = prefabOrigin != null ? prefabOrigin.gameObject : null;

			CacheComponents();
		}

		private void CacheComponents()
		{
			if (_cached)
				return;

			_spawnMeta = GetComponent<EnemySpawnMeta>();
			_follower = GetComponent<IFollower>();
			_soul = GetComponent<Soul>();
			_cached = true;
		}

		public void SetupForSpawn(SpawnerSection tokens, SpawnSection section, Transform player, EnemyKind kind, Transform inactiveParent = null, StatusMachine statusMachine = null)
		{
			if (!_cached)
				CacheComponents();

			if (_spawnMeta != null)
				_spawnMeta.Set(tokens, section, kind, inactiveParent);

			if (_follower != null)
			{
				_follower.SetTarget(player);
			}

			if (TryGetComponent<EnemyDamage>(out var enemyDamage))
			{
				enemyDamage.InitializeCreate(statusMachine);
			}
		}

		public void ReturnToPool()
		{
			ReturnedToPool?.Invoke(this);

			if (_ownerPool != null)
			{
				_ownerPool.Release(this);
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}