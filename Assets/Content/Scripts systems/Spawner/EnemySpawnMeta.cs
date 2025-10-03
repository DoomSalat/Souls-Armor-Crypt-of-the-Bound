using UnityEngine;

namespace SpawnerSystem
{
	public class EnemySpawnMeta : MonoBehaviour
	{
		public SpawnerSection Tokens { get; private set; }
		public SpawnerSystemData.SpawnSection Section { get; private set; }
		public EnemyKind Kind { get; private set; }
		public Transform InactiveParent { get; private set; }
		public int TokensToReturn { get; private set; }
		public float TimerReductionOnDeath { get; private set; }

		private ISpawnInitializable _spawnInitializable;

		private void Awake()
		{
			_spawnInitializable = GetComponent<ISpawnInitializable>();
		}

		public void Set(SpawnerSection tokens, SpawnerSystemData.SpawnSection section, EnemyKind kind = EnemyKind.Soul, Transform inactiveParent = null)
		{
			Tokens = tokens;
			Section = section;
			Kind = kind;
			InactiveParent = inactiveParent;
			TokensToReturn = 0;
			TimerReductionOnDeath = 0f;

			_spawnInitializable?.SpawnInitializate();
		}

		public void SetSpawnData(int tokensToReturn, float timerReduction)
		{
			TokensToReturn = tokensToReturn;
			TimerReductionOnDeath = timerReduction;
		}
	}
}