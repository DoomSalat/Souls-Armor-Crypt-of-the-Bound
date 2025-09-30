using UnityEngine;
using Sirenix.OdinInspector;

namespace SpawnerSystem
{
	[System.Serializable]
	public class DifficultyLevel
	{
		[Header("Enemy Spawn Weights")]
		[SerializeField, Range(0, 1)] private float _soulWeight = 0.8f;
		[SerializeField, Range(0, 1)] private float _soulVaseWeight = 0.2f;
		[SerializeField, Range(0, 1)] private float _skeletWeight = 0f;
		[SerializeField, Range(0, 1)] private float _knightWeight = 0f;

		[Header("Enemy Count Ranges")]
		[SerializeField, MinMaxSlider(1, 5, true)] private Vector2Int _soulCountRange = new Vector2Int(1, 2);
		[SerializeField, MinMaxSlider(1, 3, true)] private Vector2Int _soulVaseCountRange = new Vector2Int(1, 1);
		[SerializeField, MinMaxSlider(0, 2, true)] private Vector2Int _skeletCountRange = new Vector2Int(0, 0);
		[SerializeField, MinMaxSlider(0, 1, true)] private Vector2Int _knightCountRange = new Vector2Int(0, 0);

		[Header("Spawn Intervals")]
		[SerializeField, MinMaxSlider(1f, 30f, true)] private Vector2 _spawnIntervalRange = new Vector2(8f, 12f);

		[Header("Timer Reduction")]
		[SerializeField, MinValue(0f)] private float _timerReductionPerKill = 1f;
		[SerializeField, MinValue(0f)] private float _timerReductionPerPresetKill = 2f;

		[Header("Wave Settings")]
		[SerializeField, MinValue(0.1f)] private float _waveSpawnInterval = 2f;
		[SerializeField, MinValue(1)] private int _waveThreshold = 3;

		[Header("AFK Settings")]
		[SerializeField, MinValue(0.1f)] private float _afkSecondsPerToken = 2f;

		[Header("Level Info")]
		[SerializeField] private string _levelName = "Very Easy";
		[SerializeField, TextArea] private string _description = "Very Easy level. Minimum enemies and long spawn intervals.";

		public float SoulWeight => _soulWeight;
		public float SoulVaseWeight => _soulVaseWeight;
		public float SkeletWeight => _skeletWeight;
		public float KnightWeight => _knightWeight;

		public Vector2Int SoulCountRange => _soulCountRange;
		public Vector2Int SoulVaseCountRange => _soulVaseCountRange;
		public Vector2Int SkeletCountRange => _skeletCountRange;
		public Vector2Int KnightCountRange => _knightCountRange;

		public Vector2 SpawnIntervalRange => _spawnIntervalRange;

		public float TimerReductionPerKill => _timerReductionPerKill;
		public float TimerReductionPerPresetKill => _timerReductionPerPresetKill;

		public float WaveSpawnInterval => _waveSpawnInterval;
		public int WaveThreshold => _waveThreshold;

		public float AfkSecondsPerToken => _afkSecondsPerToken;

		public string LevelName => _levelName;
		public string Description => _description;
	}
}
