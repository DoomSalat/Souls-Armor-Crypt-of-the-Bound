using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class GreenSwordAbility : MonoBehaviour, IAbilitySword
{
	private const int DefaultHitCount = 1;

	[SerializeField] private GreenPosionSeekerSpawner _greenPosionSeekerSpawnerPrefab;
	[SerializeField] private int _timeToActivate = 5;
	[SerializeField, ReadOnly] private bool _isCountingHits = false;
	[SerializeField, ReadOnly] private bool _isReadyToSpawn = false;

	private GreenPosionSeekerSpawner _greenPosionSeekerSpawner;
	private int _hitCount = 0;
	private Coroutine _hitCountdownCoroutine;
	private WaitForSeconds _hitCountdownWait;

	public bool HasVisualEffects => true;

	public void Initialize()
	{
		_hitCountdownWait = new WaitForSeconds(_timeToActivate);
	}

	public void InitializeVisualEffects(Transform effectsParent)
	{
		_greenPosionSeekerSpawner = Instantiate(_greenPosionSeekerSpawnerPrefab, effectsParent.position, Quaternion.identity, effectsParent);
		_greenPosionSeekerSpawner.Initialize();
	}

	public void Activate()
	{
		if (_isReadyToSpawn)
		{
			_greenPosionSeekerSpawner.SetCount(_hitCount);
			_greenPosionSeekerSpawner.SpawnSeekers();

			ResetState();
			return;
		}

		if (!_isCountingHits)
		{
			StartHitCounting();
		}
		else
		{
			_hitCount++;
		}
	}

	public void Deactivate() { }

	private void StartHitCounting()
	{
		_hitCount = DefaultHitCount;
		_isCountingHits = true;
		_isReadyToSpawn = false;

		if (_hitCountdownCoroutine != null)
		{
			StopCoroutine(_hitCountdownCoroutine);
		}

		_hitCountdownCoroutine = StartCoroutine(HitCountdown());
	}

	private IEnumerator HitCountdown()
	{
		yield return _hitCountdownWait;

		_isCountingHits = false;
		_isReadyToSpawn = true;
		_hitCountdownCoroutine = null;
	}

	private void ResetState()
	{
		_hitCount = 0;
		_isCountingHits = false;
		_isReadyToSpawn = false;
	}
}
