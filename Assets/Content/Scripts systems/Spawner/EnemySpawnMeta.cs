using UnityEngine;

namespace SpawnerSystem
{
	public class EnemySpawnMeta : MonoBehaviour
	{
		public SpawnerSection Tokens { get; private set; }
		public SpawnSection Section { get; private set; }
		public EnemyKind Kind { get; private set; }
		public Transform InactiveParent { get; private set; }
		public float TokensToReturn { get; private set; }
		public float TimerReductionOnDeath { get; private set; }

		private ISpawnInitializable _spawnInitializable;

		private void Awake()
		{
			_spawnInitializable = GetComponent<ISpawnInitializable>();
		}

		public void Set(SpawnerSection tokens, SpawnSection section, EnemyKind kind = EnemyKind.Soul, Transform inactiveParent = null)
		{
			Tokens = tokens;
			Section = section;
			Kind = kind;
			InactiveParent = inactiveParent;
			TokensToReturn = 0f;
			TimerReductionOnDeath = 0f;

			_spawnInitializable?.SpawnInitializate();
		}

		public void SetSpawnData(float tokensToReturn, float timerReduction)
		{
			TokensToReturn = tokensToReturn;
			TimerReductionOnDeath = timerReduction;
		}

		private void OnDisable()
		{
			if (Tokens != null)
			{
				Tokens.Release(Section, 1);
			}
		}
	}
}