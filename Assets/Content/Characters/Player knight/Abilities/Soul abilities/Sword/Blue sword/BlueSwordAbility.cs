using UnityEngine;

public class BlueSwordAbility : MonoBehaviour, IAbilitySword
{
	public bool HasVisualEffects => false;

	public void Initialize()
	{

	}

	public void InitializeVisualEffects(Transform effectsParent)
	{

	}

	public void InitializeVisualEffects(Transform effectsParent, SwordChargeEffect chargeEffect)
	{
		InitializeVisualEffects(effectsParent);
	}

	public void Activate()
	{

	}

	public void Deactivate() { }
}
