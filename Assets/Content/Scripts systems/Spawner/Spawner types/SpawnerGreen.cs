using UnityEngine;

namespace SpawnerSystem
{
	public class SpawnerGreen : SpawnerBase
	{
		public override void Init(SpawnerDependencies dependencies)
		{
			if (_spawnStrategy == null)
			{
				_spawnStrategy = new SimpleSpawnStrategy();
			}

			base.Init(dependencies);
		}
	}
}
