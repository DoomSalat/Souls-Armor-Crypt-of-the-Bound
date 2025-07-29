using UnityEngine;

namespace StatusSystem
{
	public abstract class StatusSpawner : MonoBehaviour
	{
		public abstract void Initialize();
		public abstract void SpawnStatus(Transform statusPoint, IDamageable damageable);
		public abstract void ClearStatuses(IDamageable damageable);
	}
}