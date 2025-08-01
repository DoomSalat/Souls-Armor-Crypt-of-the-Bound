using UnityEngine;

public class YellowLegAbility : BaseLegAbility
{
	public override bool HasVisualEffects => false;

	public override void Initialize() { }
	public override void InitializeVisualEffects(Transform effectsParent) { }

	public override void Activate() { }
	public override void Deactivate() { }
}
