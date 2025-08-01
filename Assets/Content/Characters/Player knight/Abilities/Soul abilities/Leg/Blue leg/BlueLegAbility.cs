using UnityEngine;
using Sirenix.OdinInspector;

public class BlueLegAbility : BaseLegAbility
{
	public override bool HasVisualEffects => false;

	public override void Initialize() { }
	public override void Activate() { }

	public override void Deactivate() { }
	public override void InitializeVisualEffects(Transform effectsParent) { }
}