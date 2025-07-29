using System.Collections.Generic;
using UnityEngine;

namespace StatusSystem
{
	public class StatusMachine : MonoBehaviour
	{
		[SerializeField] private List<StatusSpawnerData> _statusSpawners = new List<StatusSpawnerData>();

		private Dictionary<DamageType, StatusSpawner> _spawnerMap = new Dictionary<DamageType, StatusSpawner>();

		private void Awake()
		{
			InitializeSpawnerMap();
		}

		private void InitializeSpawnerMap()
		{
			foreach (var spawnerData in _statusSpawners)
			{
				if (spawnerData.spawner != null)
				{
					_spawnerMap[spawnerData.damageType] = spawnerData.spawner;
					spawnerData.spawner.Initialize();
				}
			}
		}

		public void ApplyStatus(DamageType damageType, Transform statusPoint, IDamageable damageable)
		{
			if (_spawnerMap.TryGetValue(damageType, out StatusSpawner spawner))
			{
				spawner.SpawnStatus(statusPoint, damageable);
			}
		}

		public void ClearAllStatuses(IDamageable damageable)
		{
			foreach (var spawner in _spawnerMap.Values)
			{
				spawner.ClearStatuses(damageable);
			}
		}
	}
}