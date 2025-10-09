using UnityEngine;
using Sirenix.OdinInspector;

public abstract class BaseSoul : MonoBehaviour, ISoul
{
	[SerializeField, Required] protected Soul _soulComponent;

	public Transform Transform => transform;

	public abstract SoulType GetSoulType();

	public virtual void OnAbsorptionCompleted()
	{
		_soulComponent.OnAbsorptionCompleted();
	}

	public virtual void StartAttraction(Transform target, System.Action onAttractionCompleted)
	{
		_soulComponent.StartAttraction(target, onAttractionCompleted);
	}

	public virtual void ApplySpawnKnockback(Vector2 direction, float force)
	{
		_soulComponent.ApplySpawnKnockback(direction, force);
	}

	public virtual void SetAnimatorUnscaledTime()
	{
		_soulComponent.SetAnimatorUnscaledTime();
	}
}