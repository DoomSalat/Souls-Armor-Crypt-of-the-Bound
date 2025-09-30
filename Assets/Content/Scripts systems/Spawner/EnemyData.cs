using UnityEngine;
using Sirenix.OdinInspector;

namespace SpawnerSystem
{
	[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Spawner System/Enemy Data")]
	public class EnemyData : ScriptableObject
	{
		[Header("Enemy Info")]
		[SerializeField] private PooledEnemy _prefab;
		[SerializeField] private EnemyKind _enemyKind;

		[Header("Token Settings")]
		[SerializeField, MinValue(1)] private int _tokenValue = 1;
		[SerializeField, MinValue(0)] private int _difficultyLevel = 0;

		[Header("Section Weight")]
		[SerializeField, Range(0f, 10f)] private float _sectionWeight = 1f;

		[Header("Timer Settings")]
		[SerializeField, MinValue(0.1f)] private float _spawnCooldown = 2f;
		[SerializeField, MinValue(0f)] private float _timerReduction = 0.5f;

		[Header("Spawn Settings")]
		[SerializeField, MinValue(1)] private int _minSpawnCount = 1;
		[SerializeField, MinValue(1)] private int _maxSpawnCount = 1;
		[SerializeField, Range(0f, 1f)] private float _spawnChance = 1f;

		public PooledEnemy Prefab => _prefab;
		public EnemyKind EnemyKind => _enemyKind;

		public int TokenValue => _tokenValue;
		public int DifficultyLevel => _difficultyLevel;

		public float SectionWeight => _sectionWeight;

		public float SpawnCooldown => _spawnCooldown;
		public float TimerReduction => _timerReduction;

		public int MinSpawnCount => _minSpawnCount;
		public int MaxSpawnCount => _maxSpawnCount;
		public float SpawnChance => _spawnChance;

		public int GetRandomSpawnCount()
		{
			return Random.Range(_minSpawnCount, _maxSpawnCount + 1);
		}

		public bool ShouldSpawn()
		{
			return Random.value <= _spawnChance;
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (_maxSpawnCount < _minSpawnCount)
				_maxSpawnCount = _minSpawnCount;

			if (_spawnChance < 0f) _spawnChance = 0f;
			if (_spawnChance > 1f) _spawnChance = 1f;

			if (_sectionWeight < 0f) _sectionWeight = 0f;
			if (_spawnCooldown < 0.1f) _spawnCooldown = 0.1f;
			if (_timerReduction < 0f) _timerReduction = 0f;
		}
#endif
	}
}
