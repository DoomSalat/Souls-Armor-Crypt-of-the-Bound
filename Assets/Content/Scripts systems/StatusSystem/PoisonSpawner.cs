using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace StatusSystem
{
	public class PoisonSpawner : StatusSpawner
	{
		[SerializeField] private StatusPoison _statusPrefab;
		[SerializeField] private int _poolSize = 20;

		[Header("Pool")]
		[SerializeField, Required] private Transform _unactiveContainer;
		[SerializeField, Required] private Transform _activeContainer;

		private ObjectPool<StatusPoison> _pool;
		private Dictionary<IDamageable, List<StatusPoison>> _activeStatuses = new Dictionary<IDamageable, List<StatusPoison>>();

		public override void Initialize()
		{
			InitializePool();
			PreloadPoolObjects();
		}

		public override void SpawnStatus(Transform statusPoint, IDamageable damageable)
		{
			StatusPoison statusPoison = _pool.Get();
			statusPoison.transform.SetParent(statusPoint);
			statusPoison.transform.localPosition = Vector3.zero;
			statusPoison.Initialize(damageable);

			if (_activeStatuses.ContainsKey(damageable) == false)
			{
				_activeStatuses[damageable] = new List<StatusPoison>();
			}

			_activeStatuses[damageable].Add(statusPoison);
		}

		public override void ClearStatuses(IDamageable damageable)
		{
			if (_activeStatuses.TryGetValue(damageable, out List<StatusPoison> statuses))
			{
				foreach (var status in statuses)
				{
					ReturnToPool(status);
				}

				_activeStatuses.Remove(damageable);
			}
		}

		private void InitializePool()
		{
			_pool = new ObjectPool<StatusPoison>(
				createFunc: CreatePool,
				actionOnGet: (status) =>
				{
					status.gameObject.SetActive(true);
					status.transform.SetParent(_activeContainer);
				},
				actionOnRelease: (status) =>
				{
					status.gameObject.SetActive(false);
					status.transform.SetParent(_unactiveContainer);
				},
				actionOnDestroy: (status) => { if (status != null) Destroy(status.gameObject); },
				collectionCheck: true,
				defaultCapacity: _poolSize,
				maxSize: _poolSize
			);
		}

		private void PreloadPoolObjects()
		{
			for (int i = 0; i < _poolSize; i++)
			{
				var statusPoison = _pool.Get();
				_pool.Release(statusPoison);
			}
		}

		private StatusPoison CreatePool()
		{
			var statusPoison = Instantiate(_statusPrefab, _unactiveContainer);
			statusPoison.OnStatusEndedCallback = OnStatusEnded;
			return statusPoison;
		}

		private void OnStatusEnded(StatusPoison statusPoison, IDamageable damageable)
		{
			if (_activeStatuses.TryGetValue(damageable, out List<StatusPoison> statuses))
			{
				statuses.Remove(statusPoison);

				if (statuses.Count == 0)
				{
					_activeStatuses.Remove(damageable);
				}
			}

			damageable.StatusEnd(DamageType.Poison);
			ReturnToPool(statusPoison);
		}

		private void ReturnToPool(StatusPoison statusPoison)
		{
			_pool.Release(statusPoison);
		}

		private void OnDestroy()
		{
			_pool?.Dispose();
		}
	}
}