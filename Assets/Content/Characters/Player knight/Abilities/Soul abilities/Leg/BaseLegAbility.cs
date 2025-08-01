using Sirenix.OdinInspector;
using UnityEngine;

public abstract class BaseLegAbility : MonoBehaviour, IAbilityLeg
{
	[Header("Settings")]
	[SerializeField, MinValue(0)] protected float _speed = 1f;
	[SerializeField, MinValue(0)] protected float _durationMultiplier = 1f;

	public abstract bool HasVisualEffects { get; }

	public float Speed => _speed;
	public float DurationMultiplier => _durationMultiplier;

	public virtual void Initialize() { }
	public virtual void InitializeVisualEffects(Transform effectsParent) { }

	public virtual void Activate() { }
	public virtual void Deactivate() { }
}
