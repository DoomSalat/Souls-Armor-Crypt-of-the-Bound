using UnityEngine;
using Sirenix.OdinInspector;

public abstract class BaseArmAbility : MonoBehaviour, IAbilityArm
{
	[Header("Settings")]
	[SerializeField, MinValue(0)] protected float _swordSpeed = 1f;
	[SerializeField, MinValue(0)] protected int _swordScaleIndex = 0;

	public float SwordSpeed => _swordSpeed;
	public int SwordScaleIndex => _swordScaleIndex;

	public abstract bool HasVisualEffects { get; }

	public virtual void Initialize() { }
	public virtual void InitializeVisualEffects(Transform effectsParent) { }
	public virtual void Activate() { }
	public virtual void Deactivate() { }

	public virtual void SetSwordSettings(SwordController swordController)
	{
		swordController.SetSwordSpeed(_swordSpeed);
		swordController.SetIndexAttackZoneScale(_swordScaleIndex);
	}
}