using UnityEngine;

namespace SpawnerSystem
{
	public class SimpleSpawnStrategy : ISpawnStrategy
	{
		[Header("Randomization Settings")]
		[SerializeField, Range(0f, 1f)] private float _randomizationStrength = 0.5f;
		[SerializeField, Range(0f, 2f)] private float _distanceVariationMultiplier = 1f;

		public virtual Vector3 CalculateSpawnPosition(SpawnerSystemData.SpawnSection section, SpawnerDependencies dependencies)
		{
			return CalculateRandomSpawnPosition(section, dependencies);
		}

		protected virtual Vector3 CalculateRandomSpawnPosition(SpawnerSystemData.SpawnSection section, SpawnerDependencies dependencies)
		{
			if (dependencies.Tokens == null)
				throw new System.InvalidOperationException("SpawnerSection reference is required for spawn positioning");

			SpawnerSystemData.SectionSpawnInfo sectionInfo = dependencies.Tokens.GetSectionSpawnInfo(section);

			return CalculateRandomPositionInSection(sectionInfo);
		}

		protected virtual Vector3 CalculateRandomPositionInSection(SpawnerSystemData.SectionSpawnInfo sectionInfo)
		{
			float randomAngle = Random.Range(sectionInfo.StartAngle, sectionInfo.EndAngle);

			float angleVariation = (sectionInfo.EndAngle - sectionInfo.StartAngle) * (1f - _randomizationStrength);
			randomAngle = Random.Range(sectionInfo.StartAngle + angleVariation * 0.5f, sectionInfo.EndAngle - angleVariation * 0.5f);

			float maxDistanceVariation = sectionInfo.Radius * 0.1f * _distanceVariationMultiplier;
			float randomRadius = sectionInfo.Radius + Random.Range(-maxDistanceVariation, maxDistanceVariation) * _randomizationStrength;

			Vector3 spawnDirection = new Vector3(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle), 0f);
			Vector3 spawnPosition = sectionInfo.Center + spawnDirection * randomRadius;
			spawnPosition.z = sectionInfo.Center.z;

			return spawnPosition;
		}

		public virtual int GetSpawnCount(SpawnerSystemData.SpawnSection section)
		{
			return 1;
		}

		public virtual bool OnBeforeSpawn(Vector3 position, PooledEnemy prefab, SpawnerSystemData.SpawnSection section, EnemyKind kind)
		{
			return false;
		}

		public virtual void OnAfterSpawn(PooledEnemy spawned, Vector3 position, SpawnerSystemData.SpawnSection section, EnemyKind kind)
		{

		}
	}
}
