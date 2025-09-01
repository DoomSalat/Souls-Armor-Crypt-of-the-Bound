using UnityEngine;
using CustomPool;
using Sirenix.OdinInspector;

public class ThrowSpawner : MonoBehaviour
{
	[Header("Pool Settings")]
	[SerializeField, Required] private ThrowObject _throwPrefab;
	[SerializeField] private int _defaultCapacity = 10;
	[SerializeField] private int _maxSize = 50;
	[SerializeField] private Transform _inactiveContainer;

	[Header("Debug")]
	[SerializeField] private bool _autoInitializate = false;

	private ObjectPool<ThrowObject> _throwPool;

	private void Start()
	{
		if (_autoInitializate)
			Initialize();
	}

	public void Initialize()
	{
		InitializePool();
		PrewarmPool();
	}

	public ThrowObject SpawnThrow(Vector3 position, Vector3 direction, Quaternion rotation, float speed, float endDistance, Vector3 scale)
	{
		ThrowObject throwObject = _throwPool.Spawn(position, rotation);
		throwObject.OnSpawnFromPool();

		throwObject.Setup(speed, endDistance, scale);
		throwObject.InitializeThrow(direction);

		return throwObject;
	}

	private void InitializePool()
	{
		_throwPool = new ObjectPool<ThrowObject>();
		_throwPool.Initialize(_throwPrefab, _inactiveContainer, _defaultCapacity, _maxSize);

		for (int i = 0; i < _defaultCapacity; i++)
		{
			ThrowObject throwObject = _throwPool.Spawn(Vector3.zero);
			throwObject.Initialize();
			_throwPool.Release(throwObject);
		}
	}

	private void PrewarmPool()
	{
		_throwPool.PrewarmPool();
	}

	private void OnDestroy()
	{
		_throwPool?.Dispose();
	}
}
