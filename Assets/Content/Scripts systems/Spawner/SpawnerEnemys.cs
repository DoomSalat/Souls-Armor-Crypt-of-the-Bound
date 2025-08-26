using Sirenix.OdinInspector;
using UnityEngine;
using StatusSystem;

namespace SpawnerSystem
{
	[RequireComponent(typeof(SpawnerTokens))]
	public class SpawnerEnemys : MonoBehaviour, ISoulSpawnRequestHandler
	{
		[Header("Spawners")]
		[SerializeField, Required] private MonoBehaviour _blueSpawner;
		[SerializeField, Required] private MonoBehaviour _greenSpawner;
		[SerializeField, Required] private MonoBehaviour _redSpawner;
		[SerializeField, Required] private MonoBehaviour _yellowSpawner;

		[Header("Systems")]
		[SerializeField, Required] private Transform _playerTarget;
		[SerializeField, Required] private StatusMachine _statusMachine;
		[SerializeField, Required] private ThrowSpawner _throwSpawner;

		private EnemyPool _enemyPool;
		private SpawnerTokens _spawnerTokens;

		private ISpawner _blue;
		private ISpawner _green;
		private ISpawner _red;
		private ISpawner _yellow;

		[Header("Manual Mode")]
		[SerializeField] private bool _manualMode;

		public bool ManualMode => _manualMode;

		private void Awake()
		{
			_spawnerTokens = GetComponent<SpawnerTokens>();
			_enemyPool = GetComponent<EnemyPool>();

			_spawnerTokens.Init(_playerTarget);

			_enemyPool.Initialize(_playerTarget, _statusMachine);

			_blue = _blueSpawner as ISpawner;
			_green = _greenSpawner as ISpawner;
			_red = _redSpawner as ISpawner;
			_yellow = _yellowSpawner as ISpawner;

			var dependencies = new SpawnerDependencies
			{
				Tokens = _spawnerTokens,
				EnemyPool = _enemyPool,
				SoulSpawnRequestHandler = this,
				ThrowSpawner = _throwSpawner
			};

			_blue?.Init(dependencies);
			_green?.Init(dependencies);
			_red?.Init(dependencies);
			_yellow?.Init(dependencies);
		}

		public void SpawnEnemyManually(SoulType soulType, EnemyKind enemyKind, SpawnDirection direction = SpawnDirection.Right)
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("Manual spawn works only in Play Mode!");
				return;
			}

			var spawner = GetSpawnerBySoulType(soulType);

			if (spawner != null)
			{
				var section = _spawnerTokens.GetSectionByDirection(direction);
				var spawned = spawner.Spawn(section, enemyKind);
				_spawnerTokens.Commit(section, spawned);

				if (spawned > 0)
				{
					Debug.Log($"Spawned {enemyKind} with {soulType} soul type at {direction}");
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

		public void SpawnBlue()
		{
			var section = _spawnerTokens.SelectSection();
			var spawned = _blue?.Spawn(section) ?? 0;

			_spawnerTokens.Commit(section, spawned);
		}

		public void SpawnGreen()
		{
			var section = _spawnerTokens.SelectSection();
			var spawned = _green?.Spawn(section) ?? 0;

			_spawnerTokens.Commit(section, spawned);
		}

		public void SpawnRed()
		{
			var section = _spawnerTokens.SelectSection();
			var spawned = _red?.Spawn(section) ?? 0;

			_spawnerTokens.Commit(section, spawned);
		}

		public void SpawnYellow()
		{
			var section = _spawnerTokens.SelectSection();
			var spawned = _yellow?.Spawn(section) ?? 0;

			_spawnerTokens.Commit(section, spawned);
		}

		public void RequestSoulSpawn(SoulType soulType, Vector3 spawnPosition, DamageData damageData)
		{
			var spawner = GetSpawnerBySoulType(soulType);

			if (spawner != null)
			{
				var section = _spawnerTokens.SelectSection();
				var spawnedEnemy = spawner.SpawnAtPosition(section, EnemyKind.Soul, spawnPosition);
				_spawnerTokens.Commit(section, spawnedEnemy != null ? 1 : 0);

				if (spawnedEnemy != null)
				{
					var soul = spawnedEnemy.GetComponent<Soul>();
					if (soul != null)
					{
						soul.SpawnInitializate(enableCollisions: false);

						soul.ApplySpawnKnockback(damageData.KnockbackDirection, damageData.KnockbackForce);
					}
				}
			}
			else
			{
				Debug.LogWarning($"[{nameof(SpawnerEnemys)}] No spawner found for soul type {soulType}");
			}
		}
	}
}

