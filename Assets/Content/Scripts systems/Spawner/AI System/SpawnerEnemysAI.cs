using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System.Collections.Generic;

namespace SpawnerSystem
{
	[RequireComponent(typeof(SpawnerTokens), typeof(SpawnerEnemys))]
	public class SpawnerEnemysAI : MonoBehaviour
	{
		[Header("Difficulty Levels")]
		[SerializeField] private DifficultyLevel[] _difficultyLevels;
		[SerializeField, MinValue(0)] private int _currentDifficultyIndex = 0;

		[Header("Preset System")]
		[SerializeField] private PresetData[] _availablePresets;

		[Header("AI Settings")]
		[SerializeField, MinValue(0)] private int _maxSectionDeviation = 2;

		[Header("Glitch Spawn System")]
		[SerializeField, Range(0, 1)] private float _glitchAccumulationRate = 0.02f;
		[SerializeField, MinValue(1)] private Vector2 _glitchAccumulationTime = new(5f, 10f);
		[SerializeField, Range(0, 1)] private float _maxGlitchChance = 1f;

		[Header("Debug")]
		[SerializeField] private bool _showDebugInfo = false;
		[SerializeField] private bool _disableWaveSystem = false;

		private SpawnerTokens _spawnerTokens;
		private SpawnerEnemys _spawnerEnemys;

		private DifficultyLevel _currentDifficultyLevel;
		private Dictionary<string, int> _presetCooldowns = new Dictionary<string, int>();

		private float _currentTimer;
		private float _nextSpawnTime;
		private float _lastTimerDuration;

		[ShowInInspector, ReadOnly] private float _currentGlitchChance = 0f;
		private float _nextAccumulationTime;

		private float _currentTokens = 0f;
		private int _returnedTokens = 0;

		public int CurrentDifficultyIndex => _currentDifficultyIndex;
		public DifficultyLevel CurrentDifficultyLevel => _currentDifficultyLevel;
		public float CurrentTokens => _currentTokens;
		public int ReturnedTokens => _returnedTokens;
		public bool CanUsePresets => _returnedTokens > 0;
		public bool IsWaveMode => !_disableWaveSystem && _currentDifficultyLevel.EnableWaves && _returnedTokens >= _currentDifficultyLevel.WaveThreshold;

		private void Awake()
		{
			if (!enabled)
				return;

			_spawnerTokens = GetComponent<SpawnerTokens>();
			_spawnerEnemys = GetComponent<SpawnerEnemys>();

			InitializeDifficultySystem();
			InitializeTimerSystem();
			InitializeGlitchSystem();
			SubscribeToEvents();
		}

		private void Update()
		{
			UpdateTimer();

			if (Time.time >= _nextAccumulationTime)
			{
				AccumulateGlitchChance();
			}

			if (ShouldSpawn())
			{
				ProcessSpawnCycle();
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (_glitchAccumulationRate < 0f) _glitchAccumulationRate = 0f;
			if (_glitchAccumulationRate > 1f) _glitchAccumulationRate = 1f;
			if (_glitchAccumulationTime.x < 1f) _glitchAccumulationTime.x = 1f;
			if (_glitchAccumulationTime.y < _glitchAccumulationTime.x) _glitchAccumulationTime.y = _glitchAccumulationTime.x;
			if (_maxGlitchChance < 0f) _maxGlitchChance = 0f;
			if (_maxGlitchChance > 1f) _maxGlitchChance = 1f;

			if (_maxSectionDeviation < 0) _maxSectionDeviation = 0;
		}
#endif

		private void OnDestroy()
		{
			UnsubscribeFromEvents();
		}

		public void IncreaseDifficulty()
		{
			if (_currentDifficultyIndex < _difficultyLevels.Length - 1)
			{
				SetDifficulty(_currentDifficultyIndex + 1);
			}
		}

		public void ResetDifficulty()
		{
			SetDifficulty(0);
		}

		private void SubscribeToEvents()
		{
			_spawnerEnemys.EnemyReturnedToPoolEvent += OnEnemyReturnedToPool;
		}

		private void UnsubscribeFromEvents()
		{
			if (_spawnerEnemys != null)
				_spawnerEnemys.EnemyReturnedToPoolEvent -= OnEnemyReturnedToPool;
		}

		private void OnEnemyReturnedToPool(PooledEnemy enemy)
		{
			if (enemy == null || enemy.SpawnMeta == null)
				return;

			var meta = enemy.SpawnMeta;

			float tokensGained = meta.TokensToReturn;
			_currentTokens = Mathf.Min(_currentTokens + tokensGained, _currentDifficultyLevel.DefaultTokens);
			_returnedTokens += Mathf.RoundToInt(tokensGained);

			_nextSpawnTime -= meta.TimerReductionOnDeath;
			_currentTimer = Mathf.Max(0.1f, _nextSpawnTime - Time.time);

			if (_showDebugInfo)
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] {meta.Kind} died, +{tokensGained:F2} tokens (total: {_currentTokens:F2}/{_currentDifficultyLevel.DefaultTokens}), returned: {_returnedTokens}, timer reduced by {meta.TimerReductionOnDeath:F1}s");
		}

