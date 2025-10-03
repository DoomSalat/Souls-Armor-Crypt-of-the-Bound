using UnityEngine;

namespace SpawnerSystem
{
	[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Spawner System/Enemy Data")]
	public class EnemyData : ScriptableObject
	{
		[Header("Enemy Info")]
		[SerializeField] private PooledEnemy _prefab;
		[SerializeField] private EnemyKind _enemyKind;
		[SerializeField] private SoulType _soulType;

		[Header("Section Weight")]
		[SerializeField, Range(0f, 10f)] private float _sectionWeight = 1f;

		public PooledEnemy Prefab => _prefab;
		public EnemyKind EnemyKind => _enemyKind;
		public SoulType SoulType => _soulType;
		public float SectionWeight => _sectionWeight;
	}
}