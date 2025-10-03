using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace SpawnerSystem
{
	[System.Serializable]
	public class EnemyPlacement
	{
		[Header("Enemy Info")]
		[SerializeField] private EnemyKind _enemyKind;
		[SerializeField, ShowIf(nameof(ShowSoulType))] private SoulType _soulType = SoulType.Random;

		[Header("Section")]
		[SerializeField, Range(1, SpawnerSystemData.SectionCount)] private int _section = 1;
		[SerializeField, MinValue(1)] private int _count = 1;

		[Header("Enemy Meta")]
		[SerializeField, MinValue(0f)] private float _timerReductionOnDeath = 0.5f;

		public EnemyKind EnemyKind => _enemyKind;
		public SoulType SoulType => _soulType;
		public int Section => _section;
		public int Count => _count;
		public float TimerReductionOnDeath => _timerReductionOnDeath;

		private bool ShowSoulType => _enemyKind != EnemyKind.Knight;
	}

	[CreateAssetMenu(fileName = "New Preset Data", menuName = "Spawner System/Preset Data")]
	public class PresetData : ScriptableObject
	{
		[Header("Preset Info")]
		[SerializeField] private string _presetName = "New Preset";
		[SerializeField, TextArea] private string _description;

		[Header("Preset Settings")]
		[SerializeField, MinValue(1)] private int _tokenCost = 3;
		[SerializeField, MinValue(0.1f)] private float _presetCooldown = 6f;
		[SerializeField, MinValue(0)] private int[] _allowedDifficultyLevels = { 0 };
		[SerializeField, MinValue(0)] private int _cooldownCycles = 2;

		[Header("Enemy Placement")]
		[InfoBox("Section 1 = main enemy (center), Sections 2-12 = additional enemies around")]
		[SerializeField] private EnemyPlacement[] _enemyPlacements = new EnemyPlacement[1];

		public string PresetName => _presetName;
		public string Description => _description;

		public int TokenCost => _tokenCost;
		public float PresetCooldown => _presetCooldown;
		public int[] AllowedDifficultyLevels => _allowedDifficultyLevels;
		public int CooldownCycles => _cooldownCycles;

		public EnemyPlacement[] GetEnemyPlacements()
		{
			EnemyPlacement[] result = new EnemyPlacement[SpawnerSystemData.SectionCount + 1]; // Индексы 0-12, где 0 пустой

			if (_enemyPlacements != null)
			{
				foreach (var placement in _enemyPlacements)
				{
					if (placement != null && placement.Section >= 1 && placement.Section <= SpawnerSystemData.SectionCount)
					{
						result[placement.Section] = placement;
					}
				}
			}

			return result;
		}

		public int GetTotalEnemyCount()
		{
			int total = 0;
			if (_enemyPlacements != null)
			{
				foreach (var placement in _enemyPlacements)
				{
					total += placement.Count;
				}
			}

			return total;
		}

		public float[] GetSectionWeights()
		{
			float[] sectionWeights = new float[13];

			if (_enemyPlacements != null)
			{
				foreach (var placement in _enemyPlacements)
				{
					if (placement != null && placement.Section >= 1 && placement.Section <= SpawnerSystemData.SectionCount)
					{
						sectionWeights[placement.Section] += placement.Count;
					}
				}
			}

			return sectionWeights;
		}

		public SoulType GetRandomSoulType(SpawnerEnemys spawnerEnemys)
		{
			var availableSoulTypes = spawnerEnemys.GetAvailableSoulTypes();
			if (availableSoulTypes.Length == 0)
				return SoulType.Blue;

			return availableSoulTypes[UnityEngine.Random.Range(0, availableSoulTypes.Length)];
		}

		public bool IsAvailableForDifficulty(int difficultyLevel)
		{
			if (_allowedDifficultyLevels == null || _allowedDifficultyLevels.Length == 0)
				return false;

			return _allowedDifficultyLevels.Contains(difficultyLevel);
		}
	}
}