		private void InitializeDifficultySystem()
		{
			if (_difficultyLevels == null || _difficultyLevels.Length == 0)
			{
				Debug.LogError($"[{nameof(SpawnerEnemysAI)}] No difficulty levels configured!");
				return;
			}

			_currentDifficultyIndex = Mathf.Clamp(_currentDifficultyIndex, 0, _difficultyLevels.Length - 1);
			_currentDifficultyLevel = _difficultyLevels[_currentDifficultyIndex];
			_currentTokens = _currentDifficultyLevel.DefaultTokens;
			_returnedTokens = 0;

			InitializePresetCooldowns();

			if (_showDebugInfo)
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Difficulty initialized: {_currentDifficultyLevel.LevelName}, Tokens: {_currentTokens}");
		}

		private void InitializePresetCooldowns()
		{
			_presetCooldowns.Clear();

			if (_availablePresets == null)
				return;

			foreach (var preset in _availablePresets)
			{
				if (preset != null && preset.DifficultyLevel == _currentDifficultyIndex)
				{
					_presetCooldowns[preset.PresetName] = 0;
				}
			}

			if (_showDebugInfo)
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Preset cooldowns initialized for difficulty {_currentDifficultyIndex}");
		}

		private void InitializeTimerSystem()
		{
			_nextSpawnTime = Time.time;
			_currentTimer = 0f;
			_lastTimerDuration = 0f;
		}

		private void SetDifficulty(int level)
		{
			_currentDifficultyIndex = Mathf.Clamp(level, 0, _difficultyLevels.Length - 1);
			_currentDifficultyLevel = _difficultyLevels[_currentDifficultyIndex];

			_currentTokens = _currentDifficultyLevel.DefaultTokens;
			_returnedTokens = 0;

			_nextSpawnTime = Time.time;
			_currentTimer = 0f;
			_lastTimerDuration = 0f;

			InitializePresetCooldowns();

			if (_showDebugInfo)
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Difficulty set to level {level}: {_currentDifficultyLevel.LevelName} - {_currentDifficultyLevel.Description}");
		}

		private bool ShouldSpawn()
		{
			return Time.time >= _nextSpawnTime;
		}

		private SpawnResult SpawnPreset(PresetData presetData)
		{
			if (presetData == null)
				return new SpawnResult { Success = false };

			if (!SpendTokens(presetData.TokenCost))
				return new SpawnResult { Success = false };

			SetPresetCooldown(presetData.PresetName, presetData.CooldownCycles);
			SpawnSection mainSection = SelectSection();

			var placements = CreatePresetPlacements(presetData, mainSection);

			var spawnedEnemies = _spawnerEnemys.SpawnPresetEnemies(
				placements.ToArray(),
				presetData.TokenCost / (float)placements.Count,
				presetData.PresetCooldown / (float)placements.Count
			);

			return new SpawnResult
			{
				Success = true,
				IsPreset = true,
				Enemies = presetData.EnemyPlacements.SelectMany(p => Enumerable.Repeat(p.EnemyKind, p.Count)).ToArray(),
				SpawnedEnemies = spawnedEnemies,
				TotalCooldown = presetData.PresetCooldown,
				TokensSpent = presetData.TokenCost
			};
		}

