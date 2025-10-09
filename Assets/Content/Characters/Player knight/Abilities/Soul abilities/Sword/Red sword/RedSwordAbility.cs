using UnityEngine;
using System.Collections;

public class RedSwordAbility : MonoBehaviour, IAbilitySword
{
	[SerializeField] private RedFireSpawner _redFireSpawnerPrefab;
	[SerializeField] private int _spawnedDelay = 3;

	private RedFireSpawner _redFireSpawner;
	private SwordChargeEffect _chargeEffect;
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

	public void InitializeVisualEffects(Transform effectsParent, SwordChargeEffect chargeEffect)
	{
		_chargeEffect = chargeEffect;
		InitializeVisualEffects(effectsParent);

		_chargeEffect.PlayCharged();
	}

	public void Activate()
	{
		if (_isActive == false)
			return;

		_redFireSpawner.SpawnFire();
		_chargeEffect.Stop();

		if (_spawnCoroutine != null)
		{
			StopCoroutine(_spawnCoroutine);
		}

		_spawnCoroutine = StartCoroutine(SpawnWithDelay());
	}

	public void Deactivate()
	{
		if (_spawnCoroutine != null)
		{
			StopCoroutine(_spawnCoroutine);
			_spawnCoroutine = null;
		}

		_chargeEffect.Stop();
		_isActive = true;
	}

	private IEnumerator SpawnWithDelay()
	{
		_isActive = false;
		yield return _spawnedDelayWait;
		_isActive = true;

		_chargeEffect.PlayCharged();
	}
}
