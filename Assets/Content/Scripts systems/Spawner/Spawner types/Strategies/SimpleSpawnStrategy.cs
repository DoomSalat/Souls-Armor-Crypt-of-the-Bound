using UnityEngine;

namespace SpawnerSystem
{
	public class SimpleSpawnStrategy : ISpawnStrategy
	{
		public virtual Vector3 CalculateSpawnPosition(SpawnSection section, SpawnerDependencies dependencies)
		{
			return dependencies.Tokens.GetSpawnPosition(section);
		}

		public virtual int GetSpawnCount(SpawnSection section)
		{
			return 1;
		}

		public virtual bool OnBeforeSpawn(Vector3 position, PooledEnemy prefab, SpawnSection section, EnemyKind kind)
		{
			// Базовая реализация не обрабатывает спаун, возвращает false
			return false;
		}

		public virtual void OnAfterSpawn(PooledEnemy spawned, Vector3 position, SpawnSection section, EnemyKind kind)
		{
			// Базовая реализация не делает ничего
		}
	}
}
