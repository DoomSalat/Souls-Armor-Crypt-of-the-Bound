using UnityEngine;

namespace SpawnerSystem
{
	public sealed class SpawnerDependencies
	{
		public SpawnerSection Tokens;
		public EnemyPool EnemyPool;
		public ISoulSpawnRequestHandler SoulSpawnRequestHandler;
		public ThrowSpawner ThrowSpawner;
		public Sword PlayerSword;
	}
}
