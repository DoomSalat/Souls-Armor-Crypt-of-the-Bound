using UnityEngine;

namespace SpawnerSystem
{
	public interface ISpawnStrategy
	{
		Vector3 CalculateSpawnPosition(SpawnerSystemData.SpawnSection section, SpawnerDependencies dependencies);
		int GetSpawnCount(SpawnerSystemData.SpawnSection section);
		bool OnBeforeSpawn(Vector3 position, PooledEnemy prefab, SpawnerSystemData.SpawnSection section, EnemyKind kind);
		void OnAfterSpawn(PooledEnemy spawned, Vector3 position, SpawnerSystemData.SpawnSection section, EnemyKind kind);
	}
}