		private List<PresetEnemyInfo> CreatePresetPlacements(PresetData presetData, SpawnSection mainSection)
		{
			var placements = new List<PresetEnemyInfo>();

			foreach (var placement in presetData.EnemyPlacements)
			{
				for (int i = 0; i < placement.Count; i++)
				{
					int absoluteSection = CalculateAbsoluteSection((int)mainSection, placement.Section);

					SoulType soulType = placement.SoulType;
					if (soulType == SoulType.Random)
					{
						soulType = presetData.GetRandomSoulType(_spawnerEnemys);
					}

					placements.Add(new PresetEnemyInfo
					{
						EnemyKind = placement.EnemyKind,
						SoulType = soulType,
						Section = (SpawnSection)absoluteSection
					});
				}
			}

			return placements;
		}

		private void UpdateSpawnTimer(PresetData presetData)
		{
			_lastTimerDuration = presetData.PresetCooldown;
			_nextSpawnTime = Time.time + presetData.PresetCooldown;
			_currentTimer = presetData.PresetCooldown;
		}

		private void UpdateTimer()
		{
			if (_currentTimer > 0)
			{
				_currentTimer = Mathf.Max(0, _nextSpawnTime - Time.time);
			}
		}

		private void InitializeGlitchSystem()
		{
			_currentGlitchChance = 0f;
			_nextAccumulationTime = Time.time + UnityEngine.Random.Range(_glitchAccumulationTime.x, _glitchAccumulationTime.y);
		}

		private void AccumulateGlitchChance()
		{
			_currentGlitchChance += _glitchAccumulationRate;
			_currentGlitchChance = Mathf.Min(_currentGlitchChance, _maxGlitchChance);

			_nextAccumulationTime = Time.time + UnityEngine.Random.Range(_glitchAccumulationTime.x, _glitchAccumulationTime.y);

			if (_showDebugInfo)
			{
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Glitch chance accumulated: {_currentGlitchChance:P1} (next accumulation in {_nextAccumulationTime - Time.time:F1}s)");
			}
		}

		private SpawnSection SelectSection()
		{
			if (ShouldTriggerGlitch())
			{
				var glitchSection = (SpawnSection)UnityEngine.Random.Range(0, _spawnerTokens.Sections);

				if (_showDebugInfo)
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Glitch triggered! Random section: {glitchSection}");

				return glitchSection;
			}

			return SelectNewTargetSection();
		}

		private bool ShouldTriggerGlitch()
		{
			bool shouldGlitch = UnityEngine.Random.value < _currentGlitchChance;

			if (shouldGlitch)
			{
				_currentGlitchChance = 0f;
				_nextAccumulationTime = Time.time + UnityEngine.Random.Range(_glitchAccumulationTime.x, _glitchAccumulationTime.y);

				if (_showDebugInfo)
				{
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Glitch triggered! Chance was {_currentGlitchChance:P1}, reset to 0%");
				}
			}

			return shouldGlitch;
		}

		private SpawnSection SelectNewTargetSection()
		{
			float[] sectionWeights = _spawnerTokens.GetSectionWeights();

			int highestPowerSection = 0;
			float highestPower = sectionWeights[0];

			for (int i = 1; i < sectionWeights.Length; i++)
			{
				if (sectionWeights[i] > highestPower)
				{
					highestPower = sectionWeights[i];
					highestPowerSection = i;
				}
			}

			int oppositeSection = (highestPowerSection + _spawnerTokens.Sections / 2) % _spawnerTokens.Sections;

			int deviation = UnityEngine.Random.Range(-_maxSectionDeviation, _maxSectionDeviation + 1);
			int targetSection = (oppositeSection + deviation + _spawnerTokens.Sections) % _spawnerTokens.Sections;

			if (_showDebugInfo)
			{
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Selected section {targetSection} " +
					$"(opposite to highest power section: {highestPowerSection}, deviation: {deviation})");
			}

			return (SpawnSection)targetSection;
		}

		private bool CanAffordEnemy(EnemyKind enemyKind)
		{
			var enemyData = _spawnerEnemys.GetEnemyData(enemyKind);
			if (enemyData == null)
				return false;

			return _currentTokens >= enemyData.TokenValue;
		}

		private bool SpendTokens(float tokenCost)
		{
			if (_currentTokens >= tokenCost)
			{
				_currentTokens -= tokenCost;
				_returnedTokens += Mathf.RoundToInt(tokenCost);

				if (_showDebugInfo)
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Spent {tokenCost:F2} tokens, current: {_currentTokens:F2}, returned: {_returnedTokens}");

				return true;
			}

			return false;
		}

