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

		public override int Spawn(SpawnSection section)
		{
			return Spawn(section, EnemyKind.Knight);
		}

		public override int Spawn(SpawnSection section, int tierIndex)
		{
			return Spawn(section, EnemyKind.Knight);
		}
	}
}