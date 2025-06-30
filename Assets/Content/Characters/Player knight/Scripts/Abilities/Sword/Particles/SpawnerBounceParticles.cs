using UnityEngine;
using Sirenix.OdinInspector;
using CustomPool;

public class SpawnerBounceParticles : MonoBehaviour
{
	private const int DefaultPoolSize = 5;

	[Header("Particle Pool Settings")]
	[SerializeField, Required] private PooledBounceParticles _wallImpactParticlePrefab;
	[SerializeField, Min(1)] private int _poolSize = DefaultPoolSize;
	[SerializeField] private bool _prewarmPool = true;

	[Header("Debug")]
	[ShowInInspector, ReadOnly] private int _activeParticles;
	[ShowInInspector, ReadOnly] private int _pooledParticles;

	private ObjectPool<PooledBounceParticles> _particlePool;

	public int ActiveParticles => _particlePool?.CountActive ?? 0;
	public int PooledParticles => _particlePool?.CountInactive ?? 0;

	private void Awake()
	{
		InitializeParticlePool();
	}

	private void OnValidate()
	{
		if (_poolSize <= 0)
		{
			_poolSize = DefaultPoolSize;
		}
	}

	private void Start()
	{
		if (_prewarmPool)
		{
			PrewarmPool();
		}
	}

	private void Update()
	{
		if (_particlePool != null)
		{
			_activeParticles = _particlePool.CountActive;
			_pooledParticles = _particlePool.CountInactive;
		}
	}

	private void OnDestroy()
	{
		ClearAllParticles();
		_particlePool?.Dispose();
	}

	public void SpawnWallImpactEffect(Vector3 impactPoint, Vector3 wallNormal)
	{
		if (_particlePool == null)
		{
			Debug.LogWarning($"[{name}] Particle pool not initialized!", this);
			return;
		}

		PooledBounceParticles particle = _particlePool.Get();
		if (particle != null)
		{
			particle.PlayEffect(impactPoint, wallNormal);
		}
	}

	public void SpawnEffect(Vector3 position)
	{
		SpawnWallImpactEffect(position, Vector3.up);
	}

	[ContextMenu(nameof(PrewarmPool))]
	private void PrewarmPool()
	{
		if (_particlePool != null)
		{
			_particlePool.PrewarmPool();
		}
	}

	[ContextMenu(nameof(ClearAllParticles))]
	public void ClearAllParticles()
	{
		if (_particlePool == null)
			return;

		var activeParticles = FindObjectsByType<PooledBounceParticles>(FindObjectsSortMode.None);
		foreach (var particle in activeParticles)
		{
			if (particle.gameObject.activeInHierarchy)
			{
				particle.StopEffect();
			}
		}
	}

	private void InitializeParticlePool()
	{
		if (_wallImpactParticlePrefab == null)
		{
			Debug.LogError($"[{name}] Wall Impact Particle Prefab not assigned!", this);
			return;
		}

		_particlePool = new ObjectPool<PooledBounceParticles>();
		_particlePool.Initialize(_wallImpactParticlePrefab, transform, _poolSize, _poolSize * 2, true);
	}
}