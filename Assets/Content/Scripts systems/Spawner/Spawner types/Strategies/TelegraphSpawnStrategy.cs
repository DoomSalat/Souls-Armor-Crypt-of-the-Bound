using UnityEngine;

namespace SpawnerSystem
{
	public class TelegraphSpawnStrategy : SimpleSpawnStrategy
	{
		private const float SectionAngleRadians = Mathf.PI / 4f;

		private float _minOffsetFromPlayer = 1.0f;
		private float _maxOffsetFromPlayer = 2.5f;
		private TelegraphPool _telegraphPool;

		private SpawnerDependencies _dependencies;
		private SpawnerBase _ownerSpawner;

		public void SetOwnerSpawner(SpawnerBase spawner)
		{
			_ownerSpawner = spawner;
		}

		public override Vector3 CalculateSpawnPosition(SpawnSection section, SpawnerDependencies dependencies)
		{
			_dependencies = dependencies;
			Vector3 playerPosition = dependencies.EnemyPool.GetPlayerTarget().position;
			Vector3 direction = SectionToDirection(section);
			float distance = Random.Range(_minOffsetFromPlayer, _maxOffsetFromPlayer);

			return playerPosition + direction * distance;
		}

		public override bool OnBeforeSpawn(Vector3 position, PooledEnemy prefab, SpawnSection section, EnemyKind kind)
		{
			if (_telegraphPool != null)
			{
				Telegraph telegraph = _telegraphPool.Get();
				if (telegraph != null)
				{
					telegraph.Activate(position, () => OnTelegraphComplete(position, prefab, section, kind));
					return true;
				}
			}

			return false;
		}

		private void OnTelegraphComplete(Vector3 position, PooledEnemy prefab, SpawnSection section, EnemyKind kind)
		{
			if (_dependencies?.EnemyPool == null)
				return;

			PooledEnemy spawned = _dependencies.EnemyPool.GetPooled(prefab, position, Quaternion.identity);
			if (spawned != null)
			{
				SetupSpawned(spawned, section, kind);
			}
		}

		private void SetupSpawned(PooledEnemy spawned, SpawnSection section, EnemyKind kind)
		{
			_ownerSpawner?.SetupSpawned(spawned, section, kind);
		}

		private Vector3 SectionToDirection(SpawnSection section)
		{
			float angleInRadians = (int)section * SectionAngleRadians;

			return new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f);
		}

		public void SetSpawnDistance(float minDistance, float maxDistance)
		{
			_minOffsetFromPlayer = Mathf.Max(0.1f, minDistance);
			_maxOffsetFromPlayer = Mathf.Max(_minOffsetFromPlayer, maxDistance);
		}

		public void SetTelegraphPool(TelegraphPool telegraphPool)
		{
			_telegraphPool = telegraphPool;
		}
	}
}
