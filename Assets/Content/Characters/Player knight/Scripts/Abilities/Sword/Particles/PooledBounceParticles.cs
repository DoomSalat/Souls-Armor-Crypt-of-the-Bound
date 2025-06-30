using UnityEngine;
using System.Collections;
using CustomPool;

[RequireComponent(typeof(ParticleSystem))]
public class PooledBounceParticles : MonoBehaviour, IPool, IPoolReference
{
	private const float DefaultDirection = 90f;
	private const float DefaultFinishDelay = 0.1f;

	[Header("Particle Settings")]
	[SerializeField] private bool _playOnSpawn = true;
	[SerializeField] private bool _alignToSurface = true;
	[SerializeField] private float _defaultDirectionAngle = DefaultDirection;

	private ParticleSystem _particleSystem;
	private ObjectPool<PooledBounceParticles> _pool;
	private Coroutine _returnToPoolCoroutine;
	private WaitWhile _waitForParticlesFinish;
	private WaitForSeconds _waitForFinishDelay;

	public bool IsPlaying => _particleSystem.isPlaying;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();

		var main = _particleSystem.main;
		main.playOnAwake = false;

		_waitForParticlesFinish = new WaitWhile(() => _particleSystem.isPlaying);
		_waitForFinishDelay = new WaitForSeconds(DefaultFinishDelay);
	}

	private void OnDestroy()
	{
		if (_returnToPoolCoroutine != null)
		{
			StopCoroutine(_returnToPoolCoroutine);
		}
	}

	public void PlayEffect(Vector3 position, Vector3 surfaceNormal)
	{
		transform.position = position;

		if (_alignToSurface)
		{
			AlignToSurfaceNormal(surfaceNormal);
		}

		PlayEffect();
	}

	public void PlayEffect()
	{
		_particleSystem.Play();
		StartReturnToPoolTimer();
	}

	public void StopEffect()
	{
		if (_returnToPoolCoroutine != null)
		{
			StopCoroutine(_returnToPoolCoroutine);
			_returnToPoolCoroutine = null;
		}

		_particleSystem.Stop();
		ReturnToPool();
	}

	public void ReturnToPool()
	{
		if (_pool != null)
		{
			_pool.Release(this);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	public void OnSpawnFromPool()
	{
		if (_playOnSpawn)
		{
			PlayEffect();
		}
	}

	public void OnReturnToPool()
	{
		if (_returnToPoolCoroutine != null)
		{
			StopCoroutine(_returnToPoolCoroutine);
			_returnToPoolCoroutine = null;
		}

		_particleSystem.Stop();
		_particleSystem.Clear();

		transform.rotation = Quaternion.identity;
	}

	public void SetPool(object pool)
	{
		_pool = pool as ObjectPool<PooledBounceParticles>;
	}

	private void AlignToSurfaceNormal(Vector3 surfaceNormal)
	{
		if (surfaceNormal == Vector3.zero)
		{
			transform.rotation = Quaternion.AngleAxis(_defaultDirectionAngle, Vector3.forward);
			return;
		}

		Vector3 direction = surfaceNormal;
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	private void StartReturnToPoolTimer()
	{
		if (_returnToPoolCoroutine != null)
		{
			StopCoroutine(_returnToPoolCoroutine);
		}

		_returnToPoolCoroutine = StartCoroutine(WaitForParticlesFinish());
	}

	private IEnumerator WaitForParticlesFinish()
	{
		yield return _waitForParticlesFinish;
		yield return _waitForFinishDelay;

		ReturnToPool();
	}
}