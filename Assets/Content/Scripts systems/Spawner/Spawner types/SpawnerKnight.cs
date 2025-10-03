using UnityEngine;

namespace SpawnerSystem
{
	public class SpawnerKnight : SpawnerBase
	{
		public override void Init(SpawnerDependencies dependencies)
		{
			if (_spawnStrategy == null)
			{
				_spawnStrategy = new SimpleSpawnStrategy();
			}

			base.Init(dependencies);
		}

		public override int Spawn(SpawnerSystemData.SpawnSection section)
		{
			return Spawn(section, EnemyKind.Knight);
		}

		public override int Spawn(SpawnerSystemData.SpawnSection section, EnemyData enemyData)
		{
			return Spawn(section, enemyData.EnemyKind);
		}
	}
}