using UnityEngine;

public interface ISoul
{
	public Transform Transform { get; }

	public SoulType GetSoulType();

	public void StartAttraction(Transform target, System.Action onAttractionCompleted);
	public void OnAbsorptionCompleted();
	public void ApplySpawnKnockback(Vector2 direction, float force);
	public void SetAnimatorUnscaledTime();
}