using UnityEngine;

namespace SpawnerSystem
{
	public class SpawnerYellow : SpawnerBase
	{
		public override void Init(SpawnerDependencies dependencies)
		{
			if (_spawnStrategy == null)
			{
				_spawnStrategy = new GroupSpawnStrategy();
			}

			base.Init(dependencies);
		}
	}
}
