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
}