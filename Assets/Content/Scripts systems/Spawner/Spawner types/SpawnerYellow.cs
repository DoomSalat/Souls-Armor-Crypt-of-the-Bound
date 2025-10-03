using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System.Collections.Generic;

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

		public void SetGroupMetaData(int tokensToReturn, float timerReduction)
		{
			if (_spawnStrategy is GroupSpawnStrategy groupStrategy)
			{
				groupStrategy.SetGroupMetaData(tokensToReturn, timerReduction);
			}
		}

		public override PooledEnemy[] GetActiveEnemies()
		{
			var allActiveEnemies = base.GetActiveEnemies();
			var groupLeaders = new List<PooledEnemy>();

			var allGroups = GroupRegister.GetAllGroups();
			var leaderTransforms = new HashSet<Transform>();

			if (allGroups != null && allGroups.Count > 0)
			{
				foreach (var group in allGroups.Values)
				{
					var leader = group.Keys.FirstOrDefault();
					if (leader?.GetTransform() != null)
					{
						leaderTransforms.Add(leader.GetTransform());
					}
				}
			}

			foreach (var enemy in allActiveEnemies)
			{
				if (enemy == null || enemy.gameObject == null)
					continue;

				if (allGroups != null && allGroups.Count > 0)
				{
					if (leaderTransforms.Contains(enemy.transform))
					{
						groupLeaders.Add(enemy);
					}
				}
				else
				{
					groupLeaders.Add(enemy);
				}
			}

			return groupLeaders.ToArray();
		}
	}
}
