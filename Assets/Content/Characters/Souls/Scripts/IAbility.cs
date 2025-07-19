using UnityEngine;

public interface IAbility
{
	public bool HasVisualEffects { get; }

	void Initialize();
	void Activate();
	void Deactivate();

	void InitializeVisualEffects(Transform effectsParent);
}