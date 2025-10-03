using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System.Collections.Generic;

namespace SpawnerSystem
{
	[RequireComponent(typeof(SpawnerSection), typeof(SpawnerEnemys))]
	public class SpawnerEnemysAI : MonoBehaviour
	{
		private const float WaitingForEnemyDeath = float.MaxValue;
		private const float TimerNotActive = -1f;

		[Header("Difficulty Levels")]
		[SerializeField] private DifficultyLevel[] _difficultyLevels;
		[SerializeField, MinValue(0)] private int _currentDifficultyIndex = 0;

		[Header("Preset System")]
		[SerializeField] private PresetData[] _availablePresets;

		[Header("AI Settings")]
		[SerializeField, MinValue(0)] private int _maxSectionDeviation = 2;
		[SerializeField, MinValue(0)] private float _startupDelay = 3f;

		[Header("Glitch Spawn System")]
		[SerializeField, Range(0, 1)] private float _glitchAccumulationRate = 0.02f;
		[SerializeField, MinValue(1)] private Vector2 _glitchAccumulationTime = new(5f, 10f);
		[SerializeField, Range(0, 1)] private float _maxGlitchChance = 1f;

		[Header("Debug")]
		[SerializeField] private bool _showDebugInfo = false;
		[SerializeField] private bool _disableWaveSystem = false;

		private SpawnerSection _spawnerTokens;
		private SpawnerEnemys _spawnerEnemys;

		private DifficultyLevel _currentDifficultyLevel;
		private Dictionary<string, int> _presetCooldowns = new Dictionary<string, int>();

		[Space]
		[ShowInInspector, ReadOnly] private float _timeRemaining;
		[ShowInInspector, ReadOnly] private string _timerStatus;
		private float _currentTimer;
		private float _nextSpawnTime;
		private float _startupTimer;
		private bool _isStartupComplete;
		private bool _hasFirstEnemyDied;
		private float _accumulatedTimer;
		private bool _isWaitingForEnemyDeath;

		[Space]
		[ShowInInspector, ReadOnly] private float _currentGlitchChance = 0f;
		private float _nextAccumulationTime;

		private int _currentTokens = 0;
		private int _returnedTokens = 0;

		public int CurrentDifficultyIndex => _currentDifficultyIndex;
		public DifficultyLevel CurrentDifficultyLevel => _currentDifficultyLevel;
		public int CurrentTokens => _currentTokens;
		public int ReturnedTokens => _returnedTokens;
		public bool CanUsePresets => _returnedTokens > 0;
		public bool IsWaveMode => !_disableWaveSystem && _currentDifficultyLevel.EnableWaves && _returnedTokens >= _currentDifficultyLevel.WaveThreshold;

		private void Awake()
		{
			if (!enabled)
				return;

			_spawnerTokens = GetComponent<SpawnerSection>();
			_spawnerEnemys = GetComponent<SpawnerEnemys>();

			InitializeDifficultySystem();
			InitializeTimerSystem();
			InitializeGlitchSystem();
		}

		private void OnEnable()
		{
			SubscribeToEvents();
		}

		private void OnDisable()
		{
			UnsubscribeFromEvents();
		}

		private void Update()
		{
			UpdateStartupTimer();
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
			if (_startupDelay < 0f) _startupDelay = 0f;
		}
#endif

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
			_spawnerEnemys.EnemyMetaDataEvent += OnEnemyMetaData;
		}

		private void UnsubscribeFromEvents()
		{
			if (_spawnerEnemys != null)
			{
				_spawnerEnemys.EnemyMetaDataEvent -= OnEnemyMetaData;
			}
		}

		private void OnEnemyMetaData(EnemyMetaData metaData)
		{
			if (metaData == null)
				return;

			ProcessEnemyDeath(metaData.TokensToReturn, metaData.TimerReductionOnDeath, metaData.Kind);
		}

