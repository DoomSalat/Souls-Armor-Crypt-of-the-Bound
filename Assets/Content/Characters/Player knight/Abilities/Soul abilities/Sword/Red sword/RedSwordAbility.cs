using UnityEngine;
using System.Collections;

public class RedSwordAbility : MonoBehaviour, IAbilitySword
{
	[SerializeField] private RedFireSpawner _redFireSpawnerPrefab;
	[SerializeField] private int _spawnedDelay = 3;

	private RedFireSpawner _redFireSpawner;
	private Coroutine _spawnCoroutine;
	private WaitForSeconds _spawnedDelayWait;
	private bool _isActive;

	public bool HasVisualEffects => true;

	public void Initialize()
	{
		_spawnedDelayWait = new WaitForSeconds(_spawnedDelay);
		_isActive = true;
	}

	public void InitializeVisualEffects(Transform effectsParent)
	{
		_redFireSpawner = Instantiate(_redFireSpawnerPrefab, effectsParent.position, Quaternion.identity, effectsParent);
		_redFireSpawner.Initialize();
	}

	public void Activate()
	{
		if (_isActive == false)
			return;

		_redFireSpawner.SpawnFire();

		if (_spawnCoroutine != null)
		{
			StopCoroutine(_spawnCoroutine);
		}

		_spawnCoroutine = StartCoroutine(SpawnWithDelay());
	}

	private IEnumerator SpawnWithDelay()
	{
		_isActive = false;
		yield return _spawnedDelayWait;
		_isActive = true;
	}

	public void Deactivate() { }
}
