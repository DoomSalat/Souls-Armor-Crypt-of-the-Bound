using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
using Sirenix.OdinInspector;

public class GreenPosionSeekerSpawner : MonoBehaviour
{
	private const int DefaultCapacity = 10;
	private const int MaxSize = 50;

	[SerializeField] private GreenPoisonSeeker _greenPoisonSeekerPrefab;
	[SerializeField, MinValue(0)] private int _spawnActivatedCount = 3;

	private ObjectPool<GreenPoisonSeeker> _objectPool;
	private List<GreenPoisonSeeker> _activeSeekers = new List<GreenPoisonSeeker>();

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

	private void InitializePool()
	{
		_objectPool = new ObjectPool<GreenPoisonSeeker>(
			createFunc: CreateSeeker,
			actionOnGet: OnTakeFromPool,
			actionOnRelease: OnReturnToPool,
			actionOnDestroy: OnDestroyPoolObject,
			collectionCheck: true,
			defaultCapacity: DefaultCapacity,
			maxSize: MaxSize
		);
	}

	private GreenPoisonSeeker CreateSeeker()
	{
		GreenPoisonSeeker seeker = Instantiate(_greenPoisonSeekerPrefab, transform.position, Quaternion.identity);
		return seeker;
	}

	private void OnTakeFromPool(GreenPoisonSeeker seeker)
	{
		seeker.gameObject.SetActive(true);
		seeker.transform.SetParent(null);
		seeker.transform.position = transform.position;
		_activeSeekers.Add(seeker);

		Vector2 randomDirection = Random.insideUnitCircle.normalized;
		seeker.Initialize(randomDirection);
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
			Destroy(seeker.gameObject);
		}
	}

	public void SpawnSeekers()
	{
		for (int i = 0; i < _spawnActivatedCount; i++)
		{
			SpawnSingleSeeker();
		}
	}

	private void SpawnSingleSeeker()
	{
		_objectPool.Get();
	}
}
