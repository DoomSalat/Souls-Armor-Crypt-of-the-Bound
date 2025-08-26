using UnityEngine;

namespace SpawnerSystem
{
	[System.Serializable]
	public class EnemyTier
	{
		[SerializeField] private EnemyKind _kind;
		[SerializeField][Range(0f, 1f)] private float _weight = 1f;

		public EnemyKind Kind => _kind;
		public float Weight => _weight;
	}
}