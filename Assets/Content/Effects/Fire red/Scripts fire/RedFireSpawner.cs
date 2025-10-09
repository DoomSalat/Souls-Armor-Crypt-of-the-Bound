using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class RedFireSpawner : MonoBehaviour
{
	private const int DefaultCapacity = 5;
	private const int MaxCapacity = 10;

	[SerializeField] private RedFire _redFirePrefab;

	private ObjectPool<RedFire> _objectPool;
	private List<RedFire> _activeFires = new List<RedFire>();

	private void OnDestroy()
	{
		foreach (var fire in _activeFires)
		{
			if (fire != null)
			{
				Destroy(fire.gameObject);
			}
		}
		_activeFires.Clear();

		_objectPool?.Dispose();
	}

	public void Initialize()
	{
		InitializePool();
	}

	public RedFire SpawnFire()
	{
		if (_activeFires.Count >= MaxCapacity)
		{
			RedFire firstFire = _activeFires[0];
			_objectPool.Release(firstFire);
		}

		return _objectPool.Get();
	}

	private void InitializePool()
	{
		_objectPool = new ObjectPool<RedFire>(
			createFunc: CreateFire,
			actionOnGet: OnTakeFromPool,
			actionOnRelease: OnReturnToPool,
			actionOnDestroy: OnDestroyPoolObject,
			collectionCheck: true,
			defaultCapacity: DefaultCapacity,
			maxSize: MaxCapacity
		);
	}

	private RedFire CreateFire()
	{
		RedFire fire = Instantiate(_redFirePrefab, transform.position, Quaternion.identity);
		fire.PreInitialize();
		fire.FireDestroyed += OnFireDestroyed;

		return fire;
	}

	private void OnTakeFromPool(RedFire fire)
	{
		if (_activeFires.Count >= MaxCapacity)
		{
			_objectPool.Release(fire);
			return;
		}

		fire.gameObject.SetActive(true);
		fire.transform.SetParent(null);
		fire.transform.position = transform.position;
		_activeFires.Add(fire);

		fire.Initialize();
	}

	private void OnReturnToPool(RedFire fire)
	{
		fire.gameObject.SetActive(false);
		_activeFires.Remove(fire);
	}

	private void OnDestroyPoolObject(RedFire fire)
	{
		if (fire != null)
		{
			fire.FireDestroyed -= OnFireDestroyed;
			Destroy(fire.gameObject);
		}
	}

	private void OnFireDestroyed(RedFire fire)
	{
		if (fire != null && _activeFires.Contains(fire))
		{
			_objectPool.Release(fire);
		}
	}
}
