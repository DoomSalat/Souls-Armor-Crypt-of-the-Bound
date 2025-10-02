using Sirenix.OdinInspector;
using UnityEngine;
using StatusSystem;
using System.Linq;
using System.Collections.Generic;

namespace SpawnerSystem
{
	public enum SpawnerType
	{
		Blue,
		Green,
		Red,
		Yellow,
		Knight
	}

	[RequireComponent(typeof(SpawnerTokens))]
	public class SpawnerEnemys : MonoBehaviour, ISoulSpawnRequestHandler
	{
		[Header("Manual Mode")]
		[SerializeField] private bool _manualMode;

		[Button("Send All Active Enemies to SpawnerTokens")]
		private void SendAllActiveEnemiesToTokens()
		{
			var allActiveEnemies = GetAllActiveEnemies();
			_spawnerTokens.RecalculateSectionsByEnemyPositions(allActiveEnemies);
		}

		[Header("Spawners")]
		[SerializeField, Required] private MonoBehaviour _blueSpawner;
		[SerializeField, Required] private MonoBehaviour _greenSpawner;
		[SerializeField, Required] private MonoBehaviour _redSpawner;
		[SerializeField, Required] private MonoBehaviour _yellowSpawner;
		[SerializeField, Required] private MonoBehaviour _knightSpawner;

		[Header("Systems")]
		[SerializeField, Required] private Transform _playerTarget;
		[SerializeField, Required] private Sword _playerSword;
		[SerializeField, Required] private StatusMachine _statusMachine;
		[SerializeField, Required] private ThrowSpawner _throwSpawner;

		private EnemyPool _enemyPool;
		private SpawnerTokens _spawnerTokens;

		private ISpawner _blue;
		private ISpawner _green;
		private ISpawner _red;
		private ISpawner _yellow;
		private ISpawner _knight;

		private Dictionary<SpawnerType, ISpawner> _spawnersByType;
		private Dictionary<EnemyKind, EnemyData> _enemyDataByKind;
		private Dictionary<EnemyKind, SpawnerType> _spawnerTypeByEnemyKind;

		public bool ManualMode => _manualMode;

		public EnemyData[] AllEnemyData => GetAllEnemyDataFromSpawners();
		public Dictionary<EnemyKind, EnemyData> EnemyDataByKind => _enemyDataByKind;
		public Dictionary<EnemyKind, SpawnerType> SpawnerTypeByEnemyKind => _spawnerTypeByEnemyKind;

		private void Awake()
		{
			_spawnerTokens = GetComponent<SpawnerTokens>();
			_enemyPool = GetComponent<EnemyPool>();

			_spawnerTokens.Init(_playerTarget);
			_spawnerTokens.InitWeightsFromSpawnerEnemys(this);

			_enemyPool.Initialize(_playerTarget, _statusMachine);

			_blue = _blueSpawner as ISpawner;
			_green = _greenSpawner as ISpawner;
			_red = _redSpawner as ISpawner;
			_yellow = _yellowSpawner as ISpawner;
			_knight = _knightSpawner as ISpawner;

			var dependencies = new SpawnerDependencies
			{
				Tokens = _spawnerTokens,
				EnemyPool = _enemyPool,
				SoulSpawnRequestHandler = this,
				ThrowSpawner = _throwSpawner,
				PlayerSword = _playerSword
			};

			_blue?.Init(dependencies);
			_green?.Init(dependencies);
			_red?.Init(dependencies);
			_yellow?.Init(dependencies);
			_knight?.Init(dependencies);

			SubscribeToSpawnerEvents();

			InitializeDictionaries();
		}

		public void SpawnEnemy(SoulType soulType, EnemyKind enemyKind, SpawnDirection direction = SpawnDirection.Section1)
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("Manual spawn works only in Play Mode!");
				return;
			}

			ISpawner spawner;

			if (enemyKind == EnemyKind.Knight)
			{
				spawner = _knight;
			}
			else
			{
				spawner = GetSpawnerBySoulType(soulType);
			}

