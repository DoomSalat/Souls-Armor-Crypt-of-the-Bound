using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

public class GreenArmAbility : BaseArmAbility
{
	[SerializeField] private GreenPosionSeekerSpawner _greenPosionSeekerSpawnerPrefab;
	[SerializeField, MinValue(0)] private float _spawnRate = 1.5f;

	private GreenPosionSeekerSpawner _greenPosionSeekerSpawner;
	private Coroutine _spawnCoroutine;
	private WaitForSeconds _spawnWait;
	private bool _isActive;

	public override bool HasVisualEffects => true;

	public override void Initialize()
	{
		_greenPosionSeekerSpawnerPrefab.Initialize();

		_spawnWait = new WaitForSeconds(_spawnRate);
	}

	public override void InitializeVisualEffects(Transform effectsParent)
	{
		_greenPosionSeekerSpawner = Instantiate(_greenPosionSeekerSpawnerPrefab, effectsParent.position, Quaternion.identity, effectsParent);

		if (effectsParent.TryGetComponent<LimbEffectsData>(out var armData))
		{
			_greenPosionSeekerSpawner.Initialize(armData.SwordTarget);
		}
	}

	public override void Activate()
	{
		if (_isActive)
			return;

		_isActive = true;
		_spawnCoroutine = StartCoroutine(SpawnCycle());
	}

	public override void Deactivate()
	{
		if (_isActive == false)
			return;

		_isActive = false;

		if (_spawnCoroutine != null)
		{
			StopCoroutine(_spawnCoroutine);
			_spawnCoroutine = null;
		}
	}

	private IEnumerator SpawnCycle()
	{
		while (_isActive)
		{
			yield return _spawnWait;
			_greenPosionSeekerSpawner.SpawnSeekers();
		}
	}
}
