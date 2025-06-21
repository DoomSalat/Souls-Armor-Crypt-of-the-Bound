using UnityEngine;

public interface ISoul
{
	public Transform Transform { get; }

	public SoulType GetSoulType();
	public IAbility GetAbility();

	public void StartAttraction(Transform target, System.Action onAttractionCompleted);
	public void OnAbsorptionCompleted();
}