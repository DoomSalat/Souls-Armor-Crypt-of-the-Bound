using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
using Sirenix.OdinInspector;

public class GreenPosionSeekerSpawner : MonoBehaviour
{
	private const int DefaultCapacity = 5;
	private const int MaxCapacity = 10;

	[SerializeField] private GreenPoisonSeeker _greenPoisonSeekerPrefab;
	[SerializeField, MinValue(0)] private int _spawnActivatedCount = 3;

	private ObjectPool<GreenPoisonSeeker> _objectPool;
	private List<GreenPoisonSeeker> _activeSeekers = new List<GreenPoisonSeeker>();
	private Transform _seekerTarget;

	private void OnDestroy()
	{
		foreach (var seeker in _activeSeekers)
		{
			if (seeker != null)
			{
				Destroy(seeker.gameObject);
			}
		}
		_activeSeekers.Clear();

		_objectPool?.Dispose();
	}

	public void Initialize()
	{
		InitializePool();
	}

	public void Initialize(Transform seekerTarget)
	{
		InitializePool();
		_seekerTarget = seekerTarget;
	}

	public void SpawnSeekers()
	{
		int availableSlots = MaxCapacity - _activeSeekers.Count;
		int spawnCount = Mathf.Min(_spawnActivatedCount, availableSlots);

		for (int i = 0; i < spawnCount; i++)
		{
			SpawnSingleSeeker();
		}
	}

	private void InitializePool()
	{
		_objectPool = new ObjectPool<GreenPoisonSeeker>(
			createFunc: CreateSeeker,
			actionOnGet: OnTakeFromPool,
			actionOnRelease: OnReturnToPool,
			actionOnDestroy: OnDestroyPoolObject,
			collectionCheck: true,
			defaultCapacity: DefaultCapacity,
			maxSize: MaxCapacity
		);
	}

	private GreenPoisonSeeker CreateSeeker()
	{
		GreenPoisonSeeker seeker = Instantiate(_greenPoisonSeekerPrefab, transform.position, Quaternion.identity);
		seeker.SeekerDestroyed += OnSeekerDestroyed;
		return seeker;
	}

	private void OnTakeFromPool(GreenPoisonSeeker seeker)
	{
		if (_activeSeekers.Count >= MaxCapacity)
		{
			_objectPool.Release(seeker);
			return;
		}

		seeker.gameObject.SetActive(true);
		seeker.transform.SetParent(null);
		seeker.transform.position = transform.position;
		_activeSeekers.Add(seeker);

		if (_seekerTarget != null)
		{
			seeker.Initialize(Vector2.zero, _seekerTarget);
		}
		else
		{
			Vector2 randomDirection = Random.insideUnitCircle.normalized;
			seeker.Initialize(randomDirection);
		}
	}

	private void OnReturnToPool(GreenPoisonSeeker seeker)
	{
		seeker.gameObject.SetActive(false);
		_activeSeekers.Remove(seeker);
	}

	private void OnDestroyPoolObject(GreenPoisonSeeker seeker)
	{
		if (seeker != null)
		{
			seeker.SeekerDestroyed -= OnSeekerDestroyed;
			Destroy(seeker.gameObject);
		}
	}

	private void OnSeekerDestroyed(GreenPoisonSeeker seeker)
	{
		if (seeker != null && _activeSeekers.Contains(seeker))
		{
			_objectPool.Release(seeker);
		}
	}

	private void SpawnSingleSeeker()
	{
		if (_activeSeekers.Count < MaxCapacity)
		{
			_objectPool.Get();
		}
	}
}
