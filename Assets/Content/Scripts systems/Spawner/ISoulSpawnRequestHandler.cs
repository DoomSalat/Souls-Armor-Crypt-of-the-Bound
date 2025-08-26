using UnityEngine;

namespace SpawnerSystem
{
	public interface ISoulSpawnRequestHandler
	{
		void RequestSoulSpawn(SoulType soulType, Vector3 spawnPosition, DamageData damageData);
	}
}
