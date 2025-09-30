using UnityEngine;
using System;

namespace SpawnerSystem
{
	public interface ISpawner
	{
		event Action<PooledEnemy> EnemyReturnedToPool;

		void Init(SpawnerDependencies dependencies);
		int Spawn(SpawnSection section);
		int Spawn(SpawnSection section, EnemyData enemyData);
		int Spawn(SpawnSection section, EnemyKind enemyKind);

		PooledEnemy SpawnAtPosition(SpawnSection section, EnemyData enemyData, Vector3 position);
		PooledEnemy SpawnAtPosition(SpawnSection section, EnemyKind enemyKind, Vector3 position);

		EnemyData GetEnemyData(EnemyKind enemyKind);
		EnemyData[] GetAllEnemyData();
		EnemyKind[] GetAllEnemyKinds();

		int GetEnemyDataCount();
		void InitializeComponents(PooledEnemy pooled);

		Transform GetInactiveContainer();
	}
}
