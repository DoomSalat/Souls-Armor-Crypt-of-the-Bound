using UnityEngine;

namespace SpawnerSystem
{
	[System.Serializable]
	public class EnemyPrefabByKind
	{
		[SerializeField] private EnemyKind _kind;
		[SerializeField] private PooledEnemy _prefab;

		public EnemyKind Kind => _kind;
		public PooledEnemy Prefab => _prefab;
	}
}

