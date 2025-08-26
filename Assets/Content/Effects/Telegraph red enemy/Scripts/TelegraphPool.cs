using System;
using UnityEngine;
using UnityEngine.Pool;

public class TelegraphPool : MonoBehaviour
{
	[SerializeField] private Telegraph _prefab;
	[SerializeField] private int _defaultCapacity = 4;
	[SerializeField] private int _maxSize = 16;
	[SerializeField] private Transform _container;

	private ObjectPool<Telegraph> _pool;

	private void Awake()
	{
		_pool = new ObjectPool<Telegraph>(
			createFunc: CreateInstance,
			actionOnGet: OnGet,
			actionOnRelease: OnRelease,
			actionOnDestroy: OnDestroyInstance,
			collectionCheck: false,
			defaultCapacity: _defaultCapacity,
			maxSize: _maxSize
		);

		for (int i = 0; i < _defaultCapacity; i++)
		{
			var t = _pool.Get();
			_pool.Release(t);
		}
	}

	private Telegraph CreateInstance()
	{
		if (_prefab == null)
			return null;

		Telegraph instance = Instantiate(_prefab, _container != null ? _container : transform);
		instance.Initialize(this);
		instance.gameObject.SetActive(false);
		return instance;
	}

	private void OnGet(Telegraph instance)
	{
		if (instance == null)
			return;

		instance.gameObject.SetActive(true);
	}

	private void OnRelease(Telegraph instance)
	{
		if (instance == null) return;
		instance.gameObject.SetActive(false);
		if (_container != null)
			instance.transform.SetParent(_container, false);
	}

	private void OnDestroyInstance(Telegraph instance)
	{
		if (instance != null)
			Destroy(instance.gameObject);
	}

	public Telegraph Get()
	{
		return _pool?.Get();
	}

	public void Release(Telegraph instance)
	{
		_pool?.Release(instance);
	}
}