			if (spawner != null)
			{
				var section = ConvertDirectionToSection(direction);
				var spawned = spawner.Spawn(section, enemyKind);
				var enemyData = GetEnemyData(enemyKind);
				var costPerEnemy = enemyData?.TokenValue ?? 1f;
				_spawnerTokens.Commit(section, spawned, costPerEnemy);
			}
		}

		public PooledEnemy SpawnEnemyWithMeta(SoulType soulType, EnemyKind enemyKind, SpawnSection section, float tokensToReturn, float timerReduction)
		{
			ISpawner spawner = enemyKind == EnemyKind.Knight
				? _knight
				: GetSpawnerBySoulType(soulType);

			if (spawner == null)
				return null;

			if (spawner is SpawnerYellow yellowSpawner)
			{
				return SpawnGroupEnemyWithMeta(yellowSpawner, enemyKind, section, tokensToReturn, timerReduction);
			}

			var spawnPosition = _spawnerTokens.GetSpawnPosition(section);
			var spawnedEnemy = spawner.SpawnAtPosition(section, enemyKind, spawnPosition);

			if (spawnedEnemy != null && spawnedEnemy.SpawnMeta != null)
			{
				spawnedEnemy.SpawnMeta.SetSpawnData(tokensToReturn, timerReduction);
			}

			var enemyData = GetEnemyData(enemyKind);
			var costPerEnemy = enemyData?.SectionWeight ?? 1f;
			_spawnerTokens.Commit(section, spawnedEnemy != null ? 1 : 0, costPerEnemy);

			return spawnedEnemy;
		}

		public PooledEnemy SpawnGroupEnemyWithMeta(SpawnerYellow yellowSpawner, EnemyKind enemyKind, SpawnSection section, float tokensToReturn, float timerReduction)
		{
			if (yellowSpawner.GetSpawnStrategy() is GroupSpawnStrategy groupStrategy)
			{
				groupStrategy.SetGroupMetaData(tokensToReturn, timerReduction);
			}

			var spawnPosition = _spawnerTokens.GetSpawnPosition(section);
			var spawnedEnemy = yellowSpawner.SpawnAtPosition(section, enemyKind, spawnPosition);

			var enemyData = GetEnemyData(enemyKind);
			var costPerEnemy = enemyData?.SectionWeight ?? 1f;
			_spawnerTokens.Commit(section, spawnedEnemy != null ? 1 : 0, costPerEnemy);

			return spawnedEnemy;
		}

		public PooledEnemy[] SpawnPresetEnemies(PresetEnemyInfo[] placements, float tokensPerEnemy, float timerPerEnemy)
		{
			var spawnedEnemies = new System.Collections.Generic.List<PooledEnemy>();

			foreach (var placement in placements)
			{
				var spawned = SpawnEnemyWithMeta(
					placement.SoulType,
					placement.EnemyKind,
					placement.Section,
					tokensPerEnemy,
					timerPerEnemy
				);

				if (spawned != null)
					spawnedEnemies.Add(spawned);
			}

			return spawnedEnemies.ToArray();
		}

		private SpawnSection ConvertDirectionToSection(SpawnDirection direction)
		{
			return direction switch
			{
				SpawnDirection.Section1 => SpawnSection.Section1,
				SpawnDirection.Section2 => SpawnSection.Section2,
				SpawnDirection.Section3 => SpawnSection.Section3,
				SpawnDirection.Section4 => SpawnSection.Section4,
				SpawnDirection.Section5 => SpawnSection.Section5,
				SpawnDirection.Section6 => SpawnSection.Section6,
				SpawnDirection.Section7 => SpawnSection.Section7,
				SpawnDirection.Section8 => SpawnSection.Section8,
				SpawnDirection.Section9 => SpawnSection.Section9,
				SpawnDirection.Section10 => SpawnSection.Section10,
				SpawnDirection.Section11 => SpawnSection.Section11,
				SpawnDirection.Section12 => SpawnSection.Section12,
				_ => SpawnSection.Section1
			};
		}

		private ISpawner GetSpawnerBySoulType(SoulType soulType)
		{
			return soulType switch
			{
				SoulType.Blue => _blue,
				SoulType.Green => _green,
				SoulType.Red => _red,
				SoulType.Yellow => _yellow,
				_ => _blue
			};
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			ValidateSpawner(ref _blueSpawner, nameof(_blueSpawner));
			ValidateSpawner(ref _greenSpawner, nameof(_greenSpawner));
			ValidateSpawner(ref _redSpawner, nameof(_redSpawner));
			ValidateSpawner(ref _yellowSpawner, nameof(_yellowSpawner));
			ValidateSpawner(ref _knightSpawner, nameof(_knightSpawner));
		}

		private void ValidateSpawner(ref MonoBehaviour spawner, string fieldName)
		{
			if (spawner != null && !(spawner is ISpawner))
			{
				Debug.LogWarning($"[{nameof(SpawnerEnemys)}] {fieldName} must implement ISpawner. Field will be set to null.", this);
				spawner = null;
			}
		}
