using UnityEngine;
using Sirenix.OdinInspector;

namespace SpawnerSystem
{
	[RequireComponent(typeof(SpawnerTokens), typeof(SpawnerEnemys))]
	public class SpawnerEnemysAI : MonoBehaviour
	{
		[Header("Difficulty Levels")]
		[SerializeField] private DifficultyLevel[] _difficultyLevels;

		[Header("AI Settings")]
		[SerializeField, MinValue(0)] private float _desiredPowerPerSection = 5f;
		[SerializeField, MinValue(0)] private int _maxSectionDeviation = 2;

		[Header("Glitch Spawn System")]
		[SerializeField, Range(0, 1)] private float _glitchAccumulationRate = 0.02f;
		[SerializeField, MinValue(1)] private Vector2 _glitchAccumulationTime = new(5f, 10f);
		[SerializeField, Range(0, 1)] private float _maxGlitchChance = 1f;

		[Header("Debug")]
		[SerializeField] private bool _showDebugInfo = false;

		private SpawnerTokens _spawnerTokens;
		private SpawnerEnemys _spawnerEnemys;

		private DifficultyLevel _currentDifficultyLevel;
		private float _currentTimer;
		private float _nextSpawnTime;

		private int _currentTargetSection;
		private float _currentSectionPower;
		private bool _isTargetingNewSection = true;

		private float _currentGlitchChance = 0f;
		private float _lastGlitchAccumulationTime;
		private float _nextAccumulationTime;

		private void Awake()
		{
			if (!enabled)
				return;

			_spawnerTokens = GetComponent<SpawnerTokens>();
			_spawnerEnemys = GetComponent<SpawnerEnemys>();

			InitializeTimerSystem();
			InitializeGlitchSystem();
		}

