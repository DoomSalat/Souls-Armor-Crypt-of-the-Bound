using UnityEngine;
using Sirenix.OdinInspector;

public class BlueSoul : MonoBehaviour, ISoul
{
	[SerializeField, Required] private Soul _soulComponent;

	public Transform Transform => transform;

	public SoulType GetSoulType()
	{
		return SoulType.Blue;
	}

	public void OnAbsorptionCompleted()
	{
		_soulComponent.OnAbsorptionCompleted();
	}

	public void StartAttraction(Transform target, System.Action onAttractionCompleted)
	{
		_soulComponent.StartAttraction(target, onAttractionCompleted);
	}
}