		private void ProcessEnemyDeath(int tokensToReturn, float timerReduction, EnemyKind enemyKind)
		{
			_hasFirstEnemyDied = true;

			int tokensGained = 0;
			if (tokensToReturn > 0)
			{
				tokensGained = Mathf.RoundToInt(tokensToReturn);
				int tokensBefore = _currentTokens;
				_currentTokens = Mathf.Min(_currentTokens + tokensGained, _currentDifficultyLevel.DefaultTokens);
				_returnedTokens += tokensGained;
			}

			// Если мы ждали смерти врага, активируем таймер с накопленным временем
			if (_isWaitingForEnemyDeath)
			{
				float totalTimer = _accumulatedTimer - timerReduction;
				_nextSpawnTime = Time.time + Mathf.Max(0.1f, totalTimer);
				_currentTimer = _nextSpawnTime - Time.time;
				_timeRemaining = _currentTimer;
				_accumulatedTimer = 0f;
				_isWaitingForEnemyDeath = false;
			}
			else
			{
				// Обычное уменьшение таймера
				_nextSpawnTime -= timerReduction;
				_currentTimer = Mathf.Max(0.1f, _nextSpawnTime - Time.time);
				_timeRemaining = _currentTimer;
			}

			if (_showDebugInfo)
			{
				string tokenInfo = tokensGained > 0
					? $"Tokens: +{tokensGained} → {_currentTokens}/{_currentDifficultyLevel.DefaultTokens} (Total: {_returnedTokens})"
					: "No tokens returned";

				string timerInfo = timerReduction > 0
					? $"Timer: -{timerReduction:F1}s → Next spawn in {_timeRemaining:F1}s"
					: "No timer reduction";

				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Enemy '{enemyKind}' died | {tokenInfo} | {timerInfo}");
			}
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
				if (preset != null && preset.IsAvailableForDifficulty(_currentDifficultyIndex))
				{
					_presetCooldowns[preset.PresetName] = 0;
				}
			}