		private void Update()
		{
			UpdateTimer();

			if (Time.time >= _nextAccumulationTime)
			{
				AccumulateGlitchChance();
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

		private void InitializeTimerSystem()
		{
			ResetSpawnTimer();
		}

		public void SetDifficulty(int level)
		{
			ResetSpawnTimer();

			if (_showDebugInfo)
				Debug.Log($"[SpawnerEnemysAI] Difficulty set to level {level}: {_currentDifficultyLevel.Description}");
		}

		private void ResetSpawnTimer()
		{
			_nextSpawnTime = Time.time + Random.Range(_currentDifficultyLevel.SpawnIntervalRange.x, _currentDifficultyLevel.SpawnIntervalRange.y);
			_currentTimer = _nextSpawnTime - Time.time;
		}

		public void OnEnemyKilled()
		{
			_nextSpawnTime -= _currentDifficultyLevel.TimerReductionPerKill;
			_currentTimer = Mathf.Max(0.1f, _nextSpawnTime - Time.time);

			if (_showDebugInfo)
				Debug.Log($"[SpawnerEnemysAI] Enemy killed, timer reduced by {_currentDifficultyLevel.TimerReductionPerKill:F1}s, next spawn in {_currentTimer:F1}s");
		}

		public void OnPresetEnemiesKilled()
		{
			_nextSpawnTime -= _currentDifficultyLevel.TimerReductionPerPresetKill;
			_currentTimer = Mathf.Max(0.1f, _nextSpawnTime - Time.time);

			if (_showDebugInfo)
				Debug.Log($"[SpawnerEnemysAI] Preset enemies killed, timer reduced by {_currentDifficultyLevel.TimerReductionPerPresetKill:F1}s, next spawn in {_currentTimer:F1}s");
		}

		public bool ShouldSpawn()
		{
			if (Time.time >= _nextSpawnTime)
			{
				ResetSpawnTimer();
				return true;
			}
			return false;
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
			_lastGlitchAccumulationTime = Time.time;
			_nextAccumulationTime = Time.time + UnityEngine.Random.Range(_glitchAccumulationTime.x, _glitchAccumulationTime.y);
		}

		private void AccumulateGlitchChance()
		{
			_currentGlitchChance += _glitchAccumulationRate;
			_currentGlitchChance = Mathf.Min(_currentGlitchChance, _maxGlitchChance);

			_lastGlitchAccumulationTime = Time.time;
			_nextAccumulationTime = Time.time + UnityEngine.Random.Range(_glitchAccumulationTime.x, _glitchAccumulationTime.y);

			if (_showDebugInfo)
			{
				Debug.Log($"[SpawnerEnemysAI] Glitch chance accumulated: {_currentGlitchChance:P1} (next accumulation in {_nextAccumulationTime - Time.time:F1}s)");
			}
		}

		public SpawnSection SelectSection()
		{
			if (ShouldTriggerGlitch())
			{
				var glitchSection = (SpawnSection)UnityEngine.Random.Range(0, _spawnerTokens.Sections);

				if (_showDebugInfo)
					Debug.Log($"[SpawnerEnemysAI] Glitch triggered! Random section: {glitchSection}");

				return glitchSection;
			}

			if (_isTargetingNewSection || _currentSectionPower >= _desiredPowerPerSection)
			{
				_currentTargetSection = SelectNewTargetSection();
				_currentSectionPower = 0f;
				_isTargetingNewSection = false;
			}

			return (SpawnSection)_currentTargetSection;
		}

		private bool ShouldTriggerGlitch()
		{
			bool shouldGlitch = UnityEngine.Random.value < _currentGlitchChance;

			if (shouldGlitch)
			{
				_currentGlitchChance = 0f;
				_lastGlitchAccumulationTime = Time.time;
				_nextAccumulationTime = Time.time + UnityEngine.Random.Range(_glitchAccumulationTime.x, _glitchAccumulationTime.y);

				if (_showDebugInfo)
				{
					Debug.Log($"[SpawnerEnemysAI] Glitch triggered! Chance was {_currentGlitchChance:P1}, reset to 0%");
				}
			}

			return shouldGlitch;
		}

		private int SelectNewTargetSection()
		{
			float[] sectionWeights = _spawnerTokens.GetSectionWeights();

			int lowestPowerSection = 0;
			float lowestPower = sectionWeights[0];

			for (int i = 1; i < sectionWeights.Length; i++)
			{
				if (sectionWeights[i] < lowestPower)
				{
					lowestPower = sectionWeights[i];
					lowestPowerSection = i;
				}
			}

			int oppositeSection = (lowestPowerSection + _spawnerTokens.Sections / 2) % _spawnerTokens.Sections;
			int deviation = UnityEngine.Random.Range(-_maxSectionDeviation, _maxSectionDeviation + 1);
			int targetSection = (oppositeSection + deviation + _spawnerTokens.Sections) % _spawnerTokens.Sections;

			if (_showDebugInfo)
			{
				Debug.Log($"[SpawnerEnemysAI] Selected section {targetSection} " +
					$"(opposite to {lowestPowerSection}, deviation: {deviation})");
			}

			return targetSection;
		}

		public void ForceRetarget()
		{
			_isTargetingNewSection = true;

			if (_showDebugInfo)
				Debug.Log("[SpawnerEnemysAI] Force retarget triggered");
		}

		public void CommitSectionPower(SpawnSection section, float powerAdded)
		{
			int index = (int)section;

			if (index == _currentTargetSection)
			{
				_currentSectionPower += powerAdded;

				if (_showDebugInfo)
					Debug.Log($"[SpawnerEnemysAI] Section {section} power: {_currentSectionPower}/{_desiredPowerPerSection}");
			}
		}

		[Button(nameof(ForceGlitch))]
		private void ForceGlitch()
		{
			_currentGlitchChance = 1f;
			Debug.Log("[SpawnerEnemysAI] Glitch chance set to 100%!");
		}

		[Button(nameof(ForceRetargetDebug))]
		private void ForceRetargetDebug()
		{
			ForceRetarget();
		}

		[Button(nameof(ShowCurrentState))]
		private void ShowCurrentState()
		{
			float timeToNextAccumulation = _nextAccumulationTime - Time.time;
			float timeToNextSpawn = Mathf.Max(0, _nextSpawnTime - Time.time);

			Debug.Log($"[SpawnerEnemysAI] Current State:\n" +
				$"Target Section: {_currentTargetSection}\n" +
				$"Section Power: {_currentSectionPower}/{_desiredPowerPerSection}\n" +
				$"Is Targeting New: {_isTargetingNewSection}\n" +
				$"Current Difficulty: {_currentDifficultyLevel.LevelName}\n" +
				$"Time to Next Spawn: {timeToNextSpawn:F1}s\n" +
				$"Glitch Chance: {_currentGlitchChance:P1}\n" +
				$"Next Accumulation: {timeToNextAccumulation:F1}s");
		}
	}
}
