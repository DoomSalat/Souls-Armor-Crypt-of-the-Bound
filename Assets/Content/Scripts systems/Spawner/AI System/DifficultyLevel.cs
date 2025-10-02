using UnityEngine;
using Sirenix.OdinInspector;

namespace SpawnerSystem
{
	[System.Serializable]
	public class DifficultyLevel
	{
		[Header("Level Info")]
		[SerializeField] private string _levelName = "Very Easy";
		[SerializeField, TextArea] private string _description = "Very Easy level. Minimum enemies and long spawn intervals.";

		[Header("Token Settings")]
		[SerializeField, MinValue(1)] private int _defaultTokens = 3;

		[Header("Wave Settings")]
		[SerializeField] private bool _enableWaves = true;
		[SerializeField, MinValue(0.1f), ShowIf(nameof(_enableWaves))] private float _waveSpawnInterval = 2f;
		[SerializeField, MinValue(1), ShowIf(nameof(_enableWaves))] private int _waveThreshold = 3;
		[SerializeField, MinValue(1f), ShowIf(nameof(_enableWaves))] private float _waveDuration = 15f;

		public string LevelName => _levelName;
		public string Description => _description;

		public int DefaultTokens => _defaultTokens;

		public bool EnableWaves => _enableWaves;
		public float WaveSpawnInterval => _waveSpawnInterval;
		public int WaveThreshold => _waveThreshold;
		public float WaveDuration => _waveDuration;
	}
}
