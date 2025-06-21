using UnityEngine;
using Sirenix.OdinInspector;

public class RedSoulAbility : MonoBehaviour, ISoul
{
	[SerializeField, Required] private Soul _soulComponent;

	public Transform Transform => transform;

	public IAbility GetAbility()
	{
		return null;
	}

	public SoulType GetSoulType()
	{
		return SoulType.Red;
	}

	public void StartAttraction(Transform target, System.Action onAttractionCompleted)
	{
		_soulComponent.StartAttraction(target, onAttractionCompleted);
	}

	public void OnAbsorptionCompleted()
	{
		_soulComponent.OnAbsorptionCompleted();
	}
}