		private EnemyKind[] GetAvailableEnemiesForCurrentDifficulty()
		{
			return _spawnerEnemys.GetAvailableEnemiesForDifficulty(_currentDifficultyIndex);
		}

		private EnemyKind SelectRandomAvailableEnemy()
		{
			var availableEnemies = GetAvailableEnemiesForCurrentDifficulty();
			if (availableEnemies.Length == 0)
				return EnemyKind.Soul;

			return availableEnemies[Random.Range(0, availableEnemies.Length)];
		}

		private PresetData SelectAvailablePreset()
		{
			if (_availablePresets == null)
				return null;

			var availablePresets = new List<PresetData>();

			foreach (var preset in _availablePresets)
			{
				if (preset != null &&
					preset.DifficultyLevel == _currentDifficultyIndex &&
					_presetCooldowns.ContainsKey(preset.PresetName) &&
					_presetCooldowns[preset.PresetName] <= 0 &&
					CanAffordPreset(preset))
				{
					availablePresets.Add(preset);
				}
			}

			if (availablePresets.Count == 0)
				return null;

			float[] sectionWeights = _spawnerTokens.GetSectionWeights();
			bool allWeightsZero = sectionWeights.All(weight => weight <= 0.1f);

			if (allWeightsZero)
			{
				return SelectMostExpensivePreset(availablePresets);
			}
			else
			{
				return SelectBalancingPreset(availablePresets, sectionWeights);
			}
		}

		private bool CanAffordPreset(PresetData preset)
		{
			return _currentTokens >= preset.TokenCost;
		}

		private PresetData SelectMostExpensivePreset(List<PresetData> availablePresets)
		{
			PresetData mostExpensive = null;
			int maxCost = 0;

			foreach (var preset in availablePresets)
			{
				if (preset.TokenCost > maxCost)
				{
					maxCost = preset.TokenCost;
					mostExpensive = preset;
				}
			}

			return mostExpensive;
		}

		private PresetData SelectBalancingPreset(List<PresetData> availablePresets, float[] sectionWeights)
		{
			int weakestSection = FindWeakestSection(sectionWeights);

			PresetData bestPreset = null;
			float bestScore = float.MinValue;
			var candidatesWithSameScore = new List<PresetData>();

			foreach (var preset in availablePresets)
			{
				float score = CalculatePresetScoreForWeakSection(preset, sectionWeights, weakestSection);

				if (score > bestScore)
				{
					bestScore = score;
					bestPreset = preset;
					candidatesWithSameScore.Clear();
					candidatesWithSameScore.Add(preset);
				}
				else if (Mathf.Approximately(score, bestScore))
				{
					candidatesWithSameScore.Add(preset);
				}
			}

			if (candidatesWithSameScore.Count > 1)
			{
				return candidatesWithSameScore[Random.Range(0, candidatesWithSameScore.Count)];
			}

			return bestPreset;
		}

		private int FindWeakestSection(float[] sectionWeights)
		{
			int weakestSection = 0;
			float minWeight = sectionWeights[0];

			for (int i = 1; i < sectionWeights.Length; i++)
			{
				if (sectionWeights[i] < minWeight)
				{
					minWeight = sectionWeights[i];
					weakestSection = i;
				}
			}

			return weakestSection;
		}

		private float CalculatePresetScoreForWeakSection(PresetData preset, float[] currentSectionWeights, int weakestSection)
		{
			if (preset.EnemyPlacements == null)
				return float.MinValue;

			float[] presetWeights = preset.GetSectionWeights();
			float[] simulatedWeights = new float[_spawnerTokens.Sections + 1]; // 0-Sections, где 0 всегда пустой

			for (int i = 1; i <= _spawnerTokens.Sections; i++)
			{
				if (i - 1 < currentSectionWeights.Length)
				{
					simulatedWeights[i] = currentSectionWeights[i - 1];
				}
			}

			for (int i = 1; i <= _spawnerTokens.Sections; i++)
			{
				simulatedWeights[i] += presetWeights[i];
			}

			float mean = simulatedWeights.Skip(1).Average();
			float variance = 0f;

			for (int i = 1; i <= _spawnerTokens.Sections; i++)
			{
				float diff = simulatedWeights[i] - mean;
				variance += diff * diff;
			}

			variance /= _spawnerTokens.Sections;

			float weakSectionBonus = 0f;
			int weakSectionIndex = weakestSection + 1;
			if (weakSectionIndex >= 1 && weakSectionIndex <= _spawnerTokens.Sections && presetWeights[weakSectionIndex] > 0)
			{
				weakSectionBonus = presetWeights[weakSectionIndex] * 2.0f;
			}

			return weakSectionBonus - variance;
		}

