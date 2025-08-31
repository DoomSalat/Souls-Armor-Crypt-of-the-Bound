using UnityEngine;
using System;

namespace SpawnerSystem
{
	public interface ISoulSpawnRequestHandler
	{
		void RequestSoulSpawn(SoulType soulType, Vector3 spawnPosition, DamageData damageData, Action<PooledEnemy> onSoulSpawned = null);
	}
}
