using UnityEngine;

public interface IAbilitySword : IAbility
{
	void InitializeVisualEffects(Transform effectsParent, SwordChargeEffect chargeEffect);
}
