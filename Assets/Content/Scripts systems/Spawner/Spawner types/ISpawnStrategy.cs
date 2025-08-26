using UnityEngine;

namespace SpawnerSystem
{
	public interface ISpawnStrategy
	{
		Vector3 CalculateSpawnPosition(SpawnSection section, SpawnerDependencies dependencies);
		int GetSpawnCount(SpawnSection section);
		bool OnBeforeSpawn(Vector3 position, PooledEnemy prefab, SpawnSection section, EnemyKind kind);
		void OnAfterSpawn(PooledEnemy spawned, Vector3 position, SpawnSection section, EnemyKind kind);
	}
}
