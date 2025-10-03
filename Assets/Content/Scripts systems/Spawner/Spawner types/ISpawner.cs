using UnityEngine;
using System;

namespace SpawnerSystem
{
	public interface ISpawner
	{
		event Action<PooledEnemy> EnemyReturnedToPool;

		void Init(SpawnerDependencies dependencies);
		int Spawn(SpawnerSystemData.SpawnSection section);
		int Spawn(SpawnerSystemData.SpawnSection section, EnemyData enemyData);
		int Spawn(SpawnerSystemData.SpawnSection section, EnemyKind enemyKind);

		PooledEnemy SpawnAtPosition(SpawnerSystemData.SpawnSection section, EnemyData enemyData, Vector3 position);
		PooledEnemy SpawnAtPosition(SpawnerSystemData.SpawnSection section, EnemyKind enemyKind, Vector3 position);

		EnemyData GetEnemyData(EnemyKind enemyKind);
		EnemyData[] GetAllEnemyData();
		EnemyKind[] GetAllEnemyKinds();

		int GetEnemyDataCount();
		void InitializeComponents(PooledEnemy pooled);

		Transform GetInactiveContainer();
		ISpawnStrategy GetSpawnStrategy();
		PooledEnemy[] GetActiveEnemies();
		void RegisterSpawnedEnemy(PooledEnemy spawnedEnemy);
	}
}
