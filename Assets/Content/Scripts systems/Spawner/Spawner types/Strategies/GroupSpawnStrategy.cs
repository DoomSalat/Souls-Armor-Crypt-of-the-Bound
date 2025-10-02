using System.Collections.Generic;
using UnityEngine;

namespace SpawnerSystem
{
	public class GroupSpawnStrategy : SimpleSpawnStrategy
	{
		private const int GroupSizeRangeOffset = 1;
		private const float FullCircleDegrees = 360f;
		private const float ZPosition = 0f;
		private const int SingleEnemySpawn = 1;
		private const int LeaderIndexOffset = 1;

		private Vector2Int _groupSizeRange = new Vector2Int(2, 5);

		private float _minDistanceFromLeader = 1.0f;
		private float _maxDistanceFromLeader = 2.5f;
		private float _randomOffset = 0.3f;

		private List<PooledEnemy> _currentGroup = new List<PooledEnemy>();
		private Vector3 _leaderPosition;
		private int _expectedGroupSize;

		private float _groupTokensToReturn = 0f;
		private float _groupTimerReduction = 0f;
		private bool _hasGroupMetaData = false;

		public void SetSpawnSettings(Vector2Int groupSizeRange, float minDistanceFromLeader, float maxDistanceFromLeader, float randomOffset = 0.3f)
		{
			_groupSizeRange = groupSizeRange;
			_minDistanceFromLeader = minDistanceFromLeader;
			_maxDistanceFromLeader = maxDistanceFromLeader;
			_randomOffset = randomOffset;
		}

		public void SetGroupMetaData(float tokensToReturn, float timerReduction)
		{
			_groupTokensToReturn = tokensToReturn;
			_groupTimerReduction = timerReduction;
			_hasGroupMetaData = true;
		}

		public void ResetGroupState()
		{
			_currentGroup.Clear();
			_expectedGroupSize = 0;
			_hasGroupMetaData = false;
			_groupTokensToReturn = 0f;
			_groupTimerReduction = 0f;
		}

		public void ForceCompleteGroup()
		{
			if (_currentGroup.Count > 0)
			{
				CreateGroupFromSpawned();
				_currentGroup.Clear();
			}
		}

		public override int GetSpawnCount(SpawnSection section)
		{
			if (_currentGroup.Count > 0)
			{
				return SingleEnemySpawn;
			}

			int spawnCount = Random.Range(_groupSizeRange.x, _groupSizeRange.y + GroupSizeRangeOffset);
			_expectedGroupSize = spawnCount;
			return spawnCount;
		}

		public override Vector3 CalculateSpawnPosition(SpawnSection section, SpawnerDependencies dependencies)
		{
			if (_currentGroup.Count == 0 && _expectedGroupSize == 0)
			{
				_expectedGroupSize = Random.Range(_groupSizeRange.x, _groupSizeRange.y + GroupSizeRangeOffset);
			}

			if (_currentGroup.Count == 0)
			{
				_leaderPosition = base.CalculateSpawnPosition(section, dependencies);
			}

			if (_currentGroup.Count < _expectedGroupSize - LeaderIndexOffset)
			{
				Vector3 memberPosition = CalculatePositionAroundLeader();
				return memberPosition;
			}
			else
			{
				return _leaderPosition;
			}
		}

		private Vector3 CalculatePositionAroundLeader()
		{
			int memberIndex = _currentGroup.Count;
			int totalMembers = _expectedGroupSize - LeaderIndexOffset;

			if (totalMembers <= 0)
				return _leaderPosition;

			float angleStep = FullCircleDegrees / totalMembers;
			float angle = memberIndex * angleStep;
			float angleRad = angle * Mathf.Deg2Rad;

			float distance = Random.Range(_minDistanceFromLeader, _maxDistanceFromLeader);

			Vector3 position = _leaderPosition + new Vector3(
				Mathf.Cos(angleRad) * distance,
				Mathf.Sin(angleRad) * distance,
				ZPosition
			);

			Vector2 randomOffset = Random.insideUnitCircle * _randomOffset;

			return position + new Vector3(randomOffset.x, randomOffset.y, ZPosition);
		}

		public override bool OnBeforeSpawn(Vector3 position, PooledEnemy prefab, SpawnSection section, EnemyKind kind)
		{
			return false;
		}

		public override void OnAfterSpawn(PooledEnemy spawned, Vector3 position, SpawnSection section, EnemyKind kind)
		{
			if (spawned != null && spawned.SpawnMeta != null && _hasGroupMetaData)
			{
				spawned.SpawnMeta.SetSpawnData(_groupTokensToReturn, _groupTimerReduction);
			}

			_currentGroup.Add(spawned);

			if (_currentGroup.Count >= _expectedGroupSize)
			{
				CreateGroupFromSpawned();
				_currentGroup.Clear();
			}
		}

		private void CreateGroupFromSpawned()
		{
			PooledEnemy leaderEnemy = _currentGroup[_currentGroup.Count - LeaderIndexOffset];

			if (!leaderEnemy.TryGetComponent<IGroupController>(out var leader))
			{
				return;
			}

			List<IGroupController> groupMembers = new List<IGroupController>();
			for (int i = 0; i < _currentGroup.Count - LeaderIndexOffset; i++)
			{
				if (_currentGroup[i].TryGetComponent<IGroupController>(out var member))
				{
					groupMembers.Add(member);
				}
			}

			int groupId = GroupRegister.CreateGroup(leader, groupMembers);
			leader.InitializeGroup(groupId, true);
		}
	}
}