#endif

		public void RequestSoulSpawn(SoulType soulType, Vector3 spawnPosition, DamageData damageData, System.Action<PooledEnemy> onSoulSpawned = null)
		{
			var spawner = GetSpawnerBySoulType(soulType);

			if (spawner != null)
			{
				var section = SpawnSection.Section4;
				var spawnedEnemy = spawner.SpawnAtPosition(section, EnemyKind.Soul, spawnPosition);
				var enemyData = GetEnemyData(EnemyKind.Soul);
				var costPerEnemy = enemyData?.TokenValue ?? 1f;
				_spawnerTokens.Commit(section, spawnedEnemy != null ? 1 : 0, costPerEnemy);

				if (spawnedEnemy != null)
				{
					var soul = spawnedEnemy.Soul;
					if (soul != null)
					{
						soul.SpawnInitializate(enableCollisions: false);
						soul.ApplySpawnKnockback(damageData.KnockbackDirection, damageData.KnockbackForce);
					}

					onSoulSpawned?.Invoke(spawnedEnemy);
				}

				SendAllActiveEnemiesToTokens();
			}
			else
			{
				Debug.LogWarning($"[{nameof(SpawnerEnemys)}] No spawner found for soul type {soulType}");
			}
		}

		private EnemyData[] GetAllEnemyDataFromSpawners()
		{
			var allData = new System.Collections.Generic.List<EnemyData>();

			AddSpawnerData(_blue, allData);
			AddSpawnerData(_green, allData);
			AddSpawnerData(_red, allData);
			AddSpawnerData(_yellow, allData);
			AddSpawnerData(_knight, allData);

			return allData.ToArray();
		}

		private void AddSpawnerData(ISpawner spawner, System.Collections.Generic.List<EnemyData> list)
		{
			if (spawner != null)
			{
				var data = spawner.GetAllEnemyData();
				if (data != null)
				{
					list.AddRange(data);
				}
			}
		}

		public EnemyData GetEnemyData(EnemyKind enemyKind)
		{
			EnemyData data = _blue?.GetEnemyData(enemyKind);
			if (data != null)
				return data;

			data = _green?.GetEnemyData(enemyKind);
			if (data != null)
				return data;

			data = _red?.GetEnemyData(enemyKind);
			if (data != null)
				return data;

			data = _yellow?.GetEnemyData(enemyKind);
			if (data != null)
				return data;

			data = _knight?.GetEnemyData(enemyKind);
			return data;
		}

		public EnemyKind[] GetAvailableEnemiesForDifficulty(int difficultyLevel)
		{
			return AllEnemyData
				.Where(data => data.DifficultyLevel <= difficultyLevel)
				.Select(data => data.EnemyKind)
				.Distinct()
				.ToArray();
		}

		public SoulType[] GetAvailableSoulTypes()
		{
			var soulTypes = new List<SoulType>();

			if (_blue != null) soulTypes.Add(SoulType.Blue);
			if (_green != null) soulTypes.Add(SoulType.Green);
			if (_red != null) soulTypes.Add(SoulType.Red);
			if (_yellow != null) soulTypes.Add(SoulType.Yellow);

			return soulTypes.ToArray();
		}

		private void InitializeDictionaries()
		{
			_spawnersByType = new Dictionary<SpawnerType, ISpawner>
			{
				{ SpawnerType.Blue, _blue },
				{ SpawnerType.Green, _green },
				{ SpawnerType.Red, _red },
				{ SpawnerType.Yellow, _yellow },
				{ SpawnerType.Knight, _knight }
			};

			_enemyDataByKind = new Dictionary<EnemyKind, EnemyData>();
			_spawnerTypeByEnemyKind = new Dictionary<EnemyKind, SpawnerType>();

			FillEnemyDataDictionaries(SpawnerType.Blue, _blue);
			FillEnemyDataDictionaries(SpawnerType.Green, _green);
			FillEnemyDataDictionaries(SpawnerType.Red, _red);
			FillEnemyDataDictionaries(SpawnerType.Yellow, _yellow);
			FillEnemyDataDictionaries(SpawnerType.Knight, _knight);
		}

		private void FillEnemyDataDictionaries(SpawnerType spawnerType, ISpawner spawner)
		{
			if (spawner == null)
				return;

			var enemyData = spawner.GetAllEnemyData();
			if (enemyData == null)
				return;

			foreach (var data in enemyData)
			{
				if (data != null)
				{
					_enemyDataByKind[data.EnemyKind] = data;
					_spawnerTypeByEnemyKind[data.EnemyKind] = spawnerType;
				}
			}
		}

		private PooledEnemy[] GetAllActiveEnemies()
		{
			var allEnemies = new List<PooledEnemy>();

			AddActiveEnemiesFromSpawner(_blue, allEnemies);
			AddActiveEnemiesFromSpawner(_green, allEnemies);
			AddActiveEnemiesFromSpawner(_red, allEnemies);
			AddActiveEnemiesFromSpawner(_yellow, allEnemies);
			AddActiveEnemiesFromSpawner(_knight, allEnemies);

			return allEnemies.ToArray();
		}

		private void AddActiveEnemiesFromSpawner(ISpawner spawner, List<PooledEnemy> list)
		{
			if (spawner != null)
			{
				var activeEnemies = spawner.GetActiveEnemies();
				if (activeEnemies != null)
				{
					list.AddRange(activeEnemies);
				}
			}
		}

		public event System.Action<PooledEnemy> EnemyReturnedToPoolEvent;

		private void SubscribeToSpawnerEvents()
		{
			_blue.EnemyReturnedToPool += OnEnemyReturnedToPool;
			_green.EnemyReturnedToPool += OnEnemyReturnedToPool;
			_red.EnemyReturnedToPool += OnEnemyReturnedToPool;
			_yellow.EnemyReturnedToPool += OnEnemyReturnedToPool;
			_knight.EnemyReturnedToPool += OnEnemyReturnedToPool;
		}

		private void OnEnemyReturnedToPool(PooledEnemy enemy)
		{
			SendAllActiveEnemiesToTokens();
			EnemyReturnedToPoolEvent?.Invoke(enemy);
		}
	}
}