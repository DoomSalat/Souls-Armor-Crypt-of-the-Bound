using UnityEngine;
using Sirenix.OdinInspector;

namespace SpawnerSystem
{
	public class SpawnerYellow : SpawnerBase
	{
		[Header("Group Spawn Settings")]
		[SerializeField, MinValue(2), MaxValue(10)] private Vector2Int _groupSizeRange = new Vector2Int(2, 5);
		[Space]
		[SerializeField, MinValue(0.5f)] private float _minDistanceFromLeader = 1.0f;
		[SerializeField, MinValue(0.5f)] private float _maxDistanceFromLeader = 2.5f;
		[SerializeField, MinValue(0.1f)] private float _randomOffset = 0.3f;

		public override void Init(SpawnerDependencies dependencies)
		{
			if (_spawnStrategy == null)
			{
				_spawnStrategy = new GroupSpawnStrategy();
			}

			if (_spawnStrategy is GroupSpawnStrategy groupStrategy)
			{
				groupStrategy.SetSpawnSettings(_groupSizeRange, _minDistanceFromLeader, _maxDistanceFromLeader, _randomOffset);
			}

			base.Init(dependencies);
		}
	}
}