			if (_showDebugInfo)
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Preset cooldowns initialized for difficulty {_currentDifficultyIndex}");
		}

		private void InitializeTimerSystem()
		{
			_startupTimer = _startupDelay;
			_isStartupComplete = false;
			_nextSpawnTime = Time.time + _startupDelay;
			_currentTimer = 0f;
			_timeRemaining = 0f;
			_hasFirstEnemyDied = false;
			_accumulatedTimer = 0f;
			_isWaitingForEnemyDeath = false;
			_timerStatus = $"AI Startup ({_startupTimer:F1}s remaining)";
		}

		private void SetDifficulty(int level)
		{
			_currentDifficultyIndex = Mathf.Clamp(level, 0, _difficultyLevels.Length - 1);
			_currentDifficultyLevel = _difficultyLevels[_currentDifficultyIndex];

			_currentTokens = _currentDifficultyLevel.DefaultTokens;
			_returnedTokens = 0;

			_startupTimer = _startupDelay;
			_isStartupComplete = false;
			_nextSpawnTime = Time.time + _startupDelay;
			_currentTimer = 0f;
			_timeRemaining = 0f;
			_hasFirstEnemyDied = false;
			_accumulatedTimer = 0f;
			_isWaitingForEnemyDeath = false;
			_timerStatus = $"AI Startup ({_startupTimer:F1}s remaining)";

			InitializePresetCooldowns();

			if (_showDebugInfo)
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Difficulty set to level {level}: {_currentDifficultyLevel.LevelName} - {_currentDifficultyLevel.Description}");
		}

		private void UpdateStartupTimer()
		{
			if (!_isStartupComplete)
			{
				_startupTimer -= Time.deltaTime;

				if (_startupTimer <= 0f)
				{
					_isStartupComplete = true;
					LogStartupDebugInfo();
				}
			}
		}

		private void LogStartupDebugInfo()
		{
			int availablePresetsCount = 0;
			if (_availablePresets != null)
			{
				availablePresetsCount = _availablePresets.Count(p => p != null && p.IsAvailableForDifficulty(_currentDifficultyIndex));
			}

			Debug.Log($"[{nameof(SpawnerEnemysAI)}] AI Startup Complete!\n" +
				$"Difficulty Level: {_currentDifficultyLevel.LevelName} (Index: {_currentDifficultyIndex})\n" +
				$"Description: {_currentDifficultyLevel.Description}\n" +
				$"Current Tokens: {_currentTokens}/{_currentDifficultyLevel.DefaultTokens}\n" +
				$"Returned Tokens: {_returnedTokens}\n" +
				$"Available Presets: {availablePresetsCount}/{(_availablePresets?.Length ?? 0)}\n" +
				$"Wave System Enabled: {_currentDifficultyLevel.EnableWaves}\n" +
				$"Wave Threshold: {_currentDifficultyLevel.WaveThreshold}\n" +
				$"Max Section Deviation: {_maxSectionDeviation}\n" +
				$"Glitch Accumulation Rate: {_glitchAccumulationRate:P1}\n" +
				$"Max Glitch Chance: {_maxGlitchChance:P1}\n" +
				$"Startup Delay: {_startupDelay:F1}s");
		}

		private bool ShouldSpawn()
		{
			if (!_isStartupComplete || Time.time < _nextSpawnTime)
				return false;

			if (!_hasFirstEnemyDied)
				return true;

			if (_currentTokens <= 0)
			{
				_accumulatedTimer = Mathf.Max(0, _nextSpawnTime - Time.time);
				_isWaitingForEnemyDeath = true;
				_nextSpawnTime = WaitingForEnemyDeath;
				_currentTimer = 0f;
				_timeRemaining = 0f;
				_timerStatus = $"Waiting for enemy death... (accumulated: {_accumulatedTimer:F1}s)";

				if (_showDebugInfo)
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] No tokens available, waiting for enemy death. Accumulated timer: {_accumulatedTimer:F1}s");

				return false;
			}

			return _hasFirstEnemyDied;
		}

		private SpawnResult SpawnPreset(PresetData presetData)
		{
			if (presetData == null)
				return new SpawnResult { Success = false };

			if (!SpendTokens(presetData.TokenCost))
				return new SpawnResult { Success = false };

			if (presetData.CooldownCycles > 0)
			{
				SetPresetCooldown(presetData.PresetName, presetData.CooldownCycles);

				if (_showDebugInfo)
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Applied cooldown for preset '{presetData.PresetName}': {presetData.CooldownCycles} cycles");
			}

			SpawnerSystemData.SpawnSection mainSection = SelectSection();

			var placements = CreatePresetPlacements(presetData, mainSection);
			var spawnedEnemies = new List<PooledEnemy>();
			var tokenDistribution = DistributeTokensEvenly(presetData.TokenCost, placements.Count);

			for (int i = 0; i < placements.Count; i++)
			{
				var placement = placements[i];
				int tokensForThisEnemy = i < tokenDistribution.Length ? tokenDistribution[i] : 0;
				float timerReduction = placement.TimerReductionOnDeath;

				if (_showDebugInfo)
				{
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Spawning enemy: {placement.EnemyKind} in section {placement.Section} with {tokensForThisEnemy} tokens to return and {timerReduction:F1}s timer reduction");
				}

				var spawned = _spawnerEnemys.SpawnEnemy(
					placement.SoulType,
					placement.EnemyKind,
					placement.Section,
					tokensForThisEnemy,
					timerReduction
				);

				if (spawned != null)
					spawnedEnemies.Add(spawned);
			}

			return new SpawnResult
			{
				Success = true,
				IsPreset = true,
				Enemies = presetData.GetEnemyPlacements().Where(p => p != null).SelectMany(p => Enumerable.Repeat(p.EnemyKind, p.Count)).ToArray(),
				SpawnedEnemies = spawnedEnemies.ToArray(),
				TotalCooldown = presetData.CooldownCycles > 0 ? presetData.PresetCooldown : 0f,
				TokensSpent = presetData.TokenCost
			};
		}

		private List<PresetEnemyInfo> CreatePresetPlacements(PresetData presetData, SpawnerSystemData.SpawnSection mainSection)
		{
			var placements = new List<PresetEnemyInfo>();

			foreach (var placement in presetData.GetEnemyPlacements().Where(p => p != null))
			{
				for (int i = 0; i < placement.Count; i++)
				{
					int absoluteSection = CalculateAbsoluteSection((int)mainSection, placement.Section);

					placements.Add(new PresetEnemyInfo
					{
						EnemyKind = placement.EnemyKind,
						SoulType = placement.SoulType,
						Section = (SpawnerSystemData.SpawnSection)absoluteSection,
						TimerReductionOnDeath = placement.TimerReductionOnDeath
					});
				}
			}

			return placements;
		}

		private void UpdateSpawnTimer(PresetData presetData)
		{
			if (_nextSpawnTime == float.MaxValue)
			{
				float totalTimer = _accumulatedTimer + presetData.PresetCooldown;
				_nextSpawnTime = Time.time + totalTimer;
			}
			else
			{
				_nextSpawnTime += presetData.PresetCooldown;
			}

			_currentTimer = _nextSpawnTime - Time.time;
			_timeRemaining = _currentTimer;
			_timerStatus = $"Next spawn in: {_timeRemaining:F1}s";

			if (_showDebugInfo)
			{
				string cooldownType = presetData.CooldownCycles > 0
					? $"Cooldown cycles: {presetData.CooldownCycles}"
					: "Unlimited use";

				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Timer updated for preset '{presetData.PresetName}': " +
					$"PresetCooldown: {presetData.PresetCooldown:F1}s, " +
					$"{cooldownType}, " +
					$"Next spawn in: {_timeRemaining:F1}s");
			}
		}

		private void UpdateTimer()
		{
			if (!_isStartupComplete)
			{
				_timeRemaining = TimerNotActive;
				_timerStatus = $"AI Startup ({_startupTimer:F1}s remaining)";
			}
			else if (!_hasFirstEnemyDied)
			{
				_timeRemaining = Mathf.Max(0, _nextSpawnTime - Time.time);
				_timerStatus = _timeRemaining > 0 ? $"First spawn in: {_timeRemaining:F1}s" : "Ready for first spawn";
			}
			else if (_isWaitingForEnemyDeath)
			{
				_timeRemaining = 0f;
				_timerStatus = $"Waiting for enemy death... (accumulated: {_accumulatedTimer:F1}s)";
			}
			else
			{
				if (_nextSpawnTime != WaitingForEnemyDeath && _currentTimer > 0)
				{
					_currentTimer = Mathf.Max(0, _nextSpawnTime - Time.time);
					_timeRemaining = _currentTimer;
					_timerStatus = $"Next spawn in: {_timeRemaining:F1}s";
				}
				else
				{
					_timeRemaining = 0f;
					_timerStatus = "Ready to spawn";
				}
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

		private SpawnerSystemData.SpawnSection SelectSection()
		{
			if (ShouldTriggerGlitch())
			{
				var glitchSection = (SpawnerSystemData.SpawnSection)UnityEngine.Random.Range(0, _spawnerTokens.Sections);

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

		private SpawnerSystemData.SpawnSection SelectNewTargetSection()
		{
			float[] sectionWeights = _spawnerTokens.GetSectionWeights();

			var emptySections = new List<int>();
			for (int i = 1; i <= _spawnerTokens.Sections; i++)
			{
				if (Mathf.Approximately(sectionWeights[i], 0f))
				{
					emptySections.Add(i);
				}
			}

			if (emptySections.Count == 0)
			{
				var lowestWeightSections = new List<int>();
				float lowestWeight = float.MaxValue;

				for (int i = 1; i <= _spawnerTokens.Sections; i++)
				{
					if (sectionWeights[i] < lowestWeight)
					{
						lowestWeight = sectionWeights[i];
						lowestWeightSections.Clear();
						lowestWeightSections.Add(i);
					}
					else if (Mathf.Approximately(sectionWeights[i], lowestWeight))
					{
						lowestWeightSections.Add(i);
					}
				}
				emptySections = lowestWeightSections;
			}

			var occupiedSections = new List<int>();
			for (int i = 1; i <= _spawnerTokens.Sections; i++)
			{
				if (sectionWeights[i] > 0f)
				{
					occupiedSections.Add(i);
				}
			}

			var furthestSections = new List<int>();
			float maxMinDistance = 0f;

			foreach (int emptySection in emptySections)
			{
				float minDistance = float.MaxValue;

				foreach (int occupiedSection in occupiedSections)
				{
					int distance = CalculateSectionDistance(emptySection, occupiedSection);
					minDistance = Mathf.Min(minDistance, distance);
				}

				if (occupiedSections.Count == 0)
				{
					minDistance = 0f;
				}

				if (minDistance > maxMinDistance)
				{
					maxMinDistance = minDistance;
					furthestSections.Clear();
					furthestSections.Add(emptySection);
				}
				else if (Mathf.Approximately(minDistance, maxMinDistance))
				{
					furthestSections.Add(emptySection);
				}
			}

			int targetSection = furthestSections[UnityEngine.Random.Range(0, furthestSections.Count)];

			int deviation = UnityEngine.Random.Range(-_maxSectionDeviation, _maxSectionDeviation + 1);
			targetSection = ((targetSection - 1 + deviation + _spawnerTokens.Sections) % _spawnerTokens.Sections) + 1;

			if (_showDebugInfo)
			{
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Selected section {targetSection} " +
					$"(from furthest empty sections: [{string.Join(", ", furthestSections)}], " +
					$"occupied: [{string.Join(", ", occupiedSections)}], deviation: {deviation})");
			}

			return (SpawnerSystemData.SpawnSection)targetSection;
		}

		private int CalculateSectionDistance(int section1, int section2)
		{
			int directDistance = Mathf.Abs(section1 - section2);
			int wrapAroundDistance = _spawnerTokens.Sections - directDistance;

			return Mathf.Min(directDistance, wrapAroundDistance);
		}

		private bool SpendTokens(int tokenCost)
		{
			if (_currentTokens >= tokenCost)
			{
				_currentTokens -= tokenCost;

				if (_showDebugInfo)
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Spent {tokenCost} tokens, current: {_currentTokens}, returned: {_returnedTokens}");

				return true;
			}

			return false;
		}

		private PresetData SelectAvailablePreset()
		{
			if (_availablePresets == null)
				return null;

			var availablePresets = new List<PresetData>();

			foreach (var preset in _availablePresets)
			{
				if (preset != null &&
					preset.IsAvailableForDifficulty(_currentDifficultyIndex) &&
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
			var placements = preset.GetEnemyPlacements();
			if (placements == null || placements.All(p => p == null))
				return float.MinValue;

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

		private void ReduceCooldownCyclesForAllPresets()
		{
			foreach (var preset in _availablePresets)
			{
				if (preset != null && preset.IsAvailableForDifficulty(_currentDifficultyIndex))
				{
					if (_presetCooldowns.ContainsKey(preset.PresetName) && _presetCooldowns[preset.PresetName] > 0)
					{
						_presetCooldowns[preset.PresetName]--;

						if (_showDebugInfo)
							Debug.Log($"[{nameof(SpawnerEnemysAI)}] Reduced cooldown for preset '{preset.PresetName}': {_presetCooldowns[preset.PresetName]} cycles remaining");
					}
				}
			}
		}

		private void ProcessSpawnCycle()
		{
			ReduceCooldownCyclesForAllPresets();

			int totalTokensSpent = 0;
			var spawnedPresets = new List<string>();
			int initialTokens = _currentTokens;

			if (_showDebugInfo)
			{
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Starting spawn cycle with {_currentTokens} tokens available");
			}

			while (_currentTokens > 0)
			{
				var selectedPreset = SelectAvailablePreset();

				if (selectedPreset == null)
				{
					ReduceAllPresetCooldowns();

					if (_showDebugInfo)
						Debug.Log($"[{nameof(SpawnerEnemysAI)}] All presets on cooldown, reducing cooldowns and continuing cycle.");

					continue;
				}

				if (_showDebugInfo)
				{
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Selected preset: '{selectedPreset.PresetName}' (cost: {selectedPreset.TokenCost}, cooldown cycles: {selectedPreset.CooldownCycles})");
				}

				if (!CanAffordPreset(selectedPreset))
				{
					if (_showDebugInfo)
						Debug.Log($"[{nameof(SpawnerEnemysAI)}] Cannot afford preset '{selectedPreset.PresetName}' (cost: {selectedPreset.TokenCost}, tokens: {_currentTokens}). Skipping this preset.");
					continue;
				}

				var spawnResult = SpawnPreset(selectedPreset);

				if (!spawnResult.Success)
				{
					if (_showDebugInfo)
						Debug.Log($"[{nameof(SpawnerEnemysAI)}] Failed to spawn preset '{selectedPreset.PresetName}'. Skipping this preset.");
					continue;
				}

				totalTokensSpent += selectedPreset.TokenCost;
				spawnedPresets.Add(selectedPreset.PresetName);

				UpdateSpawnTimer(selectedPreset);

				if (_showDebugInfo)
				{
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Timer updated for preset '{selectedPreset.PresetName}': " +
						$"Cooldown: {selectedPreset.PresetCooldown:F1}s, " +
						$"Next spawn in: {_nextSpawnTime - Time.time:F1}s");
				}

				if (_showDebugInfo)
				{
					string cooldownInfo = selectedPreset.CooldownCycles > 0
						? $"Cooldown: {selectedPreset.PresetCooldown:F1}s ({selectedPreset.CooldownCycles} cycles)"
						: "No cooldown (unlimited use)";

					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Preset '{selectedPreset.PresetName}' spawned successfully:\n" +
						$"  - Tokens spent: {selectedPreset.TokenCost}\n" +
						$"  - Remaining tokens: {_currentTokens}\n" +
						$"  - {cooldownInfo}\n" +
						$"  - Next spawn in: {_nextSpawnTime - Time.time:F1}s");
				}
			}

			if (_showDebugInfo)
			{
				Debug.Log($"[{nameof(SpawnerEnemysAI)}] Spawn cycle completed:\n" +
					$"  - Initial tokens: {initialTokens}\n" +
					$"  - Total tokens spent: {totalTokensSpent}\n" +
					$"  - Presets spawned: {string.Join(", ", spawnedPresets)}\n" +
					$"  - Remaining tokens: {_currentTokens}\n" +
					$"  - Next spawn timer: {_nextSpawnTime - Time.time:F1}s");
			}

			if (totalTokensSpent > 0)
			{
				_accumulatedTimer = Mathf.Max(0, _nextSpawnTime - Time.time);

				_hasFirstEnemyDied = false;
				_nextSpawnTime = WaitingForEnemyDeath;
				_currentTimer = 0f;
				_timeRemaining = 0f;
				_isWaitingForEnemyDeath = true;
				_timerStatus = $"Waiting for enemy death... (accumulated: {_accumulatedTimer:F1}s)";

				if (_showDebugInfo)
					Debug.Log($"[{nameof(SpawnerEnemysAI)}] Reset death flag - waiting for enemy death before next spawn. Accumulated timer: {_accumulatedTimer:F1}s");
			}
		}

		private int[] DistributeTokensEvenly(int totalTokens, int enemyCount)
		{
			if (enemyCount <= 0)
				return new int[0];

			int[] tokenDistribution = new int[enemyCount];
			int baseTokensPerEnemy = totalTokens / enemyCount;
			int remainingTokens = totalTokens % enemyCount;

			for (int i = 0; i < enemyCount; i++)
			{
				tokenDistribution[i] = baseTokensPerEnemy;
			}

			if (remainingTokens > 0)
			{
				tokenDistribution[enemyCount - 1] += remainingTokens;
			}

			return tokenDistribution;
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
			public int TokensSpent;
		}

		public struct PresetSpawnResult
		{
			public bool Success;
			public string PresetName;
			public SpawnerSystemData.SpawnSection MainSection;
			public PresetEnemyInfo[] Placements;
			public PooledEnemy[] SpawnedEnemies;
			public float TotalCooldown;
			public int TokensSpent;
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
				$"Tokens: {_currentTokens}/{_currentDifficultyLevel.DefaultTokens}\n" +
				$"Returned Tokens: {_returnedTokens}\n" +
				$"Can Use Presets: {CanUsePresets}\n" +
				$"Wave System Disabled: {_disableWaveSystem}\n" +
				$"Is Wave Mode: {IsWaveMode}\n" +
				$"Time to Next Spawn: {timeToNextSpawn:F1}s\n" +
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