		private float CalculatePresetUniformityScore(PresetData preset, float[] currentSectionWeights)
		{
			if (preset.EnemyPlacements == null)
				return float.MaxValue;

			float[] presetWeights = preset.GetSectionWeights();
			float[] simulatedWeights = new float[_spawnerTokens.Sections + 1];

			for (int i = 1; i <= _spawnerTokens.Sections; i++)
			{
				if (i - 1 < currentSectionWeights.Length)
				{
					simulatedWeights[i] = currentSectionWeights[i - 1];
				}
			}

			for (int i = 1; i <= _spawnerTokens.Sections; i++)
			{
				simulatedWeights[i] += presetWeights[i];
			}

			var highWeightSections = new List<int>();
			float maxWeight = simulatedWeights.Skip(1).Max();

			for (int i = 1; i <= _spawnerTokens.Sections; i++)
			{
				if (simulatedWeights[i] >= maxWeight * 0.8f)
				{
					highWeightSections.Add(i);
				}
			}

			int weakestSection = 1;
			float minWeight = simulatedWeights[1];

			for (int i = 1; i <= _spawnerTokens.Sections; i++)
			{
				if (simulatedWeights[i] < minWeight)
				{
					minWeight = simulatedWeights[i];
					weakestSection = i;
				}
			}

			float mean = simulatedWeights.Skip(1).Average();
			float variance = 0f;

			for (int i = 1; i <= _spawnerTokens.Sections; i++)
			{
				float diff = simulatedWeights[i] - mean;
				variance += diff * diff;
			}

			variance /= _spawnerTokens.Sections;

			float weakSectionBonus = 0f;
			if (presetWeights[weakestSection] > 0)
			{
				weakSectionBonus = presetWeights[weakestSection] * 0.5f;
			}

			return variance - weakSectionBonus;
		}

