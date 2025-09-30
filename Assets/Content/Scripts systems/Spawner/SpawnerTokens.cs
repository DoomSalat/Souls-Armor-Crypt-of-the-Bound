using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using static VFolders.Libs.VUtils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpawnerSystem
{
	public class SpawnerTokens : MonoBehaviour
	{
		private const int SectionCount = 12;
		private const int EnemyKindCount = 4;
		private const float ScreenHeightMultiplier = 0.5f;
		private const float SectionAngleRadians = Mathf.PI / 4f;
		private const float DefaultWeight = 1f;

		[SerializeField, MinValue(0)] private float _decayPerSecond = 0.25f;
		[SerializeField, MinValue(0)] private float _minCost = 1f;
		[SerializeField, MinValue(0)] private float _spawnDistanceFromCenter = 3f;

		[System.Serializable]
		public class SectionDebugInfo
		{
			public int EnemyCount;
			public float SectionCost;
		}

		[Header("Debug")]
		[SerializeField] private bool _showDebugGizmos = false;
		[SerializeField, ReadOnly] private SerializableDictionary<int, SectionDebugInfo> _debugSectionsInfo = new SerializableDictionary<int, SectionDebugInfo>();

		private Dictionary<EnemyKind, float> _kindWeightsLookup;

		public int Sections => SectionCount;

		private Transform _player;
		private Camera _camera;

		private readonly float[] _costBySection = new float[SectionCount];
		private readonly int[,] _enemyKindCountsBySection = new int[SectionCount, EnemyKindCount];

		private void Awake()
		{
			_camera = Camera.main;

			for (int i = 0; i < _costBySection.Length; i++)
				_costBySection[i] = _minCost;
		}

		public void UpdateDecay()
		{
			float decay = _decayPerSecond * Time.deltaTime;

			for (int i = 0; i < _costBySection.Length; i++)
			{
				_costBySection[i] = Mathf.Max(_minCost, _costBySection[i] - decay);
			}

			UpdateDebugArrays();
		}

		public void Init(Transform player)
		{
			_player = player;
		}

		public void InitWeightsFromSpawnerEnemys(SpawnerEnemys spawnerEnemys)
		{
			if (spawnerEnemys == null)
				return;

			var allEnemyData = spawnerEnemys.AllEnemyData;

			_kindWeightsLookup = new Dictionary<EnemyKind, float>();

			foreach (var enemyData in allEnemyData)
			{
				if (enemyData != null)
				{
					_kindWeightsLookup[enemyData.EnemyKind] = enemyData.SectionWeight;
				}
			}
		}

		public float[] GetSectionWeights()
		{
			return _costBySection;
		}

		public float GetSectionWeight(int sectionIndex)
		{
			if (sectionIndex >= 0 && sectionIndex < SectionCount)
				return _costBySection[sectionIndex];
			return _minCost;
		}

		public void Commit(SpawnSection section, int enemiesSpawned, float costPerEnemy = 1f)
		{
			if (enemiesSpawned <= 0)
				return;

			int index = (int)section;
			float powerAdded = enemiesSpawned * Mathf.Max(costPerEnemy, _minCost);
			_costBySection[index] += powerAdded;

			UpdateDebugArrays();
		}

		public void Release(SpawnSection section, int enemiesReturned, float costPerEnemy = 1f)
		{
			if (enemiesReturned <= 0)
				return;

			int index = (int)section;
			_costBySection[index] = Mathf.Max(_minCost, _costBySection[index] - enemiesReturned * costPerEnemy);

			UpdateDebugArrays();
		}

		public int[] GetEnemyCountsBySection(SpawnSection section)
		{
			int sectionIndex = (int)section;
			int[] counts = new int[EnemyKindCount];

			for (int i = 0; i < EnemyKindCount; i++)
			{
				counts[i] = _enemyKindCountsBySection[sectionIndex, i];
			}

			return counts;
		}

		public int GetEnemyCountBySectionAndKind(SpawnSection section, EnemyKind kind)
		{
			int sectionIndex = (int)section;
			int kindIndex = (int)kind;

			if (sectionIndex >= 0 && sectionIndex < SectionCount &&
				kindIndex >= 0 && kindIndex < EnemyKindCount)
			{
				return _enemyKindCountsBySection[sectionIndex, kindIndex];
			}

			return 0;
		}

		public float GetKindWeight(EnemyKind kind)
		{
			if (_kindWeightsLookup != null && _kindWeightsLookup.TryGetValue(kind, out float weight))
			{
				return Mathf.Max(0f, weight);
			}

			return DefaultWeight;
		}

		public Vector3 GetSpawnPosition(SpawnSection section)
		{
			if (_player == null)
				throw new InvalidOperationException("Player reference is required for spawn positioning");

			Vector3 topScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, Mathf.Abs(_camera.transform.position.z - _player.position.z)));
			Vector3 bottomScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0f, Mathf.Abs(_camera.transform.position.z - _player.position.z)));
			float screenHeight = Mathf.Sqrt((topScreen - bottomScreen).sqrMagnitude);
			float circleRadius = screenHeight * ScreenHeightMultiplier + _spawnDistanceFromCenter;

			float angleInRadians = (int)section * SectionAngleRadians;

			Vector3 spawnDirection = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f);
			Vector3 spawnPoint = _player.position + spawnDirection * circleRadius;
			spawnPoint.z = _player.position.z;

			return spawnPoint;
		}

		public SpawnSection GetSectionByDirection(SpawnDirection direction)
		{
			return (SpawnSection)((int)direction);
		}

		public void RecalculateSectionsByEnemyPositions(PooledEnemy[] activeEnemies)
		{
			for (int i = 0; i < SectionCount; i++)
			{
				for (int j = 0; j < EnemyKindCount; j++)
				{
					_enemyKindCountsBySection[i, j] = 0;
				}
			}

			foreach (var enemy in activeEnemies)
			{
				if (enemy == null || enemy.gameObject == null)
					continue;

				var section = GetSectionByPosition(enemy.transform.position);
				int sectionIndex = (int)section;

				var spawnMeta = enemy.GetComponent<EnemySpawnMeta>();
				if (spawnMeta == null)
					continue;

				int kindIndex = (int)spawnMeta.Kind;

				if (sectionIndex >= 0 && sectionIndex < SectionCount &&
					kindIndex >= 0 && kindIndex < EnemyKindCount)
				{
					_enemyKindCountsBySection[sectionIndex, kindIndex]++;
				}
			}

			UpdateDebugArrays();
		}

		private SpawnSection GetSectionByPosition(Vector3 position)
		{
			if (_player == null)
				return SpawnSection.Right;

			Vector3 direction = (position - _player.position).normalized;
			float angle = Mathf.Atan2(direction.y, direction.x);

			// normalize angle from 0 to 2π
			if (angle < 0)
				angle += 2f * Mathf.PI;

			int sectionIndex = Mathf.FloorToInt(angle / (2f * Mathf.PI / SectionCount));
			sectionIndex = Mathf.Clamp(sectionIndex, 0, SectionCount - 1);

			return (SpawnSection)sectionIndex;
		}

		private void UpdateDebugArrays()
		{
			_debugSectionsInfo.Clear();

			for (int i = 0; i < SectionCount; i++)
			{
				int totalEnemies = 0;
				for (int j = 0; j < EnemyKindCount; j++)
				{
					totalEnemies += _enemyKindCountsBySection[i, j];
				}

				_debugSectionsInfo[i] = new SectionDebugInfo
				{
					EnemyCount = totalEnemies,
					SectionCost = _costBySection[i]
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

			float angleStep = 2f * Mathf.PI / SectionCount;

			for (int i = 0; i < SectionCount; i++)
			{
				float startAngle = i * angleStep;
				float endAngle = (i + 1) * angleStep;

				float sectionCost = _minCost;
				if (_debugSectionsInfo.TryGetValue(i, out var debugInfo))
				{
					sectionCost = debugInfo.SectionCost;
				}

				float maxCost = 20f;
				float loadFactor = Mathf.Clamp01(sectionCost / maxCost);

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
					Mathf.Cos(labelAngle) * (circleRadius + 0.5f),
					Mathf.Sin(labelAngle) * (circleRadius + 0.5f),
					_player.position.z
				);

#if UNITY_EDITOR
				Handles.Label(labelPosition, $"S{i}\n{sectionCost:F1}");
#endif
			}
		}

		private void DrawSector(Vector3 center, float radius, float startAngle, float endAngle)
		{
			Vector3 startPoint = center + new Vector3(
				Mathf.Cos(startAngle) * radius,
				Mathf.Sin(startAngle) * radius,
				center.z
			);

			Vector3 endPoint = center + new Vector3(
				Mathf.Cos(endAngle) * radius,
				Mathf.Sin(endAngle) * radius,
				center.z
			);

			Gizmos.DrawLine(center, startPoint);
			Gizmos.DrawLine(center, endPoint);
			Gizmos.DrawLine(startPoint, endPoint);
		}
	}
}
