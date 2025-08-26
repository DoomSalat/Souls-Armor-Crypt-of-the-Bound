using UnityEngine;

namespace SpawnerSystem
{
	public class GroupSpawnStrategy : SimpleSpawnStrategy
	{
		private const int GroupSizeRangeOffset = 1;

		private Vector2Int _groupSizeRange = new Vector2Int(2, 5);
		private float _spreadRadius = 1.0f;

		public override int GetSpawnCount(SpawnSection section)
		{
			return Random.Range(_groupSizeRange.x, _groupSizeRange.y + GroupSizeRangeOffset);
		}

		public override Vector3 CalculateSpawnPosition(SpawnSection section, SpawnerDependencies dependencies)
		{
			Vector3 basePosition = base.CalculateSpawnPosition(section, dependencies);
			Vector2 offset = Random.insideUnitCircle * _spreadRadius;
			return basePosition + new Vector3(offset.x, offset.y, 0f);
		}
	}
}
