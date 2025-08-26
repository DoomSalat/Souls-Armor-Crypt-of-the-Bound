using System;
using System.Collections;
using UnityEngine;

public class Telegraph : MonoBehaviour
{
	[SerializeField, Min(0f)] private float _spawnDelay = 1f;
	[SerializeField, Min(0f)] private float _returnDelay = 1f;

	private TelegraphPool _ownerPool;
	private Coroutine _spawnPermissionCoroutine;
	private Coroutine _returnCoroutine;
	private Action _onReadyToSpawnEnemy;

	private WaitForSeconds _waitSpawn;
	private WaitForSeconds _waitReturn;

	private void Awake()
	{
		RebuildWaits();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (_spawnDelay < 0f) _spawnDelay = 0f;
		if (_returnDelay < 0f) _returnDelay = 0f;
	}
#endif

	public void SetDelays(float spawnDelay, float returnDelay)
	{
		_spawnDelay = Mathf.Max(0f, spawnDelay);
		_returnDelay = Mathf.Max(0f, returnDelay);
		RebuildWaits();
	}

	private void RebuildWaits()
	{
		_waitSpawn = _spawnDelay > 0f ? new WaitForSeconds(_spawnDelay) : null;
		_waitReturn = _returnDelay > 0f ? new WaitForSeconds(_returnDelay) : null;
	}

	public void Initialize(TelegraphPool pool)
	{
		_ownerPool = pool;
	}

	public void Activate(Vector3 position, Action onReadyToSpawnEnemy)
	{
		transform.position = position;
		_onReadyToSpawnEnemy = onReadyToSpawnEnemy;
		gameObject.SetActive(true);

		if (_spawnPermissionCoroutine != null)
			StopCoroutine(_spawnPermissionCoroutine);
		if (_returnCoroutine != null)
			StopCoroutine(_returnCoroutine);

		_spawnPermissionCoroutine = StartCoroutine(SpawnPermissionCoroutine());
		_returnCoroutine = StartCoroutine(ReturnCoroutine());
	}

	private IEnumerator SpawnPermissionCoroutine()
	{
		if (_waitSpawn != null)
			yield return _waitSpawn;

		_onReadyToSpawnEnemy?.Invoke();
		_onReadyToSpawnEnemy = null;
		_spawnPermissionCoroutine = null;
	}

	private IEnumerator ReturnCoroutine()
	{
		if (_waitReturn != null)
			yield return _waitReturn;

		ReturnToPool();
	}

	private void ReturnToPool()
	{
		if (_spawnPermissionCoroutine != null)
		{
			StopCoroutine(_spawnPermissionCoroutine);
			_spawnPermissionCoroutine = null;
		}
		if (_returnCoroutine != null)
		{
			StopCoroutine(_returnCoroutine);
			_returnCoroutine = null;
		}

		_onReadyToSpawnEnemy = null;
		_ownerPool?.Release(this);
	}
}


