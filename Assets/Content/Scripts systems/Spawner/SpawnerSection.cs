using System;
using UnityEngine;
using Sirenix.OdinInspector;
using static VFolders.Libs.VUtils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpawnerSystem
{
	public class SpawnerSection : MonoBehaviour
	{
		private const float ScreenHeightMultiplier = 0.5f;

		[SerializeField, MinValue(0)] private float _spawnDistanceFromCenter = 3f;

		[System.Serializable]
		public class SectionDebugInfo
		{
			public int EnemyCount;
			public float SectionWeight;
		}

		[Header("Debug")]
		[SerializeField] private bool _showDebugGizmos = false;
		[SerializeField] private bool _showDebugLogs = false;
		[SerializeField, ReadOnly] private SerializableDictionary<int, SectionDebugInfo> _debugSectionsInfo = new SerializableDictionary<int, SectionDebugInfo>();

		public int Sections => SpawnerSystemData.SectionCount;

		private Transform _player;
		private Camera _camera;

		private readonly float[] _sectionWeights = new float[SpawnerSystemData.SectionCount + 1];

		private void Awake()
		{
			_camera = Camera.main;
			ClearSectionWeights();
		}

		public void Init(Transform player)
		{
			_player = player;
		}

		public void UpdateWeights(PooledEnemy[] activeEnemies)
		{
			ClearSectionWeights();

			if (activeEnemies == null || _player == null)
			{
				Debug.LogWarning($"[{nameof(SpawnerSection)}] Cannot recalculate weights: activeEnemies={activeEnemies?.Length ?? 0}, player={_player != null}");
				return;
			}

			if (_showDebugLogs)
				Debug.Log($"[{nameof(SpawnerSection)}] Recalculating weights for {activeEnemies.Length} enemies");

			int[] enemyCounts = new int[SpawnerSystemData.SectionCount + 1];

			foreach (var enemy in activeEnemies)
			{
				if (enemy == null || enemy.gameObject == null || !enemy.gameObject.activeInHierarchy)
					continue;

				var section = GetSectionByPosition(enemy.transform.position);
				int sectionIndex = (int)section;

				float enemyWeight = GetEnemyWeight(enemy);

				if (_showDebugLogs)
					Debug.Log($"[{nameof(SpawnerSection)}] Enemy {enemy.SpawnMeta?.Kind} at section {sectionIndex}, weight: {enemyWeight}");

				if (sectionIndex >= 1 && sectionIndex <= SpawnerSystemData.SectionCount)
				{
					_sectionWeights[sectionIndex] += enemyWeight;
					enemyCounts[sectionIndex]++;
				}
			}

			UpdateDebugArrays(enemyCounts);
		}

		public float[] GetSectionWeights()
		{
			return _sectionWeights;
		}

		public float GetSectionWeight(int sectionIndex)
		{
			if (sectionIndex >= 1 && sectionIndex <= SpawnerSystemData.SectionCount)
				return _sectionWeights[sectionIndex];

			return 0f;
		}

		private float GetEnemyWeight(PooledEnemy enemy)
		{
			if (enemy?.EnemyData == null)
			{
				Debug.LogWarning($"[{nameof(SpawnerSection)}] Enemy has no EnemyData, using default weight 1f");
				return 1f;
			}

			float weight = enemy.EnemyData.SectionWeight;

			return weight;
		}

		private void ClearSectionWeights()
		{
			for (int i = 1; i <= SpawnerSystemData.SectionCount; i++)
			{
				_sectionWeights[i] = 0f;
			}
		}

		public Vector3 GetSpawnPosition(SpawnerSystemData.SpawnSection section)
		{
			if (_player == null)
				throw new InvalidOperationException("Player reference is required for spawn positioning");

			Vector3 topScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, Mathf.Abs(_camera.transform.position.z - _player.position.z)));
			Vector3 bottomScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0f, Mathf.Abs(_camera.transform.position.z - _player.position.z)));
			float screenHeight = Mathf.Sqrt((topScreen - bottomScreen).sqrMagnitude);
			float circleRadius = screenHeight * ScreenHeightMultiplier + _spawnDistanceFromCenter;

			int sectionIndex = (int)section;
			float angleInRadians = (sectionIndex - 1) * SpawnerSystemData.SectionAngleRadians;

			Vector3 spawnDirection = new Vector3(Mathf.Sin(angleInRadians), Mathf.Cos(angleInRadians), 0f);
			Vector3 spawnPoint = _player.position + spawnDirection * circleRadius;
			spawnPoint.z = _player.position.z;

			return spawnPoint;
		}

		public SpawnerSystemData.SectionSpawnInfo GetSectionSpawnInfo(SpawnerSystemData.SpawnSection section)
		{
			if (_player == null)
				throw new InvalidOperationException("Player reference is required for spawn positioning");

			Vector3 topScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, Mathf.Abs(_camera.transform.position.z - _player.position.z)));
			Vector3 bottomScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0f, Mathf.Abs(_camera.transform.position.z - _player.position.z)));
			float screenHeight = Mathf.Sqrt((topScreen - bottomScreen).sqrMagnitude);
			float circleRadius = screenHeight * ScreenHeightMultiplier + _spawnDistanceFromCenter;

			int sectionIndex = (int)section;
			float startAngle = (sectionIndex - 1) * SpawnerSystemData.SectionAngleRadians;
			float endAngle = sectionIndex * SpawnerSystemData.SectionAngleRadians;

			return new SpawnerSystemData.SectionSpawnInfo(startAngle, endAngle, circleRadius, _player.position, section);
		}

		public SpawnerSystemData.SpawnSection GetSectionByDirection(SpawnerSystemData.SpawnSection direction)
		{
			return direction;
		}

		private SpawnerSystemData.SpawnSection GetSectionByPosition(Vector3 position)
		{
			if (_player == null)
				return SpawnerSystemData.SpawnSection.Section4;

			Vector3 direction = (position - _player.position).normalized;
			float angle = Mathf.Atan2(direction.x, direction.y);

			// Нормализуем угол от 0 до 2π
			if (angle < 0)
				angle += 2f * Mathf.PI;

			int sectionIndex = Mathf.FloorToInt(angle / SpawnerSystemData.SectionAngleRadians);
			sectionIndex = Mathf.Clamp(sectionIndex + 1, 1, SpawnerSystemData.SectionCount);

			return (SpawnerSystemData.SpawnSection)sectionIndex;
		}

		private void UpdateDebugArrays(int[] enemyCounts)
		{
			_debugSectionsInfo.Clear();

			for (int i = 1; i <= SpawnerSystemData.SectionCount; i++)
			{
				_debugSectionsInfo[i] = new SectionDebugInfo
				{
					EnemyCount = enemyCounts[i],
					SectionWeight = _sectionWeights[i]
				};
			}
		}

		private void OnDrawGizmos()
		{
			if (!_showDebugGizmos)
				return;

			if (_player == null || _camera == null)
				return;

			DrawSectorGizmos();
		}

		private void DrawSectorGizmos()
		{
			Vector3 topScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, Mathf.Abs(_camera.transform.position.z - _player.position.z)));
			Vector3 bottomScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0f, Mathf.Abs(_camera.transform.position.z - _player.position.z)));
			float screenHeight = Mathf.Sqrt((topScreen - bottomScreen).sqrMagnitude);
			float circleRadius = screenHeight * ScreenHeightMultiplier + _spawnDistanceFromCenter;

			float angleStep = SpawnerSystemData.SectionAngleRadians;

			float totalWeight = 0f;
			for (int i = 1; i <= SpawnerSystemData.SectionCount; i++)
			{
				totalWeight += _sectionWeights[i];
			}

			for (int i = 1; i <= SpawnerSystemData.SectionCount; i++)
			{
				float startAngle = (i - 1) * angleStep;
				float endAngle = i * angleStep;

				float sectionWeight = _sectionWeights[i];

				float maxWeight = 20f;
				float loadFactor = Mathf.Clamp01(sectionWeight / maxWeight);

				Color sectionColor;
				if (loadFactor <= 0.5f)
				{
					sectionColor = Color.Lerp(Color.white, Color.green, loadFactor * 2f);
				}
				else
				{
					sectionColor = Color.Lerp(Color.green, Color.red, (loadFactor - 0.5f) * 2f);
				}
				sectionColor.a = 0.6f;

				Gizmos.color = sectionColor;

				DrawSector(_player.position, circleRadius, startAngle, endAngle);

				float labelAngle = startAngle + angleStep * 0.5f;
				Vector3 labelPosition = _player.position + new Vector3(
					Mathf.Sin(labelAngle) * (circleRadius + 0.5f),
					Mathf.Cos(labelAngle) * (circleRadius + 0.5f),
					_player.position.z
				);

#if UNITY_EDITOR
				Color weightTextColor = loadFactor <= 0.5f ? Color.green : Color.red;
				Handles.color = weightTextColor;

				int enemyCount = _debugSectionsInfo.ContainsKey(i) ? _debugSectionsInfo[i].EnemyCount : 0;
				Handles.Label(labelPosition, $"S{i}\nEnemies: {enemyCount}\nWeight: {sectionWeight:F1}");
#endif
			}

#if UNITY_EDITOR
			Vector3 totalWeightPosition = _player.position + Vector3.up * 0.5f;
			Handles.color = Color.yellow;
			Handles.Label(totalWeightPosition, $"Total Weight: {totalWeight:F1}");
#endif
		}

		private void DrawSector(Vector3 center, float radius, float startAngle, float endAngle)
		{
			Vector3 startPoint = center + new Vector3(
				Mathf.Sin(startAngle) * radius,
				Mathf.Cos(startAngle) * radius,
				center.z
			);

			Vector3 endPoint = center + new Vector3(
				Mathf.Sin(endAngle) * radius,
				Mathf.Cos(endAngle) * radius,
				center.z
			);

			Gizmos.DrawLine(center, startPoint);
			Gizmos.DrawLine(center, endPoint);
			Gizmos.DrawLine(startPoint, endPoint);
		}
	}
}
