using UnityEngine;

namespace SpawnerSystem
{
	public sealed class SpawnerDependencies
	{
		public SpawnerTokens Tokens;
		public EnemyPool EnemyPool;
		public ISoulSpawnRequestHandler SoulSpawnRequestHandler;
		public ThrowSpawner ThrowSpawner;
	}
}
