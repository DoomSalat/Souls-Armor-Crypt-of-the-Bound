using UnityEngine;
using Sirenix.OdinInspector;

namespace SpawnerSystem
{
	[System.Serializable]
	public class WaveDifficultyLevel
	{
		[Header("Wave Settings")]
		[MinValue(1)] public int WavePresetCost = 3;
		[MinValue(1)] public int WaveThreshold = 5;

		[Header("Description")]
		[TextArea] public string Description = "Wave difficulty level description";
	}

	public class WaveSystem
	{
		private readonly WaveDifficultyLevel[] _difficultyLevels;
		private int _currentDifficulty = 0;

		private int _wavePresetCost;

		public System.Action<bool> OnWaveModeChanged;
		public System.Action<int> OnDifficultyChanged;

		public WaveSystem(WaveDifficultyLevel[] difficultyLevels)
		{
			_difficultyLevels = difficultyLevels ?? CreateDefaultWaveLevels();
			Initialize();
		}

		private WaveDifficultyLevel[] CreateDefaultWaveLevels()
		{
			return new WaveDifficultyLevel[]
			{
				new WaveDifficultyLevel
				{
					WavePresetCost = 1,
					WaveThreshold = 3,
					Description = "Very Easy level. Cheap presets in waves."
				},

				new WaveDifficultyLevel
				{
					WavePresetCost = 2,
					WaveThreshold = 4,
					Description = "Easy level. Moderate preset cost."
				},

				new WaveDifficultyLevel
				{
					WavePresetCost = 3,
					WaveThreshold = 5,
					Description = "Medium level. Standard preset cost."
				},

				new WaveDifficultyLevel
				{
					WavePresetCost = 4,
					WaveThreshold = 6,
					Description = "Hard level. Expensive presets for experienced players."
				}
			};
		}

		private void Initialize()
		{
			SetDifficulty(0);
		}

		public void SetDifficulty(int level)
		{
			if (level < 0 || level >= _difficultyLevels.Length)
				return;

			_currentDifficulty = level;
			var difficulty = _difficultyLevels[level];

			_wavePresetCost = difficulty.WavePresetCost;

			OnDifficultyChanged?.Invoke(level);

			Debug.Log($"[WaveSystem] Difficulty set to level {level}: {difficulty.Description}");
		}

		public int GetCurrentDifficulty()
		{
			return _currentDifficulty;
		}

		public int GetWavePresetCost()
		{
			return _wavePresetCost;
		}

		public int GetWaveThreshold()
		{
			return _difficultyLevels[_currentDifficulty].WaveThreshold;
		}

		public WaveDifficultyLevel GetCurrentDifficultySettings()
		{
			return _difficultyLevels[_currentDifficulty];
		}

		public int WavePresetCost => _wavePresetCost;
		public int WaveThreshold => _difficultyLevels[_currentDifficulty].WaveThreshold;
	}
}
