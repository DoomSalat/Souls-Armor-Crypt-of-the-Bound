using UnityEngine;

namespace SpawnerSystem
{
	public class EnemySpawnMeta : MonoBehaviour
	{
		public SpawnerTokens Tokens { get; private set; }
		public SpawnSection Section { get; private set; }
		public EnemyKind Kind { get; private set; }
		public Transform InactiveParent { get; private set; }

		private ISpawnInitializable _spawnInitializable;

		private void Awake()
		{
			_spawnInitializable = GetComponent<ISpawnInitializable>();
		}

		public void Set(SpawnerTokens tokens, SpawnSection section, EnemyKind kind = EnemyKind.Soul, Transform inactiveParent = null)
		{
			Tokens = tokens;
			Section = section;
			Kind = kind;
			InactiveParent = inactiveParent;
			Tokens?.CommitKind(section, kind, 1);

			_spawnInitializable?.SpawnInitializate();
		}

		private void OnDisable()
		{
			if (Tokens != null)
			{
				Tokens.Release(Section, 1);
				Tokens.ReleaseKind(Section, Kind, 1);
			}
		}
	}
}