		private void ReduceAllPresetCooldowns()
		{
			foreach (var key in _presetCooldowns.Keys.ToList())
			{
				if (_presetCooldowns[key] > 0)
				{
					_presetCooldowns[key]--;
				}
			}

			if (_showDebugInfo)
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] All preset cooldowns reduced by 1");
		}

		private void SetPresetCooldown(string presetName, int cooldown)
		{
			if (_presetCooldowns.ContainsKey(presetName))
			{
				_presetCooldowns[presetName] = cooldown;
			}
		}

		private void ProcessSpawnCycle()
		{
			while (_currentTokens > 0)
			{
				var selectedPreset = SelectAvailablePreset();

				if (selectedPreset == null)
				{
					ReduceAllPresetCooldowns();

					if (_showDebugInfo)
						Debug.Log($"[{nameof(SpawnerEnemysAI)}] All presets on cooldown, skipping turn and reducing cooldowns");

					break;
				}

				if (!CanAffordPreset(selectedPreset))
				{
					if (_showDebugInfo)
						Debug.Log($"[{nameof(SpawnerEnemysAI)}] Cannot afford preset '{selectedPreset.PresetName}' (cost: {selectedPreset.TokenCost}, tokens: {_currentTokens:F2})");
					break;
				}

				var spawnResult = SpawnPreset(selectedPreset);

				if (!spawnResult.Success)
				{
					if (_showDebugInfo)
						Debug.Log($"[{nameof(SpawnerEnemysAI)}] Failed to spawn preset '{selectedPreset.PresetName}'");
					break;
				}

				UpdateSpawnTimer(selectedPreset);

				if (_showDebugInfo)
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Preset '{selectedPreset.PresetName}' spawned successfully, {selectedPreset.TokenCost} tokens spent, {selectedPreset.PresetCooldown:F1}s cooldown");
			}
		}

		private PresetSpawnResult ProcessPresetSpawn(PresetData presetData)
		{
			if (presetData == null)
				return new PresetSpawnResult { Success = false };

			if (!ShouldSpawn())
				return new PresetSpawnResult { Success = false };

			if (!SpendTokens(presetData.TokenCost))
				return new PresetSpawnResult { Success = false };

			SpawnSection mainSection = SelectSection();

			var placements = new System.Collections.Generic.List<PresetEnemyInfo>();

			foreach (var placement in presetData.EnemyPlacements)
			{
				for (int i = 0; i < placement.Count; i++)
				{
					int absoluteSection = CalculateAbsoluteSection((int)mainSection, placement.Section);

					SoulType soulType = placement.SoulType;
					if (soulType == SoulType.Random)
					{
						soulType = presetData.GetRandomSoulType(_spawnerEnemys);
					}

					placements.Add(new PresetEnemyInfo
					{
						EnemyKind = placement.EnemyKind,
						SoulType = soulType,
						Section = (SpawnSection)absoluteSection
					});
				}
			}

			_lastTimerDuration = presetData.PresetCooldown;
			_nextSpawnTime = Time.time + presetData.PresetCooldown;
			_currentTimer = presetData.PresetCooldown;

			int totalEnemies = placements.Count;
			float tokensPerEnemy = totalEnemies > 0 ? presetData.TokenCost / (float)totalEnemies : 0f;
			float timerPerEnemy = totalEnemies > 0 ? presetData.PresetCooldown / (float)totalEnemies : 0f;

			var spawnedEnemies = _spawnerEnemys.SpawnPresetEnemies(placements.ToArray(), tokensPerEnemy, timerPerEnemy);

			if (_showDebugInfo)
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Preset '{presetData.PresetName}' spawned: {spawnedEnemies.Length} enemies, main section: {mainSection}, {presetData.TokenCost} tokens spent, {presetData.PresetCooldown:F1}s cooldown");

			return new PresetSpawnResult
			{
				Success = true,
				PresetName = presetData.PresetName,
				MainSection = mainSection,
				Placements = placements.ToArray(),
				SpawnedEnemies = spawnedEnemies,
				TotalCooldown = presetData.PresetCooldown,
				TokensSpent = presetData.TokenCost
			};
		}

		private int CalculateAbsoluteSection(int mainSection, int presetSection)
		{
			int offset = presetSection - 1;
			return (mainSection + offset) % _spawnerTokens.Sections;
		}

		public struct SpawnResult
		{
			public bool Success;
			public bool IsPreset;
			public EnemyKind[] Enemies;
			public PooledEnemy[] SpawnedEnemies;
			public float TotalCooldown;
			public float TokensSpent;
		}

		public struct PresetSpawnResult
		{
			public bool Success;
			public string PresetName;
			public SpawnSection MainSection;
			public PresetEnemyInfo[] Placements;
			public PooledEnemy[] SpawnedEnemies;
			public float TotalCooldown;
			public float TokensSpent;
		}

		[Button(nameof(ForceGlitch))]
		private void ForceGlitch()
		{
			_currentGlitchChance = 1f;
			Debug.Log($"[{nameof(SpawnerEnemysAI)}] Glitch chance set to 100%!");
		}

		[Button(nameof(ShowCurrentState))]
		private void ShowCurrentState()
		{
			float timeToNextAccumulation = _nextAccumulationTime - Time.time;
			float timeToNextSpawn = Mathf.Max(0, _nextSpawnTime - Time.time);

			Debug.Log($"[{nameof(SpawnerEnemysAI)}] Current State:\n" +
				$"Current Difficulty: {_currentDifficultyLevel.LevelName}\n" +
				$"Tokens: {_currentTokens:F2}/{_currentDifficultyLevel.DefaultTokens}\n" +
				$"Returned Tokens: {_returnedTokens}\n" +
				$"Can Use Presets: {CanUsePresets}\n" +
				$"Wave System Disabled: {_disableWaveSystem}\n" +
				$"Is Wave Mode: {IsWaveMode}\n" +
				$"Time to Next Spawn: {timeToNextSpawn:F1}s\n" +
				$"Last Timer Duration: {_lastTimerDuration:F1}s\n" +
				$"Glitch Chance: {_currentGlitchChance:P1}\n" +
				$"Next Accumulation: {timeToNextAccumulation:F1}s");

#if UNITY_EDITOR
			if (Application.isPlaying && !UnityEditor.EditorApplication.isPaused)
			{
				UnityEditor.EditorApplication.isPaused = true;
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Game paused for debugging. Press Play to resume.");
			}
#endif
		}
	}
}
