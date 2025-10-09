using UnityEngine;

namespace SpawnerSystem
{
	public class TelegraphSpawnStrategy : SimpleSpawnStrategy
	{
		private float _minOffsetFromPlayer = 1.0f;
		private float _maxOffsetFromPlayer = 2.5f;
		private TelegraphPool _telegraphPool;

		private SpawnerDependencies _dependencies;
		private SpawnerBase _ownerSpawner;

		public void SetOwnerSpawner(SpawnerBase spawner)
		{
			_ownerSpawner = spawner;
		}

		public override Vector3 CalculateSpawnPosition(SpawnerSystemData.SpawnSection section, SpawnerDependencies dependencies)
		{
			_dependencies = dependencies;

			SpawnerSystemData.SectionSpawnInfo sectionInfo = dependencies.Tokens.GetSectionSpawnInfo(section);

			return CalculateRandomPositionInSection(sectionInfo);
		}

		protected override Vector3 CalculateRandomPositionInSection(SpawnerSystemData.SectionSpawnInfo sectionInfo)
		{
			float randomAngle = Random.Range(sectionInfo.StartAngle, sectionInfo.EndAngle);

			float randomDistance = Random.Range(_minOffsetFromPlayer, _maxOffsetFromPlayer);

			Vector3 spawnDirection = new Vector3(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle), 0f);
			Vector3 spawnPosition = sectionInfo.Center + spawnDirection * randomDistance;
			spawnPosition.z = sectionInfo.Center.z;

			return spawnPosition;
		}

		public override bool OnBeforeSpawn(Vector3 position, PooledEnemy prefab, SpawnerSystemData.SpawnSection section, EnemyKind kind)
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

		private void OnTelegraphComplete(Vector3 position, PooledEnemy prefab, SpawnerSystemData.SpawnSection section, EnemyKind kind)
		{
			if (_dependencies?.EnemyPool == null || _ownerSpawner == null)
				return;

			PooledEnemy spawned = _dependencies.EnemyPool.GetPooled(prefab, position, Quaternion.identity);
			if (spawned != null)
			{
				SetupSpawned(spawned, section, kind);

				_ownerSpawner.RegisterSpawnedEnemy(spawned);
			}
		}

		private void SetupSpawned(PooledEnemy spawned, SpawnerSystemData.SpawnSection section, EnemyKind kind)
		{
			_ownerSpawner?.SetupEnemySpawn(spawned, section, kind);
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
