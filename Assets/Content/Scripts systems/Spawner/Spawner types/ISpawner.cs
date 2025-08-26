using UnityEngine;

namespace SpawnerSystem
{
	public interface ISpawner
	{
		void Init(SpawnerDependencies dependencies);
		int Spawn(SpawnSection section);
		int Spawn(SpawnSection section, int tierIndex);
		int Spawn(SpawnSection section, EnemyKind enemyKind);
		PooledEnemy SpawnAtPosition(SpawnSection section, EnemyKind enemyKind, Vector3 position);
		int GetTierCount();
	}
}
