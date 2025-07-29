using UnityEngine;

public class GreenPoisonAbility : MonoBehaviour, IAbilityBody
{
	[SerializeField] private GreenPosionSeekerSpawner _greenPosionSeekerSpawnerPrefab;

	private GreenPosionSeekerSpawner _greenPosionSeekerSpawner;

	public bool HasVisualEffects => true;

	public void Initialize()
	{

	}

	public void InitializeVisualEffects(Transform effectsParent)
	{
		_greenPosionSeekerSpawner = Instantiate(_greenPosionSeekerSpawnerPrefab, effectsParent);
		_greenPosionSeekerSpawner.Initialize();
	}

	public void Activate()
	{
		_greenPosionSeekerSpawner.SpawnSeekers();
	}

	public bool CanBlockDamage()
	{
		return false;
	}

	public void DamageBlocked() { }

	public void Deactivate() { }
}
