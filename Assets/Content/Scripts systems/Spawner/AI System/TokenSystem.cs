using UnityEngine;
using Sirenix.OdinInspector;

namespace SpawnerSystem
{
	[System.Serializable]
	public class TokenDifficultyLevel
	{
		[Header("Token Settings")]
		[MinValue(1)] public int DefaultTokens = 3;
		[MinValue(1)] public int MaxTokens = 10;

		[Header("Wave Settings")]
		[MinValue(1)] public int WaveThreshold = 5;
		public float WaveDuration = 15f;

		[Header("Timer Settings")]
		[MinValue(0.1f)] public float AfkSecondsPerToken = 2f;

		[Header("Description")]
		[TextArea] public string Description = "Difficulty level description";
	}

	public class TokenSystem
	{
		private readonly TokenDifficultyLevel[] _difficultyLevels;
		private int _currentDifficulty = 0;

		private int _currentTokens;
		private int _returnedTokens;
		private bool _isWaveMode;
		private float _waveStartTime;
		private float _waveDuration;

		public System.Action<int> OnTokensChanged;
		public System.Action<bool> OnWaveModeChanged;
		public System.Action<int> OnDifficultyChanged;

		public TokenSystem(TokenDifficultyLevel[] difficultyLevels)
		{
			_difficultyLevels = difficultyLevels ?? CreateDefaultDifficultyLevels();
			Initialize();
		}

		private TokenDifficultyLevel[] CreateDefaultDifficultyLevels()
		{
			return new TokenDifficultyLevel[]
			{
				new TokenDifficultyLevel
				{
					DefaultTokens = 3,
					MaxTokens = 10,
					WaveThreshold = 3,
					WaveDuration = 10f,
					AfkSecondsPerToken = 2f,
					Description = "Very Easy level. Minimum tokens and waves."
				},

				new TokenDifficultyLevel
				{
					DefaultTokens = 4,
					MaxTokens = 12,
					WaveThreshold = 4,
					WaveDuration = 15f,
					AfkSecondsPerToken = 2f,
					Description = "Easy level. Increased token amount."
				},

				new TokenDifficultyLevel
				{
					DefaultTokens = 5,
					MaxTokens = 15,
					WaveThreshold = 5,
					WaveDuration = 20f,
					AfkSecondsPerToken = 1.8f,
					Description = "Medium level. Balance between difficulty and accessibility."
				},

				new TokenDifficultyLevel
				{
					DefaultTokens = 6,
					MaxTokens = 20,
					WaveThreshold = 6,
					WaveDuration = 25f,
					AfkSecondsPerToken = 1.5f,
					Description = "Hard level. Maximum possibilities for experienced players."
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

			ResetTokens();

			_waveDuration = difficulty.WaveDuration;

			OnDifficultyChanged?.Invoke(level);

			Debug.Log($"[TokenSystem] Difficulty set to level {level}: {difficulty.Description}");
		}

		public int GetCurrentDifficulty()
		{
			return _currentDifficulty;
		}

		public TokenDifficultyLevel GetCurrentDifficultySettings()
		{
			return _difficultyLevels[_currentDifficulty];
		}

		public void AddTokens(int amount)
		{
			var difficulty = _difficultyLevels[_currentDifficulty];
			_currentTokens = Mathf.Min(_currentTokens + amount, difficulty.MaxTokens);

			OnTokensChanged?.Invoke(_currentTokens);
			CheckWaveMode();

			Debug.Log($"[TokenSystem] Tokens added: {amount}, Current: {_currentTokens}/{difficulty.MaxTokens}");
		}

		public void SpendTokens(int amount)
		{
			_currentTokens = Mathf.Max(_currentTokens - amount, 0);

			OnTokensChanged?.Invoke(_currentTokens);

			Debug.Log($"[TokenSystem] Tokens spent: {amount}, Current: {_currentTokens}");
		}

		public void ReturnTokens(int amount)
		{
			_returnedTokens += amount;

			Debug.Log($"[TokenSystem] Tokens returned: {amount}, Returned: {_returnedTokens}");
		}

		public bool CanAfford(int cost)
		{
			return _currentTokens >= cost;
		}

		public bool CanUsePresets()
		{
			return _returnedTokens > 0;
		}

		private void StartWaveMode()
		{
			_isWaveMode = true;
			_waveStartTime = Time.time;

			OnWaveModeChanged?.Invoke(true);

			Debug.Log($"[TokenSystem] Wave mode started! Tokens: {_returnedTokens}/{GetWaveThreshold()}");
		}

		private void EndWaveMode()
		{
			_isWaveMode = false;
			ResetTokens();

			OnWaveModeChanged?.Invoke(false);

			Debug.Log("[TokenSystem] Wave mode ended!");
		}

		private void CheckWaveMode()
		{
			if (!_isWaveMode && _returnedTokens >= GetWaveThreshold())
			{
				StartWaveMode();
			}
			else if (_isWaveMode && Time.time - _waveStartTime >= _waveDuration)
			{
				EndWaveMode();
			}
		}

		private int GetWaveThreshold()
		{
			return _difficultyLevels[_currentDifficulty].WaveThreshold;
		}

		public float GetAfkTimePerToken()
		{
			return _difficultyLevels[_currentDifficulty].AfkSecondsPerToken;
		}

		private void ResetTokens()
		{
			var difficulty = _difficultyLevels[_currentDifficulty];
			_currentTokens = difficulty.DefaultTokens;
			_returnedTokens = 0;

			OnTokensChanged?.Invoke(_currentTokens);
		}

		public void Update()
		{
			CheckWaveMode();
		}

		public int CurrentTokens => _currentTokens;
		public int ReturnedTokens => _returnedTokens;
		public bool IsWaveMode => _isWaveMode;
		public float WaveTimeRemaining => _isWaveMode ? Mathf.Max(0, _waveStartTime + _waveDuration - Time.time) : 0f;
	}
}
