using UnityEngine;

namespace CustomPool
{
	public class ObjectPool<T> where T : MonoBehaviour
	{
		private const int DefaultCapacity = 10;
		private const int MaxSize = 100;

		[Header("Pool Settings")]
		[SerializeField] private T _prefab;
		[SerializeField, Min(1)] private int _defaultCapacity = DefaultCapacity;
		[SerializeField, Min(1)] private int _maxSize = MaxSize;
		[SerializeField] private bool _collectionCheck = true;

		private UnityEngine.Pool.ObjectPool<T> _pool;
		private Transform _poolParent;

		public int CountInactive => _pool?.CountInactive ?? 0;
		public int CountActive => _pool?.CountActive ?? 0;
		public int CountAll => _pool?.CountAll ?? 0;

		public void Initialize(T prefab, Transform parentTransform, int defaultCapacity = DefaultCapacity, int maxSize = MaxSize, bool collectionCheck = true)
		{
			_prefab = prefab;
			_defaultCapacity = defaultCapacity;
			_maxSize = maxSize;
			_collectionCheck = collectionCheck;

			CreatePoolParent(parentTransform);
			InitializePool();
		}

		public T Get()
		{
			return _pool.Get();
		}

		public void Release(T pooledObject)
		{
			_pool.Release(pooledObject);
		}

		public T Spawn(Vector3 position, Quaternion rotation)
		{
			T pooledObject = Get();
			pooledObject.transform.position = position;
			pooledObject.transform.rotation = rotation;

			return pooledObject;
		}

		public T Spawn(Vector3 position)
		{
			return Spawn(position, Quaternion.identity);
		}

		public void Dispose()
		{
			_pool?.Clear();
		}

		public void PrewarmPool()
		{
			T[] prewarmObjects = new T[_defaultCapacity];

			for (int i = 0; i < _defaultCapacity; i++)
			{
				prewarmObjects[i] = Get();
			}

			for (int i = 0; i < _defaultCapacity; i++)
			{
				Release(prewarmObjects[i]);
			}
		}

		private void InitializePool()
		{
			_pool = new UnityEngine.Pool.ObjectPool<T>(
				CreatePooledItem,
				OnTakeFromPool,
				OnReturnedToPool,
				OnDestroyPoolObject,
				_collectionCheck,
				_defaultCapacity,
				_maxSize
			);
		}

		private void CreatePoolParent(Transform parentTransform)
		{
			if (parentTransform != null)
			{
				_poolParent = parentTransform;
			}
			else
			{
				GameObject poolParentGO = new GameObject($"{_prefab.name} Pool");
				_poolParent = poolParentGO.transform;
			}
		}

		private T CreatePooledItem()
		{
			T instance = Object.Instantiate(_prefab, _poolParent);

			if (instance.TryGetComponent<IPoolReference>(out var poolRef))
			{
				poolRef.SetPool(this);
			}

			return instance;
		}

		private void OnTakeFromPool(T pooledObject)
		{
			pooledObject.gameObject.SetActive(true);
			pooledObject.transform.SetParent(null);
		}

		private void OnReturnedToPool(T pooledObject)
		{
			pooledObject.gameObject.SetActive(false);
			pooledObject.transform.SetParent(_poolParent);
		}

		private void OnDestroyPoolObject(T pooledObject)
		{
			Object.Destroy(pooledObject.gameObject);
		}
	}
}