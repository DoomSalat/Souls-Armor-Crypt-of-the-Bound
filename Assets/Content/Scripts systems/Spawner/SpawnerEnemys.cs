using Sirenix.OdinInspector;
using UnityEngine;
using StatusSystem;
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

	[RequireComponent(typeof(SpawnerSection))]
	public class SpawnerEnemys : MonoBehaviour, ISoulSpawnRequestHandler
	{
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
		private SpawnerSection _spawnerTokens;

		private ISpawner _blue;
		private ISpawner _green;
		private ISpawner _red;
		private ISpawner _yellow;
		private ISpawner _knight;

		private Dictionary<SpawnerType, ISpawner> _spawnersByType;
		private Dictionary<EnemyKind, EnemyData> _enemyDataByKind;
		private Dictionary<EnemyKind, SpawnerType> _spawnerTypeByEnemyKind;

		public EnemyData[] AllEnemyData => GetAllEnemyDataFromSpawners();
		public Dictionary<EnemyKind, EnemyData> EnemyDataByKind => _enemyDataByKind;
		public Dictionary<EnemyKind, SpawnerType> SpawnerTypeByEnemyKind => _spawnerTypeByEnemyKind;

		public event System.Action<EnemyMetaData> EnemyMetaDataEvent;

		private void Awake()
		{
			_spawnerTokens = GetComponent<SpawnerSection>();
			_enemyPool = GetComponent<EnemyPool>();

			_spawnerTokens.Init(_playerTarget);

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

			InitializeDictionaries();
		}

		private void OnEnable()
		{
			SubscribeToSpawnerEvents();
		}

		private void OnDisable()
		{
			UnsubscribeFromSpawnerEvents();
		}

		private void UnsubscribeFromSpawnerEvents()
		{
			_blue.EnemyReturnedToPool -= OnEnemyReturnedToPool;
			_green.EnemyReturnedToPool -= OnEnemyReturnedToPool;
			_red.EnemyReturnedToPool -= OnEnemyReturnedToPool;
			_yellow.EnemyReturnedToPool -= OnEnemyReturnedToPool;
			_knight.EnemyReturnedToPool -= OnEnemyReturnedToPool;
			GroupRegister.GroupDestroyedEvent -= OnGroupDestroyed;
		}

		public void SpawnEnemy(SoulType soulType, EnemyKind enemyKind, SpawnerSystemData.SpawnSection section = SpawnerSystemData.SpawnSection.Section1)
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("Manual spawn works only in Play Mode!");
				return;
			}

			if (soulType == SoulType.Random && enemyKind != EnemyKind.Knight)
			{
				soulType = GetRandomSoulType();
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
				if (spawner is SpawnerYellow yellowSpawner)
				{
					int spawnCount = yellowSpawner.Spawn(section, enemyKind);
					if (spawnCount > 0)
					{
						_spawnerTokens.UpdateWeights(GetAllActiveEnemies());
					}
				}
				else
				{
					spawner.Spawn(section, enemyKind);
					_spawnerTokens.UpdateWeights(GetAllActiveEnemies());
				}
			}
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
			if (soulType == SoulType.Random)
			{
				soulType = GetRandomSoulType();
			}

			var spawner = GetSpawnerBySoulType(soulType);

			if (spawner != null)
			{
				var section = SpawnerSystemData.SpawnSection.Section4;
				var spawnedEnemy = spawner.SpawnAtPosition(section, EnemyKind.Soul, spawnPosition);

				_spawnerTokens.UpdateWeights(GetAllActiveEnemies());

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

		public SoulType[] GetAvailableSoulTypes()
		{
			var soulTypes = new List<SoulType>();

			if (_blue != null) soulTypes.Add(SoulType.Blue);
			if (_green != null) soulTypes.Add(SoulType.Green);
			if (_red != null) soulTypes.Add(SoulType.Red);
			if (_yellow != null) soulTypes.Add(SoulType.Yellow);

			return soulTypes.ToArray();
		}

		public SoulType GetRandomSoulType()
		{
			var availableSoulTypes = GetAvailableSoulTypes();
			if (availableSoulTypes.Length == 0)
				return SoulType.Blue;

			return availableSoulTypes[UnityEngine.Random.Range(0, availableSoulTypes.Length)];
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

		public PooledEnemy[] GetAllActiveEnemies()
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

		private void SubscribeToSpawnerEvents()
		{
			_blue.EnemyReturnedToPool += OnEnemyReturnedToPool;
			_green.EnemyReturnedToPool += OnEnemyReturnedToPool;
			_red.EnemyReturnedToPool += OnEnemyReturnedToPool;
			_yellow.EnemyReturnedToPool += OnEnemyReturnedToPool;
			_knight.EnemyReturnedToPool += OnEnemyReturnedToPool;
			GroupRegister.GroupDestroyedEvent += OnGroupDestroyed;
		}

		private void OnEnemyReturnedToPool(PooledEnemy enemy)
		{
			_spawnerTokens.UpdateWeights(GetAllActiveEnemies());

			if (enemy != null && enemy.SpawnMeta != null)
			{
				var metaData = new EnemyMetaData(
					enemy.SpawnMeta.TokensToReturn,
					enemy.SpawnMeta.TimerReductionOnDeath,
					enemy.SpawnMeta.Kind
				);
				EnemyMetaDataEvent?.Invoke(metaData);
			}
		}

		private void OnGroupDestroyed(int groupId, GroupMetaData metaData)
		{
			var enemyMeta = EnemyMetaData.FromGroupMeta(metaData);
			EnemyMetaDataEvent?.Invoke(enemyMeta);
		}

		public PooledEnemy SpawnEnemy(SoulType soulType, EnemyKind enemyKind, SpawnerSystemData.SpawnSection section, int tokensToReturn = 0, float timerReduction = 0f)
		{
			if (soulType == SoulType.Random && enemyKind != EnemyKind.Knight)
			{
				soulType = GetRandomSoulType();
			}

			ISpawner spawner = enemyKind == EnemyKind.Knight
				? _knight
				: GetSpawnerBySoulType(soulType);

			if (spawner == null)
				return null;

			if (spawner is SpawnerYellow yellowSpawner)
			{
				return SpawnGroupEnemy(yellowSpawner, enemyKind, section, tokensToReturn, timerReduction);
			}

			var activeEnemiesBefore = spawner.GetActiveEnemies();
			int enemiesCountBefore = activeEnemiesBefore?.Length ?? 0;

			int spawnCount = spawner.Spawn(section, enemyKind);

			if (spawnCount > 0)
			{
				var activeEnemiesAfter = spawner.GetActiveEnemies();
				if (activeEnemiesAfter != null && activeEnemiesAfter.Length > enemiesCountBefore)
				{
					var spawnedEnemy = activeEnemiesAfter[enemiesCountBefore];

					if (spawnedEnemy != null && spawnedEnemy.SpawnMeta != null)
					{
						spawnedEnemy.SpawnMeta.SetSpawnData(tokensToReturn, timerReduction);
					}

					_spawnerTokens.UpdateWeights(GetAllActiveEnemies());
					return spawnedEnemy;
				}
			}

			return null;
		}

		private PooledEnemy SpawnGroupEnemy(SpawnerYellow yellowSpawner, EnemyKind enemyKind, SpawnerSystemData.SpawnSection section, int tokensToReturn, float timerReduction)
		{
			if (yellowSpawner.GetSpawnStrategy() is GroupSpawnStrategy groupStrategy)
			{
				groupStrategy.SetGroupMetaData(tokensToReturn, timerReduction);
			}
			else
			{
				Debug.LogWarning($"[{nameof(SpawnerEnemys)}] SpawnerYellow does not have GroupSpawnStrategy!");
			}

			int spawnCount = yellowSpawner.Spawn(section, enemyKind);

			if (spawnCount > 0)
			{
				_spawnerTokens.UpdateWeights(GetAllActiveEnemies());
			}

			return null;
		}

	}
}