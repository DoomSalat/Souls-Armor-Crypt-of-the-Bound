using UnityEngine;

namespace SpawnerSystem
{
	public class SpawnerRed : SpawnerBase
	{
		private const float MinSpawnDistance = 0.1f;

		[Header("Spawn Distance Settings")]
		[SerializeField, Min(0.1f)] private float _minSpawnDistancePlayer = 1.0f;
		[SerializeField, Min(0.1f)] private float _maxSpawnDistancePlayer = 2.5f;

		[Header("Telegraph Configuration")]
		[SerializeField] private TelegraphPool _telegraphPool;

		private TelegraphSpawnStrategy _telegraphStrategy;

		public override void Init(SpawnerDependencies dependencies)
		{
			_telegraphStrategy = new TelegraphSpawnStrategy();
			_telegraphStrategy.SetOwnerSpawner(this);
			_telegraphStrategy.SetSpawnDistance(_minSpawnDistancePlayer, _maxSpawnDistancePlayer);
			_telegraphStrategy.SetTelegraphPool(_telegraphPool);

			_spawnStrategy = _telegraphStrategy;

			base.Init(dependencies);
		}

		public void SetSpawnDistance(float minDistance, float maxDistance)
		{
			_minSpawnDistancePlayer = Mathf.Max(MinSpawnDistance, minDistance);
			_maxSpawnDistancePlayer = Mathf.Max(_minSpawnDistancePlayer, maxDistance);

			if (_telegraphStrategy != null)
			{
				_telegraphStrategy.SetSpawnDistance(_minSpawnDistancePlayer, _maxSpawnDistancePlayer);
			}
		}

		public void SetTelegraphPool(TelegraphPool telegraphPool)
		{
			_telegraphPool = telegraphPool;

			if (_telegraphStrategy != null)
			{
				_telegraphStrategy.SetTelegraphPool(_telegraphPool);
			}
		}


#if UNITY_EDITOR
		private void OnValidate()
		{
			if (_telegraphStrategy != null)
			{
				_telegraphStrategy.SetSpawnDistance(_minSpawnDistancePlayer, _maxSpawnDistancePlayer);
				_telegraphStrategy.SetTelegraphPool(_telegraphPool);
			}
		}
#endif
	}
}
